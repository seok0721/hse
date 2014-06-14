using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Service.Network
{
    public class NetResponse
    {
        public const int ServiceReadyInMinutes = 120; // n분 동안 새로운 사용자를 위해 서비스 대기

        public const int ServiceReadyForNewUser = 220; // 새로운 사용자를 위해 서비스 대기

        public const int ServiceClosingControlConnection = 221; // 서비스 종료, 제어 접속 해제, 로그아웃

        public const int UserLoggedIn = 230; // 사용자 로그인 확인, 계속 진행.

        public const int RequestPassword = 331; // 사용자 아이디 확인, 패스워드 요청

        public const int RequestUserId = 332; // 사용자 아이디 요청

        public const int ServiceNotAvailable = 421; // 서비스 사용 불가능, 제어 접속이 닫힘.

        public const int UnknownCommandError = 500; // 문법 에러, 알 수 없는 명령어

        public const int InvalidArgumentError = 501; // 문법 에러, 잘못된 인수

        public const int NotLoggedIn = 530; // 비 로그인 상태.

        public int Code { get; set; }
        public String Message { get; set; }

        public override string ToString()
        {
            if (Message == null)
            {
                return String.Format("{0}", Code);
            }
            else
            {
                return String.Format("{0} {1}", Code, Message);
            }
        }
    }
}