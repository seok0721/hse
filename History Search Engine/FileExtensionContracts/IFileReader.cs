using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtensionContracts
{
    public interface IFileReader
    {
        string FilePath { get; set; }
        string Read();
    }
}
