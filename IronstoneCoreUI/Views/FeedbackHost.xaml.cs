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

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for FeedbackHost.xaml
    /// </summary>
    public partial class FeedbackHost : HostedUserControl
    {
        public FeedbackHost(UIElement childElement)
        {
            InitializeComponent();
            DataContext = childElement;
            feedbackLink.Click += FeedbackLinkOnClick;

            StyleResource.Source = new Uri("../AutocadStyleResourceDictionary.xaml", UriKind.Relative);

#if AC2019
            StyleResource.Source = new Uri("../AutocadStyleResourceDictionary.2019.xaml", UriKind.Relative);
#endif
        }

        private void FeedbackLinkOnClick(object sender, RoutedEventArgs e)
        {
            Commands.OpenFeedbackPage();
        }

        public override void Show() { }
        public override void Hide() { }
    }
}
