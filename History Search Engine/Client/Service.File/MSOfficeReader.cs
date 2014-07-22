using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Service.File
{
    abstract public class MSOfficeReader
    {
        public object FilePath { get; set; }

        abstract public string Read();
    }
}
