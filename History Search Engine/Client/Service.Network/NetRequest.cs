using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Service.Network
{
    public class NetRequest
    {
        /* FTP: Access Control Commands */
        public const String UserId = "USER"; // 로그인, 사용자 아이디

        public const String Password = "PASS"; // 로그인, 비밀번호

        public const String Logout = "QUIT"; // 로그아웃

        /* FTP: Transfer Parameter Commands */
        public const String DataPort = "PORT"; // 데이터 포트 요청

        /* FTP: FTP Service Commands */
        public const String Retrieve = "RETR"; // 파일 수신

        public const String Store = "STOR"; // 파일 송신

        public const String List = "LIST"; // 파일 목록 조회 (검색 결과 조회)

        /* Custom Commands */
        public const String Accumulate = "ACML"; // 데이터 서버로 축적

        public String Command { get; set; }
        public String Argument { get; set; }

        public override string ToString()
        {
            if (Argument == null)
            {
                return String.Format("{0:4}", Command);
            }
            else
            {
                return String.Format("{0:4} {1}", Command, Argument);
            }
        }
    }
}
