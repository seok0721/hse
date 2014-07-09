using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

using Client.Utility;
using Client.Service.Network;

namespace Client.View
{
    public class UserConsoleInterface
    {
        private ILog logger = LogManager.GetLogger(typeof(UserConsoleInterface));
        private ProtocolInterpretor userPI = new ProtocolInterpretor();
        private Properties props = new Properties();

        /// <summary>
        /// 환경 설정 파일을 불러옵니다.
        /// </summary>
        /// <returns>파일을 불러오면 true, 실패하면 false</returns>
        public bool LoadConfiguration()
        {
            String configPath = AppDomain.CurrentDomain.BaseDirectory + "config.properties";

            try
            {
                props.Load(configPath);

                return true;
            }
            catch (Exception)
            {
                logger.Error("환경설정 파일을 읽어오는 도중 오류가 발생하였습니다.");
                logger.ErrorFormat("다음 파일을 확인하십시오: {0}", configPath);

                return false;
            }
        }

        /// <summary>
        /// 사용자 콘솔을 시작합니다.
        /// </summary>
        public void Start()
        {
            /* 환경 변수 로드 */
            if (!LoadConfiguration())
            {
                Environment.Exit(1);
            }

            /* 사용자 명령어 해석기 초기화 */
            userPI.Properties = props;
            userPI.Init();

            /* 서버 명령어 해석기에 접속 */
            if (!userPI.Connect())
            {
                Environment.Exit(1);
            }

            /* 사용자 콘솔 시작 */
            UserInput userInput;
            String[] args;

            while ((userInput = ReadUserInput()) != null)
            {
                userInput.Command = userInput.Command.ToUpper();

                logger.DebugFormat("명령: {0}", userInput.Command);
                logger.DebugFormat("인자: {0}", userInput.Argument);

                switch (userInput.Command)
                {
                    case "LOGIN":
                        if (userInput.Argument == null)
                        {
                            Console.Out.WriteLine("아이디와 패스워드를 입력하세요.");
                        }
                        else
                        {
                            args = userInput.Argument.Split(' ');

                            if (args.Length == 2)
                            {
                                userPI.Login(args[0], args[1]);
                            }
                            else
                            {
                                Console.Out.WriteLine("아이디와 패스워드를 입력하세요.");
                            }
                        }
                        break;
                    case "LS":
                    case "LIST":
                        userPI.GetFileList(null);
                        break;
                    case "EXIT":
                    case "QUIT":
                        userPI.Logout();
                        logger.Error("프로그램을 종료합니다.");
                        return;
                    default:
                        logger.Error("알 수 없는 에러가 발생하였습니다.");
                        logger.ErrorFormat("명령:{0}, 인자{1}", userInput.Command, userInput.Argument);
                        break;
                }
            }
        }

        /// <summary>
        /// 사용자의 입력을 읽습니다.
        /// </summary>
        /// <returns>사용자가 입력한 명령어 및 인자값</returns>
        private UserInput ReadUserInput()
        {
            String[] split;
            char[] seperator = { ' ' };

            Console.Out.Write(">> ");
            split = Console.In.ReadLine().Split(seperator, 2);

            if (split.Length < 2)
            {
                return new UserInput(split[0], null);
            }
            else
            {
                return new UserInput(split[0], split[1]);
            }
        }

        private void TestLoginAndSendFile()
        {
            if (!userPI.Login("admin", "0000"))
            {
                logger.Info("로그인에 실패하였습니다.");
            }
            else
            {
                // if (!userPI.StoreFile("C:\\Users\\이왕석\\Docum ents\\카카오톡 받은 파일\\KakaoTalk_Video_20140629_2110_08_440.mp4"))

                // if (!userPI.StoreFile("C:\\Users\\이왕석\\Documents\\카카오톡 받은 파일\\a.txt"))
                if (!userPI.StoreFile("C:\\Users\\이왕석\\Documents\\카카오톡 받은 파일\\b.txt"))
                {
                    logger.Info("파일 전송에 실패하였습니다.");
                }

                //userPI.RetrieveFile(3, "C:\\Users\\이왕석\\a.png");

                //Console.Out.WriteLine(userPI.GetFileList("png"));
            }

            userPI.Logout();
        }

        /// <summary>
        /// 콘솔에서이 사용자 입력
        /// </summary>
        public class UserInput
        {
            public String Command { get; set; }
            public String Argument { get; set; }

            public UserInput(String cmd, String arg)
            {
                this.Command = cmd;
                this.Argument = arg;
            }

            public UserInput()
            {

            }
        }
    }
}
