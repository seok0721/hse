using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using log4net;

namespace Server.DataSource
{
    /**
     * 데이터베이스와의 접속 정보 및 상태를 관리합니다.
     */
    public interface IDataSource
    {
        /**
         * 설정되어 있는 속성들을 이용하여 데이터베이스로 접속을 시도합니다.
         */
        DbConnection GetConnection();

        /**
         * 특정 접속 문자열과 설정되어 있는 나머지 속성들을 이용하여 데이터베이스로 접속을 시도합니다.
         */
        DbConnection GetConnection(String connectionString);

        /**
         * 특정 접속 문자열, 데이터베이스 이름과 설정되어 있는 나머지 속성들을 이용하여 데이터베이스로 접속을 시도합니다.
         */
        DbConnection GetConnection(String connectionString, String database);

        /**
         * 특정 접속 문자열, 데이터베이스 이름, 사용자 이름, 비밀번호를 이용하여 데이터베이스로 접속을 시도합니다.
         */
        DbConnection GetConnection(String connectionString, String database, String username, String password);

        /**
         * 접속 문자열을 설정하거나 가져옵니다.
         */
        String ConnectionString { get; set; }

        /**
         * 데이터베이스로 접속을 시도할 때 최대 지연 시간을 설정하거나 가져옵니다.
         */
        int ConnectionTimeout{ get; set; }

        /**
         * 접속할 데이터베이스를 설정하거나 가져옵니다.
         */
        String Database { get; set; }

        /**
         * 데이터베이스 접속 시 사용할 사용자 아이디를 설정하거나 가져옵니다.
         */
        String Username { get; set; }

        /**
         * 데이터베이스 접속 시 사용할 비밀번호를 설정하거나 가져옵니다.
         */
        String Password { get; set; }
    }
}
