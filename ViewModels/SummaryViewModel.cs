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

namespace Frogy.ViewModels
{
    class SummaryViewModel : INotifyPropertyChanged
    {
        public SummaryViewModel()
        {
            SummaryChart = generateChart();

            OverviewChartLables = new string[] { "sun", "mon", "tus", "the", "wen", "fri", "sat"};

            OverviewChartFormatter = value => value + "min";
        }

        private SeriesCollection generateChart()
        {
            DateTime firstDay = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
            SeriesCollection result = new SeriesCollection { };
            ChartValues<double> values = new ChartValues<double>();

            for (int i = 0; i < 7; i++)
            {
                DateTime day = firstDay.AddDays(i);
                ((App)Application.Current).appData.Load(day);
                List<MyTimeDuration> timeline = ((App)Application.Current).appData.AllDays[day].GetTimeline();

                TimeSpan totalTime = new TimeSpan();
                
                foreach(MyTimeDuration timeDuration in timeline)
                {
                    string appName = timeDuration.TimeDurationTask.ApplicationName;
                    if (string.IsNullOrEmpty(appName) || appName == "Frogy") continue;

                    totalTime = totalTime.Add(timeDuration.Duration);
                }

                values.Add(Math.Round(totalTime.TotalMinutes, 2));
            }

            result.Add(new StackedColumnSeries
            {
                Title = "Total",
                DataLabels = false,
                Values = values,
                IsHitTestVisible = false
            });

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
                OnPropertyChanged();
            }
        }

        private SeriesCollection summaryChart = new SeriesCollection();
        public SeriesCollection SummaryChart
        {
            get { return summaryChart; }
            set { summaryChart = value; OnPropertyChanged(); }
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
