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
using Jpp.Ironstone.Core.UI.ViewModels;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for ObjectInspectorView.xaml
    /// </summary>
    public partial class DatabaseExplorerView : UserControl
    {
        public DatabaseExplorerView(DatabaseExplorerViewModel devm)
        {
            InitializeComponent();
            DataContext = devm;
        }
    }
}
