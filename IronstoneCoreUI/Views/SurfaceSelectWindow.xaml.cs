using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Surface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for SurfaceSelectWindow.xaml
    /// </summary>
    public partial class SurfaceSelectWindow : Window
    {
        public string SelectedSurface { get; set; }
        public ObservableCollection<string> Surfaces { get; set; }

        public SurfaceSelectWindow()
        {
            InitializeComponent();

            Surfaces = new ObservableCollection<string>();

            using (Transaction trans =
                Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                ObjectIdCollection SurfaceIds = CivilApplication.ActiveDocument.GetSurfaceIds();
                foreach (ObjectId surfaceId in SurfaceIds)
                {
                    Surfaces.Add((surfaceId.GetObject(OpenMode.ForRead) as Surface).Name);
                }
            }

            this.DataContext = this;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
