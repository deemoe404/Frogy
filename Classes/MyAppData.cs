using Frogy.Methods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using System.IO;

using Frogy.Resources.Language;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;

namespace Frogy.Classes
{
    /// <summary>
    /// Interaction logic for AppData Class
    /// </summary>
    public class MyAppData
    {
        private DispatcherTimer mainLogicLoop = new DispatcherTimer() { Interval = new TimeSpan(0,0,1) };
        private DispatcherTimer savingLogicLoop = new DispatcherTimer() { Interval = new TimeSpan(0,1,0) };

        /// <summary>
        /// 所有时间线
        /// </summary>
        public Dictionary<DateTime, MyDay> AllDays = new Dictionary<DateTime, MyDay>();

        public void InitializeMyAppData()
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
                        ComputerStatus = 2 //software offline
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

            //如果设备状态不为1
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
                if (nowFocusProcess.Id == 0) 
                    return;

                string nowFocusProcessName = MyProcessHelper.GetProcessName(nowFocusProcess);
                if (string.IsNullOrEmpty(nowFocusProcessName)) 
                    return;

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
                todayTimeLine.Last().StopTime = now;

            AllDays[today].TimeLine = todayTimeLine;
        }

        /// <summary>
        /// App language
        /// </summary>
        private string languageSetting = LanguageHelper.PreferenceLanguage;
        public string LanguageSetting
        {
            get { return languageSetting; }
            set
            {
                //If dose not support input language code, try setting to UI language or default(English). 
                if (LanguageHelper.SupportedLanguage.ContainsKey(value)) 
                    languageSetting = value;
                else
                {
                    languageSetting = LanguageHelper.SupportedLanguage.ContainsKey(System.Globalization.CultureInfo.CurrentUICulture.Name) ?
                        System.Globalization.CultureInfo.CurrentUICulture.Name : LanguageHelper.SupportedLanguage.First().Key;
                }

                Properties.Settings.Default.Language = languageSetting;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// App theme
        /// </summary>
        private int themeSetting = (Properties.Settings.Default.Theme) >= 0 && (Properties.Settings.Default.Theme) <= 2 ?
            Properties.Settings.Default.Theme : 0;
        public int ThemeSetting
        {
            get { return themeSetting; }
            set
            {
                //If input out of range, ignore.
                if (value >= 0 && value <= 1)
                {
                    themeSetting = value;
                    Properties.Settings.Default.Theme = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string storagePath = Directory.Exists(Properties.Settings.Default.AppDataPath) ?
            Properties.Settings.Default.AppDataPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Frogy\\");
        public string StoragePath
        {
            get { return storagePath; }
            set
            {
                if (Directory.Exists(value))
                {
                    MyDataHelper.TransferFolder(storagePath, value);

                    storagePath = value;
                    Properties.Settings.Default.AppDataPath = value;
                    Properties.Settings.Default.Save();

                    Save();
                }
            }
        }

        public bool Saving { get; set; }
        public void Save()
        {
            Saving = true;

            Directory.CreateDirectory(storagePath);

            string savePath = Path.Combine(storagePath, DateTime.Today.ToString("yyyyMMdd") + ".json");
            string tempPath = Path.Combine(storagePath, DateTime.Today.ToString("yyyyMMdd") + "temp.json");
            string content = MyDataHelper.CoverObjectToJson(AllDays[DateTime.Today]);
            
            MyDataHelper.WriteFile(tempPath, content);
            MyDataHelper.RenameFile(tempPath, savePath);

            Saving = false;
        }

        public void Load(DateTime date)
        {
            if (!AllDays.ContainsKey(date))
            {
                string loadPath = Path.Combine(storagePath, date.ToString("yyyyMMdd") + ".json");
                if (File.Exists(loadPath))
                {
                    string json;
                    try
                    {
                        json = MyDataHelper.ReadFile(loadPath);
                        AllDays.Add(date, JsonConvert.DeserializeObject<MyDay>(json));
                    }
                    catch (Newtonsoft.Json.JsonReaderException)
                    {
                        ((App)Application.Current).ShowNotification(
                            LanguageHelper.InquireLocalizedWord("TaskBar_AppName"),
                            LanguageHelper.InquireLocalizedWord("System_ExcptionBadData") + date.ToString("yyyy/MM/dd"),
                            BalloonIcon.Error);

                        File.Delete(loadPath);
                        AllDays.Add(date, new MyDay());
                    }
                }
                else
                    AllDays.Add(date, new MyDay());
            }
        }

        public void StartLogic()
        {
            mainLogicLoop.Start();
            savingLogicLoop.Start();
        }

        public void StopLogic()
        {
            mainLogicLoop.Stop();
            savingLogicLoop.Stop();
        }
    }
}