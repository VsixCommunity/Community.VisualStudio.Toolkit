using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    public class RunnerWindow : BaseToolWindow<RunnerWindow>
    {
        public override string GetTitle(int toolWindowId) => "Runner Window";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            Version version = await VS.Shell.GetVsVersionAsync();
            RunnerWindowMessenger messenger = await Package.GetServiceAsync<RunnerWindowMessenger, RunnerWindowMessenger>();
            return new RunnerWindowControl(version, messenger);
        }

        [Guid("d3b3ebd9-87d1-41cd-bf84-268d88953417")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.StatusInformation;
                ToolBar = new CommandID(PackageGuids.TestExtension, PackageIds.RunnerWindowToolbar);
                WindowFrameAvailable += (_, _) => Debug.WriteLine("RunnerWindow frame is now available");
            }

            public override void OnToolWindowCreated()
            {
                base.OnToolWindowCreated();
                GetWindowFrame().OnShow += (_, args) => Debug.WriteLine($"RunnerWindow state changed: {args.Reason}");
            }
        }
    }
}
