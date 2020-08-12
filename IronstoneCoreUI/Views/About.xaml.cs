using Jpp.Ironstone.Core.UI.ViewModels;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : HostedUserControl
    {
        public About(AboutViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }
    }
}
