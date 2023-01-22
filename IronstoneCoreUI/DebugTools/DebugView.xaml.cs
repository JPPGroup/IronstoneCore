using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Jpp.Ironstone.Core.UI.ViewModels;

namespace Jpp.Ironstone.Core.UI.Debug
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class DebugView : HostedUserControl
    {
        DebugViewModel _viewModel;

        public DebugView(DebugViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            _viewModel = viewModel;

        }

        public override void Show()
        {
            _viewModel.Scan();
        }

        public override void Hide()
        {
        }

        public void namedObjectCopy_Click(object sender, RoutedEventArgs args)
        {
            Debugger.Launch();
            var s = (args.Source as MenuItem).DataContext;
            _viewModel.CopyDataToClipboard(s as string);
        }
    }
}
