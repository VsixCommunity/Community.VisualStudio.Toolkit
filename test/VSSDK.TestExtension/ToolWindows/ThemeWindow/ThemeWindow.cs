using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace TestExtension
{
    public class ThemeWindow : BaseToolWindow<ThemeWindow>
    {
        public override string GetTitle(int toolWindowId) => "Theme Window";

        public override Type PaneType => typeof(Pane);

        public async override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            Guid currentTheme = await GetCurrentThemeAsync(cancellationToken);

            return new ThemeWindowControl { DataContext = new ThemeWindowControlViewModel(currentTheme) };
        }

        private async Task<Guid> GetCurrentThemeAsync(CancellationToken cancellationToken)
        {
            const string COLLECTION_NAME = @"ApplicationPrivateSettings\Microsoft\VisualStudio";
            const string PROPERTY_NAME = "ColorTheme";

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var manager = (IVsSettingsManager)await VS.Services.GetSettingsManagerAsync();
            SettingsStore store = new ShellSettingsManager(manager).GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (store.CollectionExists(COLLECTION_NAME))
            {
                if (store.PropertyExists(COLLECTION_NAME, PROPERTY_NAME))
                {
                    // The value is made up of three parts, separated
                    // by a star. The third part is the GUID of the theme.
                    var parts = store.GetString(COLLECTION_NAME, PROPERTY_NAME).Split('*');
                    if (parts.Length == 3)
                    {
                        if (Guid.TryParse(parts[2], out Guid value))
                        {
                            return value;
                        }
                    }
                }
            }

            return Guid.Empty;
        }

        [Guid("e3be6dd3-f017-4d6e-ae88-2b29319a77a2")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ColorPalette;
            }
        }
    }
}
