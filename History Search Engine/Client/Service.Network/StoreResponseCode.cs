using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Service.Network
{
    /// <summary>
    /// 사용자가 "STOR" 명령을 전송하였을 때, DTP에서 수신하는 첫번째 체크섬에 따른 코드
    /// </summary>
    public class StoreResponseCode
    {
        /// <summary>
        /// 변화된 파일 전송을 요청
        /// </summary>
        public const int RequestDifferenceStream = 0;

        /// <summary>
        /// 파일 전송을 요청
        /// </summary>
        public const int RequestNewFile = 1;

        /// <summary>
        /// 이미 최신 파일 유지
        /// </summary>
        public const int AlreadyTheNewestFile = 2;

        private StoreResponseCode() { }
    }
}
