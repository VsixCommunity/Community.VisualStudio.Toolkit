using System;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;

namespace TestExtension
{
    public class ThemeWindowControlViewModel : ObservableObject
    {
        private bool _useVsTheme = true;

        public bool UseVsTheme
        {
            get { return _useVsTheme; }
            set { SetProperty(ref _useVsTheme, value); }
        }

#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
        public ICommand OpenDialogCommand => new DelegateCommand(() => OpenDialog());
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed

        private void OpenDialog()
        {
            var dialog = new ThemeWindowDialog { DataContext = new ThemeWindowDialogViewModel { UseVsTheme = UseVsTheme } };
            dialog.ShowModal();
        }
    }
}
