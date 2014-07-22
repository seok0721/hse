using System;

namespace Reference.Protocol
{
    public class ProtocolRequest
    {
        /// <summary>
        /// 클라이언트에서 로그인 요청을 전송합니다. 사용자 아이디가 인자에 포함됩니다.
        /// </summary>
        public const String UserId = "USER"; // <UserId:String>

        /// <summary>
        /// 클라이언트에서 로그인에 필요한 비밀번호를 전송합니다.
        /// </summary>
        public const String Password = "PASS"; // <Password:String>

        /// <summary>
        /// 클라이언트에서 접속을 종료합니다.
        /// </summary>
        public const String Logout = "QUIT";

        /// <summary>
        /// 클라이언트에서 만든 데이터 포트에 서버를 초대합니다.
        /// </summary>
        public const String DataPort = "PORT"; // <Host-Number:int,int,int,int> <Port-Number:int,int>

        /// <summary>
        /// 클라이언트가 서버에 파일 수신을 요청합니다.
        /// </summary>
        public const String Retrieve = "RETR"; // <FileId:Int>

        /// <summary>
        /// 클라이언트에서 서버로 파일을 전송합니다.
        /// </summary>
        public const String Store = "STOR"; // <FileModel.toString()>

        /// <summary>
        /// 서버에 저장된 파일의 목록을 조회합니다
        /// </summary>
        public const String List = "LIST"; // "DATE" <yyyy-MM-dd:String> | "WORD" <Keyword:String>

        /// <summary>
        /// 로컬 데이터베이스에 쌓여있는 데이터를 서버로 전송합니다.
        /// </summary>
        public const String Accumulate = "ACML"; // TODO 정의할 것

        /// <summary>
        /// 파일 IO 발생 정보를 서버로 보냅니다.
        /// </summary>
        public const String FileOperation = "FLIO"; // <FileModel.toString()> <FileIOLog.toString()>

        /// <summary>
        /// 파일에 포함된 단어를 전송합니다.
        /// </summary>
        public const String FileWord = "FLWD"; // <FileModel.toString()> "WORD"

        /// <summary>
        /// 웹 문서에 포함된 단어를 전송합니다.
        /// </summary>
        public const String HtmlWord = "HTWD"; // <URL:String> "WORD"

        public String Command { get; set; }
        public String Argument { get; set; }
    }
}
