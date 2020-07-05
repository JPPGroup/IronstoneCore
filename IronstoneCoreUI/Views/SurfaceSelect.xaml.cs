using System;
using System.Windows.Controls;
using Jpp.Ironstone.Core.UI.ViewModels;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for SurfaceSelect.xaml
    /// </summary>
    public partial class SurfaceSelect : UserControl
    {
        public string SelectedSurface { get; set; }

        public SurfaceSelect()
        {
            InitializeComponent();
        }
    }
}
