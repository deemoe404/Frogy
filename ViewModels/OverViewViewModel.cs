using Frogy.Classes;
using Frogy.Methods;
using Frogy.Models;
using HandyControl.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Frogy.ViewModels
{
    class OverViewViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 按顺序打印OverView
        /// </summary>
        /// <param name="TodayOverview">今日OverView</param>
        /// <returns>List<OverViewItem></returns>
        private List<OverViewItem> PrintOverview(Dictionary<string, Software> TodayOverview)
        {
            List<OverViewItem> result = new List<OverViewItem>();
            //排序
            var dicSort = from objDic in TodayOverview orderby objDic.Value.Duration descending select objDic;

            foreach (KeyValuePair<string, Software> kvp in dicSort)
            {
                OverViewItem tmp =
                    new OverViewItem()
                    {
                        AppName = kvp.Key,
                        AppDuration = kvp.Value.Duration.ToString(),
                        AppIcon = kvp.Value.Icon
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
                    tmpdic[appName][start.Hours] += Math.Round(spent.TotalMinutes, 2);
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
                                tmpdic[appName][start.Hours + i] = 59.59d;
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
                        DataLabels = false,
                        Title = key,
                        IsHitTestVisible = false
                    });
                }
            });

            return result;
        }

        /// <summary>
        /// 打印SummaryView
        /// </summary>
        /// <returns></returns>
        private List<DetailViewItem> PrintSummaryView(List<MyTimeDuration> durations)
        {
            List<DetailViewItem> result = new List<DetailViewItem>();

            foreach (MyTimeDuration timeSpan in durations)
            {
                DetailViewItem tmp = new DetailViewItem()
                {
                    StartTime = timeSpan.StartTime.ToString(),
                    StopTime = timeSpan.StopTime.ToString(),
                    AppDuration = timeSpan.Duration.ToString(),
                    AppIcon = MyDataHelper.BitmapToBitmapImage(MyDataHelper.Base64StringToImage(timeSpan.TimeDurationTask.ApplicationIcon_Base64)),
                    AppName = timeSpan.TimeDurationTask.ApplicationName,
                    WindowTitle = timeSpan.TimeDurationTask.ApplicationTitle,
                    SystemState = timeSpan.TimeDurationTask.ComputerStatus.ToString()
                };
                result.Add(tmp);
            }
            return result;
        }

        public OverViewViewModel()
        {
            Update();

            OverviewChartLables = new string[] { "0:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00",
            "7:00", "8:00", "9:00", "10:00", "11:00", "12:00",
            "13:00", "14:00", "15:00", "16:00", "17:00", "18:00",
            "19:00", "20:00", "21:00", "22:00", "23:00"};

            OverviewChartFormatter = value => value + "min";
        }

        private async void Update()
        {
            ((App)Application.Current).appData.Load(displayDate);
            MyDay today = ((App)Application.Current).appData.AllDays[displayDate];

            await Task.Run(() =>
            {
                Overview = PrintOverview(today.GetOverView());

                #if DEBUG
                DetailView = PrintSummaryView(today.GetTimeline());
                #endif
            });

            OverviewChart.Clear();
            await Task.Run(() =>
            {
                SeriesCollection OverviewChart_tmp = PrintOverviewChart(today.TimeLine);
                foreach (StackedColumnSeries i in OverviewChart_tmp)
                {
                    OverviewChart.Add(i);
                    //Thread.Sleep(20);
                }
            });
        }

        /// <summary>
        /// 详细数据
        /// </summary>
        private List<DetailViewItem> detailView;
        public List<DetailViewItem> DetailView
        {
            get
            {
                return detailView;
            }
            set
            {
                detailView = value;
                OnPropertyChanged();
            }
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

        #region 刷新按钮
        private ICommand refresh;
        public ICommand Refresh
        {
            get
            {
                if (refresh == null)
                {
                    refresh = new RelayCommand(
                        param => this.RefreshButton_Click(),
                        param => true
                    );
                }
                return refresh;
            }
        }
        private void RefreshButton_Click()
        {
            Update();
        }
        #endregion

        #region Yesterday button
        private ICommand preDay;
        public ICommand PreDay
        {
            get
            {
                if (preDay == null)
                {
                    preDay = new RelayCommand(
                        param => this.PreDayButton_Click(),
                        param => true
                    );
                }
                return preDay;
            }
        }
        private void PreDayButton_Click()
        {
            DisplayDate = displayDate.AddDays(-1);
        }
        #endregion

        #region Tomorrow button
        private ICommand nextDay;
        public ICommand NextDay
        {
            get
            {
                if (nextDay == null)
                {
                    nextDay = new RelayCommand(
                        param => this.NextDayButton_Click(),
                        param => true
                    );
                }
                return nextDay;
            }
        }
        private void NextDayButton_Click()
        {
            DisplayDate = displayDate.AddDays(1);
        }
        #endregion


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