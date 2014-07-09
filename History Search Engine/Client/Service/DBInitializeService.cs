using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ADOX;

using log4net;

namespace Client.Service
{
    /// <summary>
    /// 네트워크 사용이 불가능할 때, 로컬에서 사용할 데이터베이스를 생성하는 서비스
    /// </summary>
    public class DBInitializeService
    {
        private ILog logger = LogManager.GetLogger(typeof(DBInitializeService));
        private readonly String dbPath = AppDomain.CurrentDomain.BaseDirectory + "client.accdb";
        private Catalog catalog = new Catalog();
        private bool isDebugMode = true;

        /// <summary>
        /// 네트워크 연결이 안된 경우를 대비하여 클라이언트에서 임시로 데이터를 저장할 데이터베이스를 생성합니다.
        /// </summary>
        public void CreateSchema()
        {
            if (File.Exists(dbPath))
            {
                if (!isDebugMode)
                {
                    logger.Warn("데이터베이스가 이미 존재합니다.");
                    return;
                }

                /* 디버그 모드일 경우 */
                logger.Warn("디버그 모드에서는 데이터베이스를 새로 생성합니다.");

                File.Delete(dbPath);
            }

            catalog.Create(String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5", dbPath));
            catalog.Tables.Append(CreateTableFile());
            catalog.Tables.Append(CreateTableFileIOLog());
            catalog.Tables.Append(CreateTableDocument());
            catalog.Tables.Append(CreateTableWord());
        }

        /// <summary>
        /// 파일 기본 정보 테이블을 생성합니다.
        /// </summary>
        /// <returns>파일 기본 정보 테이블</returns>
        private Table CreateTableFile()
        {
            Table table = new Table();
            table.Name = "TBL_FILE";

            table.Columns.Append("FILE_ID", DataTypeEnum.adInteger);
            table.Columns.Append("UNIQUE_ID", DataTypeEnum.adVarChar, 100);
            table.Columns.Append("USR_ID", DataTypeEnum.adVarWChar, 20);
            table.Columns.Append("FILE_PATH", DataTypeEnum.adVarWChar, 200);
            table.Columns.Append("FILE_NM", DataTypeEnum.adVarWChar, 200);
            table.Columns.Append("FILE_SIZE", DataTypeEnum.adInteger);
            table.Columns.Append("LAST_UPDATE_TIME", DataTypeEnum.adDate);
            table.Columns.Append("REMOVE_YN", DataTypeEnum.adChar, 1);

            table.Keys.Append("PK_FILE_ID", KeyTypeEnum.adKeyPrimary, "FILE_ID");
            table.Keys.Append("UK_UNIQUE_ID", KeyTypeEnum.adKeyUnique, "UNIQUE_ID");

            return table;
        }

        /// <summary>
        /// 파일 I/O 로그 테이블을 생성합니다.
        /// </summary>
        /// <returns>파일 I/O 로그 테이블</returns>
        private Table CreateTableFileIOLog()
        {
            Table table = new Table();
            table.Name = "TBL_FILE_IO_LOG";

            table.Columns.Append("FILE_ID", DataTypeEnum.adInteger);
            table.Columns.Append("IO_LOG_SEQ", DataTypeEnum.adInteger);
            table.Columns.Append("IO_TYPE", DataTypeEnum.adWChar, 1);
            table.Columns.Append("IO_NAME", DataTypeEnum.adDate);

            table.Keys.Append("PK_FILE_ID", KeyTypeEnum.adKeyPrimary, "FILE_ID");
            table.Keys.Append("FK_FILE_ID", KeyTypeEnum.adKeyForeign, "FILE_ID", "TBL_FILE", "FILE_ID");

            return table;
        }

        /// <summary>
        /// HTTP에서 받은 문서의 기본 정보 테이블을 생성합니다.
        /// </summary>
        /// <returns>문서 기본 정보 테이블</returns>
        private Table CreateTableDocument()
        {
            Table table = new Table();
            table.Name = "TBL_DOCUMENT";

            table.Columns.Append("DOC_ID", DataTypeEnum.adInteger);
            table.Columns.Append("DOC_URL", DataTypeEnum.adVarWChar, 200);
            table.Columns.Append("CREATE_TIME", DataTypeEnum.adDate);
            table.Columns.Append("UPDATE_TIME", DataTypeEnum.adDate);

            table.Keys.Append("PK_DOC_ID", KeyTypeEnum.adKeyPrimary, "DOC_ID");

            return table;
        }

        /// <summary>
        /// HTTP에서 받은 문서를 파싱하여 추출된 단어가 저장되는 테이블을 생성합니다.
        /// </summary>
        /// <returns>단어 빈도 테이블</returns>
        private Table CreateTableWord()
        {
            Table table = new Table();
            table.Name = "TBL_WORD";

            table.Columns.Append("WORD_ID", DataTypeEnum.adInteger);
            table.Columns.Append("DOC_ID", DataTypeEnum.adInteger);
            table.Columns.Append("WORD", DataTypeEnum.adVarWChar, 200);
            table.Columns.Append("WORD_CNT", DataTypeEnum.adInteger);

            table.Keys.Append("PK_WORD_ID", KeyTypeEnum.adKeyPrimary, "WORD_ID");
            table.Keys.Append("FK_DOC_ID", KeyTypeEnum.adKeyForeign, "DOC_ID", "TBL_DOCUMENT", "DOC_ID");

            return table;
        }
    }
}
