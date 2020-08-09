using Frogy.ViewModels;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Input;

namespace Frogy.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window
    {
        MainPageViewModel viewModel = new MainPageViewModel();

        public MainPage()
        {
            this.DataContext = viewModel;
            this.Closing += viewModel.MainPage_Closing;

            InitializeComponent();
        }



        //将ListView的鼠标滚轮事件向上发送给Grid
        private void ListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = MouseWheelEvent;
            eventArg.Source = sender;

            ((StackPanel)((System.Windows.Controls.ListView)sender).Parent).RaiseEvent(eventArg);
        }
    }
}