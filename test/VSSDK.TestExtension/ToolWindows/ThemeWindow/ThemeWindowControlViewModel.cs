using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    public class ThemeWindowControlViewModel : ObservableObject
    {
        private Theme _selectedTheme;
        private bool _useVsTheme = true;

        public ThemeWindowControlViewModel(Guid currentTheme)
        {
            Themes = new[] {
                new Theme {
                    Name = "Dark",
                    Guid = new Guid("{1DED0138-47CE-435E-84EF-9EC1F439B749}"),
                    ResourceName = "Theme.Dark.vssettings"
                },
                new Theme {
                    Name = "Light",
                    Guid = new Guid("{DE3DBBCD-F642-433C-8353-8F1DF4370ABA}"),
                    ResourceName = "Theme.Light.vssettings"
                },
                new Theme {
                    Name = "Blue",
                    Guid = new Guid("{A4D6A176-B948-4B29-8C66-53C97A1ED7D0}"),
                    ResourceName = "Theme.Blue.vssettings"
                },
                new Theme {
                    Name = "Blue (Extra Contrast)",
                    Guid = new Guid("{CE94D289-8481-498B-8CA9-9B6191A315B9}"),
                    ResourceName = "Theme.BlueHighContrast.vssettings"
                }
            };

            _selectedTheme = Themes.FirstOrDefault((x) => x.Guid.Equals(currentTheme)) ?? Themes[0];
        }

        public IReadOnlyList<Theme> Themes { get; }

        public Theme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                SetProperty(ref _selectedTheme, value);
                ApplyThemeAsync(value).FireAndForget();
            }
        }

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
            ThemeWindowDialog dialog = new ThemeWindowDialog { DataContext = new ThemeWindowDialogViewModel { UseVsTheme = UseVsTheme } };
            dialog.ShowModal();
        }

        private async Task ApplyThemeAsync(Theme theme)
        {
            // There doesn't appear to be a way to set the theme programatically, 
            // but we can change the theme by using a pre-defined settings file and importing those settings.
            // The settings files were created by exporting only the "Options/General/Font and Colors" setting.
            var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n") + ".vssettings");

            using (Stream resource = GetType().Assembly.GetManifestResourceStream($"TestExtension.Resources.{theme.ResourceName}"))
            {
                using (FileStream file = System.IO.File.Open(tempFileName, FileMode.Create, FileAccess.Write))
                {
                    await resource.CopyToAsync(file);
                }
            }

            try
            {
                await KnownCommands.Tools_ImportandExportSettings.ExecuteAsync($@"/import:""{tempFileName}""");
            }
            finally
            {
                System.IO.File.Delete(tempFileName);
            }
        }

        public class Theme
        {
            public string Name { get; set; }
            public Guid Guid { get; set; }
            public string ResourceName { get; set; }
        }

    }
}
