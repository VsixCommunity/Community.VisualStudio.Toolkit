using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Events related to the selection in Visusal Studio.
    /// </summary>
    public class SelectionEvents : IVsSelectionEvents
    {
        internal SelectionEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var monitor = (IVsMonitorSelection)ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection));
            Assumes.Present(monitor);
            monitor.AdviseSelectionEvents(this, out _);
        }

        /// <summary>
        /// Fires when the selection changes
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        /// <summary>
        /// Fires when the UI Context changes.
        /// </summary>
        public event EventHandler<UIContextChangedEventArgs>? UIContextChanged;

        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                SolutionItem? from = await SolutionItem.FromHierarchyAsync(pHierOld, itemidOld);
                SolutionItem? to = await SolutionItem.FromHierarchyAsync(pHierNew, itemidNew);

                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(from, to));
            }).FireAndForget();

            return VSConstants.S_OK;
        }

        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            UIContextChanged?.Invoke(this, new UIContextChangedEventArgs(fActive == 1));
            return VSConstants.S_OK;
        }
    }

    /// <summary>
    /// EventArgs for when the selection changes in Visual Studio
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the EventArgs.
        /// </summary>
        public SelectionChangedEventArgs(SolutionItem? from, SolutionItem? to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// What the selection was before the change.
        /// </summary>
        public SolutionItem? From { get; }

        /// <summary>
        /// What the selection is currently after the change.
        /// </summary>
        public SolutionItem? To { get; }
    }
}
