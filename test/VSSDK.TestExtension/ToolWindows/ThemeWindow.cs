using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    public class ThemeWindow : BaseToolWindow<ThemeWindow>
    {
        public override string GetTitle(int toolWindowId) => "Theme Window";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new ThemeWindowControl { DataContext = new ThemeWindowControlViewModel() });
        }

        [Guid("e3be6dd3-f017-4d6e-ae88-2b29319a77a2")]
        public class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ColorPalette;
            }
        }
    }
}
