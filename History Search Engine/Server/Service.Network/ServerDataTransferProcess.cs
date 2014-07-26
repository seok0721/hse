using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Reference.Model;
using Reference.Utility;
using Reference;
using Server.Dao;
using log4net;

namespace Server.Service.Network
{
    public class ServerDataTransferProcess
    {
        private ILog logger = LogManager.GetLogger(typeof(ServerDataTransferProcess));
        private Socket serverSocket;
        private FileDao fileDao = new FileDao();
        private HtmlDao htmlDao = new HtmlDao();

        public String BaseDirectory { get; set; }
        public String Host { get; set; }
        public int Port { get; set; }

        public void Init()
        {
            fileDao.Session = NHibernateLoader.Instance.Session;
            htmlDao.Session = NHibernateLoader.Instance.Session;
        }

        public bool Connected
        {
            get
            {
                return (serverSocket != null && serverSocket.Connected);
            }
        }

        public bool ConnectUserDTP()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            logger.InfoFormat("사용자 DTP에 접속 시도: {0}:{1}", Host, Port);

            try
            {
                serverSocket.Connect(Host, Port);
                logger.Info("사용자 DTP에 접속되었습니다.");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                serverSocket.Close();
                return false;
            }

            return true;
        }

        public void CloseServerDTP()
        {
            serverSocket.Close();
        }

        public void SendFileList(String userId, String keyword)
        {
            byte[] newline = new byte[1] { (byte)'\n' };
            byte[] buffer = new byte[Constants.BufferSize];
            int remainder = 0;

            String[] keywordArray = keyword.Split(' ');
            logger.Info("11111111111111111111111");
            ArrayList fileList = fileDao.ReadFileList(userId, keywordArray);
            logger.Info("22222222222222222");
            ArrayList htmlList = htmlDao.ReadHtmlList(userId, keywordArray);
            logger.Info("3333333333333");
            String header = String.Format("{0} {1}", fileList.Count, htmlList.Count);

            for (buffer = Encoding.UTF8.GetBytes(header), remainder = header.Length; remainder > 0; )
            {
                remainder -= serverSocket.Send(buffer, header.Length - remainder,
                    ((remainder < buffer.Length) ? remainder : buffer.Length), SocketFlags.None);
            }

            serverSocket.Send(newline, 0, newline.Length, SocketFlags.None);

            foreach (String file in fileList)
            {
                for (buffer = Encoding.UTF8.GetBytes(file), remainder = file.Length; remainder > 0; )
                {
                    remainder -= serverSocket.Send(buffer, file.Length - remainder,
                        ((remainder < buffer.Length) ? remainder : buffer.Length), SocketFlags.None);
                }

                serverSocket.Send(newline, 0, newline.Length, SocketFlags.None);
            }

            for (int i = 0; i < htmlList.Count; i++)
            {
                String html = (String)htmlList[i];

                for (buffer = Encoding.UTF8.GetBytes(html), remainder = html.Length; remainder > 0; )
                {
                    remainder -= serverSocket.Send(buffer, html.Length - remainder,
                        ((remainder < buffer.Length) ? remainder : buffer.Length), SocketFlags.None);
                }

                if (i != htmlList.Count - 1)
                {
                    serverSocket.Send(newline, 0, newline.Length, SocketFlags.None);
                }
            }
        }

        public void SendToRequestNewFile()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(new String('0', 32));

            serverSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void SendAlreadyTheNewestFile()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(new String('F', 32));

            serverSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void SendData(String data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            serverSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void SendChecksumStream(FileModel model)
        {
            StringBuilder builder = new StringBuilder();
            FileInfo fi = new FileInfo(String.Format("{0}\\{1}", BaseDirectory, model.FileId));
            int remainder = 0;
            int length = 0;
            byte[] buffer = new byte[Constants.BufferSize];
            byte[] endOfChecksum = Encoding.UTF8.GetBytes(new String('F', 32));
            byte[] checksum;

            if (fi.Length == 0)
            {
                checksum = Encoding.UTF8.GetBytes(HashUtils.HashMD5(String.Empty));
                serverSocket.Send(checksum, 0, checksum.Length, SocketFlags.None);
                serverSocket.Send(endOfChecksum, 0, endOfChecksum.Length, SocketFlags.None);
                return;
            }

            using (FileStream fs = File.OpenRead(fi.FullName))
            {
                while (true)
                {
                    builder.Clear();
                    remainder = Constants.BlockSize;

                    while (remainder > 0)
                    {
                        length = fs.Read(buffer, 0, (remainder < Constants.BlockSize) ? remainder : Constants.BlockSize);

                        if (length == 0)
                        {
                            break;
                        }

                        remainder -= length;
                        builder.Append(Encoding.UTF8.GetString(buffer, 0, length));
                    }

                    if (builder.Length == 0)
                    {
                        break;
                    }

                    logger.Info(builder.ToString());

                    checksum = Encoding.UTF8.GetBytes(HashUtils.HashMD5(builder.ToString()));
                    serverSocket.Send(checksum, 0, checksum.Length, SocketFlags.None);
                }

                serverSocket.Send(endOfChecksum, 0, endOfChecksum.Length, SocketFlags.None);
            }
        }

        /// <summary>
        /// 사용자에게 파일 스트림을 전송합니다.
        /// </summary>
        /// <param name="model">파일 기본 정보를 담고 있는 모델</param>
        public void SendFileStream(int fileId)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", BaseDirectory, fileId)))
            {
                while ((length = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    serverSocket.Send(buffer, 0, length, SocketFlags.None);
                }
            }
        }

        public void ReceiveFileStream(FileModel model)
        {
            String filePath = String.Format("{0}\\{1}", BaseDirectory, model.FileId);
            byte[] buffer = new byte[Constants.BufferSize];
            long remainder = model.Size;
            int length = 0;

            logger.Debug("새 파일 수신을 시작합니다.");

            using (FileStream fs = File.OpenWrite(filePath))
            {
                while (remainder > 0)
                {
                    length = serverSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    remainder -= length;

                    fs.Write(buffer, 0, length);
                }
            }

            logger.Debug("새 파일을 성공적으로 받았습니다.");
        }

        /// <summary>
        /// 
        /// 클라이언트에서 전송한 파일 변경 내역을 수신합니다.
        /// 
        /// <para>추가항목 : +&lt;Offset,Long,64&gt;&lt;Length,Long,64&gt;&lt;Stream&gt;</para>
        /// <para>삭제항목 : -&lt;Offset,Long,64&gt;&lt;Length,Long,64&gt;</para>
        /// <para>전송완료 : 0</para>
        /// 
        /// </summary>
        /// <param name="model">파일 모델</param>
        public void ReceiveDifferenceStream(FileModel model)
        {
            String filePath = String.Format("{0}\\{1}", BaseDirectory, model.FileId);
            FileStream fsNew = null;
            FileStream fsOld = null;
            long diffOffset = 0;
            long diffLength = 0;
            long remainder = 0;
            long difference = 0;
            int readLength;
            int flag;
            byte[] buffer = new byte[Constants.BufferSize];

            if (File.Exists(filePath + ".tmp"))
            {
                File.Delete(filePath + ".tmp");
            }

            using (fsNew = File.OpenWrite(filePath + ".tmp"))
            using (fsOld = File.OpenRead(filePath))
            {
                while (true)
                {
                    flag = char.Parse(ReadFromUserSocket(1));
                    diffOffset = int.Parse(ReadFromUserSocket(64));
                    diffLength = int.Parse(ReadFromUserSocket(64));

                    logger.InfoFormat("{0},{1},{2}", flag, diffOffset, diffLength);

                    if (flag == '0')
                    {
                        if (fsNew.Position < diffLength)
                        {
                            remainder = diffLength - fsNew.Position;

                            while (remainder > 0)
                            {
                                readLength = fsOld.Read(buffer, 0, remainder < buffer.Length ? (int)remainder : buffer.Length);
                                remainder -= readLength;

                                fsNew.Write(buffer, 0, readLength);
                                fsNew.Flush();
                            }
                        }

                        break;
                    }

                    if (flag == '+')
                    {
                        difference = diffOffset - fsOld.Position;

                        while (difference > 0)
                        {
                            readLength = fsOld.Read(buffer, 0, (difference < buffer.Length) ? (int)difference : buffer.Length);
                            difference -= readLength;

                            fsNew.Write(buffer, 0, readLength);
                            fsNew.Flush();
                        }

                        remainder = diffLength;

                        while (remainder > 0)
                        {
                            readLength = serverSocket.Receive(buffer, 0, ((remainder < buffer.Length) ? (int)remainder : buffer.Length), SocketFlags.None);
                            remainder -= readLength;

                            fsNew.Write(buffer, 0, readLength);
                            fsNew.Flush();
                        }

                        continue;
                    }

                    if (flag == '-')
                    {
                        difference = diffOffset - fsOld.Position;

                        while (difference > 0)
                        {
                            readLength = fsOld.Read(buffer, 0, (difference < buffer.Length) ? (int)difference : buffer.Length);
                            difference -= readLength;

                            fsNew.Write(buffer, 0, readLength);
                            fsNew.Flush();
                        }

                        fsOld.Seek(diffLength, SeekOrigin.Current);
                        continue;
                    }
                }
            }

            File.Move(filePath, filePath + ".bak");
            File.Move(filePath + ".tmp", filePath);
            File.Delete(filePath + ".bak");

            logger.Info("최신화 완료.");
        }

        private String ReadFromUserSocket(int criterion)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int cursor = 0;

            logger.Info("소켓에서 데이터 읽기");

            while (cursor < criterion)
            {
                cursor += serverSocket.Receive(buffer, cursor, criterion - cursor, SocketFlags.None);
            }

            logger.Info(Encoding.UTF8.GetString(buffer, 0, criterion));

            return Encoding.UTF8.GetString(buffer, 0, criterion);
        }
    }
}
