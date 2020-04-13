using Jpp.Ironstone.Core.UI.ViewModels;

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
    }
}
