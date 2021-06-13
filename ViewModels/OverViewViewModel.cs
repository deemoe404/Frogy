using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using LiveCharts;
using LiveCharts.Wpf;

using Frogy.Classes;
using Frogy.Methods;
using Frogy.Models;
using Frogy.Resources.Language;

namespace Frogy.ViewModels
{
    class OverviewViewModel : INotifyPropertyChanged
    {
        Task<List<OverViewItem>> GenerateList(Dictionary<string, Software> OverView)
        {
            return Task.Run(() =>
            {
                List<OverViewItem> result = new List<OverViewItem>();
                
                var dicSort = from objDic in OverView orderby objDic.Value.Duration descending select objDic;

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
            });
        }

        Task<SeriesCollection> GenerateChart(List<MyTimeDuration> AllTask)
        {
            return Task.Run(() =>
            {
                SeriesCollection result = new SeriesCollection { };
                Dictionary<string, ChartValues<double>> rawData = new Dictionary<string, ChartValues<double>>();
                foreach (MyTimeDuration duration in AllTask)
                {
                    string appName = duration.TimeDurationTask.ApplicationName;
                    if (string.IsNullOrEmpty(appName) || appName == "Frogy") continue;

                    if (!rawData.ContainsKey(appName)) rawData.Add(appName,
                        new ChartValues<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                    TimeSpan start = duration.StartTime;
                    TimeSpan stop = duration.StopTime;
                    TimeSpan spent = duration.Duration;

                    if (start.Hours == stop.Hours)
                        rawData[appName][start.Hours] += Math.Round(spent.TotalMinutes, 2);
                    else
                    {
                        if (stop.Hours - start.Hours == 1)
                        {
                            rawData[appName][start.Hours] += Math.Round((59.59d - start.Minutes), 2);
                            rawData[appName][stop.Hours] += stop.Minutes;
                        }
                        else
                        {
                            if (stop.Hours - start.Hours > 1)
                            {
                                int tmpint = stop.Hours - start.Hours;

                                rawData[appName][start.Hours] += Math.Round((59.59d - start.Minutes), 2);
                                rawData[appName][stop.Hours] += stop.Minutes;

                                for (int i = 1; i < tmpint; i++)
                                    rawData[appName][start.Hours + i] = 59.59d;
                            }
                        }
                    }
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    foreach (string key in rawData.Keys)
                    {
                        result.Add(new StackedColumnSeries
                        {
                            Values = rawData[key],
                            DataLabels = false,
                            Title = key,
                            IsHitTestVisible = false
                        });
                    }
                });

                return result;
            });
        }
        
        Task<string> GenerateSumTime(Dictionary<string, Software> AllTask)
        {
            return Task.Run(() =>
            {
                double totalTime = 0;
                foreach (KeyValuePair<string, Software> pair in AllTask)
                    totalTime += pair.Value.Duration.TotalMinutes;

                int hour = (int)totalTime / 60;
                int minutes = (int)(totalTime - hour * 60);

                return hour + LanguageHelper.InquireLocalizedWord("General_Hour") + minutes + LanguageHelper.InquireLocalizedWord("General_Minute");
            });
        }

        public OverviewViewModel()
        {
            Update();
        }

        private async void Update()
        {
            DateChangeable = false;
            Loading = Visibility.Visible;

            ((App)Application.Current).AppData.Load(displayDate);
            MyDay today = ((App)Application.Current).AppData.AllDays[displayDate];

            Dictionary<string, Software> overView = today.GetOverView();

            DailyTime = await GenerateSumTime(overView);
            OverviewChart = await GenerateChart(today.GetTimeline());
            UpdateTime = LanguageHelper.InquireLocalizedWord("General_LastUpdate") + DateTime.Now.ToString("H:mm");

            Overview.Clear();
            foreach (OverViewItem item in await GenerateList(overView))
            {
                Overview.Add(item);
                MyDeviceHelper.DoEvents();
            }

            DateChangeable = true;
            Loading = Visibility.Hidden;
        }

        private ObservableCollection<OverViewItem> overview = new ObservableCollection<OverViewItem>();
        public ObservableCollection<OverViewItem> Overview
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

        private string updateTime = DateTime.Now.ToString("H:mm");
        public string UpdateTime
        {
            get
            {
                return updateTime;
            }
            set
            {
                updateTime = value;
                OnPropertyChanged();
            }
        }

        private bool dateChangeable = true;
        public bool DateChangeable
        {
            get 
            { 
                return dateChangeable; 
            }
            set
            {
                dateChangeable = value;
                OnPropertyChanged();
            }
        }

        private Visibility loading = Visibility.Hidden;
        public Visibility Loading
        {
            get
            {
                return loading;
            }
            set
            {
                loading = value;
                OnPropertyChanged();
            }
        }

        private string dailyTime;
        public string DailyTime
        {
            get
            {
                return dailyTime;
            }
            set
            {
                dailyTime = value;
                OnPropertyChanged();
            }
        }

        #region 表格
        private SeriesCollection overviewChart = new SeriesCollection();
        public SeriesCollection OverviewChart
        {
            get 
            { 
                return overviewChart; 
            }
            set 
            { 
                overviewChart = value; 
                OnPropertyChanged(); 
            }
        }

        public string[] OverviewChartLables
        {
            get 
            {
                return new string[] { "0:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00", "7:00", "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };
            }
        }

        private Func<double, string> overviewChartFormatter = value => value + LanguageHelper.InquireLocalizedWord("General_Minute");
        public Func<double, string> OverviewChartFormatter
        {
            get 
            {
                return overviewChartFormatter; 
            }
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