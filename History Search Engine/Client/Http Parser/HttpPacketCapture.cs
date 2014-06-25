using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using PacketDotNet;

namespace Client.Http_Parser
{
    public delegate void HttpPacketHandler(object sender, EventArgs e);

    class HttpPacketCapture
    {
        private CaptureDeviceList devices;

        /// <summary>
        /// Basic constructor of this class
        /// </summary>
        public HttpPacketCapture()
        {
            devices = CaptureDeviceList.Instance;
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

        private static void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;

            var rawSourcePort = new Byte[2];
            Array.Copy(e.Packet.Data, 34, rawSourcePort, 0, 2);
            string sourcePort = ByteArrayToString(rawSourcePort);

            var rawDesinationPort = new Byte[2];
            Array.Copy(e.Packet.Data, 36, rawDesinationPort, 0, 2);
            string desinationPort = ByteArrayToString(rawDesinationPort);

            if (String.Compare(sourcePort,"0050")==0 || String.Compare(desinationPort, "0050")==0)
            {
                Console.WriteLine("{0}:{1}:{2}:{3} Len={4}", time.Hour, time.Minute, time.Second, time.Millisecond, len);
                Console.WriteLine("{0}", ByteArrayToString(e.Packet.Data));
            }
        }

        private static string ByteArrayToString(byte[] array)
        {
            string hex = BitConverter.ToString(array);
            return hex.Replace("-", "");
        }
    }
}
