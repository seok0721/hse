using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Reference;
using Reference.Model;
using Reference.Utility;
using Reference.Protocol;
using log4net;

namespace Client.Service.Network
{
    public class UserDataTransferProcess
    {
        private ILog logger = LogManager.GetLogger(typeof(UserDataTransferProcess));
        private Socket userSocket;
        private Socket serverSocket;
        private List<char> diffFlagList = new List<char>();
        private List<long> diffOffsetList = new List<long>();
        private List<long> diffLengthList = new List<long>();

        public bool Opened
        {
            get
            {
                return (userSocket != null) && (userSocket.IsBound);
            }
        }

        public bool Connected
        {
            get
            {
                return (serverSocket != null) && (serverSocket.Connected);
            }
        }

        public int Port
        {
            get
            {
                return ((IPEndPoint)userSocket.LocalEndPoint).Port;
            }
        }

        public void Init()
        {
            userSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            userSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public bool OpenUserDTP(int backlog)
        {
            try
            {
                userSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                userSocket.Listen(backlog);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool WaitServerDTP()
        {
            try
            {
                serverSocket = userSocket.Accept();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public void CloseServerDTP()
        {
            if (serverSocket != null && serverSocket.Connected)
            {
                serverSocket.Close();
                serverSocket = null;
            }
        }

        public int ReceiveChecksumStream(FileModel model)
        {
            FileInfo fi = new FileInfo(String.Format("{0}\\{1}", model.Path, model.Name));
            String checksum = null;

            diffFlagList.Clear();
            diffOffsetList.Clear();
            diffLengthList.Clear();

            if ((checksum = ReceiveFromServerSocket(Constants.MD5Length)) == null)
            {
                throw new Exception("네트워크 스트림을 수신 받는 도중 오류가 발생하였습니다.");
            }

            logger.Info(checksum);

            if (checksum == new String('0', 32))
            {   
                return StoreCommandResponseCode.RequestNewFile;
            }

            if (checksum == new String('F', 32))
            {
                return StoreCommandResponseCode.AlreadyTheNewestFile;
            }

            using (FileStream fs = File.OpenRead(fi.FullName))
            {
                do
                {
                    logger.Info(checksum);

                    if (checksum == new String('F', 32))
                    {
                        break;
                    }

                    AnalyzeDifferenceChecksum(fs, checksum, fi.Length);
                } while ((checksum = ReceiveFromServerSocket(Constants.MD5Length)) != null);

                if (fs.Position < fs.Length)
                {
                    diffFlagList.Add('+');
                    diffOffsetList.Add(fs.Position);
                    diffLengthList.Add(fs.Length - fs.Position);
                }
            }

            return StoreCommandResponseCode.RequestDifferenceStream;
        }

        public void SendFileStream(FileModel model)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", model.Path, model.Name)))
            {
                while ((length = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    serverSocket.Send(buffer, 0, length, SocketFlags.None);
                }
            }

            logger.Info("새 파일 모두 전송 완료");
        }

        /// <summary>
        /// 파일의 스트림을 수신 합니다.
        /// </summary>
        /// <param name="model">파일의 기본 정보를 담고 있는 모델</param>
        public void ReceiveFileStream(String filePath)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;
            logger.Info("11111111111111111111");
            logger.Debug("새 파일 수신을 시작합니다.");

            using (FileStream fs = File.OpenWrite(filePath))
            {
                while ((length = serverSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                {
                    fs.Write(buffer, 0, length);
                }
            }
            logger.Info("222222222222222222");
        }

        /// <summary>
        /// 스트림이 종료될 때 까지 데이터를 받아옵니다.
        /// </summary>
        /// <returns></returns>
        public String ReceiveData()
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;

            while ((length = serverSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, length));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 서버로 변경된 파일 스트림을 전송합니다.
        /// <para>Header: &lt;flag:char:1&gt;&lt;offset:long:64&gt;&lt;length:long:64&gt;[payload:byte[]]</para>
        /// <para>Trailer: 0:129</para>
        /// </summary>
        /// <param name="model">파일 기본 정보를 담고 있는 모델</param>
        public void SendDifferenceStream(FileModel model)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            byte[] header = null;
            long remainder = 0;
            int length = 0;

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", model.Path, model.Name)))
            {
                logger.Info("총 변경 블록의 수 : " + diffFlagList.Count);
                logger.Info(String.Join(",", diffFlagList.ToArray()));
                logger.Info(String.Join(",", diffOffsetList.ToArray()));
                logger.Info(String.Join(",", diffLengthList.ToArray()));

                for (int i = 0; i < diffFlagList.Count; i++)
                {
                    header = Encoding.UTF8.GetBytes(String.Format("{0}{1,64:D64}{2,64:D64}",
                        diffFlagList[i], diffOffsetList[i], diffLengthList[i]));

                    serverSocket.Send(header, 0, header.Length, SocketFlags.None);

                    if (diffFlagList[i] == '+')
                    {
                        fs.Seek(diffOffsetList[i], SeekOrigin.Begin);

                        remainder = diffLengthList[i];

                        while (remainder > 0)
                        {
                            length = fs.Read(buffer, 0, (diffLengthList[i] < buffer.Length) ? (int)diffLengthList[i] : buffer.Length);
                            remainder -= length;

                            serverSocket.Send(buffer, 0, length, SocketFlags.None);
                        }
                    }
                }

                logger.Info("변경 내역 전송 완료");
            }

            /* 스트림의 끝을 알리기 위해 트레일러를 전송 */
            logger.Info("111");
            logger.Info(String.Format("{0}{1,64:D64}{2,64:D64}", '0', 0, model.Size));
            logger.Info("222");
            serverSocket.Send(Encoding.UTF8.GetBytes(String.Format("{0}{1,64:D64}{2,64:D64}",
                '0', 0, model.Size)), 0, 129, SocketFlags.None);
        }

        private void AnalyzeDifferenceChecksum(FileStream fs, String checksum, long fileLength)
        {
            long criterion = fs.Position;
            long offset = criterion;
            long remainder = fileLength - fs.Position;

            while (true)
            {
                String data = ReadFromFile(fs, Constants.BlockSize);

                if (data.Length == 0)
                {
                    diffFlagList.Add('-');
                    diffOffsetList.Add(criterion);
                    diffLengthList.Add((remainder < Constants.BlockSize) ? remainder : Constants.BlockSize);

                    fs.Seek(criterion, SeekOrigin.Begin);

                    return;
                }

                if (HashUtils.HashMD5(data) == checksum)
                {
                    if (offset - criterion > 0)
                    {
                        diffFlagList.Add('+');
                        diffOffsetList.Add(criterion);
                        diffLengthList.Add(offset - criterion);
                    }

                    return;
                }

                offset++;
                fs.Seek(offset, SeekOrigin.Begin);
            }
        }

        private String ReceiveFromServerSocket(int criterion)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[Constants.BufferSize];
            int remainder = criterion;
            int length = 0;

            while (remainder > 0)
            {
                length = serverSocket.Receive(buffer, 0, (remainder < buffer.Length) ? remainder : buffer.Length, SocketFlags.None);
                remainder -= length;
                builder.Append(Encoding.UTF8.GetString(buffer, 0, length));
            }

            return builder.ToString();
        }

        private String ReadFromFile(FileStream fs, int criterion)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[Constants.BufferSize];
            int remainder = criterion;
            int length = 0;

            while (remainder > 0)
            {
                if ((length = fs.Read(buffer, 0, (remainder < buffer.Length) ? remainder : buffer.Length)) == 0)
                {
                    break;
                }

                remainder -= length;
                builder.Append(Encoding.UTF8.GetString(buffer, 0, length));
            }

            return builder.ToString();
        }
    }
}
