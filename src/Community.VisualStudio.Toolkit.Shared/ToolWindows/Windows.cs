using System;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit.Shared.ExtensionMethods;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to windows.</summary>
    public class Windows
    {
        internal Windows()
        { }

        /// <summary>
        /// Output window panes provided by Visual Studio.
        /// </summary>
        public enum VSOutputWindowPane
        {
            /// <summary>The General pane.</summary>
            General,
            /// <summary>The Build pane.</summary>
            Build,
            /// <summary>The Debug pane.</summary>
            Debug
        }

        /// <summary>Manipulates the Call Browser for debugging.</summary>
        public Task<IVsCallBrowser> GetCallBrowserAsync() => VS.GetRequiredServiceAsync<SVsCodeWindow, IVsCallBrowser>();

        /// <summary>Allows navigation to an object in Class View.</summary>
        public Task<IVsClassView> GetClassViewAsync() => VS.GetRequiredServiceAsync<SVsClassView, IVsClassView>();

        /// <summary>Represents a multiple-document interface (MDI) child that contains one or more code views.</summary>
        public Task<IVsCodeWindow> GetCodeWindowAsync() => VS.GetRequiredServiceAsync<SVsCodeWindow, IVsCodeWindow>();

        /// <summary>Enables the package to use the Command Window.</summary>
        public Task<IVsCommandWindow> GetCommandWindowAsync() => VS.GetRequiredServiceAsync<SVsCommandWindow, IVsCommandWindow>();

        /// <summary>Implemented by the environment. Used by VsPackages that want to manipulate Object Browser.</summary>
        public Task<IVsObjBrowser> GetObjectBrowserAsync() => VS.GetRequiredServiceAsync<SVsObjBrowser, IVsObjBrowser>();

        /// <summary>Manages and controls functions specific to the Output tool window that has multiple panes.</summary>
        public Task<IVsOutputWindow> GetOutputWindowAsync() => VS.GetRequiredServiceAsync<SVsOutputWindow, IVsOutputWindow>();

        /// <summary>
        /// Creates a new Output window pane with the given name.
        /// The pane can be created now or lazily upon the first write to it.
        /// </summary>
        /// <param name="name">The name (title) of the new pane.</param>
        /// <param name="lazyCreate">Whether to lazily create the pane upon first write.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public Task<OutputWindowPane> CreateOutputWindowPaneAsync(string name, bool lazyCreate = true) => OutputWindowPane.CreateAsync(name, lazyCreate);

        /// <summary>
        /// Gets an existing Visual Studio Output window pane (General, Build, Debug).
        /// If the General pane does not already exist then it will be created, but that is not the case
        /// for Build or Debug, in which case the method returns null if the pane doesn't already exist.
        /// </summary>
        /// <param name="pane">The Visual Studio pane to get.</param>
        /// <returns>A new OutputWindowPane or null.</returns>
        public Task<OutputWindowPane?> GetOutputWindowPaneAsync(VSOutputWindowPane pane) => OutputWindowPane.GetAsync(pane);

        /// <summary>
        /// Gets an existing Output window pane.
        /// Returns null if a pane with the specified guid does not exist.
        /// </summary>
        /// <param name="guid">The pane's unique identifier.</param>
        /// <returns>A new OutputWindowPane or null.</returns>
        public Task<OutputWindowPane?> GetOutputWindowPaneAsync(Guid guid) => OutputWindowPane.GetAsync(guid);

        /// <summary>Manages lists of task items supplied by task providers.</summary>
        public Task<IVsTaskList> GetTaskListAsync() => VS.GetRequiredServiceAsync<SVsTaskList, IVsTaskList>();

        /// <summary>Used to manage the Toolbox.</summary>
        public Task<IVsToolbox2> GetToolboxAsync() => VS.GetRequiredServiceAsync<SVsToolbox, IVsToolbox2>();

        /// <summary>Shows a window as a dialog.</summary>
        public async Task<bool?> ShowDialogAsync(Window window, WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner)
            => await window.ShowDialogAsync(windowStartupLocation);
    }
}