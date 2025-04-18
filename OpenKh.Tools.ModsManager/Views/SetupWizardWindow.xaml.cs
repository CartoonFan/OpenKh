using OpenKh.Common;
using OpenKh.Tools.ModsManager.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for SetupWizardWindow.xaml
    /// </summary>
    public partial class SetupWizardWindow : Window
    {
        private readonly SetupWizardViewModel _vm;

        public SetupWizardWindow()
        {
            InitializeComponent();
            DataContext = _vm = new SetupWizardViewModel();

            _vm.PageIsoSelection = PageIsoSelection;
            _vm.PageEosInstall = PageEosInstall;
            _vm.PageEosConfig = PageEosConfig;
            _vm.PageRegion = PageRegion;
            _vm.LastPage = LastPage;
        }

        public string ConfigIsoLocation { get => _vm.IsoLocation; set => _vm.IsoLocation = value; }
        public int ConfigGameEdition { get => _vm.GameEdition; set => _vm.GameEdition = value; }
        public string ConfigOpenKhGameEngineLocation { get => _vm.OpenKhGameEngineLocation; set => _vm.OpenKhGameEngineLocation = value; }
        public string ConfigPcsx2Location { get => _vm.Pcsx2Location; set => _vm.Pcsx2Location = value; }
        public string ConfigPcReleaseLocation { get => _vm.PcReleaseLocation; set => _vm.PcReleaseLocation = value; }
        public string ConfigGameDataLocation { get => _vm.GameDataLocation; set => _vm.GameDataLocation = value; }
        public int ConfigRegionId { get => _vm.RegionId; set => _vm.RegionId = value; }
        public bool ConfigBypassLauncher { get => _vm.BypassLauncher; set => _vm.BypassLauncher = value; }
        public string ConfigEpicGamesUserID { get => _vm.EpicGamesUserID; set => _vm.EpicGamesUserID = value; }

        private void Wizard_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NavigateURL(object sender, RequestNavigateEventArgs e) =>
            new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = e.Uri.AbsoluteUri
                }
            }.Using(x => x.Start());
    }
}
