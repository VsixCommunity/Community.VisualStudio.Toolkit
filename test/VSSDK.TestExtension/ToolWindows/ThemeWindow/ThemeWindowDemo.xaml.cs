using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    public partial class ThemeWindowDemo : UserControl, INotifyPropertyChanged
    {
        private double _progress;

        public ThemeWindowDemo()
        {
            InitializeComponent();
            UpdateProgressAsync().FireAndForget();
        }

        private async Task UpdateProgressAsync()
        {
            while (true)
            {
                await Task.Delay(250).ConfigureAwait(true);
                Progress = (Progress + 5) % 101;
            }
        }

        public IEnumerable<string> ListItems { get; } = new string[] {
                "First",
                "Second",
                "Third",
                "Fourth",
                "Fifth",
                "Sixth"
        };

        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        private void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
