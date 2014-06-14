using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Client.Utility;
using log4net;

namespace Client.Service.Network
{
    public class ProtocolInterpretor
    {
        private ILog logger = LogManager.GetLogger(typeof(ProtocolInterpretor));
        private Properties props = new Properties();
        private Socket socket;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;

        public void RunClient()
        {
            props.Load(AppDomain.CurrentDomain.BaseDirectory + "config.properties");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(props["server.ip"], int.Parse(props["server.port"]));

            logger.DebugFormat("Connect to server, IP: {0}", props["server.ip"]);
            logger.DebugFormat("Connect to server, Port: {0}", props["server.port"]);

            stream = new NetworkStream(socket);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            EstablishConnection();
        }

        private void EstablishConnection()
        {
            NetResponse response = ReceiveResponse();

            if (response.Code == NetResponse.ServiceNotAvailable)
            {
                throw new Exception("서버에 접속할 수 없습니다.");
            }
        }

        public void Logout()
        {
            SendRequest(NetRequest.Logout);
            ReceiveResponse();
        }

        public bool Login(String userId, String passwd)
        {
            NetResponse response;

            SendRequest(NetRequest.UserId, userId);
            response = ReceiveResponse();

            switch (response.Code)
            {
                case NetResponse.UserLoggedIn:
                    return true;
                case NetResponse.RequestPassword:
                    logger.Debug("Send password");
                    SendRequest(NetRequest.Password, passwd);
                    response = ReceiveResponse();
                    break;
                default:
                    logger.Error(response.ToString());
                    return false;
            }

            logger.Info(response.ToString());

            switch (response.Code)
            {
                case NetResponse.UserLoggedIn:
                    return true;
                default:
                    logger.Error(response.ToString());
                    return false;
            }
        }

        private void SendRequest(String command)
        {
            logger.DebugFormat("Send: {0}", command);

            writer.WriteLine(String.Format("{0}", command));
            writer.Flush();
        }

        private void SendRequest(String command, String argument)
        {
            logger.DebugFormat("Send: {0} {1}", command, argument);

            writer.WriteLine(String.Format("{0} {1}", command, argument));
            writer.Flush();
        }

        private NetResponse ReceiveResponse()
        {
            NetResponse response = new NetResponse();
            String[] pair;
            String line;
            char[] split = { ' ' };

            if (socket == null || !socket.Connected)
            {
                response.Code = NetResponse.ServiceNotAvailable;
                response.Message = "서버와 연결되지 않았습니다.";

                return response;
            }

            line = reader.ReadLine();

            if (line == null)
            {
                return null;
            }

            pair = line.Trim().Split(split, 2);

            if (pair[0].Length != 3) // Response Code Length: 3
            {
                return null;
            }

            response.Code = int.Parse(pair[0]);

            if (pair.Length == 1)
            {
                logger.DebugFormat("Receive: {0}", pair[0]);
            }
            else if (pair.Length == 2)
            {
                logger.DebugFormat("Receive: {0} {1}", pair[0], pair[1]);

                response.Message = pair[1];
            }

            return response;
        }
    }
}