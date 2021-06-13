using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

using LiveCharts.Wpf;
using LiveCharts;

using static Frogy.Models.WeeklyModel;
using Frogy.Classes;
using Frogy.Methods;
using Frogy.Resources.Language;

namespace Frogy.ViewModels
{
    class WeeklyViewModel : INotifyPropertyChanged
    {
        public WeeklyViewModel()
        {
            Update();
        }

        async void Update()
        {
            busy = true;

            DateTime firstDay = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);

            SeriesCollection tmpChart = await GenerateChart(firstDay);
            List<SummaryListItem> tmpList = await generateList(firstDay);

            OverviewChartLables = generateChartLables(firstDay);
            AverageDailyTime = generateAverageDailyTime((ChartValues<double>)tmpChart[0].Values);
            SummaryChart = tmpChart;
            UpdateTime = LanguageHelper.InquireLocalizedWord("General_LastUpdate") + DateTime.Now.ToString("H:mm");

            SummaryList.Clear();
            foreach (SummaryListItem listItem in tmpList)
            {
                SummaryList.Add(listItem);
                MyDeviceHelper.DoEvents();
            }

            busy = false;
        }

        //todo:不要输入datetime

        private Task<SeriesCollection> GenerateChart(DateTime firstDay)
        {
            return Task.Run(() =>
            {
                SeriesCollection result = new SeriesCollection { };
                ChartValues<double> values = new ChartValues<double>();

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = firstDay.AddDays(i);
                    ((App)Application.Current).AppData.Load(day);
                    List<MyTimeDuration> timeline = ((App)Application.Current).AppData.AllDays[day].GetTimeline();

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
                    ((App)Application.Current).AppData.Load(day);

                    foreach (KeyValuePair<string, Software> kvp in ((App)Application.Current).AppData.AllDays[day].GetOverView())
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

        //private int averageDailyTime(double[] Week)
        //{
        //    double result = 0;

        //    for (int i = 0; i < Week.Count(); i++)
        //        result += Week[i] / 7;

        //    return (int)result;
        //}

        private Task<string> generateCompare(DateTime firstDay)
        {
            return Task.Run(()=>
            {
                string result = "";

                DateTime totalTime = new DateTime();
                DateTime lastWeek = firstDay.AddDays(-7);

                for(int i= 0; i < 7; i++)
                {
                    totalTime.AddMinutes(lastWeek.AddDays(i).Minute);
                }

                int avg = totalTime.Minute / 7;

                return result;
            });
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
                Update();
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

        private Func<double, string> overviewChartFormatter = value => value + LanguageHelper.InquireLocalizedWord("General_Minute");
        public Func<double, string> OverviewChartFormatter
        {
            get 
            { 
                return overviewChartFormatter; 
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

        private string compareLastWeek;
        public string CompareLastWeek
        {
            get
            {
                return compareLastWeek;
            }
            set
            {
                compareLastWeek = value;
                OnPropertyChanged();
            }
        }

        #region App busy controller
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

        private bool busy = false;
        public bool Busy
        {
            get
            {
                return busy;
            }
            set
            {
                busy = value;
                Loading = busy ? Visibility.Visible : Visibility.Hidden;
                DateChangeable = !busy;
            }
        }
        #endregion

        #region Refresh button
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

        #region PreWeek button
        private ICommand preWeek;
        public ICommand PreWeek
        {
            get
            {
                if (preWeek == null)
                {
                    preWeek = new RelayCommand(
                        param => this.PreWeekButton_Click(),
                        param => true
                    );
                }
                return preWeek;
            }
        }
        private void PreWeekButton_Click()
        {
            SelectedDate = selectedDate.AddDays(-7);
        }
        #endregion

        #region PostWeek button
        private ICommand postWeek;
        public ICommand PostWeek
        {
            get
            {
                if (postWeek == null)
                {
                    postWeek = new RelayCommand(
                        param => this.PostWeekButton_Click(),
                        param => true
                    );
                }
                return postWeek;
            }
        }
        private void PostWeekButton_Click()
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
