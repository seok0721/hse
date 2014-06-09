using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using log4net;

namespace Server.DataSource
{
    /**
     * SQL 데이터베이스 서버와의 접속 정보 및 상태를 관리합니다.
     * 
     * GetConnection 함수들을 호출하면 실제로 서버에 접속을 시도합니다.
     * 
     * connectionString은 key=value 문자열이 ;으로 연결되어 있는 형태로 구성됩니다.
     * 자세한 정보는 아래 링크를 확인하세요.
     * http://msdn.microsoft.com/ko-kr/library/system.data.sqlclient.sqlconnection.connectionstring%28v=vs.110%29.aspx
     * 
     * connectionTimeout은 디폴트 값으로 5초가 설정되어 있으며, 임의로 변경 가능합니다.
     */
    public class SqlDataSource : IDataSource
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SqlDataSource));
        private String connectionString;
        private int connectionTimeout = 5;
        private String database;
        private String username;
        private String password;

        private SqlConnection connection;

        public SqlDataSource()
        {

        }

        public SqlDataSource(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlDataSource(String connectionString, String database)
        {
            this.connectionString = connectionString;
            this.database = database;
        }

        public SqlDataSource(String connectionString, String database, String username, String password)
        {
            this.connectionString = connectionString;
            this.database = database;
            this.username = username;
            this.password = password;
        }
        
        public DbConnection GetConnection()
        {
            if (connection != null)
            {
                return connection;
            }

            if (connectionString == null)
            {
                logger.Warn("ConnectionString is null.");
                return null;
            }

            Dictionary<String, String> dict = parseConnectionString();

            if (dict.Count == 0)
            {
                logger.Warn("ConnectionString is empty.");
                return null;
            }

            // 데이터베이스 프로퍼티 검사
            if (!dict.ContainsKey("database") && database != null)
            {
                dict.Add("database", database);
            }

            // 사용자 아이디 프로퍼티 검사
            if(!dict.ContainsKey("user id") && !dict.ContainsKey("uid"))
            {
                if (username != null)
                {
                    dict.Add("uid", username);
                }
                else if (!dict.ContainsKey("trusted_connection"))
                {
                    dict.Add("trusted_connection", "true");
                }
            }

            // 비밀번호 프로퍼티 검사
            if (!dict.ContainsKey("password") && !dict.ContainsKey("pwd") && password != null)
            {
                dict.Add("pwd", password);
            }

            // 타임아웃 프로퍼티 검사
            if (!dict.ContainsKey("timeout"))
            {
                dict.Add("timeout", connectionTimeout.ToString());
            }

            connection = new SqlConnection(toConnectionString(dict));
            
            logger.Debug("Try to connect sql server.");
            logger.Debug("Connection String: " + connection.ConnectionString);

            // 데이터베이스 접속 시도
            try
            {
                connection.Open();
                logger.Debug("Success to open connection.");
            }
            catch (SqlException ex)
            {
                logger.Error("Failrue to open connection.");
                logger.Error(ex.Message);
            }

            return connection;
        }

        public DbConnection GetConnection(string connectionString)
        {
            this.connectionString = connectionString;
         
            return GetConnection();
        }

        public DbConnection GetConnection(string connectionString, string database)
        {
            this.connectionString = connectionString;
            this.database = database;

            return GetConnection();
        }

        public DbConnection GetConnection(string connectionString, string database, string username, string password)
        {
            this.connectionString = connectionString;
            this.database = database;
            this.username = username;
            this.password = password;

            return GetConnection();
        }

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return connectionTimeout;
            }
            set
            {
                connectionTimeout = value;
            }
        }

        public string Database
        {
            get
            {
                return database;
            }
            set
            {
                database = value;
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        private Dictionary<String, String> parseConnectionString()
        {
            Dictionary<String, String> dict = new Dictionary<String, String>();
            String[] connectionProperties = connectionString.Split(';');
            String[] pair;
            String key;
            String value;

            foreach (String property in connectionProperties)
            {
                pair = property.Trim().Split('=');

                if (pair.Length != 2)
                {
                    throw new Exception("ConnectionString is invalid.");
                }

                key = pair[0];
                value = pair[1];

                dict.Add(key, value);
            }

            return dict;
        }

        private String toConnectionString(Dictionary<String, String> dict)
        {
            StringBuilder builder = new StringBuilder();

            foreach (String key in dict.Keys)
            {
                builder
                    .Append(key.ToLower())
                    .Append("=")
                    .Append(dict[key])
                    .Append(";");
            }

            return builder.ToString();
        }
    }
}