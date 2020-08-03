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

namespace Frogy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MyTimeLine taskList = new MyTimeLine();

        private readonly DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = new TimeSpan(10000000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan nowTimeSpan = new TimeSpan(
                DateTime.Now.Hour,
                DateTime.Now.Minute,
                DateTime.Now.Second);

            IntPtr nowFoucsWindow = MyProcessHelper.GetFocueWindow();
            Process nowFocusTaks = MyProcessHelper.GetWindowPID(nowFoucsWindow);
            bool nowIsUWP = nowFocusTaks.MainModule.FileVersionInfo.ProductName == "Microsoft® Windows® Operating System";
            List<IntPtr> allChildWindows = new MyWindowHelper(nowFocusTaks.MainWindowHandle).GetAllChildHandles();

            MyFocusTask nowMyFocusTask =
                new MyFocusTask()
                {
                    ApplicationName = nowIsUWP ? MyWindowHelper.GetWindowTitle(allChildWindows[2]) : nowFocusTaks.MainModule.FileVersionInfo.ProductName,
                    ApplicationFilePath = nowFocusTaks.MainModule.FileName,
                    FormName = MyWindowHelper.GetWindowTitle(nowFoucsWindow),
                    IsApplicationUWP = nowIsUWP
                };

            //如果时间线为空 则直接添加现在的进程信息然后return
            if (taskList.MyDay.Count == 0)
            {
                taskList.MyDay.Add(new MyTimeSpan() { StartTime = nowTimeSpan, TimeSpanTask = nowMyFocusTask });
                return;
            }

            //如果现在的窗口窗口名一个不同，则认为切换了任务任务

            //同一个浏览器，切换窗口也认为是切换了任务
            bool a;
            a = taskList.MyDay.Last().TimeSpanTask.FormName.Equals(nowMyFocusTask.FormName);
            if (!a)
            {
                taskList.MyDay[taskList.MyDay.Count - 1].StopTime = nowTimeSpan;
                taskList.MyDay.Add(new MyTimeSpan() { StartTime = nowTimeSpan, TimeSpanTask = nowMyFocusTask });
            }


            PrintList();
        }

        private void PrintList()
        {
            //打印列表              
            test.Items.Clear();
            foreach (MyTimeSpan timeSpan in taskList.MyDay)
            {
                string result = timeSpan.TimeSpanTask.ApplicationName + "    " + timeSpan.StartTime + "    " + timeSpan.StopTime;
                test.Items.Add(result);
            }
        }

    }
}
