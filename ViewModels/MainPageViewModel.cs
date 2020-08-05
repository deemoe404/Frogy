using Frogy.Methods;
using Frogy.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Frogy.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private MyAppData appData = new MyAppData();
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "//data.txt";
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// 加载App数据
        /// </summary>
        private void LoadAppData()
        {
            try
            {
                appData.Load(appDataPath);
                if (!appData.AppData_AllTimeLines.ContainsKey(DateTime.Today))
                    appData.AppData_AllTimeLines.Add(DateTime.Today, new MyDay());
            }
            catch{ appData.AppData_AllTimeLines.Add(DateTime.Today, new MyDay()); }
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

            //如果设备处于锁定状态，则不累加计数器
            if (MyDeviceHelper.DeviceState == 0)
            {
                appData.AppData_AllTimeLines[nowDate].TimeLine[appData.AppData_AllTimeLines[nowDate].TimeLine.Count - 1].StopTime = nowTimeSpan;
                bool b = appData.AppData_AllTimeLines[nowDate].TimeLine.Last().TimeDurationTask.ApplicationName== "Computer Locked";

                if(!b)
                    appData.AppData_AllTimeLines[nowDate].TimeLine.Add(new MyTimeDuration()
                    {
                        StartTime = nowTimeSpan,
                        TimeDurationTask = new MyTask() { ApplicationName="Computer Locked"},
                        StopTime = nowTimeSpan
                    });

                return;
            }


            //获取焦点窗口
            IntPtr nowFoucsWindow = MyProcessHelper.GetFocueWindow();
            //获取焦点窗口对应进程 如果获取进程失败，Return
            Process nowFocusTaks;
            try
            {
                nowFocusTaks = MyProcessHelper.GetWindowPID(nowFoucsWindow);
            }
            catch { return; }

            //通过FileDescription判断进程是否为UWP
            bool nowIsUWP;
            try
            {
                nowIsUWP = nowFocusTaks.MainModule.FileVersionInfo.FileDescription == "Application Frame Host";
            }
            catch { return; }

            //如果是UWP进程，那么枚举所有子窗口来获得程序名（猜测x）
            string nowApplicationName_UWP = "";
            List<IntPtr> allChildWindows = new MyWindowHelper(nowFocusTaks.MainWindowHandle).GetAllChildHandles();
            foreach (IntPtr ptr in allChildWindows)
                if (!string.IsNullOrEmpty(MyWindowHelper.GetWindowTitle(ptr)) && MyWindowHelper.GetWindowTitle(ptr) != "CoreInput")
                    nowApplicationName_UWP = MyWindowHelper.GetWindowTitle(ptr);

            //构造MyFocusTask
            MyTask nowMyFocusTask =
                new MyTask()
                {
                    ApplicationName = nowIsUWP ? nowApplicationName_UWP : (nowFocusTaks.MainModule.FileVersionInfo.ProductName == "Microsoft® Windows® Operating System") ? nowFocusTaks.MainModule.FileVersionInfo.FileDescription : string.IsNullOrEmpty(nowFocusTaks.MainModule.FileVersionInfo.ProductName) ? nowFocusTaks.ProcessName : nowFocusTaks.MainModule.FileVersionInfo.ProductName,
                    ApplicationFilePath = nowFocusTaks.MainModule.FileName,
                    FormName = MyWindowHelper.GetWindowTitle(nowFoucsWindow),
                    IsApplicationUWP = nowIsUWP
                };

            //如果时间线为空 则直接添加现在的进程信息然后return
            if (((appData.AppData_AllTimeLines[nowDate])).TimeLine.Count == 0)
            {
                ((appData.AppData_AllTimeLines[nowDate])).TimeLine.Add(new MyTimeDuration()
                {
                    StartTime = nowTimeSpan,
                    TimeDurationTask = nowMyFocusTask
                });
                return;
            }

            //如果现在的窗口名与上一个不同，则认为切换了任务任务
            //同一个浏览器，切换窗口也认为是切换了任务
            bool a;
            ((appData.AppData_AllTimeLines[nowDate])).TimeLine[((appData.AppData_AllTimeLines[nowDate])).TimeLine.Count - 1].StopTime = nowTimeSpan;
            a = ((appData.AppData_AllTimeLines[nowDate])).TimeLine.Last().TimeDurationTask.FormName == nowMyFocusTask.FormName;
            if (!a)
                ((appData.AppData_AllTimeLines[nowDate])).TimeLine.Add(new MyTimeDuration() { StartTime = nowTimeSpan, TimeDurationTask = nowMyFocusTask, StopTime = nowTimeSpan });


            //PrintList();
            //PrintList_OverView(taskList.AppData_AllTimeLines[nowDate].GetOverView());
            //taskList.Save(ApplicationData);
            Overview = PrintOverview(appData.AppData_AllTimeLines[DateTime.Today].OverView);
            SourceData = PrintSourceData(appData.AppData_AllTimeLines[DateTime.Today].TimeLine);
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
                string tmp = timeSpan.TimeDurationTask.ApplicationName + "    " + timeSpan.TimeDurationTask.FormName + "    " + timeSpan.StartTime + "    " + timeSpan.StopTime;
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
