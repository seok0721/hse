using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Reference.Utility
{
    public class FileUtils
    {
        public static String GetUniqueIdentifier(String filePath)
        {
            FileInformation fi = GetFileInformation(filePath);

            return String.Format("{0}{1}{2}",
                fi.VolumeSerialNumber.ToString("x8"),
                fi.FileIndexHigh.ToString("x8"),
                fi.FileIndexLow.ToString("x8")).ToUpper();
        }

        public static DateTime GetLastWriteTime(String filePath)
        {
            FileInformation fi = GetFileInformation(filePath);

            return DateTime.FromFileTime(((long)(fi.LastWriteTime.dwHighDateTime) << 32) + fi.LastWriteTime.dwLowDateTime);
        }

        [DllImport("Kernel32.dll")]
        private static extern void GetFileInformationByHandle(int handle, out FileInformation fileInformation);

        private static FileInformation GetFileInformation(String filePath)
        {
            FileInformation fi = new FileInformation();

            using (FileStream fs = File.OpenRead(filePath))
            {
                GetFileInformationByHandle(fs.SafeFileHandle.DangerousGetHandle().ToInt32(), out fi);
            }

            return fi;
        }

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
