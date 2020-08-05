using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

using Frogy.Methods;
using System.Collections;
using Frogy.Classes;

namespace Frogy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MyAppData taskList = new MyAppData();

        private readonly DispatcherTimer timer = new DispatcherTimer();

        //保存位置
        private static readonly string ApplicationData = System.IO.Path.GetTempPath() + "data.txt";

        public MainWindow()
        {
            InitializeComponent();

            //尝试读取已保存的数据
            try
            {
                taskList.Load(ApplicationData);
                if(!taskList.AppData_AllTimeLines.ContainsKey(DateTime.Today))
                    taskList.AppData_AllTimeLines.Add(DateTime.Today, new MyDay());
            }
            catch/*(Exception e)*/
            {
                //if (e.HResult == -2147024894)
                taskList.AppData_AllTimeLines.Add(DateTime.Today, new MyDay());
            }

            timer.Interval = new TimeSpan(10000000);
            timer.Tick += Timer_Tick;
            timer.Start();
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
                if (!string.IsNullOrEmpty(MyWindowHelper.GetWindowTitle(ptr)))
                    nowApplicationName_UWP = MyWindowHelper.GetWindowTitle(ptr);

            //构造MyFocusTask
            MyTask nowMyFocusTask =
                new MyTask()
                {
                    ApplicationName = nowIsUWP ? nowApplicationName_UWP : (nowFocusTaks.MainModule.FileVersionInfo.ProductName == "Microsoft® Windows® Operating System") ? nowFocusTaks.MainModule.FileVersionInfo.FileDescription: nowFocusTaks.MainModule.FileVersionInfo.ProductName,
                    ApplicationFilePath = nowFocusTaks.MainModule.FileName,
                    FormName = MyWindowHelper.GetWindowTitle(nowFoucsWindow),
                    IsApplicationUWP = nowIsUWP
                };

            //如果时间线为空 则直接添加现在的进程信息然后return
            if (((taskList.AppData_AllTimeLines[nowDate])).TimeLine.Count == 0)
            {
                ((taskList.AppData_AllTimeLines[nowDate])).TimeLine.Add(new MyTimeDuration() { 
                    StartTime = nowTimeSpan, 
                    TimeDurationTask = nowMyFocusTask });
                return;
            }

            //如果现在的窗口名与上一个不同，则认为切换了任务任务
            //同一个浏览器，切换窗口也认为是切换了任务
            bool a;
            ((taskList.AppData_AllTimeLines[nowDate])).TimeLine[((taskList.AppData_AllTimeLines[nowDate])).TimeLine.Count - 1].StopTime = nowTimeSpan;
            a = ((taskList.AppData_AllTimeLines[nowDate])).TimeLine.Last().TimeDurationTask.FormName.Equals(nowMyFocusTask.FormName);
            if (!a) 
                ((taskList.AppData_AllTimeLines[nowDate])).TimeLine.Add(new MyTimeDuration() { StartTime = nowTimeSpan, TimeDurationTask = nowMyFocusTask, StopTime = nowTimeSpan });


            //PrintList();
            PrintList_OverView(GetOverView(taskList.AppData_AllTimeLines[nowDate]));
            taskList.Save(ApplicationData);
        }

        Dictionary<string,TimeSpan> GetOverView(MyDay myDay)
        {
            Dictionary<string, TimeSpan> overView = new Dictionary<string, TimeSpan>();

            foreach(MyTimeDuration item in myDay.TimeLine)
            {
                string nowAppName = item.TimeDurationTask.ApplicationName;
                TimeSpan duration = item.StopTime - item.StartTime;
                if (overView.ContainsKey(nowAppName))
                    overView[nowAppName] += duration;
                else
                    overView.Add(nowAppName, duration);
            }

            return overView;
        }

        private void PrintList_OverView(Dictionary<string,TimeSpan> keyValuePairs)
        {
            test.Items.Clear();
            foreach(string key in keyValuePairs.Keys)
            {
                string result = key + "     " + keyValuePairs[key];
                test.Items.Add(result);
            }
        }

        private void PrintList()
        {
            //打印列表              
            test.Items.Clear();
            foreach (MyTimeDuration timeSpan in ((MyDay)(taskList.AppData_AllTimeLines[DateTime.Today])).TimeLine)
            {
                string result = timeSpan.TimeDurationTask.ApplicationName + "    " + timeSpan.StartTime + "    " + timeSpan.StopTime;
                test.Items.Add(result);
            }
        }

    }
}