using Jpp.Ironstone.Core.UI.ViewModels;

namespace Jpp.Ironstone.Core.UI.Views
{
    /// <summary>
    /// Interaction logic for Review.xaml
    /// </summary>
    public partial class Review : HostedUserControl
    {
        public Review(ReviewViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        public override void Show()
        {
            ((ReviewViewModel)this.DataContext).Refresh();
        }

        public override void Hide()
        {
        }
    }
}