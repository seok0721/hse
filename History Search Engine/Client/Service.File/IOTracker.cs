using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Service.File
{
    delegate void OnChangedHandler(object source, FileSystemEventArgs e);
    delegate void OnRenamedHandler(object source, RenamedEventArgs e);

    class IOTracker
    {
        /// <summary>
        /// 쓰레드 종료용 boolean.
        /// </summary>
        private volatile bool mStopThread;

        /// <summary>
        /// 감시하는 경로.
        /// </summary>
        private volatile string mPath = null;


        /// <summary>
        /// 파일 IO 감시자들 담는 딕셔너리.
        /// </summary>
        private volatile Dictionary<string, FileSystemWatcher> watchers;



        private OnChangedHandler mOnChanged;
        private OnRenamedHandler mOnRenamed;


        /// <summary>
        /// 파일 경로와 함께 IOTracker 초기화.
        /// </summary>
        /// <param name="path"> 감시할 디렉토리 경로. </param>
        /// 
        public IOTracker(String path, OnChangedHandler onChanged, OnRenamedHandler onRenamed)
        {
            mOnChanged = onChanged;
            mOnRenamed = onRenamed;
            mPath = path;
            watchers = new Dictionary<string, FileSystemWatcher>();
        }



        /// <summary>
        /// 감시할 파일 타입을 추가합니다.
        /// </summary>
        /// <param name="fileType">파일 타입 (예:txt)</param>
        public void AddFileType(string fileType,
            OnChangedHandler onChangeHandler,
            OnRenamedHandler onRenamedHandler)
        {
            // 새 파일 감시자를 만듬. 한 파일 감시자는 한가지의 파일타입만 감시가능하기에...
            FileSystemWatcher newWatcher = new FileSystemWatcher();

            //경로 설정.
            newWatcher.Path = mPath;
            //이벤트 필터.
            newWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            newWatcher.Filter = "*." + fileType;

            //하위 경로 포함.
            newWatcher.IncludeSubdirectories = true;

            //이벤트 핸들러 추가.
            newWatcher.Changed += new FileSystemEventHandler(onChangeHandler);
            newWatcher.Created += new FileSystemEventHandler(onChangeHandler);
            newWatcher.Deleted += new FileSystemEventHandler(onChangeHandler);
            newWatcher.Renamed += new RenamedEventHandler(onRenamedHandler);

            //딕셔너리에 파일감시자 추가.
            watchers.Add(fileType, newWatcher);
        }

        /// <summary>
        /// 현재 감시중인 파일 타입들의 스트링 배열을 얻습니다.
        /// </summary>
        /// <returns>파일 타입 스트링 배열</returns>
        public String[] getFileTypeList()
        {
            return watchers.Keys.ToArray<string>();
        }

        /// <summary>
        /// 특정 파일 타입의 감시를 종료합니다.
        /// </summary>
        /// <param name="fileType">파일 타입 (예: txt)</param>
        /// <returns>성공하면 true 실패하면 false</returns>
        public bool removeFileType(string fileType)
        {
            watchers[fileType].EnableRaisingEvents = false;
            return watchers.Remove(fileType);
        }

        /// <summary>
        /// 파일 입출력 감시를 시작합니다.
        /// </summary>
        public void StartWatch()
        {
            if (watchers.Count == 0)
            {
                throw new System.Exception("지정된 확장자가 전혀 없습니다.");
            }

            foreach (var fsw in watchers)
            {
                fsw.Value.EnableRaisingEvents = true;
            }
            Thread thread = new Thread(Run);
            thread.Start();
        }

        /// <summary>
        /// 파일 입출력 감시를 종료합니다.
        /// </summary>
        public void StopWatch()
        {
            Console.Out.WriteLine("Stop called!");
            foreach (var fsw in watchers)
            {
                fsw.Value.EnableRaisingEvents = false;
            }
            mStopThread = true;
        }

        /// <summary>
        /// 파일 아이오 감시하는 쓰레드 실행부.
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Run()
        {
            while (!mStopThread) ;
            mStopThread = false;
        }

    }
}
