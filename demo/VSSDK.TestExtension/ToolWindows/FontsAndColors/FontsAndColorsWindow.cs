using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;

namespace TestExtension
{
    public class FontsAndColorsWindow : BaseToolWindow<FontsAndColorsWindow>
    {
        public override string GetTitle(int toolWindowId) => "Fonts and Colors Window";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return new FontsAndColorsWindowControl
            {
                DataContext = new FontsAndColorsWindowViewModel(
                    await VS.FontsAndColors.GetConfiguredFontAndColorsAsync<DemoFontAndColorCategory>(),
                    Package.JoinableTaskFactory
                )
            };
        }

        [Guid("b7141d35-7b95-4ad0-a37d-58220c1aa5e3")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ColorDialog;
            }

            protected override void Dispose(bool disposing)
            {
                ((Content as FontsAndColorsWindowControl).DataContext as IDisposable)?.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
