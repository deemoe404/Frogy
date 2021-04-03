using Frogy.Classes;
using Frogy.Views;
using HandyControl.Tools;
using Hardcodet.Wpf.TaskbarNotification;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Security.Principal;
using Frogy.Resources.Language;

using Frogy.Methods;

namespace Frogy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MyAppData appData = new MyAppData();
        private TaskbarIcon taskbarIcon;
        private static Mutex mutex;

        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            SystemEvents.SessionEnded += SystemEvents_SessionEnded;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Launage switch
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri(@"Resources\Language\" + LanguageHelper.PreferenceLanguage + ".xaml", UriKind.Relative);
            ConfigHelper.Instance.SetLang(LanguageHelper.PreferenceLanguage == "zh-CN" ? "zh-cn" : "en");
            Current.Resources.MergedDictionaries.Add(dict);

            //theme switch
            ResourceDictionary themeDictionary = new ResourceDictionary();
            ResourceDictionary chartDictionary = new ResourceDictionary();
            switch (appData.ThemeSetting)
            {
                case 0:
                    themeDictionary.Source = new Uri(@"pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml", UriKind.Absolute);
                    chartDictionary.Source = new Uri(@"Resources\Theme\DefaultTheme.xaml", UriKind.Relative);
                    break;
                case 1:
                    themeDictionary.Source = new Uri(@"pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml", UriKind.Absolute);
                    chartDictionary.Source = new Uri(@"Resources\Theme\DarkTheme.xaml", UriKind.Relative);
                    break;
                default: //bright default
                    themeDictionary.Source = new Uri(@"pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml", UriKind.Absolute);
                    chartDictionary.Source = new Uri(@"Resources\Theme\DefaultTheme.xaml", UriKind.Relative);
                    break;
            }
            Current.Resources.MergedDictionaries.Add(themeDictionary);
            Current.Resources.MergedDictionaries.Add(chartDictionary);

            //tray icon
            mutex = new Mutex(true, "FrogyMainProgram");
            string startupArg = e.Args.Count() > 0 ? e.Args[0] : null;
            
            //if frogy not running
            if (mutex.WaitOne(0, false) || startupArg == "restart")
            {
                //Promote permission if not Administrator
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    MyDeviceHelper.PromotePermission();

                taskbarIcon = (TaskbarIcon)FindResource("icon");

                taskbarIcon.ShowBalloonTip(
                    dict["TaskBar_AppName"].ToString(),
                    dict["TaskBar_StartUp"].ToString(), 
                    BalloonIcon.Info);

                appData.StartLogic();

                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show(
                    dict["TaskBar_Overlap"].ToString(),
                    dict["TaskBar_AppName"].ToString(), 
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Environment.Exit(1);
            }
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Process Exit");
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(LanguageHelper.InquireLocalizedWord("System_ExcptionInfo") + e.Exception.Message + sender.ToString(), 
                LanguageHelper.InquireLocalizedWord("TaskBar_AppName"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(LanguageHelper.InquireLocalizedWord("System_ExcptionInfo") + e.ToString() + sender.ToString(),
                LanguageHelper.InquireLocalizedWord("TaskBar_AppName"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            appData.Save();
            SessionEndReasons reason = e.Reason;
            switch (reason)
            {
                case SessionEndReasons.Logoff:
                    break;
                case SessionEndReasons.SystemShutdown:
                    break;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            appData.Save();
            base.OnExit(e);
        }
    }
}
