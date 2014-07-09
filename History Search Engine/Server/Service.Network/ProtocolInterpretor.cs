using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Server.Dao;
using Server.Domain;
using Server.Utility;

using NHibernate;

using log4net;

namespace Server.Service.Network
{
    /// <summary>
    /// 사용자가 보내는 명령을 해석하고 처리하는 명령 해석기
    /// </summary>
    public class ProtocolInterpretor
    {
        private ILog logger = LogManager.GetLogger(typeof(ProtocolInterpretor));
        private DataTransferProcess serverDTP = new DataTransferProcess();
        private NetworkStream ns;
        private StreamReader reader;
        private StreamWriter writer;
        private Boolean isLogin = false;
        private String userId;
        private String passwd;
        private String basedir;

        public Socket Socket { get; set; }
        public FileDao FileDao { get; set; }
        public FileIOLogDao FileIOLogDao { get; set; }
        public UserDao UserDao { get; set; }
        public Properties Properties { get; set; }

        /// <summary>
        /// 서버 명령 해석기를 초기화 합니다.
        /// </summary>
        public void Init()
        {
            logger.Info("서버 명령 해석기 초기화");

            if (Socket != null && Socket.Connected)
            {
                ns = new NetworkStream(Socket);
                writer = new StreamWriter(ns);
                reader = new StreamReader(ns);
            }

            basedir = String.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, Properties["basedir"]);
        }

        /// <summary>
        /// 서버 명령 해석기를 시작합니다.
        /// </summary>
        public void Start()
        {
            logger.Info("서버 명령 해석기 시작");

            EstablishConnection();
            FetchRequest();
            ReleaseConnection();
        }

        /// <summary>
        /// 사용자의 연결을 성립합니다.
        /// </summary>
        private void EstablishConnection()
        {
            logger.InfoFormat("사용자 접속 승인, {0}", Socket.RemoteEndPoint.ToString());

            SendResponse(SearchEngineResponse.ServiceReadyForNewUser, "검색 엔진 서버에 접속 하신걸 환영합니다.");
        }

        /// <summary>
        /// 사용자가 보내는 명령을 해석하고 처리합니다.
        /// </summary>
        private void FetchRequest()
        {
            FileModel mFile = null;
            FileModel mFileTemp = null;
            FileIOLog mFileIOLog = null;
            String[] args = null;

            while (true)
            {
                SearchEngineRequest request;

                /* 사용자가 보낸 명령을 수신 */
                try
                {
                    request = ReceiveRequest();
                }
                /* 인터넷 장애 등으로 인하여 접속이 끊기는 경우, 사용자의 명령을 더이상 수신하지 않음 */
                catch (Exception)
                {
                    logger.ErrorFormat("사용자와 접속이 끊겼습니다, {0}", Socket.RemoteEndPoint.ToString());
                    return;
                }

                /* 사용자자 보낸 명령을 해석 */
                switch (request.Command)
                {
                    /* 로그인을 위한 사용자 아이디를 수신하는 경우 */
                    case SearchEngineRequest.UserId:
                        #region USER <UserName:String>
                        /* 이미 로그인 되어 있을 때 */
                        if (isLogin)
                        {
                            /* 이미 로그인 중이라는 응답 전송 */
                            SendResponse(SearchEngineResponse.UserLoggedIn, "이미 로그인 중 입니다.");
                            break;
                        }

                        /* 로그인 되어 있지 않으면, 비밀번호를 요청 */
                        SendResponse(SearchEngineResponse.RequestPassword, "비밀번호를 입력하세요.");

                        userId = request.Argument;

                        #endregion
                        break;
                    /* 로그인을 위한 비밀번호를 수신하는 경우 */
                    case SearchEngineRequest.Password:
                        #region PASS <MD5 Hash Password:String>
                        /* 이미 로그인 되어 있을 때 */
                        if (isLogin)
                        {
                            /* 이미 로그인 중이라는 응답 전송 */
                            SendResponse(SearchEngineResponse.UserLoggedIn, "이미 접속 중 입니다.");
                            break;
                        }

                        passwd = request.Argument;

                        /* 클라이언트가 보낸 정보로 로그인 성공 시 */
                        if (Login(userId, passwd))
                        {
                            /* 로그인 성공 메시지 전송 */
                            SendResponse(SearchEngineResponse.UserLoggedIn, "검색 엔진 서버에 로그인 되었습니다.");

                            isLogin = true;
                        }
                        else
                        {
                            /* 실패 시, 로그인 실패 메시지 전송 */
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인에 실패하였습니다.");

                            isLogin = false;
                            userId = null;
                            passwd = null;
                        }
                        #endregion
                        break;
                    /* 로그아웃 요청을 받은 경우 */
                    case SearchEngineRequest.Logout:
                        #region QUIT
                        /* 정상 종료 메시지를 전송 */
                        SendResponse(SearchEngineResponse.ServiceClosingControlConnection, "서버와의 연결이 종료되었습니다.");
                        #endregion
                        return;
                    /* 데이터 전송을 위한 포트에 접속 대기를 알리는 경우 */
                    case SearchEngineRequest.DataPort:
                        #region PORT <Host-Number:int,int,int,int>,<Port-Number:int,int>
                        /* 로그인 되어 있지 않을 때 */
                        if (!isLogin)
                        {
                            /* 로그인 상태 아님을 알리는 응답 전송 */
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인 후 사용하세요.");
                            break;
                        }

                        /* 이미 접속 되어 있으면 확인 응답 */
                        if (serverDTP.Connected)
                        {
                            SendResponse(SearchEngineResponse.CommandOkay, "데이터 접속이 성립되었습니다.");
                            break;
                        }

                        args = request.Argument.Split(',');

                        /* 인수 검사 */
                        if (args.Length != 6)
                        {
                            SendResponse(SearchEngineResponse.InvalidArgumentError, "인자 형식이 잘못되었습니다.");
                            break;
                        }

                        /* IP와 Port파싱 후 User-DTP 파라미터 초기화 */
                        serverDTP.IPAddress = String.Format("{0}.{1}.{2}.{3}", args[0], args[1], args[2], args[3]); ;
                        serverDTP.Port = (int.Parse(args[4]) << 8) + int.Parse(args[5]);

                        SendResponse(SearchEngineResponse.CommandOkay, "데이터 접속이 성립되었습니다.");

                        #endregion
                        break;
                    /* 2.5. 데이터 전송 요청 */
                    case SearchEngineRequest.Store:
                        #region STOR <String Of FileModel:String>

                        /* 비 로그인 상태인 경우 */
                        if (!isLogin)
                        {
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인 후 사용하세요.");
                            break;
                        }

                        /* Server-DTP가 연결되어 있지 않은 경우 */
                        if (!serverDTP.Connected)
                        {
                            SendResponse(SearchEngineResponse.OpenDataConnection, "접속을 시도합니다.");

                            /* Server-DTP가 연결에 실패하는 경우 */
                            if (!serverDTP.Connect())
                            {
                                SendResponse(SearchEngineResponse.CannotOpenDataConnection, "데이터 포트에 접속할 수 없습니다.");
                                break;
                            }
                        }
                        /* Server-DTP가 이미 연결되어 있는 경우 */
                        else
                        {
                            SendResponse(SearchEngineResponse.DataConnectionAlreadyOpen, "이미 접속 되어 있습니다. 데이터 전송을 시작합니다.");
                        }

                        /* 사용자 보낸 파일 정보로 서버에 파일이 존재하는지 확인 */
                        FileModel userFile = FileModel.FromString(request.Argument);
                        FileModel serverFile = FileDao.ReadFileUsingUniqueId(userFile.UniqueId);

                        /* 서버에 파일이 있을 경우 */
                        if (serverFile != null)
                        {
                            logger.Info("사용자가 파일의 최신화를 요청합니다.");

                            /* 서버 파일의 최종 수정 시간과 사용자 파일의 최종 수정 시간이 같을 때 */
                            if (serverFile.LastUpdateTime == userFile.LastUpdateTime)
                            {
                                logger.Info("이미 최신 파일이 존재합니다.");

                                serverDTP.SendAlreadyTheNewestFile();
                                serverDTP.ReleaseConnection();

                                SendResponse(SearchEngineResponse.CloseDataConnection, "이미 파일의 상태가 최신입니다.");
                            }
                            /* 서버 파일의 최종 수정 시간과 사용자 파일의 최중 수정 시간이 다를 때 */
                            else
                            {
                                logger.Info("최신화를 위해 체크섬을 전송합니다.");

                                serverDTP.SendChecksumStream(serverFile);

                                logger.Info("변경 스트림을 수신합니다.");
                                serverDTP.ReceiveDifferenceStream(serverFile);
                                serverDTP.ReleaseConnection();

                                userFile.UserId = userId;
                                userFile.RemoveYn = 'N';

                                FileDao.UpdateFile(userFile);
                                FileDao.Session.Flush();

                                SendResponse(SearchEngineResponse.CloseDataConnection, "파일의 최신화가 완료되었습니다.");
                            }
                        }
                        /* 서버에 파일이 없을 경우 */
                        else
                        {
                            logger.Info("새 파일 전송을 요청합니다.");

                            userFile.ID = FileDao.ReadMaxFileId() + 1;
                            userFile.UserId = userId;
                            userFile.RemoveYn = 'N';

                            FileDao.CreateFile(userFile);
                            FileDao.Session.Flush();

                            serverDTP.SendToRequestNewFile();
                            serverDTP.ReceiveFileStream(userFile);
                            serverDTP.ReleaseConnection();

                            SendResponse(SearchEngineResponse.CloseDataConnection, "파일이 정상적으로 수신되었습니다.");
                        }

                        #endregion
                        break;
                    /* 파일 연산 정보 전송 */
                    case SearchEngineRequest.FileOperation:
                        #region FLIO <FileModel.ToString()>|<FileIOLog.ToString()>
                        /* 로그인 되어 있지 않을 때 */
                        if (!isLogin)
                        {
                            /* 로그인 상태 아님을 알리는 응답 전송 */
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인 후 사용하세요.");
                            break;
                        }

                        /* 사용자에게서 넘어온 인자 값 파싱 */
                        args = request.Argument.Split('|');

                        try
                        {
                            if (args.Length != 2)
                            {
                                throw new Exception();
                            }

                            mFile = FileModel.FromString(args[0]);
                            mFileIOLog = FileIOLog.FromString(args[1]);
                        }
                        catch (Exception)
                        {
                            SendResponse(SearchEngineResponse.InvalidArgumentError, "인자 값이 포맷과 일치하지 않습니다.");
                            break;
                        }

                        /* 데이터베이스에서 파일 기본 정보 조회 */
                        mFileTemp = this.FileDao.ReadFileUsingUniqueId(mFile.UniqueId);

                        /* 파일 기본 정보가 없으면 새로 생성 */
                        if (mFileTemp == null)
                        {
                            mFile.ID = this.FileDao.ReadMaxFileId() + 1;
                            mFile.UserId = userId;
                            mFile.RemoveYn = 'N';

                            FileDao.CreateFile(mFile);
                            FileDao.Session.Flush();
                        }

                        /* 파일 IO 로그 저장 */
                        mFileIOLog.ID = mFile.ID;
                        mFileIOLog.Sequence = this.FileIOLogDao.ReadMaxSequence(mFileIOLog.ID) + 1;

                        FileIOLogDao.CreateFileIOLog(mFileIOLog);
                        FileIOLogDao.Session.Flush();

                        SendResponse(SearchEngineResponse.CommandOkay, "파일 I/O 로그가 저장되었습니다.");

                        #endregion
                        break;
                    case SearchEngineRequest.List:
                        #region
                        /* 비 로그인 상태인 경우 */
                        if (!isLogin)
                        {
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인 후 사용하세요.");
                            break;
                        }

                        /* Server-DTP가 연결되어 있지 않은 경우 */
                        if (!serverDTP.Connected)
                        {
                            SendResponse(SearchEngineResponse.OpenDataConnection, "접속을 시도합니다.");

                            /* Server-DTP가 연결에 실패하는 경우 */
                            if (!serverDTP.Connect())
                            {
                                SendResponse(SearchEngineResponse.CannotOpenDataConnection, "데이터 포트에 접속할 수 없습니다.");
                                break;
                            }
                        }
                        /* Server-DTP가 이미 연결되어 있는 경우 */
                        else
                        {
                            SendResponse(SearchEngineResponse.DataConnectionAlreadyOpen, "이미 접속 되어 있습니다. 데이터 전송을 시작합니다.");
                        }

                        /* 파일 목록 전송 */
                        IList<FileModel> result = FileDao.ReadFileListLikeName(userId, request.Argument);

                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            serverDTP.SendData(result[i].ToString() + "\n");
                            logger.Debug(result[i].ToString() + "\n");
                        }

                        serverDTP.SendData(result[result.Count - 1].ToString());
                        logger.Debug(result[result.Count - 1].ToString());

                        SendResponse(SearchEngineResponse.CloseDataConnection, "파일이 정상적으로 수신되었습니다.");

                        /* 연결 해제 */
                        serverDTP.ReleaseConnection();

                        #endregion
                        break;
                    case SearchEngineRequest.Retrieve:
                        #region
                        /* 비 로그인 상태인 경우 */
                        if (!isLogin)
                        {
                            SendResponse(SearchEngineResponse.NotLoggedIn, "로그인 후 사용하세요.");
                            break;
                        }

                        /* Server-DTP가 연결되어 있지 않은 경우 */
                        if (!serverDTP.Connected)
                        {
                            SendResponse(SearchEngineResponse.OpenDataConnection, "접속을 시도합니다.");

                            /* Server-DTP가 연결에 실패하는 경우 */
                            if (!serverDTP.Connect())
                            {
                                SendResponse(SearchEngineResponse.CannotOpenDataConnection, "데이터 포트에 접속할 수 없습니다.");
                                break;
                            }
                        }
                        /* Server-DTP가 이미 연결되어 있는 경우 */
                        else
                        {
                            SendResponse(SearchEngineResponse.DataConnectionAlreadyOpen, "이미 접속 되어 있습니다. 데이터 전송을 시작합니다.");
                        }

                        /* 파일 전송 */
                        serverDTP.SendFileStream(int.Parse(request.Argument));

                        SendResponse(SearchEngineResponse.CloseDataConnection, "파일이 정상적으로 수신되었습니다.");

                        /* 연결 해제 */
                        serverDTP.ReleaseConnection();

                        #endregion
                        break;
                    default:
                        SendResponse(SearchEngineResponse.UnknownCommandError, "알 수 없는 명령어입니다.");
                        break;
                }
            }
        }

        /// <summary>
        /// 아이디와 패스워드를 사용하여 로그인을 시도합니다.
        /// </summary>
        /// <returns>로그인 성공 시 true, 아니면 false</returns>
        private bool Login(String userId, String passwd)
        {
            logger.InfoFormat("로그인 시도, {0}", userId);

            User model = new User();
            model.ID = userId;
            model = this.UserDao.ReadUser(userId);

            if (model == null)
            {
                logger.ErrorFormat("일치하는 사용자 아이디가 없습니다, {0}", userId);

                return false;
            }

            if (!model.Password.Equals(passwd))
            {
                logger.ErrorFormat("비밀번호가 일치하지 않습니다, {0}", userId);

                return false;
            }

            CreateUserDirectory(userId);

            logger.InfoFormat("로그인 되었습니다, {0}", userId);

            return true;
        }

        /// <summary>
        /// 사용자 디렉토리를 생성합니다.
        /// </summary>
        private void CreateUserDirectory(String userId)
        {
            DirectoryInfo di = new DirectoryInfo(String.Format("{0}\\{1}", basedir, userId));

            if (!di.Exists)
            {
                logger.InfoFormat("사용자 파일이 저장되는 디렉토리를 생성합니다, {0}", userId);

                di.Create();
            }
        }

        /// <summary>
        /// 접속에 사용된 리소스를 모두 해제합니다.
        /// </summary>
        private void ReleaseConnection()
        {
            isLogin = false;
            userId = null;
            passwd = null;

            reader.Close();
            writer.Close();
            ns.Close();
            Socket.Close();
        }

        /// <summary>
        /// 클라이언트로 응답 코드를 전송합니다.
        /// </summary>
        /// <param name="code">응답코드</param>
        private void SendResponse(int code)
        {
            logger.DebugFormat("송신: {0}", code);

            writer.WriteLine(code);
            writer.Flush();
        }

        /// <summary>
        /// 클라이언트로 응답 코드와 메시지를 전송합니다.
        /// </summary>
        /// <param name="code">응답코드</param>
        /// <param name="message">메시지</param>
        private void SendResponse(int code, String message)
        {
            logger.DebugFormat("송신: {0} {1}", code, message);

            writer.WriteLine(String.Format("{0} {1}", code, message));
            writer.Flush();
        }

        /// <summary>
        /// 클라이언트에서 보내는 명령과 파라미터를 수신합니다.
        /// </summary>
        /// <returns>클라이언트에서 보낸 SearchEngineRequest 객체</returns>
        private SearchEngineRequest ReceiveRequest()
        {
            SearchEngineRequest request = new SearchEngineRequest();
            String[] pair;
            String line = reader.ReadLine();
            char[] split = { ' ' };

            if (line == null)
            {
                return null;
            }

            pair = line.Trim().Split(split, 2);

            /* 커맨드의 길이는 4바이트로 고정 */
            if (pair[0].Length != 4)
            {
                return null;
            }

            request.Command = pair[0];

            if (pair.Length == 1)
            {
                logger.DebugFormat("수신: {0}", pair[0]);
            }
            else if (pair.Length == 2)
            {
                logger.DebugFormat("수신: {0} {1}", pair[0], pair[1]);

                request.Argument = pair[1];
            }

            return request;
        }
    }
}