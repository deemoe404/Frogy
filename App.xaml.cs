using Frogy.Classes;
using Frogy.Views;
using Hardcodet.Wpf.TaskbarNotification;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Frogy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon taskbarIcon;
        private static System.Threading.Mutex mutex;

        public MyAppData appData = new MyAppData();

        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "FrogyMainProgram");
            if (mutex.WaitOne(0, false))
            {
                taskbarIcon = (TaskbarIcon)FindResource("icon");
                taskbarIcon.ShowBalloonTip("Frogy MainProgram", "Frogy is now running. Enjoy your time!", BalloonIcon.Info);

                appData.StartLogic();

                base.OnStartup(e);
            }
            else
            {
                new TaskbarIcon().ShowBalloonTip("Frogy MainProgram", "Frogy is already running. Go check your taskbar icon!", BalloonIcon.Warning);
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            appData.Save();
            base.OnExit(e);
        }
    }
}
