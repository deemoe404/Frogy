using Frogy.ViewModels;
using System.Windows;

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


    }
}