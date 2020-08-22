using Frogy.ViewModels;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Input;
using System;

namespace Frogy.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Window
    {
        DashboardViewModel viewModel = new DashboardViewModel();

        public DashboardView()
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }
    }
}