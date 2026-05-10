using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CortexCommandModManager.Models;
using CortexCommandModManager.Services;
using CortexCommandModManager.Helpers;

namespace CortexCommandModManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ModInfo> _mods = new();
        private string _statusMessage = "";
        private bool _isBusy;
        private string _gamePath = "";
        private bool _isSettingsOpen;
        private AppConfig _appConfig;
        private string _autoSteamPath;
        private string _autoGamePath;
        
        public LocalizationService Strings => LocalizationService.Instance;

        public ObservableCollection<ModInfo> Mods
        {
            get => _mods;
            set { _mods = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set { _isSettingsOpen = value; OnPropertyChanged(); }
        }

        public string ResolvedSteamPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_appConfig.ManualSteamPath)) return _appConfig.ManualSteamPath;
                return string.IsNullOrEmpty(_autoSteamPath) ? Strings["PathNotFound"] : _autoSteamPath;
            }
            set
            {
                if (value == Strings["PathNotFound"] || value == _autoSteamPath)
                {
                    _appConfig.ManualSteamPath = string.Empty;
                }
                else
                {
                    _appConfig.ManualSteamPath = value;
                }
                SaveAppConfig();
                OnPropertyChanged();
            }
        }

        public string ResolvedGamePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_appConfig.ManualGamePath)) return _appConfig.ManualGamePath;
                return string.IsNullOrEmpty(_autoGamePath) ? Strings["PathNotFound"] : _autoGamePath;
            }
            set
            {
                if (value == Strings["PathNotFound"] || value == _autoGamePath)
                {
                    _appConfig.ManualGamePath = string.Empty;
                }
                else
                {
                    _appConfig.ManualGamePath = value;
                }
                SaveAppConfig();
                OnPropertyChanged();
            }
        }

        public ICommand LoadModsCommand { get; }
        public ICommand ToggleModCommand { get; }
        public ICommand EnableAllCommand { get; }
        public ICommand DisableAllCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand CloseSettingsCommand { get; }
        public ICommand ResetPathsCommand { get; }
        public ICommand ClearConfigCommand { get; }
        public ICommand ChangeLanguageCommand { get; }

        public MainViewModel()
        {
            _appConfig = AppConfigManager.Load();
            Strings.CurrentLanguage = _appConfig.Language;

            LoadModsCommand = new RelayCommand(async _ => await LoadModsAsync(), _ => !IsBusy);
            ToggleModCommand = new RelayCommand(async param => await ToggleModAsync(param as ModInfo), _ => !IsBusy);
            EnableAllCommand = new RelayCommand(async _ => await ToggleAllModsAsync(true), _ => !IsBusy);
            DisableAllCommand = new RelayCommand(async _ => await ToggleAllModsAsync(false), _ => !IsBusy);
            
            OpenSettingsCommand = new RelayCommand(_ => IsSettingsOpen = true);
            CloseSettingsCommand = new RelayCommand(_ => IsSettingsOpen = false);
            ResetPathsCommand = new RelayCommand(_ => ResetPaths());
            ClearConfigCommand = new RelayCommand(_ => ClearConfig());
            ChangeLanguageCommand = new RelayCommand(lang => ChangeLanguage(lang as string));
            
            _ = LoadModsAsync();
        }

        private void SaveAppConfig()
        {
            AppConfigManager.Save(_appConfig);
        }

        public bool IsRuSelected => _appConfig.Language == "ru";
        public bool IsEnSelected => _appConfig.Language == "en";
        public bool IsArSelected => _appConfig.Language == "ar";
        public bool IsZhSelected => _appConfig.Language == "zh";
        public bool IsJpSelected => _appConfig.Language == "jp";

        private void ChangeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return;
            _appConfig.Language = lang;
            Strings.CurrentLanguage = lang;
            SaveAppConfig();
            OnPropertyChanged(nameof(ResolvedSteamPath));
            OnPropertyChanged(nameof(ResolvedGamePath));
            OnPropertyChanged(nameof(IsRuSelected));
            OnPropertyChanged(nameof(IsEnSelected));
            OnPropertyChanged(nameof(IsArSelected));
            OnPropertyChanged(nameof(IsZhSelected));
            OnPropertyChanged(nameof(IsJpSelected));
        }

        private void ResetPaths()
        {
            _appConfig.ManualSteamPath = string.Empty;
            _appConfig.ManualGamePath = string.Empty;
            SaveAppConfig();
            OnPropertyChanged(nameof(ResolvedSteamPath));
            OnPropertyChanged(nameof(ResolvedGamePath));
            _ = LoadModsAsync();
        }

        private void ClearConfig()
        {
            if (string.IsNullOrEmpty(_gamePath)) return;
            string configPath = Path.Combine(_gamePath, "modmanager_config.json");
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
            _ = LoadModsAsync();
        }

        private async Task LoadModsAsync()
        {
            IsBusy = true;
            StatusMessage = Strings["SearchProgress"];

            try
            {
                await Task.Run(() =>
                {
                    _autoSteamPath = SteamLocator.GetSteamPath();
                    
                    var manualSteam = _appConfig.ManualSteamPath;
                    var manualGame = _appConfig.ManualGamePath;
                    
                    _autoGamePath = SteamLocator.FindCortexCommandPath(manualSteam);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPropertyChanged(nameof(ResolvedSteamPath));
                        OnPropertyChanged(nameof(ResolvedGamePath));
                    });

                    _gamePath = !string.IsNullOrWhiteSpace(manualGame) 
                        ? manualGame 
                        : _autoGamePath;

                    if (string.IsNullOrEmpty(_gamePath) || !Directory.Exists(_gamePath))
                    {
                        Application.Current.Dispatcher.Invoke(() => StatusMessage = Strings["GameNotFound"]);
                        return;
                    }

                    // If manual paths were empty but we found the game automatically, we could optionally set them or just leave them empty.
                    // Leaving them empty is better so "Reset to default" makes sense.

                    var config = ConfigManager.LoadConfig(_gamePath);
                    var workshopFolders = SteamLocator.FindWorkshopModsFolders(manualSteam);
                    var discoveredMods = ModService.FindModsInWorkshop(workshopFolders);

                    foreach (var mod in discoveredMods)
                    {
                        var existing = config.AvailableMods.FirstOrDefault(m => m.ArchivePath == mod.ArchivePath);
                        if (existing != null)
                        {
                            mod.IsInstalled = existing.IsInstalled;
                            mod.InstalledPath = existing.InstalledPath;
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Mods.Clear();
                        foreach (var mod in discoveredMods)
                        {
                            Mods.Add(mod);
                        }
                        StatusMessage = string.Format(Strings["FoundMods"], Mods.Count);
                    });
                });
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(Strings["Error"], ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ToggleAllModsAsync(bool enable)
        {
            if (string.IsNullOrEmpty(_gamePath)) return;
            
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    foreach (var mod in Mods)
                    {
                        if (mod.IsInstalled != enable)
                        {
                            Application.Current.Dispatcher.Invoke(() => StatusMessage = string.Format(enable ? Strings["InstallProgress"] : Strings["UninstallProgress"], mod.Name));
                            if (enable)
                            {
                                ModService.InstallMod(mod, _gamePath);
                            }
                            else
                            {
                                ModService.UninstallMod(mod, _gamePath);
                            }
                        }
                    }
                    var config = new ModManagerConfig { AvailableMods = Mods.ToList() };
                    ConfigManager.SaveConfig(_gamePath, config);
                });

                Application.Current.Dispatcher.Invoke(() => 
                {
                    StatusMessage = string.Format(Strings["FoundMods"], Mods.Count);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => StatusMessage = string.Format(Strings["Error"], ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ToggleModAsync(ModInfo mod)
        {
            if (mod == null || string.IsNullOrEmpty(_gamePath)) return;

            IsBusy = true;
            bool targetState = !mod.IsInstalled;
            
            try
            {
                await Task.Run(() =>
                {
                    if (targetState)
                    {
                        Application.Current.Dispatcher.Invoke(() => StatusMessage = string.Format(Strings["InstallProgress"], mod.Name));
                        ModService.InstallMod(mod, _gamePath);
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => StatusMessage = string.Format(Strings["UninstallProgress"], mod.Name));
                        ModService.UninstallMod(mod, _gamePath);
                    }

                    var config = new ModManagerConfig { AvailableMods = Mods.ToList() };
                    ConfigManager.SaveConfig(_gamePath, config);
                });

                Application.Current.Dispatcher.Invoke(() => 
                {
                    StatusMessage = string.Format(targetState ? Strings["InstallSuccess"] : Strings["UninstallSuccess"], mod.Name);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => 
                {
                    StatusMessage = string.Format(Strings["Error"], ex.Message);
                    mod.IsInstalled = !targetState;
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
