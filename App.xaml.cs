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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

namespace Frogy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MyAppData appData = new MyAppData();
        public MyAppData AppData
        {
            get { return appData; }
        }
        
        private TaskbarIcon taskbarIcon;
        private static Mutex mutex;

        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            appData.StopLogic();

            if (appData.Saving)
                e.Cancel = true;
            else
                Environment.Exit(0);
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

                ShowNotification(
                    LanguageHelper.InquireLocalizedWord("TaskBar_AppName"),
                    LanguageHelper.InquireLocalizedWord("TaskBar_StartUp"),
                    BalloonIcon.Info);

                appData.InitializeMyAppData();
                appData.StartLogic();

                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show(
                    LanguageHelper.InquireLocalizedWord("TaskBar_Overlap"),
                    LanguageHelper.InquireLocalizedWord("TaskBar_AppName"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Environment.Exit(1);
            }
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {

        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(LanguageHelper.InquireLocalizedWord("System_ExcptionInfo") + e.Exception.Message + sender.ToString(), 
                LanguageHelper.InquireLocalizedWord("TaskBar_AppName"), 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);

            Environment.Exit(1);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit -= new EventHandler(CurrentDomain_ProcessExit);

            appData.Save();
            base.OnExit(e);
        }

        public void ShowNotification(string title, string content, BalloonIcon icon)
        {
            taskbarIcon.ShowBalloonTip(
                title,
                content,
                icon);
        }
    }
}
