using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Reference.Utility;
using Reference.Model;
using Reference.Protocol;
using log4net;

namespace Client.Service.Network
{
    public class UserProtocolInterpretor
    {
        private ILog logger = LogManager.GetLogger(typeof(UserProtocolInterpretor));
        private UserDataTransferProcess userDTP = new UserDataTransferProcess();
        private StreamReader reader;
        private StreamWriter writer;
        private Socket socket;
        private Properties properties = new Properties();

        public void Init()
        {
            logger.Info("사용자 명령 해석기 초기화");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            LoadConfiguration();
            userDTP.Init();
        }

        public bool Connect()
        {
            logger.Info("서버로 접속을 시도합니다.");

            try
            {
                socket.Connect(properties["SERVER_IP"], int.Parse(properties["SERVER_PORT"]));
            }
            catch (Exception ex)
            {
                logger.Error("서버에 접속하는 도중 오류가 발생하였습니다.");
                logger.Error(ex.Message);

                return false;
            }

            logger.Info("서버에 접속되었습니다.");

            reader = new StreamReader(new NetworkStream(socket));
            writer = new StreamWriter(new NetworkStream(socket));

            try
            {
                return EstablishConnection();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool Login(String userId, String password)
        {
            int code;

            try
            {
                code = SendUserCommand(userId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

            switch (code)
            {
                case ProtocolResponse.UserLoggedIn:
                    return true;
                case ProtocolResponse.RequestPassword:
                    break;
                default:
                    return false;
            }

            try
            {
                return SendPassCommand(password);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                SendQuitCommand();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public bool StoreFile(FileInfo fileInfo)
        {
            try
            {
                if (!SendPortCommand())
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

            try
            {
                FileModel model = FileModel.FromFileInfo(fileInfo);

                return SendStorCommand(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool StoreFileWord(FileInfo fileInfo, String wordList)
        {
            try
            {
                return SendFlwdCommand(FileModel.FromFileInfo(fileInfo), wordList);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool StoreHtml(String url, String wordList)
        {
            try
            {
                HtmlModel model = new HtmlModel();
                model.URL = url;

                return SendHtwdCommand(model, wordList);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool RetriveFileList(String keyword, out String result)
        {
            try
            {
                if (!SendPortCommand())
                {
                    result = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = null;
                return false;
            }

            try
            {
                return SendListCommand(keyword, out result);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                result = null;
                return false;
            }
        }

        public bool RetriveFile(int fileId, String filePath, String fileName)
        {
            try
            {
                if (!SendPortCommand())
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

            FileModel model = new FileModel();
            model.FileId = fileId;
            model.Path = filePath;
            model.Name = fileName;

            try
            {
                return SendRetrCommand(model);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        private bool SendHtwdCommand(HtmlModel htmlModel, String wordList)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.HtmlWord, String.Format("{0},{1}", htmlModel.ToString(), wordList));
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                case ProtocolResponse.CommandOkay:
                    logger.Info(response.Message);
                    return true;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendFlwdCommand(FileModel fileModel, String wordList)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.FileWord, String.Format("{0},{1}", fileModel.UniqueId, wordList));
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.NotLoggedIn:
                case ProtocolResponse.FileUnavailable:
                    logger.Info(response.Message);
                    return false;
                case ProtocolResponse.CommandOkay:
                    logger.Info(response.Message);
                    return true;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendRetrCommand(FileModel fileModel)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.Store, fileModel.FileId.ToString());
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    if (!userDTP.Connected) // 사용자 DTP가 서버 DTP와 접속되지 않았을 경우 접속 대기
                    {
                        logger.Debug("서버 접속을 기다리는 중...");

                        if (!userDTP.WaitServerDTP())
                        {
                            logger.Error("서버 데이터 전송 프로세스의 연결을 기다리는 도중 오류가 발생하였습니다.");
                            return false;
                        }

                        logger.Debug("접속되었습니다.");
                    }

                    break;
                case ProtocolResponse.NeedAccountForStoringFiles:
                case ProtocolResponse.FileUnavailable:
                case ProtocolResponse.InsuffcientStorageSpaceInSystem:
                case ProtocolResponse.FileNameNotAllowed:
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.ServiceNotAvailable:
                case ProtocolResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            userDTP.ReceiveFileStream(String.Format("{0}\\{1}", fileModel.Path, fileModel.Name));
            response = ReceiveResponse();

            switch (response.Code)
            {
                // case ProtocolResponse.RestartMarkerReply:
                case ProtocolResponse.CloseDataConnection:
                    logger.Info(response.Message);

                    if (userDTP.Connected)
                    {
                        userDTP.CloseServerDTP();
                    }

                    return true;
                case ProtocolResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.CannotOpenDataConnection:
                case ProtocolResponse.ConnectionClosed:
                case ProtocolResponse.LocalErrorInProcessing:
                case ProtocolResponse.PageTypeUnknown:
                case ProtocolResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendListCommand(String keyword, out String result)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.List, keyword);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    if (!userDTP.Connected)
                    {
                        logger.Debug("서버 접속을 기다리는 중...");

                        if (!userDTP.WaitServerDTP())
                        {
                            logger.Error("서버 데이터 전송 프로세스의 연결을 기다리는 도중 오류가 발생하였습니다.");
                            result = null;
                            return false;
                        }

                        logger.Debug("접속되었습니다.");
                    }

                    break;
                case ProtocolResponse.NeedAccountForStoringFiles:
                case ProtocolResponse.FileUnavailable:
                case ProtocolResponse.InsuffcientStorageSpaceInSystem:
                case ProtocolResponse.FileNameNotAllowed:
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.ServiceNotAvailable:
                case ProtocolResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    result = null;
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            result = userDTP.ReceiveStream();
            response = ReceiveResponse();

            switch (response.Code)
            {
                // case ProtocolResponse.RestartMarkerReply:
                case ProtocolResponse.CloseDataConnection:
                    logger.Info(response.Message);

                    if (userDTP.Connected)
                    {
                        userDTP.CloseServerDTP();
                    }

                    return true;
                case ProtocolResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.CannotOpenDataConnection:
                case ProtocolResponse.ConnectionClosed:
                case ProtocolResponse.LocalErrorInProcessing:
                case ProtocolResponse.PageTypeUnknown:
                case ProtocolResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendStorCommand(FileModel fileModel)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.Store, fileModel.ToString());
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.DataConnectionAlreadyOpen:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.OpenDataConnection:
                    logger.Info(response.Message);

                    if (!userDTP.Connected) // 사용자 DTP가 서버 DTP와 접속되지 않았을 경우 접속 대기
                    {
                        logger.Debug("서버 접속을 기다리는 중...");

                        if (!userDTP.WaitServerDTP())
                        {
                            logger.Error("서버 데이터 전송 프로세스의 연결을 기다리는 도중 오류가 발생하였습니다.");
                            return false;
                        }

                        logger.Debug("접속되었습니다.");
                    }

                    break;
                case ProtocolResponse.NeedAccountForStoringFiles:
                case ProtocolResponse.FileUnavailable:
                case ProtocolResponse.InsuffcientStorageSpaceInSystem:
                case ProtocolResponse.FileNameNotAllowed:
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.ServiceNotAvailable:
                case ProtocolResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            switch (userDTP.ReceiveChecksumStream(fileModel))
            {
                case StoreCommandResponseCode.RequestDifferenceStream:
                    userDTP.SendDifferenceStream(fileModel);
                    break;
                case StoreCommandResponseCode.RequestNewFile:
                    userDTP.SendFileStream(fileModel);
                    break;
                case StoreCommandResponseCode.AlreadyTheNewestFile:
                default:
                    break;
            }

            response = ReceiveResponse();

            switch (response.Code)
            {
                // case ProtocolResponse.RestartMarkerReply:
                case ProtocolResponse.CloseDataConnection:
                    logger.Info(response.Message);

                    if (userDTP.Connected)
                    {
                        userDTP.CloseServerDTP();
                    }

                    return true;
                case ProtocolResponse.FileActionCompleted:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.CannotOpenDataConnection:
                case ProtocolResponse.ConnectionClosed:
                case ProtocolResponse.LocalErrorInProcessing:
                case ProtocolResponse.PageTypeUnknown:
                case ProtocolResponse.ExceededStorageAllocation:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendPortCommand()
        {
            ProtocolResponse response;

            if (userDTP.Connected)
            {
                return true;
            }

            if (!userDTP.Opened)
            {
                if (!userDTP.OpenUserDTP(int.Parse(properties["USER_DTP_BACKLOG"])))
                {
                    return false;
                }
            }

            SendRequest(ProtocolRequest.DataPort, String.Format("{0},{1},{2}",
                GetIP().Replace('.', ','), (userDTP.Port & 0xFF00) >> 8, (userDTP.Port & 0x00FF)));

            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.CommandOkay:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.ServiceNotAvailable:
                case ProtocolResponse.NotLoggedIn:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private int SendUserCommand(String userId)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.UserId, userId);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.UserLoggedIn:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.NotLoggedIn:
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.NeedAccountForLogin: // FIXME : 이 응답 코드의 용도가 불명확합니다.
                    logger.Warn(response.ToString());
                    break;
                case ProtocolResponse.RequestPassword:
                    break;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            return response.Code;
        }

        private bool SendPassCommand(String password)
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.Password, password);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.UserLoggedIn:
                case ProtocolResponse.CommandNotImplemented:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.NotLoggedIn:
                case ProtocolResponse.UnknownCommandError:
                case ProtocolResponse.InvalidArgumentError:
                case ProtocolResponse.BadSequenceOfCommands:
                case ProtocolResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    return false;
                case ProtocolResponse.NeedAccountForLogin: // FIXME : 이 응답 코드의 용도가 불명확합니다.
                    logger.Warn(response.ToString());
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool SendQuitCommand()
        {
            ProtocolResponse response;

            SendRequest(ProtocolRequest.Logout);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.ServiceClosingControlConnection:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.UnknownCommandError:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private bool LoadConfiguration()
        {
            String configPath = AppDomain.CurrentDomain.BaseDirectory + "config.properties";

            try
            {
                properties.Load(configPath);

                return true;
            }
            catch (Exception)
            {
                logger.Error("환경설정 파일을 읽어오는 도중 오류가 발생하였습니다.");
                logger.ErrorFormat("다음 파일을 확인하십시오: {0}", configPath);

                return false;
            }
        }

        private bool EstablishConnection()
        {
            ProtocolResponse response;

            response = ReceiveResponse();

            switch (response.Code)
            {
                case ProtocolResponse.ServiceReadyInMinutes:
                    logger.Info(response.Message);
                    break;
                case ProtocolResponse.ServiceReadyForNewUser:
                    logger.Info(response.Message);
                    return true;
                case ProtocolResponse.ServiceNotAvailable:
                    logger.Info(response.Message);
                    return false;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }

            response = ReceiveResponse(); // 검색 엔진 서버에서 제한시간 내 응답이 오기를 대기 

            switch (response.Code)
            {
                case ProtocolResponse.ServiceReadyForNewUser:
                    logger.Info(response.Message);
                    return true;
                default:
                    throw new Exception("알 수 없는 에러가 발생하였습니다.");
            }
        }

        private String GetIP()
        {
             WebClient client = new WebClient();
             //FIXME 공인 아이피 아니면 안됨...
             return client.DownloadString("http://icanhazip.com").Replace("\r", "").Replace("\n", "");
             //return "127.0.0.1";

            //IEnumerator<IPAddress> e = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Reverse().GetEnumerator();

            //while (e.MoveNext())
            //{
            //    if (e.Current.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return e.Current.ToString();
            //    }
            //}

            //throw new Exception("인터넷 장치를 찾을 수 없습니다.");
        }

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

        private void SendRequest(String command)
        {
            SendRequest(command, null);
        }

        private ProtocolResponse ReceiveResponse()
        {
            ProtocolResponse response = new ProtocolResponse();
            String[] pair = null;
            String line = null;
            char[] split = { ' ' };

            // 서버 명령 해석기와 연결되지 않았을 경우 
            if (socket == null || !socket.Connected)
            {
                response.Code = ProtocolResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            try
            {
                line = reader.ReadLine();
            }
            catch (Exception)
            {
                response.Code = ProtocolResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            // 서버 명령 해석기로 부터 받은 응답이 없을 경우 
            if (line == null)
            {
                response.Code = ProtocolResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            pair = line.Trim().Split(split, 2);

            // 잘못된 응답 코드를 수신한 경우 
            if (pair[0].Length != 3)
            {
                throw new Exception("알 수 없는 응답 코드가 수신되었습니다.");
            }

            // 서버 명령 해석기의 응답을 파싱 
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
