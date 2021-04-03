using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Frogy.Models;
using Frogy.Classes;
using LiveCharts.Wpf;
using static Frogy.Models.SummaryModel;
using System.Windows.Input;
using Frogy.Methods;
using Frogy.Resources.Language;
using System.Collections.ObjectModel;

namespace Frogy.ViewModels
{
    class SummaryViewModel : INotifyPropertyChanged
    {
        public SummaryViewModel()
        {
            update();

            OverviewChartFormatter = value => value + LanguageHelper.InquireLocalizedWord("General_Minute");
        }

        async void update()
        {
            DateChangeable = false;
            Loading = Visibility.Visible;

            DateTime firstDay = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);

            var a = await generateChart(firstDay);
            var b = await generateList(firstDay);

            SummaryChart = a;

            OverviewChartLables = generateChartLables(firstDay);
            AverageDailyTime = generateAverageDailyTime((ChartValues<double>)SummaryChart[0].Values);

            foreach (var c in b)
            {
                SummaryList.Add(c);
                MyDeviceHelper.DoEvents();
            }

            DateChangeable = true;
            Loading = Visibility.Hidden;
        }

        private Task<SeriesCollection> generateChart(DateTime firstDay)
        {
            return Task.Run(() =>
            {
                SeriesCollection result = new SeriesCollection { };
                ChartValues<double> values = new ChartValues<double>();

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = firstDay.AddDays(i);
                    ((App)Application.Current).appData.Load(day);
                    List<MyTimeDuration> timeline = ((App)Application.Current).appData.AllDays[day].GetTimeline();

                    TimeSpan totalTime = new TimeSpan();

                    foreach (MyTimeDuration timeDuration in timeline)
                    {
                        string appName = timeDuration.TimeDurationTask.ApplicationName;
                        if (string.IsNullOrEmpty(appName) || appName == "Frogy") continue;

                        totalTime = totalTime.Add(timeDuration.Duration);
                    }

                    values.Add(Math.Round(totalTime.TotalMinutes, 2));
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    result.Add(new StackedColumnSeries
                    {
                        Title = LanguageHelper.InquireLocalizedWord("General_Total"),
                        DataLabels = false,
                        Values = values,
                        IsHitTestVisible = false
                    });
                });

                return result;
            });
        }

        private Task<List<SummaryListItem>> generateList(DateTime firstDay)
        {
            return Task.Run(() =>
            {
                List<SummaryListItem> result = new List<SummaryListItem>();
                Dictionary<string, Software> sumData = new Dictionary<string, Software>();

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = firstDay.AddDays(i);
                    ((App)Application.Current).appData.Load(day);

                    foreach (KeyValuePair<string, Software> kvp in ((App)Application.Current).appData.AllDays[day].GetOverView())
                    {
                        if (sumData.ContainsKey(kvp.Key))
                            sumData[kvp.Key].Duration += kvp.Value.Duration;
                        else
                            sumData.Add(kvp.Key, kvp.Value);
                    }
                }

                var dicSort = from objDic in sumData orderby objDic.Value.Duration descending select objDic;

                foreach (KeyValuePair<string, Software> kvp in dicSort)
                {
                    SummaryListItem tmp =
                        new SummaryListItem()
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

        private string[] generateChartLables(DateTime firstDay)
        {
            string[] result = new string[7];

            for (int i = 0; i < 7; i++)
            {
                DateTime day = firstDay.AddDays(i);
                result[i] = day.Date.ToString("M.d");
            }
            
            return result;
        }

        private string generateAverageDailyTime(ChartValues<double> weeklyData)
        {
            string result;
            double sum = 0;
            int avg;

            for(int i = 0; i < weeklyData.Count; i++)
                sum += weeklyData[i];

            avg = (int)(sum / (weeklyData.Count));
            result = (avg / 60).ToString() + LanguageHelper.InquireLocalizedWord("General_Hour") +
                (avg - (avg / 60 * 60)).ToString() + LanguageHelper.InquireLocalizedWord("General_Minute");
            return result;
        }

        private DateTime selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get
            {
                return selectedDate;
            }
            set
            {
                selectedDate = value;
                update();
                OnPropertyChanged();
            }
        }

        private SeriesCollection summaryChart = new SeriesCollection();
        public SeriesCollection SummaryChart
        {
            get { return summaryChart; }
            set { summaryChart = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SummaryListItem> summaryList = new ObservableCollection<SummaryListItem>();
        public ObservableCollection<SummaryListItem> SummaryList
        {
            get
            {
                return summaryList;
            }
            set
            {
                summaryList = value;
                OnPropertyChanged();
            }
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

        private string averageDailyTime;
        public string AverageDailyTime
        {
            get
            {
                return averageDailyTime;
            }
            set
            {
                averageDailyTime = value;
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
            update();
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
            SelectedDate = selectedDate.AddDays(-7);
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
            SelectedDate = selectedDate.AddDays(7);
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
