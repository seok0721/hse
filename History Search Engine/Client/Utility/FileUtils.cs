using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.Utility
{
    public class FileUtils
    {
        public static String GetUniqueIdentifier(String filePath)
        {
            FileInformation fi = GetFileInformation(filePath);

            return String.Format("{0}{1}{2}", fi.VolumeSerialNumber.ToString("x8"),
                fi.FileIndexHigh.ToString("x8"), fi.FileIndexLow.ToString("x8")).ToUpper();
        }

        public static DateTime GetLastWriteTime(String filePath)
        {
            FileInformation fi = GetFileInformation(filePath);

            return DateTime.FromFileTime(((long)(fi.LastWriteTime.dwHighDateTime) << 32) + fi.LastWriteTime.dwLowDateTime);
        }

        public static String ToBitArray(uint value)
        {
            StringBuilder builder = new StringBuilder();
            uint mask = 0x80000000;
            for (int i = 0; i < 32; i++)
            {
                builder.Append((value & mask) == 0 ? "0" : "1");
                mask >>= 1;
            }
            return builder.ToString();
        }

        private static FileInformation GetFileInformation(String filePath)
        {
            FileInformation fi = new FileInformation();

            using (FileStream fs = File.OpenRead(filePath))
            {
                GetFileInformationByHandle(fs.Handle.ToInt32(), out fi);
            }
            
            return fi;
        }

        [DllImport("Kernel32.dll")]
        private static extern void GetFileInformationByHandle(int handle, out FileInformation fileInformation);

        private struct FileInformation
        {
            public uint FileAttributes { get; set; }
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime { get; set; }
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime { get; set; }
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime { get; set; }
            public uint VolumeSerialNumber { get; set; }
            public uint FileSizeHigh { get; set; }
            public uint FileSizeLow { get; set; }
            public uint NumberOfLinks { get; set; }
            public uint FileIndexHigh { get; set; }
            public uint FileIndexLow { get; set; }
        }

        private FileUtils() { }
    }
}
