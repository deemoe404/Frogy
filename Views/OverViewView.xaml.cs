using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frogy.ViewModels;

namespace Frogy.Views
{
    /// <summary>
    /// Interaction logic for OverViewPage.xaml
    /// </summary>
    public partial class OverViewView : Page
    {
        OverViewViewModel viewModel = new OverViewViewModel();

        public OverViewView()
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = MouseWheelEvent;
            eventArg.Source = sender;

            ((StackPanel)((ListView)sender).Parent).RaiseEvent(eventArg);
        }

        private void ListView_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            var evenArg = new TouchEventArgs(e.TouchDevice, e.Timestamp);
            evenArg.RoutedEvent = TouchMoveEvent;
            evenArg.Source = sender;

            ((StackPanel)((ListView)sender).Parent).RaiseEvent(evenArg);
        }
    }
}
