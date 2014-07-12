using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using PacketDotNet;
using System.IO;
using System.IO.Compression;

namespace Client.Http_Parser
{
    public class HttpPacket
    {
        // Variables for reassembling TCP Packet
        private Queue<TcpPacket> arrivedTcpPackets;
        private uint nextSequenceNumber;
        private uint relativeNextSequenceNumber;

        private string protocol;
        private int status;
        private Dictionary<string, string> header;
        private string content = "";

        private Boolean isCompressed;
        private int contentLength;
        private byte[] body = null;

        private Boolean isAssembleEnded;

        /// <summary>
        /// Basic constructor for initialize variables
        /// </summary>
        public HttpPacket()
        {
            arrivedTcpPackets = new Queue<TcpPacket>();
            nextSequenceNumber = 0;
            relativeNextSequenceNumber = 0;
            contentLength = 0;

            header = new Dictionary<string, string>();
            isCompressed = false;

            isAssembleEnded = false;
        }

        /// <summary>
        /// Initialize Http Packet
        /// </summary>
        /// <param name="packet">First TCP packet which consist of Http packet</param>
        public HttpPacket(Packet packet)
            : this()
        {
            try
            {
                TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
                // Update next sequence number
                nextSequenceNumber = tcpPacket.SequenceNumber + (uint)packet.PayloadPacket.PayloadPacket.PayloadData.Length;
                ParsingHeader(packet.PayloadPacket.PayloadPacket.PayloadData);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("invalid packet data");
            }
        }

        private void ParsingHeader(byte[] rawData)
        {
            string data = Encoding.UTF8.GetString(rawData);

            string[] splitData = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            protocol = splitData[0];

            if (splitData.Length == 1)
            {
                throw new ArgumentException("invalid packet data");
            }

            for (int i = 1; i < splitData.Length; i++)
            {
                string element = splitData[i];
                string[] headerInfo = element.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                if (headerInfo.Length == 2)
                {
                    if (headerInfo[0].Equals("Content-Encoding") && headerInfo[1].Equals("gzip"))
                    {
                        isCompressed = true;
                    }
                    else if (headerInfo[0].Equals("Content-Length"))
                    {
                        contentLength = int.Parse(headerInfo[1]);
                        body = new byte[contentLength];
                        byte startContentByte = 31;
                        int index = Array.IndexOf(rawData, startContentByte);
                        Array.Copy(rawData, index, body, 0, rawData.Length - index);
                        relativeNextSequenceNumber += (uint)(rawData.Length - index);
                    }
                    header.Add(headerInfo[0], headerInfo[1]);
                }
                else
                {
                    break;
                }
            }
        }

        public Boolean AssembleTcpPacket(Packet packet)
        {
            TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
            byte[] data = packet.PayloadPacket.PayloadPacket.PayloadData;

            string content = Encoding.UTF8.GetString(data);

            if (tcpPacket.SequenceNumber == nextSequenceNumber && data.Length != 0) 
            {
                if (body != null)
                {
                    Array.Copy(data, 0, body, relativeNextSequenceNumber, data.Length);
                    // Update next sequence number
                    // Next Sequence Number = Current Sequence Number + Packet Payload Data Length
                    nextSequenceNumber += (uint)data.Length;
                    relativeNextSequenceNumber += (uint)data.Length;

                    isAssembleEnded = relativeNextSequenceNumber >= body.Length;
                }
                else
                {
                    isAssembleEnded = true;
                }
            }

            return isAssembleEnded;
        }

        private Boolean IsValidMessage(string message)
        {
            string[] startString = new string[] { "HTTP", "GET" };
            foreach (string element in startString)
            {
                if (message.IndexOf(element) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];
            int cnt;
            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static string UnzipGzip(byte[] body)
        {
            using (var msi = new MemoryStream(body))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }


        public byte[] Body
        {
            get
            {
                return this.body;
            }
        }

        public string Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        public int Status
        {
            get
            {
                return this.status;
            }
        }

        public Dictionary<string, string> Header
        {
            get
            {
                return this.header;
            }
        }

        public string Content
        {
            get
            {
                if (body == null)
                {
                    return null;
                }

                if (this.isCompressed)
                {
                    return content = HttpPacket.UnzipGzip(body);
                }
                else
                {
                    return content = Encoding.UTF8.GetString(body);
                }
            }
        }

        public uint NextSequenceNumber
        {
            get
            {
                return this.nextSequenceNumber;
            }
        }

        public Boolean IsAssembleEnded
        {
            get
            {
                return this.isAssembleEnded;
            }
        }

        public Boolean IsCompressed
        {
            get
            {
                return this.isCompressed;
            }
        }
    }
}
