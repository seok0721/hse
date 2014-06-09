using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Network
{
    /**
     * 클라이언트 Protocol Interpretor가 보내는 명령어입니다.
     *
     * Custom Commands를 제외한 나머지는 FTP 프로토콜을 참고하였으며,
     * 1라인에 1명령어와 파라미터를 전송하는 방식으로 설계되었습니다.
     * 
     * [Command] [Argument1,Argument2,...]
     */
    enum SenderCommand
    {
        /* FTP: Access Control Commands */
        USER_NAME = "USER", // 로그인, 사용자 아이디
        PASSWORD = "PASS", // 로그인, 비밀번호
        LOGOUT = "QUIT", // 로그아웃

        /* FTP: Transfer Parameter Commands */
        DATA_PORT = "PORT", // 데이터 포트 요청

        /* FTP: FTP Service Commands */
        RETRIEVE = "RETR", // 파일 수신
        STORE = "STOR", // 파일 송신
        LIST = "LIST", // 파일 목록 조회 (검색 결과 조회)

        /* Custom Commands */
        ACCUMULATE_DATABASE = "ACML" // 데이터 서버로 축적
    }
}
