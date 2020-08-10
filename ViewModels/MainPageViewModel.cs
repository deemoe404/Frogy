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
using Frogy.Models;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace Frogy.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 按顺序打印概览视图
        /// </summary>
        /// <param name="keyValuePairs">Overview</param>
        /// <returns>List<string></returns>
        private List<OverViewItem> PrintOverview(Dictionary<string, Software> keyValuePairs)
        {
            List<OverViewItem> result = new List<OverViewItem>();
            var dicSort = from objDic in keyValuePairs orderby objDic.Value.Duration descending select objDic;

            foreach (KeyValuePair<string, Software> kvp in dicSort)
            {
                OverViewItem tmp = 
                    new OverViewItem() 
                    {
                        AppName=kvp.Key,
                        AppDuration=kvp.Value.Duration.ToString(),
                        AppIcon=kvp.Value.Icon 
                    };
                result.Add(tmp);
            }
            return result;
        }

        /// <summary>
        /// 打印表格
        /// </summary>
        /// <param name="mies"></param>
        private SeriesCollection PrintOverviewChart(List<MyTimeDuration> tmp)
        {
            SeriesCollection result = new SeriesCollection { };
            Dictionary<string, ChartValues<double>> tmpdic = new Dictionary<string, ChartValues<double>>();
            foreach (MyTimeDuration duration in tmp)
            {
                string appName = duration.TimeDurationTask.ApplicationName;
                if (string.IsNullOrEmpty(appName) || appName == "Frogy") continue;

                if (!tmpdic.ContainsKey(appName)) tmpdic.Add(appName,
                    new ChartValues<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                TimeSpan start = duration.StartTime;
                TimeSpan stop = duration.StopTime;
                TimeSpan spent = duration.Duration;

                if (start.Hours == stop.Hours)
                {
                    tmpdic[appName][start.Hours] += Math.Round(spent.TotalMinutes, 2);
                }
                else
                {
                    if (stop.Hours - start.Hours == 1)
                    {
                        tmpdic[appName][start.Hours] += Math.Round((59.59d - start.Minutes), 2);
                        tmpdic[appName][stop.Hours] += stop.Minutes;
                    }
                    else
                    {
                        if (stop.Hours - start.Hours > 1)
                        {
                            int tmpint = stop.Hours - start.Hours;

                            tmpdic[appName][start.Hours] += Math.Round((59.59d - start.Minutes), 2);
                            tmpdic[appName][stop.Hours] += stop.Minutes;

                            for (int i = 1; i <= tmpint; i++)
                            {
                                tmpdic[appName][start.Hours + i] = 59.59d;
                            }
                        }
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (string key in tmpdic.Keys)
                {
                    result.Add(new StackedColumnSeries
                    {
                        Values = tmpdic[key],
                        DataLabels = true,
                        Title = key,
                        IsHitTestVisible = false,
                        FontSize = 0.1
                    });
                }
            });

            return result;
        }

        public MainPageViewModel()
        {
            Update();

            OverviewChartLables = new string[] { "0:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00",
            "7:00", "8:00", "9:00", "10:00", "11:00", "12:00",
            "13:00", "14:00", "15:00", "16:00", "17:00", "18:00",
            "19:00", "20:00", "21:00", "22:00", "23:00"};
            DataPath = ((App)Application.Current).appData.StoragePath;
            OverviewChartFormatter = value => value + "min";
        }

        private async void Update()
        {
            await Task.Run(() => { ((App)Application.Current).appData.Load(displayDate); });
            MyDay tmp = ((App)Application.Current).appData.AllDays[displayDate];

            Overview = await Task.Run(() => { return PrintOverview(tmp.GetOverView()); });
            //SeriesCollection OverviewChart_tmp = await Task.Run(() => { return PrintOverviewChart(tmp.TimeLine); });

            OverviewChart.Clear();
            await Task.Run(() =>
            {
                SeriesCollection OverviewChart_tmp = PrintOverviewChart(tmp.TimeLine);
                foreach (StackedColumnSeries i in OverviewChart_tmp)
                {
                    OverviewChart.Add(i);
                    Thread.Sleep(220);
                }
            });
        }

        /// <summary>
        /// 整体视图
        /// </summary>
        private List<OverViewItem> overview;
        public List<OverViewItem> Overview 
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
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 数据保存路径
        /// </summary>
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

        private Visibility isDisabled = Visibility.Hidden;
        public Visibility IsDisabled
        {
            get { return isDisabled; }
            set
            {
                isDisabled = value;
                OnPropertyChanged();
            }
        }

        #region 表格
        /// <summary>
        /// 表格
        /// </summary>
        private SeriesCollection overviewChart = new SeriesCollection();
        public SeriesCollection OverviewChart
        {
            get { return overviewChart; }
            set { overviewChart = value; OnPropertyChanged(); }
        }

        private string[] overviewChartLables;
        public string[] OverviewChartLables
        {
            get { return overviewChartLables; }
            set { overviewChartLables = value; OnPropertyChanged(); }
        }

        private Func<double, string> overviewChartFormatter;
        public Func<double, string> OverviewChartFormatter
        {
            get { return overviewChartFormatter; }
            set { overviewChartFormatter = value; OnPropertyChanged(); }
        }
        #endregion

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

        public void MainPage_Closing(object sender, CancelEventArgs e)
        {
            
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
