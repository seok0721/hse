using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

using Server.Domain;
using Server.Utility;
using log4net;

namespace Server.Service.Network
{
    public class DataTransferProcess
    {
        private ILog logger = LogManager.GetLogger(typeof(DataTransferProcess));
        private Socket socket;
        private NetworkStream ns;
        private StreamReader reader;
        private StreamWriter writer;

        public String IPAddress { get; set; }
        public int Port { get; set; }
        public String Basedir { get; set; }

        /// <summary>
        /// 서버 데이터 전송 프로세스를 초기화 합니다.
        /// </summary>
        public void Init()
        {

        }

        /// <summary>
        /// User-DTP로 접속합니다.
        /// </summary>
        /// <param name="address">IP 주소</param>
        /// <param name="port">포트</param>
        public bool Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            logger.DebugFormat("사용자 DTP에 접속 시도: {0}:{1}", IPAddress, Port);

            try
            {
                socket.Connect(IPAddress, Port);
                logger.Info("접속 성공");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                socket.Close();
                return false;
            }

            ns = new NetworkStream(socket);
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns);

            return true;
        }

        /// <summary>
        /// User-DTP와 접속 되어 있는지 확인합니다.
        /// </summary>
        public bool Connected
        {
            get
            {
                return (socket != null && socket.Connected);
            }
        }

        /// <summary>
        /// 접속에 사용된 리소스를 모두 해제합니다.
        /// </summary>
        public void ReleaseConnection()
        {
            reader.Close();
            writer.Close();
            socket.Close();
        }

        /// <summary>
        /// 사용자에게 새 파일 전송을 요청하는 헤더를 보냅니다.
        /// </summary>
        /// <param name="model"></param>
        public void SendToRequestNewFile()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(new String('0', 32));

            ns.Write(buffer, 0, buffer.Length);
            ns.Flush();
        }

        /// <summary>
        /// 사용자에게 이미 최신 파일을 가지고 있다는 헤더를 보냅니다.
        /// </summary>
        /// <param name="model"></param>
        public void SendAlreadyTheNewestFile()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(new String('F', 32));

            ns.Write(buffer, 0, buffer.Length);
            ns.Flush();
        }

        /// <summary>
        /// 사용자에게 데이터를 바로 전송합니다.
        /// </summary>
        /// <param name="data"></param>
        public void SendData(String data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            ns.Write(buffer, 0, buffer.Length);
            ns.Flush();
        }

        /// <summary>
        /// 클라이언트에서 요청한 파일의 체크섬 스트림을 전송합니다.
        /// <para>Usage: &lt;Header&gt;,&lt;Payload&gt;</para>
        /// <para>Header: &lt;File ID,Int,32&gt;&lt;Block Size,Int,32&gt;&lt;Block Count,Int,32&gt;</para>
        /// </summary>
        /// <param name="model">파일 모델</param>
        public void SendChecksumStream(FileModel model)
        {
            StringBuilder builder = new StringBuilder();
            FileInfo fi = new FileInfo(String.Format("{0}\\{1}", Basedir, model.ID));
            int remainder = 0;
            int length = 0;
            byte[] buffer = new byte[Constants.BufferSize];

            using (FileStream fs = File.OpenRead(fi.FullName))
            {
                while (true)
                {
                    builder.Clear();
                    remainder = Constants.HashInput;

                    while (remainder > 0)
                    {
                        length = fs.Read(buffer, 0, (buffer.Length < remainder) ? buffer.Length : remainder);

                        if (length == 0)
                        {
                            break;
                        }

                        remainder -= length;

                        builder.Append(Encoding.UTF8.GetString(buffer, 0, length));
                    }

                    logger.Debug("checksum: " + HashUtils.HashMD5(builder.ToString()));

                    byte[] checksum = Encoding.UTF8.GetBytes(HashUtils.HashMD5(builder.ToString()));

                    ns.Write(checksum, 0, Constants.MD5Length);
                    ns.Flush();

                    if (length == 0)
                    {
                        break;
                    }
                }

                byte[] endOfChecksum = Encoding.UTF8.GetBytes(new String('F', 32));

                ns.Write(endOfChecksum, 0, endOfChecksum.Length);
                ns.Flush();
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

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", Basedir, fileId)))
            {
                while ((length = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ns.Write(buffer, 0, length);
                    ns.Flush();
                }
            }
        }

        /// <summary>
        /// 파일의 스트림을 수신 합니다.
        /// </summary>
        /// <paramFile name="model">파일의 기본 정보를 담고 있는 모델</param>
        public void ReceiveFileStream(FileModel model)
        {
            String filePath = String.Format("{0}\\{1}", Basedir, model.ID);
            byte[] buffer = new byte[Constants.BufferSize];
            long remainder = model.Size;
            int length = 0;

            logger.Debug("새 파일 수신을 시작합니다.");

            using (FileStream fs = File.OpenWrite(filePath))
            {
                while (remainder > 0)
                {
                    length = ns.Read(buffer, 0, buffer.Length);
                    remainder -= length;

                    fs.Write(buffer, 0, length);
                }
            }
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
            String filePath = String.Format("{0}\\{1}", Basedir, model.ID);
            FileStream fsNew = null;
            FileStream fsOld = null;
            long diffOffset = 0;
            long diffLength = 0;
            long fileOffset = 0;
            long cursor = 0;
            long remainder = 0;
            int readLength;
            int flag;
            byte[] buf = new byte[Constants.BufferSize];

            if (File.Exists(filePath + ".tmp"))
            {
                File.Delete(filePath + ".tmp");
            }

            fsNew = File.OpenWrite(filePath + ".tmp");
            fsOld = File.OpenRead(filePath);

            logger.Debug("최신화를 위한 헤더 수신 시작");

            while (true)
            {
                /* 스트림 헤더 파싱 */
                flag = ns.ReadByte();

                logger.Debug("플래그 : " + flag);

                if (flag == 0)
                {
                    logger.Debug("메시지 전송이 끝났습니다.");
                    break;
                }

                if (flag == -1)
                {
                    throw new Exception("스트림 수신 도중 오류가 발생하였습니다.");
                }

                cursor = 0;

                while (cursor < 64)
                {
                    cursor += ns.Read(buf, (int)cursor, 64 - (int)cursor);
                }

                diffOffset = int.Parse(Encoding.UTF8.GetString(buf, 0, 64));
                cursor = 0;

                while (cursor < 64)
                {
                    cursor += ns.Read(buf, (int)cursor, 64 - (int)cursor);
                }

                diffLength = int.Parse(Encoding.UTF8.GetString(buf, 0, 64));

                logger.DebugFormat("헤더 : {0}, {1}, {2}", (char)flag, diffOffset, diffLength);

                /* 스트림 헤더가 파일의 추가된 부분을 가리킬 때 */
                if (flag == '+')
                {
                    logger.Debug("추가된 블록 생성 시작");

                    remainder = diffLength;
                    fileOffset += diffLength;

                    while (remainder > 0)
                    {
                        readLength = ns.Read(buf, 0, ((remainder > buf.Length) ? buf.Length : (int)remainder));
                        fsNew.Write(buf, 0, readLength);
                        fsNew.Flush();
                        remainder -= readLength;
                    }
                    logger.Debug("생성 끝");
                }

                /* 스트림 헤더가 파일의 삭제된 부분을 가리킬 때 */
                if (flag == '-')
                {
                    logger.Debug("삭제된 블록 시작");

                    remainder = diffOffset - fileOffset;
                    fileOffset = diffOffset;

                    while (remainder > 0)
                    {
                        readLength = fsOld.Read(buf, 0, ((remainder > buf.Length) ? buf.Length : (int)remainder));
                        fsNew.Write(buf, 0, readLength);
                        remainder -= readLength;
                    }
                    logger.Debug("삭제 끝");
                }
            }

            logger.Debug("최신화 완료.");

            File.Move(filePath, filePath + ".bak");
            File.Move(filePath + ".tmp", filePath);
            File.Delete(filePath + ".bak");
        }
    }
}
