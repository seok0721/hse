using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Server.Utility;
using NHibernate;
using Server.Domain;
using log4net;

namespace Server.Service.Network
{
    public class ProtocolInterpretor
    {
        private Properties props = new Properties();
        private Socket socket;
        private ILog logger = LogManager.GetLogger(typeof(ProtocolInterpretor));

        public ISession Session { get; set; }

        public void RunServer()
        {
            props.Load(AppDomain.CurrentDomain.BaseDirectory + "config.properties");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(props["server.port"])));
            socket.Listen(int.Parse(props["server.backlog"]));

            logger.Info("Server Port: " + props["server.port"]);

            while (true)
            {
                ClientSocket client = new ClientSocket();
                client.Socket = socket.Accept();
                client.Session = Session;

                logger.Info("New Client: " + client.Socket.RemoteEndPoint.ToString());

                Task task = new Task(client.HandleSocket);
                task.Start();
            }
        }

        class ClientSocket
        {
            private ILog logger = LogManager.GetLogger(typeof(ClientSocket));
            private NetworkStream stream;
            private StreamReader reader;
            private StreamWriter writer;
            private Boolean isLogin = false;
            private String userId;
            private String passwd;
            private Stack<String> workingDirectory = new Stack<String>();

            public Socket Socket { get; set; }
            public ISession Session { get; set; }

            public void HandleSocket()
            {
                stream = new NetworkStream(Socket);
                writer = new StreamWriter(stream);
                reader = new StreamReader(stream);

                EstablishConnection();
                FetchRequest();

                Socket.Close();
            }

            private void EstablishConnection()
            {
                SendResponse(NetResponse.ServiceReadyForNewUser, "서버에 접속하신걸 환영합니다.");
            }

            private void FetchRequest()
            {
                while (true)
                {
                    NetRequest request = null;

                    try
                    {
                        request = ReceiveRequest();
                    }
                    catch (Exception)
                    {
                        logger.InfoFormat("클라이언트의 접속이 끊겼습니다, {0}", Socket.RemoteEndPoint.ToString());
                        return;
                    }

                    switch (request.Command)
                    {
                        case NetRequest.UserId:
                            #region Command: USER, Argument: "User Id"
                            if (isLogin)
                            {
                                SendResponse(NetResponse.UserLoggedIn, "이미 접속되어있습니다.");
                                break;
                            }

                            userId = request.Argument;

                            if (passwd == null)
                            {
                                SendResponse(NetResponse.RequestPassword, "비밀번호를 입력하세요.");
                                break;
                            }

                            if (Login())
                            {
                                isLogin = true;
                                SendResponse(NetResponse.UserLoggedIn, "서버에 접속되었습니다.");
                            }
                            else
                            {
                                SendResponse(NetResponse.NotLoggedIn, "로그인에 실패하였습니다.");
                            }
                            break;
                            #endregion
                        case NetRequest.Password:
                            #region Command: PASS, Argument: "Password"
                            if (isLogin)
                            {
                                SendResponse(NetResponse.UserLoggedIn, "이미 접속되어있습니다.");
                                break;
                            }

                            passwd = request.Argument;

                            if (userId == null)
                            {
                                SendResponse(NetResponse.RequestUserId, "사용자 아이디를 입력하세요.");
                                break;
                            }

                            if (Login())
                            {
                                isLogin = true;
                                SendResponse(NetResponse.UserLoggedIn, "서버에 접속되었습니다.");
                            }
                            else
                            {
                                SendResponse(NetResponse.NotLoggedIn, "로그인에 실패하였습니다.");
                            }
                            break;
                            #endregion
                        case NetRequest.Logout:
                            #region Command: QUIT
                            isLogin = false;
                            userId = null;
                            passwd = null;

                            SendResponse(NetResponse.ServiceClosingControlConnection, "서버와의 연결이 종료되었습니다.");
                            return;
                            #endregion
                        default:
                            SendResponse(NetResponse.UnknownCommandError, "알 수 없는 명령어입니다.");
                            break;
                    }
                }
            }

            private bool Login()
            {
                User model = new User();
                model.UserId = userId;

                model = Session.Get<User>(userId);

                if (model == null)
                {
                    return false;
                }

                logger.Debug("Login User ID: " + passwd);
                logger.Debug("Login User Password: " + passwd);
                logger.Debug("Login Real Password: " + model.Password);

                if (!model.Password.Equals(passwd))
                {
                    return false;
                }

                return true;
            }

            private void SendResponse(int code)
            {
                logger.DebugFormat("Send: {0}", code);

                writer.WriteLine(String.Format("{0}", code));
                writer.Flush();
            }

            private void SendResponse(int code, String message)
            {
                logger.DebugFormat("Send: {0} {1}", code, message);

                writer.WriteLine(String.Format("{0} {1}", code, message));
                writer.Flush();
            }

            private NetRequest ReceiveRequest()
            {
                NetRequest request = new NetRequest();
                String[] pair;
                String line = reader.ReadLine();
                char[] split = { ' ' };
                
                if (line == null)
                {
                    return null;
                }

                pair = line.Trim().Split(split, 2);

                if (pair[0].Length != 4) // Request Command Length: 4
                {
                    return null;
                }

                request.Command = pair[0];

                if (pair.Length == 1)
                {
                    logger.DebugFormat("Receive: {0}", pair[0]);
                }
                else if (pair.Length == 2)
                {
                    logger.DebugFormat("Receive: {0} {1}", pair[0], pair[1]);

                    request.Argument = pair[1];
                }

                return request;
            }
        }
    }
}