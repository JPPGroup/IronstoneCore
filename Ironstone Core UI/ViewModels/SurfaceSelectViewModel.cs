using System.Windows.Input;
using Jpp.Common;
using Jpp.Ironstone.Core.UI.Views;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    public class SurfaceSelectViewModel : BaseNotify
    {
        public ICommand ShowSurfaceSelector { get; set; }

        public string SelectedSurfaceName
        {
            get { return _selectedSurfaceName; }
            set { SetField(ref _selectedSurfaceName, value, nameof(SelectedSurfaceName)); }
        }

        private string _selectedSurfaceName;

        public SurfaceSelectViewModel()
        {
            ShowSurfaceSelector = new DelegateCommand(() =>
            {
                SurfaceSelectWindow surfaceSelectWindow = new SurfaceSelectWindow();
                // Open the dialog box modally 
                var result = surfaceSelectWindow.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    SelectedSurfaceName = surfaceSelectWindow.SelectedSurface;
                }

            }, () => { return CoreExtensionApplication.Civil3D;});
        }
    }
}
