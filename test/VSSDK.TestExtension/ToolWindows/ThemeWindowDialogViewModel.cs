using Microsoft.VisualStudio.PlatformUI;

namespace TestExtension
{
    public class ThemeWindowDialogViewModel : ObservableObject
    {
        private bool _useVsTheme = true;

        public bool UseVsTheme
        {
            get { return _useVsTheme; }
            set { SetProperty(ref _useVsTheme, value); }
        }
    }
}
