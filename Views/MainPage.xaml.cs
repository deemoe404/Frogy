using Frogy.ViewModels;
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
using System.Windows.Shapes;

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
            InitializeComponent();

            this.DataContext = viewModel;

            this.Closing += viewModel.MainPage_Closing;
        }


    }
}
