using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Service.Network
{
    public class SearchEngineResponse
    {
        public int Code { get; set; }
        public String Message { get; set; }

        public const int ServiceReadyInMinutes = 120; // n분 동안 새로운 사용자를 위해 서비스 대기

        public const int DataConnectionAlreadyOpen = 125; // 데이터 접속이 이미 열림

        public const int OpenDataConnection = 150; // 데이터 접속을 열기

        public const int CommandOkay = 200; // 커맨드 성공적으로 수신 완료.

        public const int CommandNotImplemented = 202; // 커맨드가 구현되지 않았습니다, 이 사이트에서는 불필요합니다.

        public const int ServiceReadyForNewUser = 220; // 새로운 사용자를 위해 서비스 대기

        public const int ServiceClosingControlConnection = 221; // 서비스 종료, 제어 접속 해제, 로그아웃

        public const int CloseDataConnection = 226; // 데이터 커넥션 닫기

        public const int UserLoggedIn = 230; // 사용자 로그인 확인, 계속 진행.

        public const int FileActionCompleted = 250; // 파일 액션이 완료됨.

        public const int RequestPassword = 331; // 사용자 아이디 확인, 패스워드 요청

        public const int NeedAccountForLogin = 332; // 사용자 아이디 요청

        public const int ServiceNotAvailable = 421; // 서비스 사용 불가능, 제어 접속이 닫힘.

        public const int CannotOpenDataConnection = 425; // 데이터 접속을 열 수 없음

        public const int ConnectionClosed = 426; // 접속이 닫혀있음.

        public const int RequestedActionAborted = 451; // 요청된 액션이 취소되었음, 처리도중 로컬 에러 발생

        public const int UnknownCommandError = 500; // 문법 에러, 알 수 없는 명령어

        public const int InvalidArgumentError = 501; // 문법 에러, 잘못된 인수

        public const int BadSequenceOfCommands = 503; // 명령들의 잘못된 순서

        public const int NotLoggedIn = 530; // 비 로그인 상태.
    }
}