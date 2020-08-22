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
    /// Interaction logic for OptionView.xaml
    /// </summary>
    public partial class OptionView : Page
    {
        OptionViewModel viewModel = new OptionViewModel();

        public OptionView()
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }
    }
}
