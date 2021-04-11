using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    public class RunnerWindow : BaseToolWindow<RunnerWindow>
    {
        public override string GetTitle(int toolWindowId) => "Runner Window";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            // Simulate long running background task
            await Task.Delay(2000);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            DTE2 dte = await VS.GetDTEAsync();
            return new RunnerWindowControl(dte);
        }

        [Guid("d3b3ebd9-87d1-41cd-bf84-268d88953417")]
        public class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.StatusInformation;
            }
        }
    }
}
