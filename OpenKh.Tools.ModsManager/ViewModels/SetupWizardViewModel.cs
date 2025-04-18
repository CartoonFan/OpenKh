using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xe.IO;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class SetupWizardViewModel : BaseNotifyPropertyChanged
    {
        private const int BufferSize = 65536;
        private static readonly string PanaceaDllName = "OpenKH.Panacea.dll";
        private static string ApplicationName = Utilities.GetApplicationName();
        private static List<FileDialogFilter> _isoFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PlayStation 2 game ISO", "iso");
        private static List<FileDialogFilter> _openkhGeFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("OpenKH Game Engine Executable", "*Game.exe");
        private static List<FileDialogFilter> _pcsx2Filter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PCSX2 Emulator", "exe");

        const int OpenKHGameEngine = 0;
        const int PCSX2 = 1;
        const int EpicGames = 2;

        private int _gameEdition;
        private string _isoLocation;
        private string _openKhGameEngineLocation;
        private string _pcsx2Location;
        private string _pcReleaseLocation;
        private string _gameDataLocation;

        private Xceed.Wpf.Toolkit.WizardPage _wizardPageAfterIntro;
        public Xceed.Wpf.Toolkit.WizardPage WizardPageAfterIntro
        {
            get => _wizardPageAfterIntro;
            private set
            {
                _wizardPageAfterIntro = value;
                OnPropertyChanged();
            }
        }
        private Xceed.Wpf.Toolkit.WizardPage _wizardPageAfterGameData;
        public Xceed.Wpf.Toolkit.WizardPage WizardPageAfterGameData
        {
            get => _wizardPageAfterGameData;
            private set
            {
                _wizardPageAfterGameData = value;
                OnPropertyChanged();
            }
        }
        public Xceed.Wpf.Toolkit.WizardPage PageIsoSelection { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageEosInstall { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageEosConfig { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageRegion { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage LastPage { get; internal set; }

        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public string Title => $"Set-up wizard | {ApplicationName}";

        public RelayCommand SelectIsoCommand { get; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string IsoLocation
        {
            get => _isoLocation;
            set
            {
                _isoLocation = value;
                if (File.Exists(_isoLocation))
                {
                    var game = GameService.DetectGameId(_isoLocation);
                    GameId = game?.Id;
                    GameName = game?.Name;
                }
                else
                {
                    GameId = null;
                    GameName = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIsoSelected));
                OnPropertyChanged(nameof(GameId));
                OnPropertyChanged(nameof(GameName));
                OnPropertyChanged(nameof(GameRecognizedVisibility));
                OnPropertyChanged(nameof(GameNotRecognizedVisibility));
                OnPropertyChanged(nameof(IsGameRecognized));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
            }
        }
        public bool IsIsoSelected => (!string.IsNullOrEmpty(IsoLocation) && File.Exists(IsoLocation));
        public bool IsGameRecognized => (IsIsoSelected && GameId != null);
        public Visibility GameRecognizedVisibility => IsIsoSelected && GameId != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameNotRecognizedVisibility => IsIsoSelected && GameId == null ? Visibility.Visible : Visibility.Collapsed;

        public bool IsGameSelected
        {
            get
            {
                return GameEdition switch
                {
                    OpenKHGameEngine => !string.IsNullOrEmpty(OpenKhGameEngineLocation) && File.Exists(OpenKhGameEngineLocation),
                    PCSX2 => !string.IsNullOrEmpty(Pcsx2Location) && File.Exists(Pcsx2Location),
                    EpicGames => !string.IsNullOrEmpty(PcReleaseLocation) &&
                        Directory.Exists(PcReleaseLocation) &&
                        File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll")),
                    _ => false,
                };
            }
        }

        public int GameEdition
        {
            get => _gameEdition;
            set
            {
                _gameEdition = value;
                WizardPageAfterIntro = GameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => PageIsoSelection,
                    EpicGames => IsLastPanaceaVersionInstalled ? PageEosConfig : PageEosInstall,
                    _ => null,
                };
                WizardPageAfterGameData = GameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => PageRegion,
                    EpicGames => LastPage,
                    _ => null,
                };

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(OpenKhGameEngineConfigVisibility));
                OnPropertyChanged(nameof(Pcsx2ConfigVisibility));
                OnPropertyChanged(nameof(PcReleaseConfigVisibility));
            }
        }
        public RelayCommand SelectOpenKhGameEngineCommand { get; }
        public Visibility OpenKhGameEngineConfigVisibility => GameEdition == OpenKHGameEngine ? Visibility.Visible : Visibility.Collapsed;
        public string OpenKhGameEngineLocation
        {
            get => _openKhGameEngineLocation;
            set
            {
                _openKhGameEngineLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcsx2Command { get; }
        public Visibility Pcsx2ConfigVisibility => GameEdition == PCSX2 ? Visibility.Visible : Visibility.Collapsed;
        public string Pcsx2Location
        {
            get => _pcsx2Location;
            set
            {
                _pcsx2Location = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcReleaseCommand { get; }
        public Visibility PcReleaseConfigVisibility => GameEdition == EpicGames ? Visibility.Visible : Visibility.Collapsed;
        public string PcReleaseLocation
        {
            get => _pcReleaseLocation;
            set
            {
                _pcReleaseLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(IsGameDataFound));
            }
        }

        public RelayCommand SelectGameDataLocationCommand { get; }
        public string GameDataLocation
        {
            get => _gameDataLocation;
            set
            {
                _gameDataLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
            }
        }

        public bool IsNotExtracting { get; private set; }
        public bool IsGameDataFound => (IsNotExtracting && GameService.FolderContainsUniqueFile(GameId, GameDataLocation) || 
            (GameEdition == EpicGames && GameService.FolderContainsUniqueFile("kh2", GameDataLocation)));
        public Visibility GameDataNotFoundVisibility => !IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameDataFoundVisibility => IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility => IsNotExtracting ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ExtractionCompleteVisibility => ExtractionProgress == 1f ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ExtractGameDataCommand { get; set; }
        public float ExtractionProgress { get; set; }

        public int RegionId { get; set; }

        public RelayCommand InstallPanaceaCommand { get; }
        public RelayCommand RemovePanaceaCommand { get; }
        private string PanaceaSourceLocation => Path.Combine(AppContext.BaseDirectory, PanaceaDllName);
        private string PanaceaDestinationLocation => Path.Combine(PcReleaseLocation, "DBGHELP.dll");
        public Visibility PanaceaNotInstalledVisibility => !IsLastPanaceaVersionInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PanaceaInstalledVisibility => IsLastPanaceaVersionInstalled ? Visibility.Visible : Visibility.Collapsed;
        public bool IsLastPanaceaVersionInstalled
        {
            get
            {
                if (PcReleaseLocation == null)
                    // Won't be able to find the source location
                    return false;

                if (!File.Exists(PanaceaSourceLocation))
                    // While debugging it is most likely to not have the compiled
                    // DLL into the right place. So don't bother.
                    return true;

                if (!File.Exists(PanaceaDestinationLocation))
                    // DBGHELP.dll is not installed
                    return false;

                byte[] CalculateChecksum(string fileName) =>
                    System.Security.Cryptography.MD5.Create().Using(md5 =>
                        File.OpenRead(fileName).Using(md5.ComputeHash));
                bool IsEqual(byte[] left, byte[] right)
                {
                    for (var i = 0; i < left.Length; i++)
                        if (left[i] != right[i])
                            return false;
                    return true;
                }

                return IsEqual(
                    CalculateChecksum(PanaceaSourceLocation),
                    CalculateChecksum(PanaceaDestinationLocation));
            }
        }

        public Visibility BypassLauncherVisibility => BypassLauncher ? Visibility.Visible : Visibility.Collapsed;
        private bool _bypassLauncher;

        public bool BypassLauncher
        {
            get => _bypassLauncher;
            set
            {
                _bypassLauncher = value;
                OnPropertyChanged(nameof(BypassLauncherVisibility));
            }
        }
        public string EpicGamesUserID { get; set; }

        public SetupWizardViewModel()
        {
            IsNotExtracting = true;
            SelectIsoCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => IsoLocation = fileName, _isoFilter));
            SelectOpenKhGameEngineCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => OpenKhGameEngineLocation = fileName, _openkhGeFilter));
            SelectPcsx2Command = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => Pcsx2Location = fileName, _pcsx2Filter));
            SelectPcReleaseCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => PcReleaseLocation = path));
            SelectGameDataLocationCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => GameDataLocation = path));
            ExtractGameDataCommand = new RelayCommand(async _ => 
            { 
                BEGIN:
                try
                {
                    await ExtractGameData(IsoLocation, GameDataLocation);
                }

                catch (IOException _ex)
                {
                    var _sysMessage = MessageBox.Show(_ex.Message + "\n\nWould you like to try again?", "An Exception was Caught!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (_sysMessage)
                    {
                        case MessageBoxResult.Yes:
                            goto BEGIN;
                            break;
                    }
                }
            });
            InstallPanaceaCommand = new RelayCommand(_ =>
            {
                if (File.Exists(PanaceaSourceLocation))
                    // Again, do not bother in debug mode
                    File.Copy(PanaceaSourceLocation, PanaceaDestinationLocation, true);
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
            });
            RemovePanaceaCommand = new RelayCommand(_ =>
            {
                if (File.Exists(PanaceaDestinationLocation))
                    File.Delete(PanaceaDestinationLocation);
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
            });
        }

        private async Task ExtractGameData(string isoLocation, string gameDataLocation)
        {
            switch (GameEdition)
            {

                default:
                {
                    var fileBlocks = File.OpenRead(isoLocation).Using(stream =>
                    {
                        var bufferedStream = new BufferedStream(stream);
                        var idxBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IDX;1");
                        var imgBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IMG;1");
                        return (idxBlock, imgBlock);
                    });

                    if (fileBlocks.idxBlock == -1 || fileBlocks.imgBlock == -1)
                    {
                        MessageBox.Show(
                            $"Unable to find the files KH2.IDX and KH2.IMG in the ISO at '{isoLocation}'. The extraction will stop.",
                            "Extraction error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    IsNotExtracting = false;
                    ExtractionProgress = 0;
                    OnPropertyChanged(nameof(IsNotExtracting));
                    OnPropertyChanged(nameof(IsGameDataFound));
                    OnPropertyChanged(nameof(ProgressBarVisibility));
                    OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                    OnPropertyChanged(nameof(ExtractionProgress));

                    await Task.Run(() =>
                    {
                        using var isoStream = File.OpenRead(isoLocation);

                        var idxOffset = fileBlocks.idxBlock * 0x800L;
                        var idx = Idx.Read(new SubStream(isoStream, idxOffset, isoStream.Length - idxOffset));

                        var imgOffset = fileBlocks.imgBlock * 0x800L;
                        var imgStream = new SubStream(isoStream, imgOffset, isoStream.Length - imgOffset);
                        var img = new Img(imgStream, idx, true);

                        var fileCount = img.Entries.Count;
                        var fileProcessed = 0;
                        foreach (var fileEntry in img.Entries)
                        {
                            var fileName = IdxName.Lookup(fileEntry) ?? $"@{fileEntry.Hash32:08X}_{fileEntry.Hash16:04X}";
                            using var stream = img.FileOpen(fileEntry);
                            var fileDestination = Path.Combine(gameDataLocation, fileName);
                            var directoryDestination = Path.GetDirectoryName(fileDestination);
                            if (!Directory.Exists(directoryDestination))
                                Directory.CreateDirectory(directoryDestination);
                            File.Create(fileDestination).Using(dstStream => stream.CopyTo(dstStream, BufferSize));

                            fileProcessed++;
                            ExtractionProgress = (float)fileProcessed / fileCount;
                            OnPropertyChanged(nameof(ExtractionProgress));
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsNotExtracting = true;
                            ExtractionProgress = 1.0f;
                            OnPropertyChanged(nameof(IsNotExtracting));
                            OnPropertyChanged(nameof(IsGameDataFound));
                            OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                            OnPropertyChanged(nameof(GameDataFoundVisibility));
                            OnPropertyChanged(nameof(ProgressBarVisibility));
                            OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                            OnPropertyChanged(nameof(ExtractionProgress));
                        });
                    });
                }
                break;

                case EpicGames:
                {
                    IsNotExtracting = false;
                    ExtractionProgress = 0;
                    OnPropertyChanged(nameof(IsNotExtracting));
                    OnPropertyChanged(nameof(IsGameDataFound));
                    OnPropertyChanged(nameof(ProgressBarVisibility));
                    OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                    OnPropertyChanged(nameof(ExtractionProgress));

                    await Task.Run(() =>
                    {
                        var _nameList = new string[]
                        {
                            "first",
                            "second",
                            "third",
                            "fourth",
                            "fifth",
                            "sixth"
                        };

                        var _totalFiles = 0;
                        var _procTotalFiles = 0;

                        for (int i = 0; i < 6; i++)
                        {
                            using var _stream = new FileStream(Path.Combine(_pcReleaseLocation, "Image", "en", "kh2_" + _nameList[i] + ".hed"), FileMode.Open);
                            var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                            _totalFiles += _hedFile.Count();
                        }

                        for (int i = 0; i < 6; i++)
                        {
                            var outputDir = gameDataLocation;
                            using var hedStream = File.OpenRead(Path.Combine(_pcReleaseLocation, "Image", "en", "kh2_" + _nameList[i] + ".hed"));
                            using var img = File.OpenRead(Path.Combine(_pcReleaseLocation, "Image", "en", "kh2_" + _nameList[i] + ".pkg"));

                            foreach (var entry in OpenKh.Egs.Hed.Read(hedStream))
                            {
                                var hash = OpenKh.Egs.Helpers.ToString(entry.MD5);
                                if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var fileName))
                                    fileName = $"{hash}.dat";

                                var outputFileName = Path.Combine(outputDir, fileName);

                                OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileName);

                                var hdAsset = new OpenKh.Egs.EgsHdAsset(img.SetPosition(entry.Offset));

                                File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

                                outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

                                foreach (var asset in hdAsset.Assets)
                                {
                                    var outputFileNameRemastered = Path.Combine(OpenKh.Egs.EgsTools.GetHDAssetFolder(outputFileName), asset);
                                    OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileNameRemastered);

                                    var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                                    File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                                    _procTotalFiles++;

                                    ExtractionProgress = (float)_procTotalFiles / _totalFiles;
                                    OnPropertyChanged(nameof(ExtractionProgress));
                                }
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsNotExtracting = true;
                            ExtractionProgress = 1.0f;
                            OnPropertyChanged(nameof(IsNotExtracting));
                            OnPropertyChanged(nameof(IsGameDataFound));
                            OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                            OnPropertyChanged(nameof(GameDataFoundVisibility));
                            OnPropertyChanged(nameof(ProgressBarVisibility));
                            OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                            OnPropertyChanged(nameof(ExtractionProgress));
                        });
                    });
                }
                break;
            }
        }
    }
}
