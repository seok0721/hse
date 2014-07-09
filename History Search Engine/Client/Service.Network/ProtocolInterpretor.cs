using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using Client.Domain;
using Client.Utility;
using Client.Dao;

using log4net;

namespace Client.Service.Network
{
    /// <summary>
    /// 사용자 명령 해석기는 서버 명령 해석기와 통신합니다.
    /// 
    /// 명령어와 인자를 전송하고, 응답 코드와 메시지를 수신합니다.
    /// </summary>
    public class ProtocolInterpretor
    {
        private ILog logger = LogManager.GetLogger(typeof(ProtocolInterpretor));
        private DataTransferProcess userDTP = new DataTransferProcess();
        private StreamReader reader;
        private StreamWriter writer;
        private Socket socket;

        public Properties Properties { get; set; }
        public FileDao FileDao { get; set; }

        /// <summary>
        /// 사용자 명령 해석기를 초기화 합니다.
        /// 서버 명령 해석기에 접속하기 위한 소켓을 초기화 합니다.
        /// </summary>
        public void Init()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        /// <summary>
        /// 서버 명령 해석기에 접속을 시도합니다.
        /// </summary>
        /// <returns>접속 성공 시 true, 실패 시 false</returns>
        public bool Connect()
        {
            try
            {
                socket.Connect(Properties["server.ip"], int.Parse(Properties["server.port"]));
            }
            catch (Exception ex)
            {
                logger.Error("서버에 접속하는 도중 오류가 발생하였습니다.");
                logger.Error(ex.Message);

                return false;
            }

            reader = new StreamReader(new NetworkStream(socket));
            writer = new StreamWriter(new NetworkStream(socket));

            return EstablishConnection();
        }

        /// <summary>
        /// 검색 엔진 서버에 로그인 합니다.
        /// </summary>
        /// <param name="userId">사용자 아이디</param>
        /// <param name="passwd">비밀번호</param>
        /// <returns>로그인 성공 시 true, 아니면 false</returns>
        public bool Login(String userId, String passwd)
        {
            SearchEngineResponse response = null;

            /* USER 커맨드 송신 및 응답 수신 */
            SendRequest(SearchEngineRequest.UserId, userId);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.UserLoggedIn:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.NotLoggedIn:
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    return false;
                case SearchEngineResponse.NeedAccountForLogin:
                    /* FIXME : 이 응답 코드의 용도가 불명확합니다. */
                    logger.Warn(response.ToString());
                    return false;
                case SearchEngineResponse.RequestPassword:
                    break;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            /* PASS 커맨드 송신 및 응답 수신 */
            SendRequest(SearchEngineRequest.Password, passwd);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.UserLoggedIn:
                case SearchEngineResponse.CommandNotImplemented:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.NotLoggedIn:
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.BadSequenceOfCommands:
                case SearchEngineResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    return false;
                case SearchEngineResponse.NeedAccountForLogin:
                    /* FIXME : 이 응답 코드의 용도가 불명확합니다. */
                    logger.Warn(response.ToString());
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 검색 엔진 서버와의 접속을 종료합니다.
        /// </summary>
        /// <returns>로그아웃 성공 시 true, 아니면 false</returns>
        public bool Logout()
        {
            SearchEngineResponse response = null;

            SendRequest(SearchEngineRequest.Logout);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.ServiceClosingControlConnection:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.UnknownCommandError:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="operation"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool SendFileOperation(String filePath, char operation, DateTime time)
        {
            SearchEngineResponse response = null;
            FileModel mFile = GetFileModel(filePath);
            FileIOLog mFileIOLog = new FileIOLog();
            mFileIOLog.ID = 0;
            mFileIOLog.Sequence = 0;
            mFileIOLog.IOType = operation;
            mFileIOLog.IOTime = time;

            SendRequest(SearchEngineRequest.FileOperation, String.Format("{0}|{1}", mFile.ToString(), mFileIOLog.ToString()));

            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.CommandOkay:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                case SearchEngineResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 서버로 파일을 저장합니다. 이미 파일이 있으면 최신화 시킵니다.
        /// 서버에 파일이 없는 경우 수신 데이터는 "00000000000000000000000000000000" 이며
        /// 이미 최신 파일을 가지고 있으면 "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"를 수신합니다.
        /// 나머지는 최신화를 요청하는 스트림입니다.
        /// </summary>
        /// <param name="model">파일 기본 정보를 담고있는 모델</param>
        public bool StoreFile(String filePath)
        {
            SearchEngineResponse response = null;
            FileModel model = null;

            if (!OpenDataPort())
            {
                return false;
            }

            /* 파일에 대한 정보가 없으면 에러 */
            try
            {
                model = GetFileModel(filePath);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

            /* 서버로 파일의 유니크한 아이디와 최종 변경 시간을 전송 */
            SendRequest(SearchEngineRequest.Store, model.ToString());

            /* 파일 처리에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case SearchEngineResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    /* 사용자 DTP가 서버 DTP와 접속되지 않았을 경우 접속 대기 */
                    if (!userDTP.Connected)
                    {
                        logger.Debug("서버 접속을 기다리는 중...");
                        userDTP.WaitForConnection();
                    }

                    break;
                case SearchEngineResponse.NeedAccountForStoringFiles:
                case SearchEngineResponse.FileUnavailable:
                case SearchEngineResponse.InsuffcientStorageSpaceInSystem:
                case SearchEngineResponse.FileNameNotAllowed:
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                case SearchEngineResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            /* 체크섬 스트림 수신, 파일 차이 스트림 전송 */
            switch (userDTP.ReceiveChecksumStream(model))
            {
                case StoreResponseCode.RequestDifferenceStream:
                    userDTP.SendDifferenceStream(model);
                    break;
                case StoreResponseCode.RequestNewFile:
                    userDTP.SendFileStream(model);
                    break;
                case StoreResponseCode.AlreadyTheNewestFile:
                default:
                    break;
            }

            /* 파일 전송에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                // case SearchEngineResponse.RestartMarkerReply:
                case SearchEngineResponse.CloseDataConnection:
                    logger.Info(response.Message);

                    if(userDTP.Connected)
                    {
                        userDTP.ReleaseConnection();
                    }

                    return true;
                case SearchEngineResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.CannotOpenDataConnection:
                case SearchEngineResponse.ConnectionClosed:
                case SearchEngineResponse.LocalErrorInProcessing:
                case SearchEngineResponse.PageTypeUnknown:
                case SearchEngineResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        // FIXME : 나중에 수정
        public bool RetrieveFile(int fileId, String filePath)
        {
            SearchEngineResponse response = null;

            if (!OpenDataPort())
            {
                return false;
            }

            SendRequest(SearchEngineRequest.Retrieve, fileId.ToString());

            /* 파일 처리에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case SearchEngineResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    /* 사용자 DTP가 서버 DTP와 접속되지 않았을 경우 접속 대기 */
                    if (!userDTP.Connected)
                    {
                        logger.Debug("서버 접속을 기다리는 중...");
                        userDTP.WaitForConnection();
                    }

                    break;
                case SearchEngineResponse.NeedAccountForStoringFiles:
                case SearchEngineResponse.FileUnavailable:
                case SearchEngineResponse.InsuffcientStorageSpaceInSystem:
                case SearchEngineResponse.FileNameNotAllowed:
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                case SearchEngineResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            /* 서버에서 보낸 리스트를 수신 */
            userDTP.ReceiveFileStream(filePath);
            
            /* 파일 전송에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                // case SearchEngineResponse.RestartMarkerReply:
                case SearchEngineResponse.CloseDataConnection:
                    if (userDTP.Connected)
                    {
                        userDTP.ReleaseConnection();
                    }

                    return true;
                case SearchEngineResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.CannotOpenDataConnection:
                case SearchEngineResponse.ConnectionClosed:
                case SearchEngineResponse.LocalErrorInProcessing:
                case SearchEngineResponse.PageTypeUnknown:
                case SearchEngineResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        // FIXME 아래 함수
        public String GetFileList(String searchWord)
        {
            return null;
        }

        // FIXME StoreFile에서 카피캣 한거라 프로토콜과 일치하지 않을수 있음
        public String GetFileList(String searchWord, DateTime workingDay)
        {
            SearchEngineResponse response = null;

            if (searchWord == null && workingDay == null)
            {
                throw new Exception("검색어 또는 작업일을 입력하세요.");
            }   

            if (!OpenDataPort())
            {
                throw new Exception("데이터 포트를 여는데 실패하였습니다.");
            }

            if (searchWord == null && workingDay != null)
            {
                SendRequest(SearchEngineRequest.List, String.Format("D:{0}", workingDay.ToString("yyyy-MM-dd")));
            }

            if (workingDay != null && workingDay == null)
            {

            }

            if (workingDay != null && workingDay != null)
            {

            }

            /* 파일 처리에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case SearchEngineResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    /* 사용자 DTP가 서버 DTP와 접속되지 않았을 경우 접속 대기 */
                    if (!userDTP.Connected)
                    {
                        logger.Debug("서버 접속을 기다리는 중...");
                        userDTP.WaitForConnection();
                    }

                    break;
                case SearchEngineResponse.NeedAccountForStoringFiles:
                case SearchEngineResponse.FileUnavailable:
                case SearchEngineResponse.InsuffcientStorageSpaceInSystem:
                case SearchEngineResponse.FileNameNotAllowed:
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                case SearchEngineResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return null;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            /* 서버에서 보낸 리스트를 수신 */
            String result = userDTP.ReceiveData();
            logger.Debug(result);
            /* 파일 전송에 관련된 응답 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                // case SearchEngineResponse.RestartMarkerReply:
                case SearchEngineResponse.CloseDataConnection:
                    if (userDTP.Connected)
                    {
                        userDTP.ReleaseConnection();
                    }

                    return result;
                case SearchEngineResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return result;
                case SearchEngineResponse.CannotOpenDataConnection:
                case SearchEngineResponse.ConnectionClosed:
                case SearchEngineResponse.LocalErrorInProcessing:
                case SearchEngineResponse.PageTypeUnknown:
                case SearchEngineResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return null;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 검색 엔진 서버와 연결을 성립합니다.
        /// </summary>
        private bool EstablishConnection()
        {
            SearchEngineResponse response = null;

            /*  검색 엔진 서버로 부터 연결 성립 응답코드 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.ServiceReadyInMinutes:
                    logger.Info(response.Message);
                    break;
                case SearchEngineResponse.ServiceReadyForNewUser:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            /*  검색 엔진 서버에서 제한시간 내 응답이 오기를 대기 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.ServiceReadyForNewUser:
                    logger.Info(response.Message);
                    return true;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 파일의 기본 정보를 처리합니다.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private FileModel GetFileModel(string filePath)
        {
            FileModel model = new FileModel();
            FileInfo fi = new FileInfo(filePath);
            
            if (!fi.Exists)
            {
                throw new Exception("파일이 존재하지 않습니다.");
            }

            model.ID = 0;
            model.UniqueId = FileUtils.GetUniqueIdentifier(fi.FullName);
            model.UserId = null;
            model.Path = fi.DirectoryName;
            model.Name = fi.Name;
            model.Size = fi.Length;
            model.LastUpdateTime = FileUtils.GetLastWriteTime(fi.FullName);
            model.RemoveYn = 'N';

            return model;
        }

        /// <summary>
        /// 검색 엔진 서버와 데이터를 주고받을 포트를 엽니다.
        /// </summary>
        /// <returns>서버에서 User-DTP 소켓으로 접속 성공 시 true, 아니면 false</returns>
        private bool OpenDataPort()
        {
            SearchEngineResponse response = null;

            /* 사용자 DTP에 주소가 바인딩 되지 않았으면 초기화 */
            if (!userDTP.IsBound)
            {
                userDTP.Init();
            }

            /* 사용자 DTP 소켓의 포트를 확인한 후, 서버로 아이피 주소와 함께 전송 */
            SendRequest(SearchEngineRequest.DataPort, String.Format("{0},{1},{2}",
                GetIP().Replace('.', ','), (userDTP.Port & 0xFF00) >> 8, (userDTP.Port & 0x00FF)));

            /* 서버에서 처리 결과 응답코드 수신 */
            response = ReceiveResponse();

            switch (response.Code)
            {
                case SearchEngineResponse.CommandOkay:
                    logger.Info(response.Message);
                    return true;
                case SearchEngineResponse.UnknownCommandError:
                case SearchEngineResponse.InvalidArgumentError:
                case SearchEngineResponse.ServiceNotAvailable:
                case SearchEngineResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        /// <summary>
        /// 호스트의 IP 주소를 가져옵니다.
        /// </summary>
        /// <returns>IP 주소</returns>
        private String GetIP()
        {
            WebClient client = new WebClient();
            return client.DownloadString("http://icanhazip.com").Replace("\r", "").Replace("\n", "");
        }

        /// <summary>
        /// 서버로 명령어를 전송합니다.
        /// </summary>
        /// <param name="command">명령어</param>
        private void SendRequest(String command)
        {
            logger.DebugFormat("송신: {0}", command);

            writer.WriteLine(command);
            writer.Flush();
        }

        /// <summary>
        /// 서버로 명령어와 인자를 전송합니다.
        /// </summary>
        /// <param name="command">명령어</param>
        /// <param name="argument">인자</param>
        private void SendRequest(String command, String argument)
        {
            if (command == null)
            {
                throw new Exception("전송할 명령어가 없습니다.");
            }

            if (argument == null)
            {
                logger.DebugFormat("송신: {0}", command);

                writer.WriteLine(command);
            }
            else
            {
                logger.DebugFormat("송신: {0} {1}", command, argument);

                writer.WriteLine("{0} {1}", command, argument);
            }

            writer.Flush();
        }

        /// <summary>
        /// 서버에서 보내는 응답을 수신합니다.
        /// </summary>
        /// <returns>서버 응답 코드 및 메시지 객체</returns>
        private SearchEngineResponse ReceiveResponse()
        {
            SearchEngineResponse response = new SearchEngineResponse();
            String[] pair = null;
            String line = null;
            char[] split = { ' ' };

            /* 서버 명령 해석기와 연결되지 않았을 경우 */
            if (socket == null || !socket.Connected)
            {
                response.Code = SearchEngineResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            line = reader.ReadLine();

            /* 서버 명령 해석기로 부터 받은 응답이 없을 경우 */
            if (line == null)
            {
                response.Code = SearchEngineResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            pair = line.Trim().Split(split, 2);

            /* 잘못된 응답 코드를 수신한 경우 */
            if (pair[0].Length != 3)
            {
                throw new Exception("알 수 없는 응답 코드가 수신되었습니다.");
            }

            /* 서버 명령 해석기의 응답을 파싱 */
            response.Code = int.Parse(pair[0]);

            if (pair.Length == 1)
            {
                logger.DebugFormat("수신: {0}", pair[0]);
            }
            else if (pair.Length == 2)
            {
                logger.DebugFormat("수신: {0} {1}", pair[0], pair[1]);

                response.Message = pair[1];
            }

            return response;
        }
    }
}
