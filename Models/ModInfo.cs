using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CortexCommandModManager.Models
{
    public class ModInfo : INotifyPropertyChanged
    {
        private bool _isInstalled;

        public string Name { get; set; }
        public string ArchivePath { get; set; }
        public string RteFolderName { get; set; }
        
        // Path to the installed .rte folder in the game directory (if installed)
        public string InstalledPath { get; set; }

        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                if (_isInstalled != value)
                {
                    _isInstalled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IconPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
