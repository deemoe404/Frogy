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

        protected override void OnStartup(StartupEventArgs e)
        {
            taskbarIcon = (TaskbarIcon)FindResource("icon");
            taskbarIcon.ShowBalloonTip("Frogy MainProgram", "Frogy is now running. Enjoy your time!", BalloonIcon.Info);
            base.OnStartup(e);
        }
    }

    public class NotifyIconViewModel
    {
        /// <summary>
        /// 如果窗口没显示，就显示窗口
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => 
                    {
                        try
                        {
                            return Application.Current.MainWindow.Visibility == Visibility.Hidden;
                        }
                        catch
                        {
                            return false;
                        }
                    },
                    CommandAction = () =>
                    {
                        //Application.Current.MainWindow = new MainPage();
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Hide(),
                    CanExecuteFunc = () =>
                    {
                        try
                        {
                            return Application.Current.MainWindow.Visibility == Visibility.Visible;
                        }
                        catch
                        {
                            return false;
                        }
                    },
                };
            }
        }


        /// <summary>
        /// 关闭软件
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => 
                {
                    Application.Current.MainWindow.Close();
                    Application.Current.Shutdown();
                } };
            }
        }
    }

    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}
