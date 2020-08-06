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
            TimeSpan nowTimeSpan = new TimeSpan(
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                DateTime.Now.Second);

            DateTime nowDate = DateTime.Today;

            if (!appData.AllDays.ContainsKey(nowDate)) appData.AllDays.Add(nowDate, new MyDay());
            List<MyTimeDuration> todayTimeLine = appData.AllDays[nowDate].TimeLine;

            //如果设备处于锁定状态
            if (MyDeviceHelper.DeviceState == 0)
            {
                todayTimeLine.Last().StopTime = nowTimeSpan;
                if (!(todayTimeLine.Last().TimeDurationTask.ComputerStatus == 0))
                    todayTimeLine.Add(new MyTimeDuration()
                    {
                        StartTime = nowTimeSpan,
                        TimeDurationTask = new MyTask() { ComputerStatus = 0 },
                        StopTime = nowTimeSpan
                    });
                return;
            }

            IntPtr nowFoucsWindow = MyWindowHelper.GetFocueWindow();
            string nowFoucsWindowTitle = MyWindowHelper.GetWindowTitle(nowFoucsWindow);

            if (todayTimeLine.Count == 0 || todayTimeLine.Last().TimeDurationTask.FormName != nowFoucsWindowTitle)
            {
                Process nowFocusProcess = MyProcessHelper.GetWindowPID(nowFoucsWindow);

                if (nowFocusProcess.Id == 0) return;

                string nowFocusProcessName = MyProcessHelper.GetProcessName(nowFocusProcess);

                if (string.IsNullOrEmpty(nowFocusProcessName)) return;

                MyTask nowFocusTask =
                    new MyTask()
                    {
                        ApplicationName = nowFocusProcessName,
                        ApplicationFilePath = MyProcessHelper.GetProcessPath(nowFocusProcess),
                        FormName = MyWindowHelper.GetWindowTitle(nowFoucsWindow)
                    };

                MyTimeDuration nowTimeDuration =
                    new MyTimeDuration()
                    {
                        StartTime = nowTimeSpan,
                        StopTime = nowTimeSpan,
                        TimeDurationTask = nowFocusTask
                    };

                if (todayTimeLine.Count != 0)
                    todayTimeLine.Last().StopTime = nowTimeSpan;

                todayTimeLine.Add(nowTimeDuration);

            }
            else
            {
                todayTimeLine.Last().StopTime = nowTimeSpan;
            }

            appData.AllDays[nowDate].TimeLine = todayTimeLine;
            Overview = PrintOverview(appData.AllDays[DateTime.Today].OverView);
            //SourceData = PrintSourceData(appData.AllDays[DateTime.Today].TimeLine);
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
                string tmp = timeSpan.TimeDurationTask.ComputerStatus.ToString() + "  " + 
                    timeSpan.TimeDurationTask.ApplicationName + "    " + 
                    timeSpan.TimeDurationTask.FormName + "    " + 
                    timeSpan.StartTime + "    " + 
                    timeSpan.StopTime + "    " + 
                    timeSpan.TimeDurationTask.ApplicationFilePath;
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
