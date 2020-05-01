using System;
using System.Windows;
using System.Windows.Controls;
using Jpp.Ironstone.Core.UI.ViewModels;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for Libraries.xaml
    /// </summary>
    public partial class Libraries : HostedUserControl
    {
        

        public Libraries()
        {
            InitializeComponent();
        }

        public override void Show()
        {
            this.DataContext = new LibrariesViewModel();
        }

        public override void Hide()
        {
        }

        private void LoadBlock(object sender, RoutedEventArgs e)
        {
            string templateId = ((Guid)((MenuItem)sender).Tag).ToString();
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute($"Core_Lib_LoadIntoDrawing {templateId}\n", true, false, false);
        }

        private void InsertBlock(object sender, RoutedEventArgs e)
        {
            string templateId = ((Guid)((MenuItem)sender).Tag).ToString();
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute($"Core_Lib_AddToDrawing {templateId}\n", true, false, false);
        }
    }
}
