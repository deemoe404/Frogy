using Frogy.Methods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Threading;

namespace Frogy.Classes
{
    /// <summary>
    /// Interaction logic for AppData Class
    /// </summary>
    public class MyAppData
    {
        private DispatcherTimer mainLogicLoop = new DispatcherTimer() { Interval = new TimeSpan(10000000) };
        private DispatcherTimer savingLogicLoop = new DispatcherTimer() { Interval = new TimeSpan(600000000) };

        /// <summary>
        /// 所有时间线
        /// </summary>
        public Dictionary<DateTime, MyDay> AllDays = new Dictionary<DateTime, MyDay>();

        public MyAppData()
        {
            DateTime today = DateTime.Today;

            //默认退出时间为今天上午0点，启动时间为现在
            TimeSpan ExitTime = new TimeSpan(0, 0, 0),
                LoginTime = new TimeSpan(
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                DateTime.Now.Second
                ); ;

            Load(today);

            //如果时间线不为空，则认为软件是从最后一个软件stop退出的
            //否则认为今日0点退出
            if (AllDays[today].TimeLine.Count != 0)
                ExitTime = AllDays[today].TimeLine.Last().StopTime;

            //计算软件离线时间
            AllDays[today].TimeLine.Add(
                new MyTimeDuration()
                {
                    StartTime = ExitTime,
                    StopTime = LoginTime,
                    TimeDurationTask =
                    new MyTask()
                    {
                        ComputerStatus = 2
                    }
                });


            mainLogicLoop.Tick += MainLogicLoop_Tick;
            savingLogicLoop.Tick += SavingLogicLoop_Tick;
        }

        /// <summary>
        /// 定时保存应用数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavingLogicLoop_Tick(object sender, EventArgs e)
        {
            Save();
        }

        /// <summary>
        /// 主逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainLogicLoop_Tick(object sender, EventArgs e)
        {
            TimeSpan now = new TimeSpan(
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                DateTime.Now.Second);

            DateTime today = DateTime.Today;

            //运行时遇到日期变更，则增加新key
            if (!AllDays.ContainsKey(today)) 
                AllDays.Add(today, new MyDay());

            List<MyTimeDuration> todayTimeLine = AllDays[today].TimeLine;

            //如果设备状态不为1（运行中）
            if (MyDeviceHelper.DeviceState != 1)
            {
                if (todayTimeLine.Count == 0 || todayTimeLine.Last().TimeDurationTask.ComputerStatus != MyDeviceHelper.DeviceState)
                {
                    MyTimeDuration duration = new MyTimeDuration()
                    {
                        StartTime = now,
                        TimeDurationTask = new MyTask() { ComputerStatus = MyDeviceHelper.DeviceState },
                        StopTime = now
                    };

                    todayTimeLine.Add(duration);
                }
                else
                {
                    todayTimeLine.Last().StopTime = now;
                }
                AllDays[today].TimeLine = todayTimeLine;
                return;
            }

            IntPtr nowFocusWindow = MyWindowHelper.GetFocusWindow();
            string nowFocusWindowTitle = MyWindowHelper.GetWindowTitle(nowFocusWindow);

            //如果今天还未记录到任务 或 切换了任务
            if (todayTimeLine.Count == 0 || todayTimeLine.Last().TimeDurationTask.ApplicationTitle != nowFocusWindowTitle)
            {
                Process nowFocusProcess = MyProcessHelper.GetWindowPID(nowFocusWindow);
                if (nowFocusProcess.Id == 0) return;

                string nowFocusProcessName = MyProcessHelper.GetProcessName(nowFocusProcess);
                if (string.IsNullOrEmpty(nowFocusProcessName)) return;

                MyTask nowFocusTask =
                    new MyTask()
                    {
                        ApplicationName = nowFocusProcessName,
                        ApplicationFilePath = MyProcessHelper.GetProcessPath(nowFocusProcess),
                        ApplicationTitle = MyWindowHelper.GetWindowTitle(nowFocusWindow),
                        ApplicationIcon_Base64 = MyDataHelper.ImgToBase64String(MyProcessHelper.GetProcessIcon(nowFocusProcess))
                    };

                MyTimeDuration nowTimeDuration =
                    new MyTimeDuration()
                    {
                        StartTime = now,
                        StopTime = now,
                        TimeDurationTask = nowFocusTask
                    };

                if (todayTimeLine.Count != 0)
                    todayTimeLine.Last().StopTime = now;

                todayTimeLine.Add(nowTimeDuration);
            }
            else
            {
                todayTimeLine.Last().StopTime = now;
            }

            AllDays[today].TimeLine = todayTimeLine;
        }

        /// <summary>
        /// 应用数据存储路径
        /// </summary>
        private string storagePath = System.IO.Directory.Exists(Properties.Settings.Default.AppDataPath) ?
            Properties.Settings.Default.AppDataPath : System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Frogy\\");
        public string StoragePath
        {
            get { return storagePath; }
            set
            {
                if (System.IO.Directory.Exists(value))
                {
                    MyDataHelper.TransferFolder(storagePath, value);

                    storagePath = value;
                    Properties.Settings.Default.AppDataPath = value;
                    Properties.Settings.Default.Save();

                    Save();
                }
                else
                {
                    throw new ArgumentException("Illegal path");
                }
            }
        }

        /// <summary>
        /// 保存应用数据
        /// </summary>
        public void Save()
        {
            System.IO.Directory.CreateDirectory(storagePath);

            string savePath = System.IO.Path.Combine(storagePath, DateTime.Today.ToString("yyyyMMdd") + ".json");

            //string savePath = storagePath + (storagePath.EndsWith("\\") ? "" : "\\") + DateTime.Today.ToString("yyyyMMdd") + ".json";
            string Content = MyDataHelper.CoverObjectToJson(AllDays[DateTime.Today]);
            try
            {
                MyDataHelper.WriteFile(savePath, Content);
            }
            catch(Exception e) { throw e; }
        }

        /// <summary>
        /// 加载应用数据
        /// </summary>
        /// <param name="LoadDate">日期</param>
        public void Load(DateTime LoadDate)
        {
            //加载时间线数据
            try
            {
                string loadPath = StoragePath + (StoragePath.EndsWith("\\") ? "" : "\\") + LoadDate.ToString("yyyyMMdd") + ".json";
                if (!AllDays.ContainsKey(LoadDate))
                {
                    string Json = MyDataHelper.ReadFile(loadPath);
                    AllDays.Add(LoadDate, MyDataHelper.CoverJsonToObject<MyDay>(Json));
                }
            }
            catch
            {
                if (!AllDays.ContainsKey(LoadDate))
                    AllDays.Add(LoadDate, new MyDay());
            }
            //加载App计时器数据
            //if(AllDays[LoadDate].)
        }

        /// <summary>
        /// 开始运行软件主逻辑
        /// </summary>
        public void StartLogic()
        {
            mainLogicLoop.Start();
            savingLogicLoop.Start();
        }
    }
}
