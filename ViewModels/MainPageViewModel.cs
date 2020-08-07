using Frogy.Methods;
using Frogy.Classes;
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
using Frogy.Views;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Interop;
using System.Windows.Input;

namespace Frogy.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(10000000) };

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
            DataPath = ((App)Application.Current).appData.StoragePath;

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!((App)Application.Current).appData.AllDays.ContainsKey(displayDate))
                ((App)Application.Current).appData.Load(displayDate);

            Overview = PrintOverview(((App)Application.Current).appData.AllDays[displayDate].OverView);
            SourceData = PrintSourceData(((App)Application.Current).appData.AllDays[displayDate].TimeLine);
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
        /// 显示数据日期
        /// </summary>
        private DateTime displayDate = DateTime.Today;
        public DateTime DisplayDate
        {
            get
            {
                return displayDate;
            }
            set
            {
                displayDate = value;
                OnPropertyChanged();
            }
        }

        private string dataPath = "";
        public string DataPath
        {
            get
            {
                return dataPath;
            }
            set
            {
                dataPath = value;
                OnPropertyChanged();
            }
        }

        private ICommand button_Click;
        public ICommand Button_Click
        {
            get
            {
                if (button_Click == null)
                {
                    button_Click = new RelayCommand(
                        param => this.ChangeDataPath(),
                        param => true
                    );
                }
                return button_Click;
            }
        }

        private void ChangeDataPath()
        {
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    ((App)Application.Current).appData.StoragePath = dialog.FileName;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            DataPath = ((App)Application.Current).appData.StoragePath;
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
