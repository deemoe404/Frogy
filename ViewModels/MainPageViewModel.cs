using Frogy.Methods;
using Frogy.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Frogy.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private MyAppData appData = new MyAppData();

        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer saver = new DispatcherTimer();

        /// <summary>
        /// 加载App数据
        /// </summary>
        private void LoadAppData()
        {
            try
            {
                appData.Load(appDataPath);
                if (!appData.AllDays.ContainsKey(DateTime.Today))
                    appData.AllDays.Add(DateTime.Today, new MyDay());
            }
            catch{ appData.AllDays.Add(DateTime.Today, new MyDay()); }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //当前时间
            TimeSpan nowTimeSpan = new TimeSpan(
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                DateTime.Now.Second);

            //当前日期 用于从字典中读取当日时间线
            DateTime nowDate = DateTime.Today;

            //今日时间线
            List<MyTimeDuration> todayTimeLine = appData.AllDays[nowDate].TimeLine;

            //如果设备处于锁定状态
            if (MyDeviceHelper.DeviceState == 0)
            {
                todayTimeLine.Last().StopTime = nowTimeSpan;
                //如果上一次记录是设备未处于锁定状态
                if (!(todayTimeLine.Last().TimeDurationTask.ComputerStatus == 0))
                    todayTimeLine.Add(new MyTimeDuration()
                    {
                        StartTime = nowTimeSpan,
                        TimeDurationTask = new MyTask() { ComputerStatus = 0 },
                        StopTime = nowTimeSpan
                    });
                return;
            }

            //获取焦点窗口
            IntPtr nowFoucsWindow = MyProcessHelper.GetFocueWindow();

            //获取焦点窗口对应进程
            //通过FileDescription判断进程是否为UWP
            Process nowFocusTaks;
            bool nowIsUWP;
            try
            {
                nowFocusTaks = MyProcessHelper.GetWindowPID(nowFoucsWindow);
                nowIsUWP = nowFocusTaks.MainModule.FileVersionInfo.FileDescription == "Application Frame Host";
            }
            catch { return; }

            string nowApplicationName_UWP = "";
            if (nowIsUWP)
            {
                try
                {
                    List<IntPtr> allChildWindows = new MyWindowHelper(nowFocusTaks.MainWindowHandle).GetAllChildHandles();
                    foreach (IntPtr ptr in allChildWindows)
                    {
                        Process uwpProcess = MyProcessHelper.GetWindowPID(ptr);
                        uwpProcess.WaitForInputIdle();
                        if (uwpProcess.MainModule.ModuleName != "ApplicationFrameHost.exe")
                        {
                            nowApplicationName_UWP = MyAppxPackageHelper.GetAppDisplayNameFromProcess(uwpProcess);
                        }
                    }
                    if (string.IsNullOrEmpty(nowApplicationName_UWP)) return;
                }
                catch { return; }
            }

            //构造MyFocusTask
            MyTask nowMyFocusTask =
                new MyTask()
                {
                    ApplicationName = 
                    nowIsUWP ? 
                    nowApplicationName_UWP : nowFocusTaks.MainModule.FileVersionInfo.FileDescription,
                    
                    ApplicationFilePath = nowFocusTaks.MainModule.FileName,
                    FormName = MyWindowHelper.GetWindowTitle(nowFoucsWindow),
                    IsApplicationUWP = nowIsUWP
                };

            //如果时间线为空 则直接添加现在的进程信息然后return
            if (todayTimeLine.Count == 0)
            {
                todayTimeLine.Add(new MyTimeDuration()
                {
                    StartTime = nowTimeSpan,
                    StopTime = nowTimeSpan,
                    TimeDurationTask = nowMyFocusTask
                });
                return;
            }

            //如果现在的窗口名与上一个不同，则认为切换了任务任务
            //同一个浏览器，切换窗口也认为是切换了任务
            todayTimeLine.Last().StopTime = nowTimeSpan;
            if (todayTimeLine.Last().TimeDurationTask.FormName != nowMyFocusTask.FormName)
                todayTimeLine.Add(new MyTimeDuration() 
                { 
                    StartTime = nowTimeSpan, 
                    TimeDurationTask = nowMyFocusTask, 
                    StopTime = nowTimeSpan 
                });

            appData.AllDays[nowDate].TimeLine = todayTimeLine;
            Overview = PrintOverview(appData.AllDays[DateTime.Today].OverView);
            SourceData = PrintSourceData(appData.AllDays[DateTime.Today].TimeLine);
        }

        private void Saver_Tick(object sender, EventArgs e)
        {
            appData.Save(appDataPath);
        }

        /// <summary>
        /// 按顺序打印概览视图
        /// </summary>
        /// <param name="keyValuePairs">Overview</param>
        /// <returns>List<string></returns>
        private List<string> PrintOverview(Dictionary<string, TimeSpan> keyValuePairs)
        {
            List<string> result = new List<string>();
            var dicSort = from objDic in keyValuePairs orderby objDic.Value descending select objDic;

            foreach (KeyValuePair<string, TimeSpan> kvp in dicSort)
            {
                string tmp = kvp.Key + "：" + kvp.Value;
                result.Add(tmp);
            }
            return result;
        }

        /// <summary>
        /// 打印元数据
        /// </summary>
        /// <param name="durations"></param>
        /// <returns></returns>
        private List<string> PrintSourceData(List<MyTimeDuration> durations)
        {
            List<string> result = new List<string>();
            foreach (MyTimeDuration timeSpan in durations)
            {
                string tmp = timeSpan.TimeDurationTask.ComputerStatus.ToString() + "  " + timeSpan.TimeDurationTask.ApplicationName + "    " + timeSpan.TimeDurationTask.FormName + "    " + timeSpan.StartTime + "    " + timeSpan.StopTime;
                result.Add(tmp);
            }
            return result;
        }

        public MainPageViewModel()
        {
            LoadAppData();

            timer.Interval = new TimeSpan(10000000);
            timer.Tick += Timer_Tick;
            timer.Start();

            saver.Interval = new TimeSpan(600000000);
            saver.Tick += Saver_Tick;
            saver.Start();
        }

        /// <summary>
        /// 整体视图
        /// </summary>
        private List<string> overview;
        public List<string> Overview 
        { 
            get
            {
                return overview;
            }
            set
            {
                overview = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 元数据
        /// </summary>
        private List<string> sourceData;
        public List<string> SourceData
        {
            get
            {
                return sourceData;
            }
            set
            {
                sourceData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 应用数据路径
        /// </summary>
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//data.txt";
        public string AppDataPath
        {
            get
            {
                return appDataPath;
            }
            set
            {
                appDataPath = value;
                OnPropertyChanged();
            }
        }

        public void MainPage_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            saver.Stop();

            appData.Save(appDataPath);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
