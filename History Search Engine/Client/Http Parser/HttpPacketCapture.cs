using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using PacketDotNet;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Client.Http_Parser
{
    public delegate void HttpPacketArriveEventHandler(object sender, HttpPacketArriveEvnetArgs e);

    class HttpPacketCapture
    {
        private CaptureDeviceList devices;
        private const int HTTP_PORT = 80;    // HTTP packet port
        private const int HTTPS_PORT = 443;  // HTTPS packet port

        private static List<HttpPacket> readyAssembleHttpPacket;
        private static Queue<uint> seqNumbers;

        public event HttpPacketArriveEventHandler OnHttpPacketArrival;

        /// <summary>
        /// Basic constructor of this class
        /// </summary>
        public HttpPacketCapture()
        {
            devices = CaptureDeviceList.Instance;
            readyAssembleHttpPacket = new List<HttpPacket>();
            seqNumbers = new Queue<uint>();
        }

        /// <summary>
        /// Get available ethernet device list
        /// </summary>
        /// <returns>Ethternet device list</returns>
        public CaptureDeviceList GetEthernetDeviceList()
        {
            if (devices.Count < 1)
            {
                return null;
            }
            return devices;
        }

        /// <summary>
        /// Capture the packet from all devices
        /// </summary>
        /// <param name="timeout"></param>
        public void Start(int timeout)
        {
            foreach(ICaptureDevice device in devices)
            {
                Start(timeout, device);
            }
        }

        /// <summary>
        /// Capturedevice the packet form specific 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="device"></param>
        public void Start(int timeout, ICaptureDevice device)
        {
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(OnPacketArrival);
            device.Open(DeviceMode.Promiscuous, timeout);
            device.StartCapture();
        }

        public void Stop()
        {
            foreach(ICaptureDevice device in devices) 
            {
                device.StopCapture();
            }
        }

        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;

            RawCapture rawCapture = e.Packet;
            Packet packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            TcpPacket tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));

            if (tcpPacket != null)
            {
                /// Filtering HTTP Packet using source port and desination port
                if (IsHttpPacket(tcpPacket))
                {
                    if (tcpPacket.Psh && tcpPacket.Ack)
                    {
                        seqNumbers.Enqueue(tcpPacket.AcknowledgmentNumber);
                    }

                    // If arrived packet is the first packet of the Http packet
                    if (IsFirstPacket(tcpPacket))
                    {
                        if (packet.PayloadPacket.PayloadPacket.PayloadData.Length != 0)
                        {
                            try
                            {
                                readyAssembleHttpPacket.Add(new HttpPacket(packet));
                            }
                            catch (ArgumentException exep)
                            {
                                Debug.WriteLine(exep.Message);
                            }
                        }
                    }
                    else
                    {
                        foreach (HttpPacket element in readyAssembleHttpPacket)
                        {
                            if (element.NextSequenceNumber == tcpPacket.SequenceNumber && !element.IsAssembleEnded)
                            {
                                // Check assembling work is done
                                if(element.AssembleTcpPacket(packet))
                                {
                                    HttpPacketArriveEvnetArgs packetEvent = new HttpPacketArriveEvnetArgs(element);
                                    OnHttpPacketArrived(packetEvent);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnHttpPacketArrived(HttpPacketArriveEvnetArgs e)
        {
            HttpPacketArriveEventHandler handler = OnHttpPacketArrival;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static Boolean IsFirstPacket(TcpPacket packet)
        {
            foreach (uint element in seqNumbers)
            {
                if (packet.SequenceNumber - element == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Boolean IsHttpPacket(TcpPacket packet)
        {
            return packet.SourcePort == HTTP_PORT || 
                   packet.SourcePort == HTTPS_PORT ||
                   packet.DestinationPort == HTTP_PORT || 
                   packet.DestinationPort == HTTPS_PORT;
        }

        private static string ByteArrayToString(byte[] array)
        {
            string hex = BitConverter.ToString(array);
            return hex.Replace("-", "");
        }
    }

    public class HttpPacketArriveEvnetArgs : EventArgs
    {
        private HttpPacket packet;
        /// <summary>
        /// Cunstructor
        /// </summary>
        /// <param name="packet"></param>
        public HttpPacketArriveEvnetArgs(HttpPacket packet)
        {
            this.packet = packet;
        }

        /// <summary>
        /// Http packet was captured
        /// </summary>
        public HttpPacket Packet
        {
            get
            {
                return packet;
            }
        }
    }
}
