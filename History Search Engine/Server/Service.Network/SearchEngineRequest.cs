using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Service.Network
{
    public class SearchEngineRequest
    {
        public String Command { get; set; }
        public String Argument { get; set; }

        /* FTP: Access Control Commands */

        /// <summary>
        /// 클라이언트에서 로그인 요청과 동시에 사용자 이름을 전송합니다.
        /// <para>Usage: USER &lt;UserId:String&gt;</para>
        /// </summary>
        public const String UserId = "USER";

        /// <summary>
        /// 클라이언트에서 로그인에 필요한 비밀번호를 전송합니다.
        /// <para>Usage: PASS &lt;Password:String&gt;</para>
        /// </summary>
        public const String Password = "PASS";

        /// <summary>
        /// 클라이언트에서 접속을 종료합니다.
        /// </summary>
        public const String Logout = "QUIT";

        /* FTP: Transfer Parameter Commands */
        /// <summary>
        /// 클라이언트에서 만든 데이터 포트에 서버를 초대합니다.
        /// 
        /// <para>Usage: PORT &lt;SP&gt; &lt;host-number&gt;,&lt;port-number&gt; &lt;CRLF&gt;</para>
        /// <para>Usage: PORT &lt;SP&gt; &lt;number&gt;,&lt;number&gt;,&lt;number&gt;,&lt;number&gt;,&lt;number&gt;,&lt;number&gt; &lt;CRLF&gt;</para>
        /// </summary>
        public const String DataPort = "PORT"; // 데이터 포트 요청

        /* FTP: FTP Service Commands */
        public const String Retrieve = "RETR"; // 파일 수신

        /// <summary>
        /// 클라이언트에서 서버로 파일을 전송합니다.
        /// 
        /// <para>Usage: STOR &lt;SP&gt; &lt;FileModel.toString()&gt; &lt;CRLF&gt;</para>
        /// </summary>
        public const String Store = "STOR";

        public const String List = "LIST"; // 파일 목록 조회 (검색 결과 조회)
        
        /* Custom Commands */
        public const String Accumulate = "ACML"; // 데이터 서버로 축적

        /// <summary>
        /// Command : FLIO
        /// Argument : &lt;FileModel.toString()&gt;,&lt;FileIOLog.toString()&gt;
        /// </summary>
        public const String FileOperation = "FLIO"; // 파일 I/O 발생 정보 전송
    }
}