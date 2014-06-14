using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Http_Parser;
using SharpPcap;


namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("HSE Client Debug Console");

            HttpPacketCapture capture = new HttpPacketCapture();
            CaptureDeviceList devices = capture.getEthernetDeviceList();
            ICaptureDevice device = devices[2];

            foreach(ICaptureDevice element in devices)
            {
                Console.WriteLine("{0}", element.ToString());
            }

            capture.setDevice(device);

            Console.WriteLine("--Listening on {0}, hit 'Enter' to stop...", device.Name);

            capture.start(1000);

            Console.ReadLine();

            capture.stop();
        }
    }
}
