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
    public class MultiInstanceWindow : BaseToolWindow<MultiInstanceWindow>
    {
        public override string GetTitle(int toolWindowId) => $"Multi Instance Window #{toolWindowId}";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new MultiInstanceWindowControl(toolWindowId));
        }

        [Guid("13dccc25-9d1d-417d-8525-40c4c14ff0a2")]
        public class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.MultiView;
            }
        }
    }
}
