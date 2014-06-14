using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpPcap;
using PacketDotNet;

namespace Client.Http_Parser
{
    class HttpPacketCapture
    {
        private ICaptureDevice device;

        /**
         * Default Constructor
         **/
        public HttpPacketCapture()
        {
            device = null;
        }

        public CaptureDeviceList getEthernetDeviceList()
        {
            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                return null;
            }
            return devices;
        }

        public void setDevice(ICaptureDevice device)
        {
            this.device = device;
        }

        public void start(int timeout)
        {
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(onPacketArrival);
            device.Open(DeviceMode.Promiscuous, timeout);
            device.StartCapture();
        }

        public void stop()
        {
            device.StopCapture();
        }

        private void onPacketArrival(object sender, CaptureEventArgs e)
        {
            DateTime time = e.Packet.Timeval.Date;
            int len = e.Packet.Data.Length;
            Console.WriteLine("{0}:{1}:{2}:{3} Len={4}", time.Hour, time.Minute, time.Second, time.Millisecond, len);
            Console.WriteLine("{0}", e.Packet.Data);
        }
    }
}
