using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Server.Dao;
using Reference.Model;
using Reference.Protocol;
using Reference.Utility;
using log4net;

namespace Server.Service.Network
{
    public class ServerProtocolInterpretor
    {
        private ILog logger = LogManager.GetLogger(typeof(ServerProtocolInterpretor));
        private ServerDataTransferProcess serverDTP = new ServerDataTransferProcess();
        private StreamReader reader;
        private StreamWriter writer;
        private Boolean isLogin = false;
        private String userId;
        private String password;
        private String baseDirectory;
        private FileDao fileDao = new FileDao();
        private FileWordDao fileWordDao = new FileWordDao();
        private FileIOLogDao fileIOLogDao = new FileIOLogDao();
        private HtmlDao htmlDao = new HtmlDao();
        private HtmlWordDao htmlWordDao = new HtmlWordDao();
        private UserDao userDao = new UserDao();

        public Socket Socket { get; set; }
        public Properties Properties { get; set; }

        public void Init()
        {
            if (Socket == null && !Socket.Connected)
            {
                throw new Exception("서버 명령 해석기를 초기화 하는 도중 오류가 발생하였습니다.");
            }

            logger.Info("서버 명령 해석기 초기화");

            writer = new StreamWriter(new NetworkStream(Socket));
            reader = new StreamReader(new NetworkStream(Socket));

            fileDao.Session = NHibernateLoader.Instance.Session;
            fileWordDao.Session = NHibernateLoader.Instance.Session;
            fileIOLogDao.Session = NHibernateLoader.Instance.Session;
            htmlDao.Session = NHibernateLoader.Instance.Session;
            htmlWordDao.Session = NHibernateLoader.Instance.Session;
            userDao.Session = NHibernateLoader.Instance.Session;
        }

        public void Start()
        {
            logger.Info("서버 명령 해석기 시작");

            EstablishConnection();
            FetchRequest();
            ReleaseConnection();
        }

        private void EstablishConnection()
        {
            logger.InfoFormat("사용자 접속 성립, {0}", Socket.RemoteEndPoint.ToString());

            SendResponse(ProtocolResponse.ServiceReadyForNewUser, "검색 엔진 서버에 접속 하신걸 환영합니다.");
        }

        private void FetchRequest()
        {
            while (true)
            {
                ProtocolRequest request;

                try
                {
                    request = ReceiveRequest();
                }
                catch (Exception)
                {
                    logger.ErrorFormat("사용자와 접속이 끊겼습니다, {0}", Socket.RemoteEndPoint.ToString());
                    return;
                }

                switch (request.Command)
                {
                    case ProtocolRequest.UserId:
                        HandleUserCommand(request.Argument);
                        break;
                    case ProtocolRequest.Password:
                        HandlePassCommand(request.Argument);
                        break;
                    case ProtocolRequest.Logout:
                        HandleQuitCommand();
                        return;
                    case ProtocolRequest.DataPort:
                        HandlePortCommand(request.Argument);
                        break;
                    case ProtocolRequest.Store:
                        HandleStorCommand(request.Argument);
                        break;
                    case ProtocolRequest.Retrieve:
                        HandleRetrCommand(request.Argument);
                        break;
                    case ProtocolRequest.FileWord:
                        HandleFlwdCommand(request.Argument);
                        break;
                    case ProtocolRequest.HtmlWord:
                        HandleHtwdCommand(request.Argument);
                        break;
                    default:
                        SendResponse(ProtocolResponse.UnknownCommandError, "알 수 없는 명령어입니다.");
                        break;
                }
            }
        }

        private void HandleHtwdCommand(String argument)
        {
            if (!isLogin)
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인 후 사용하세요.");
                return;
            }

            String[] split = argument.Split(',');
            HtmlModel mHtml = new HtmlModel();
            mHtml.UserId = userId;
            mHtml.HtmlId = htmlDao.ReadMaxHtmlId(userId) + 1;
            mHtml.URL = split[0].Split('|')[2];
            mHtml.CreateTime = DateTime.Now;
            htmlDao.CreateHtml(mHtml);

            HtmlWord mHtmlWord = new HtmlWord();
            mHtmlWord.UserId = mHtml.UserId;
            mHtmlWord.HtmlId = mHtml.HtmlId;
            mHtmlWord.HtmlWordId = htmlWordDao.ReadMaxHtmlWordId(mHtml);
            mHtmlWord.WordCount = 1;

            foreach (String htmlWord in split[1].Trim().Split(' '))
            {
                mHtmlWord.Word = htmlWord;
                htmlWordDao.CreateHtmlWord(mHtmlWord);
            }

            SendResponse(ProtocolResponse.CommandOkay, "HTML 정보가 추가되었습니다.");
        }

        private void HandleFlwdCommand(String argument)
        {
            if (!isLogin)
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인 후 사용하세요.");
                return;
            }

            logger.Info(argument);
            logger.Info("단어 전송을 위해 파일 정보 읽기를 시작합니다.");

            String[] split = argument.Split(',');
            FileModel mFile = new FileModel();
            mFile.UserId = userId;
            mFile.UniqueId = split[0];

            mFile = fileDao.ReadFileUsingUniqueId(mFile);
            logger.Info(mFile.ToString());

            if (mFile == null)
            {
                logger.Info("연관된 파일이 없습니다.");

                SendResponse(ProtocolResponse.FileUnavailable, "연관된 파일이 없습니다.");
                return;
            }

            logger.Info("단어를 파싱합니다.");

            FileWord mFileWord = new FileWord();
            mFileWord.FileId = mFile.FileId;
            mFileWord.UserId = mFile.UserId;

            logger.Info(mFile.ToString());

            // FIXME 기존 파일의 카운트를 지우고 다시 생성할 것.
            foreach (String word in split[1].Trim().Split(' '))
            {
                mFileWord.Word = word;

                logger.Info(word);
                logger.Info("1111");
                mFileWord = fileWordDao.ReadFileWordUsingWord(mFileWord);
                logger.Info("2222");

                if (mFileWord == null)
                {
                    mFileWord = new FileWord();
                    logger.Info("3333");
                    mFileWord.FileId = mFile.FileId;
                    logger.Info("333311111111111");
                    mFileWord.UserId = mFile.UserId;
                    logger.Info("33332222222222222");
                    mFileWord.FileWordId = fileWordDao.ReadMaxFileWordId(mFile) + 1;
                    logger.Info("33333333333333333");
                    mFileWord.Word = word;
                    logger.Info("3333444444444444");
                    mFileWord.WordCount = 1;
                    logger.Info("33335555555555555");
                    if (mFileWord.FileWordId == -1)
                    {
                        break;
                    }
                    logger.Info("4444");
                    fileWordDao.CreateFileWord(mFileWord);
                    logger.Info("5555");
                }
                else
                {
                    mFileWord.WordCount += 1;

                    logger.Info("6666");
                    fileWordDao.UpdateFileWord(mFileWord);
                    logger.Info("7777");
                }

                logger.Info(word);
            }

            logger.Info("파일 내용 저장이 종료되었습니다.");

            SendResponse(ProtocolResponse.CommandOkay, "파일 내용 저장이 종료되었습니다.");
        }

        private bool ConnectUserDTP()
        {
            if (!serverDTP.Connected)
            {
                SendResponse(ProtocolResponse.OpenDataConnection, "접속을 시도합니다.");

                if (!serverDTP.ConnectUserDTP())
                {
                    SendResponse(ProtocolResponse.CannotOpenDataConnection, "데이터 포트에 접속할 수 없습니다.");
                    return false;
                }
            }
            else
            {
                SendResponse(ProtocolResponse.DataConnectionAlreadyOpen, "이미 접속 되어 있습니다. 파일 작업을 시작합니다.");
            }

            return true;
        }

        private void HandleRetrCommand(String argument)
        {
            if (!isLogin)
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인 후 사용하세요.");
                return;
            }

            if (!ConnectUserDTP())
            {
                return;
            }

            serverDTP.SendFileStream(int.Parse(argument));

            SendResponse(ProtocolResponse.CloseDataConnection, "파일이 정상적으로 수신되었습니다.");

            serverDTP.CloseServerDTP();
        }

        private void HandleStorCommand(String argument)
        {
            if (!isLogin)
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인 후 사용하세요.");
                return;
            }

            if (!ConnectUserDTP())
            {
                return;
            }

            FileModel userFile = FileModel.FromString(argument);
            userFile.UserId = userId;
            
            FileModel serverFile = fileDao.ReadFileUsingUniqueId(userFile);

            if (serverFile != null)
            {
                logger.Info("파일 최신화를 시작합니다.");
                logger.Info(serverFile.ToString());

                if (serverFile.LastUpdateTime == userFile.LastUpdateTime)
                {
                    logger.Info("이미 최신화 되어 있습니다.");

                    serverDTP.SendAlreadyTheNewestFile();
                    serverDTP.CloseServerDTP();

                    SendResponse(ProtocolResponse.CloseDataConnection, "이미 최신화 되어 있습니다.");
                }
                else
                {
                    logger.Info("최신화를 위해 체크섬을 전송합니다.");
                    serverDTP.SendChecksumStream(serverFile);

                    logger.Info("변경 스트림을 수신합니다.");
                    serverDTP.ReceiveDifferenceStream(serverFile);
                    serverDTP.CloseServerDTP();

                    SendResponse(ProtocolResponse.CloseDataConnection, "파일의 최신화가 완료되었습니다.");
                }

                serverFile.Name = userFile.Name; // 2014-07-24 ADD by KS
                serverFile.Size = userFile.Size; // 2014-07-24 ADD by KS
                serverFile.LastUpdateTime = userFile.LastUpdateTime;
                fileDao.UpdateFile(serverFile);
            }
            else
            {
                logger.Info("새 파일 전송을 시작합니다.");

                userFile.UserId = userId;
                userFile.FileId = fileDao.ReadMaxFileId(userId) + 1;
                fileDao.CreateFile(userFile);

                serverDTP.SendToRequestNewFile();
                serverDTP.ReceiveFileStream(userFile);
                serverDTP.CloseServerDTP();

                SendResponse(ProtocolResponse.CloseDataConnection, "파일이 정상적으로 수신되었습니다.");
            }
        }

        private void HandlePortCommand(String argument)
        {
            if (!isLogin)
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인 후 사용하세요.");
                return;
            }

            if (serverDTP.Connected)
            {
                SendResponse(ProtocolResponse.CommandOkay, "이미 연결되어 있습니다.");
                return;
            }

            String[] args = argument.Split(',');

            if (args.Length != 6)
            {
                SendResponse(ProtocolResponse.InvalidArgumentError, "인자 형식이 잘못되었습니다.");
                return;
            }

            serverDTP.Host = String.Format("{0}.{1}.{2}.{3}", args[0], args[1], args[2], args[3]);
            serverDTP.Port = (int.Parse(args[4]) << 8) + int.Parse(args[5]);
            serverDTP.BaseDirectory = baseDirectory;

            SendResponse(ProtocolResponse.CommandOkay, "데이터 접속이 성립되었습니다.");
        }

        private void HandleQuitCommand()
        {
            logger.InfoFormat("사용자 PI와 접속을 종료합니다, {0}", Socket.RemoteEndPoint.ToString());
            SendResponse(ProtocolResponse.ServiceClosingControlConnection, "서버와의 연결이 종료되었습니다.");
        }

        private void HandlePassCommand(String password)
        {
            logger.InfoFormat("로그인 시도, {0}", password);

            if (isLogin)
            {
                SendResponse(ProtocolResponse.UserLoggedIn, "이미 접속 중 입니다.");
                return;
            }

            this.password = password;

            if (Login(this.userId, this.password))
            {
                SendResponse(ProtocolResponse.UserLoggedIn, "검색 엔진 서버에 로그인 되었습니다.");
            }
            else
            {
                SendResponse(ProtocolResponse.NotLoggedIn, "로그인에 실패하였습니다.");
            }
        }

        private void HandleUserCommand(String userId)
        {
            if (isLogin)
            {
                SendResponse(ProtocolResponse.UserLoggedIn, "이미 접속 중 입니다.");
                return;
            }

            SendResponse(ProtocolResponse.RequestPassword, "비밀번호를 입력하세요.");
            this.userId = userId;
        }

        private bool Login(String userId, String password)
        {
            logger.InfoFormat("로그인 시도, {0}", userId);

            UserModel model = new UserModel();
            model.UserId = userId;
            model = this.userDao.ReadUser(userId);

            if (model == null)
            {
                logger.ErrorFormat("일치하는 사용자 아이디가 없습니다, {0}", userId);

                this.isLogin = false;
                this.userId = null;
                this.password = null;

                return false;
            }

            if (!model.Password.Equals(password))
            {
                logger.ErrorFormat("비밀번호가 일치하지 않습니다, {0}", userId);

                this.isLogin = false;
                this.userId = null;
                this.password = null;

                return false;
            }

            isLogin = true;

            CreateUserDirectory(userId);

            logger.InfoFormat("로그인 되었습니다, {0}", userId);

            return true;
        }

        private void CreateUserDirectory(String userId)
        {
            DirectoryInfo di = new DirectoryInfo(String.Format("{0}\\{1}\\{2}",
                AppDomain.CurrentDomain.BaseDirectory, Properties["BASEDIR"], userId));

            baseDirectory = di.FullName;

            if (!di.Exists)
            {
                logger.InfoFormat("사용자 파일이 저장되는 디렉토리를 생성합니다, {0}", userId);

                di.Create();
            }
        }

        private void ReleaseConnection()
        {
            isLogin = false;
            userId = null;
            password = null;

            reader.Close();
            writer.Close();
            Socket.Close();
        }

        private void SendResponse(int code)
        {
            logger.DebugFormat("송신: {0}", code);

            writer.WriteLine(code);
            writer.Flush();
        }

        private void SendResponse(int code, String message)
        {
            logger.DebugFormat("송신: {0} {1}", code, message);

            writer.WriteLine(String.Format("{0} {1}", code, message));
            writer.Flush();
        }

        private ProtocolRequest ReceiveRequest()
        {
            ProtocolRequest request = new ProtocolRequest();
            String[] pair;
            String line = reader.ReadLine();
            char[] split = { ' ' };

            if (line == null)
            {
                return null;
            }

            pair = line.Trim().Split(split, 2);

            // 커맨드의 길이는 4바이트로 고정
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

            logger.Info(request.Command + " " + request.Argument);

            return request;
        }
    }
}
