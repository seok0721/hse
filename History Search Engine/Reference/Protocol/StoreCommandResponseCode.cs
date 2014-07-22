using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reference.Protocol
{
    public class StoreCommandResponseCode
    {
        public const int RequestDifferenceStream = 0;

        public const int RequestNewFile = 1;

        public const int AlreadyTheNewestFile = 2;

        private StoreCommandResponseCode() { }
    }
}
