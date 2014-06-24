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
        public CaptureDeviceList getEthernetDeviceList()
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
        public void start(int timeout)
        {
            foreach(ICaptureDevice device in devices)
            {
                start(timeout, device);
            }
        }

        /// <summary>
        /// Capturedevice the packet form specific 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="device"></param>
        public void start(int timeout, ICaptureDevice device)
        {
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(onPacketArrival);
            device.Open(DeviceMode.Promiscuous, timeout);
            device.StartCapture();
        }

        public void stop()
        {
            foreach(ICaptureDevice device in devices) 
            {
                device.StopCapture();
            }
        }

        private static void onPacketArrival(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;
            var rawSourcePort = new Byte[2];
            Array.Copy(e.Packet.Data, 15, rawSourcePort, 0, 2);
            string sourcePort = ByteArrayToString(rawSourcePort);
            System.Diagnostics.Debug.WriteLine("{0}:{1}:{2} Source Port={3}", time.Hour, time.Minute, time.Second, sourcePort);
            if (sourcePort == "0005")
            {
                Console.WriteLine("{0}:{1}:{2}:{3} Len={4}", time.Hour, time.Minute, time.Second, time.Millisecond, len);
                Console.WriteLine("{0}", sourcePort);
            }
        }

        private static string ByteArrayToString(byte[] array)
        {
            string hex = BitConverter. ToString(array);
            return hex.Replace("-", "");
        }
    }
}
