﻿using Frogy.Classes;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            //Launage switch
            ResourceDictionary dict = new ResourceDictionary();
            switch (appData.LanguageSetting)
            {
                case "en-US":
                    dict.Source = new Uri(@"Resources\Language\en-US.xaml", UriKind.Relative);
                    ConfigHelper.Instance.SetLang("en");
                    break;
                case "zh-CN":
                    dict.Source = new Uri(@"Resources\Language\zh-CN.xaml", UriKind.Relative);
                    ConfigHelper.Instance.SetLang("zh-cn");
                    break;
                default: //english default
                    dict.Source = new Uri(@"Resources\Language\en-US.xaml", UriKind.Relative);
                    break;
            }
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
            if (mutex.WaitOne(0, false))
            {
                taskbarIcon = (TaskbarIcon)FindResource("icon");

                taskbarIcon.ShowBalloonTip(
                    dict["TaskBar_AppName"].ToString(),
                    dict["TaskBar_StartUp"].ToString(), 
                    BalloonIcon.Info);

                appData.StartLogic();

                SystemEvents.SessionEnded += SystemEvents_SessionEnded;

                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show(
                    dict["TaskBar_Overlap"].ToString(),
                    dict["TaskBar_AppName"].ToString(), 
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Current.Shutdown();
            }
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
