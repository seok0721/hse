using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using Client.Domain;
using Client.Utility;

using log4net;

namespace Client.Service.Network
{
    public class DataTransferProcess
    {
        private ILog logger = LogManager.GetLogger(typeof(DataTransferProcess));
        private Socket userSocket;
        private Socket serverSocket;
        private NetworkStream ns;
        private List<char> diffFlagList = new List<char>();
        private List<long> diffOffsetList = new List<long>();
        private List<long> diffLengthList = new List<long>();

        /// <summary>
        /// 사용자 데이터 전송 프로세스를 초기화 합니다.
        /// 사용자 소켓이 서버 소켓의 접속을 대기합니다.
        /// </summary>
        public void Init()
        {
            userSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            userSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            userSocket.Listen(1);

            logger.DebugFormat("사용자 DTP 초기화. {0}", userSocket.LocalEndPoint.ToString());
        }

        /// <summary>
        /// 사용자 DTP가 바인드 되었는지 확인합니다.
        /// </summary>
        public bool IsBound
        {
            get
            {
                return (userSocket != null) && userSocket.IsBound;
            }
        }

        /// <summary>
        /// 사용자 소켓과 서버 소켓의 접속 상태
        /// </summary>
        public bool Connected
        {
            get
            {
                return (serverSocket != null) && serverSocket.Connected;
            }
        }

        /// <summary>
        /// 사용자 소켓의 포트
        /// </summary>
        public int Port
        {
            get
            {
                return (userSocket.LocalEndPoint as IPEndPoint).Port;
            }
        }

        /// <summary>
        /// 접속과 관련된 리소스를 모두 해제합니다.
        /// </summary>
        public void ReleaseConnection()
        {
            ns.Close();
            serverSocket.Close();
        }

        /// <summary>
        /// 서버가 접속할 때 까지 기다립니다.
        /// </summary>
        public void WaitForConnection()
        {
            serverSocket = userSocket.Accept();
            ns = new NetworkStream(serverSocket);
        }

        /// <summary>
        /// 서버로 전송할 파일의 체크섬 스트림을 수신합니다.
        /// </summary>
        /// <param name="model">파일의 기본 정보를 담고 있는 모델</param>
        /// <returns>스트림 요청 시 0, 새 파일 요청 시 1, 이미 최신 파일이면 2</returns>
        public int ReceiveChecksumStream(FileModel model)
        {
            String header = null;
            String checksum = null;
            long fileLength = new FileInfo(model.Path + "\\" + model.Name).Length;

            diffFlagList.Clear();
            diffOffsetList.Clear();
            diffLengthList.Clear();

            /* 데이터를 수신받지 못하면 에러 발생 */
            if ((header = ReadFromNetworkStream(Constants.MD5Length)) == null)
            {
                throw new Exception("네트워크 스트림을 수신 받는 도중 오류가 발생하였습니다.");
            }

            logger.Debug("Header : " + header);

            /* 새 파일을 요청하는 경우 */
            if (header == new String('0', 32))
            {
                return StoreResponseCode.RequestNewFile;
            }

            /* 이미 최신 파일을 가지고 있는 경우 */
            if (header == new String('F', 32))
            {
                return StoreResponseCode.AlreadyTheNewestFile;
            }

            /* 나머지 경우는 최신화를 요청하는 경우 */
            checksum = header;

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", model.Path, model.Name)))
            {
                /* 서버에서 수신받은 해시와 파일에서 읽은 데이터의 해시 비교 시작 */
                do
                {
                    /* 체크섬이 끝에 도달한 경우 */
                    if (checksum == new String('F', 32))
                    {
                        break;
                    }

                    AnalyzeDifferenceChecksum(fs, checksum, fileLength);
                } while ((checksum = ReadFromNetworkStream(Constants.MD5Length)) != null);
            }

            logger.Debug("루프 빠져나옴");

            return StoreResponseCode.RequestDifferenceStream;
        }

        /// <summary>
        /// 서버로 파일 스트림을 전송합니다.
        /// </summary>
        /// <param name="model">파일 기본 정보를 담고 있는 모델</param>
        public void SendFileStream(FileModel model)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;

            using (FileStream fs = File.OpenRead(String.Format("{0}\\{1}", model.Path, model.Name)))
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
        /// <param name="model">파일의 기본 정보를 담고 있는 모델</param>
        public void ReceiveFileStream(String filePath)
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int length = 0;

            logger.Debug("새 파일 수신을 시작합니다.");

            using (FileStream fs = File.OpenWrite(filePath))
            {
                while ((length = ns.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, length);
                }
            }
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

            while ((length = ns.Read(buffer, 0, buffer.Length)) > 0)
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
                logger.Debug("총 변경 블록의 수 : " + diffFlagList.Count);

                for (int i = 0; i < diffFlagList.Count; i++)
                {
                    /* 플래그, 위치, 길이를 헤더로 구성하여 서버로 전송 */
                    header = Encoding.UTF8.GetBytes(String.Format("{0}{1,64:D64}{2,64:D64}",
                        diffFlagList[i], diffOffsetList[i], diffLengthList[i]));

                    ns.Write(header, 0, header.Length);
                    ns.Flush();

                    /* 플래그가 '+'일 경우 뒤에 페이로드를 붙여서 전송 */
                    if (diffFlagList[i] == '+')
                    {
                        fs.Seek(diffOffsetList[i], SeekOrigin.Begin);
                        remainder = diffLengthList[i];

                        while (remainder > 0)
                        {
                            length = fs.Read(buffer, 0, buffer.Length);
                            remainder -= length;

                            ns.Write(buffer, 0, length);
                            ns.Flush();
                        }
                    }
                }

                logger.Debug("변경 내역 전송 완료");
            }

            /* 스트림의 끝을 알리기 위해 트레일러를 전송 */
            ns.Write(Encoding.UTF8.GetBytes(String.Format("{0}{1,128:D128}", '\0', 0)), 0, 129);
            ns.Flush();
        }

        /// <summary>
        /// 파일에서 체크섬과 일치하는 부분이 있는지 분석합니다.
        /// </summary>
        /// <param name="fs">파일 스트림</param>
        /// <param name="checksum">비교할 체크섬</param>
        private void AnalyzeDifferenceChecksum(FileStream fs, String checksum, long fileLength)
        {
            long criterion = fs.Position;
            long offset = criterion;

            while (true)
            {
                /* 파일에서 데이터 읽기 */
                String data = ReadFromFile(fs, Constants.HashInput);

                /* 더이상 파일에서 읽을 데이터가 없는 경우, 추가된 데이터로 인식 */
                if (data.Length == 0)
                {
                    diffFlagList.Add('+');
                    diffOffsetList.Add(criterion);
                    diffLengthList.Add(Constants.HashInput < fileLength ? Constants.HashInput : fileLength);

                    fs.Seek(criterion, SeekOrigin.Begin);

                    return;
                }

                /* 해시가 일치 하는 경우 */
                if (HashUtils.HashMD5(data) == checksum)
                {
                    /* 읽은 데이터 앞부분에 갭이 생겼을 경우, 삭제된 데이터로 인식 */
                    if (offset - criterion > 0)
                    {
                        diffFlagList.Add('-');
                        diffOffsetList.Add(criterion);
                        diffLengthList.Add(offset - criterion);
                    }

                    return;
                }

                /* 해시가 일치하지 않으면 옵셋을 증가하여 반복 */
                offset++;
                fs.Seek(offset, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// 네트워크 스트림에서 원하는 기준 값 만큼 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="criterion">기준 값</param>
        /// <returns>스트림에서 읽어 온 문자열, 연결이 끊기면 null</returns>
        private String ReadFromNetworkStream(int criterion)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[Constants.BufferSize];
            int remainder = criterion;
            int length = 0;

            while (remainder > 0)
            {
                logger.Debug("Read Begin");
                /* 연결이 끊기는 경우 스트림에서 0바이트를 읽음 */
                if ((length = ns.Read(buffer, 0, (remainder < buffer.Length) ? remainder : buffer.Length)) == 0)
                {
                    return null;
                }

                remainder -= length;
                builder.Append(Encoding.UTF8.GetString(buffer, 0, length));

                logger.Debug("Read End");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 파일에서 원하는 기준 값 만큼 데이터를 읽어옵니다.
        /// </summary>
        /// <param name="fs">파일 스트림</param>
        /// <param name="criterion">기준 값</param>
        /// <returns>스트림에서 읽어 온 문자열</returns>
        private String ReadFromFile(FileStream fs, int criterion)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[Constants.BufferSize];
            int remainder = criterion;
            int length = 0;

            while (remainder > 0)
            {
                /* 더이상 읽을 데이터가 없는 경우 */
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