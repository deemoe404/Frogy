﻿using System;
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
        OverviewViewModel viewModel = new OverviewViewModel();

        public OverViewView()
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

    }
}
