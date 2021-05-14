using System;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit.Shared.Helpers;
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
        public Task<IVsCallBrowser> GetCallBrowserAsync() => VS.GetServiceAsync<SVsCodeWindow, IVsCallBrowser>();

        /// <summary>Allows navigation to an object in Class View.</summary>
        public Task<IVsClassView> GetClassViewAsync() => VS.GetServiceAsync<SVsClassView, IVsClassView>();

        /// <summary>Represents a multiple-document interface (MDI) child that contains one or more code views.</summary>
        public Task<IVsCodeWindow> GetCodeWindowAsync() => VS.GetServiceAsync<SVsCodeWindow, IVsCodeWindow>();

        /// <summary>Enables the package to use the Command Window.</summary>
        public Task<IVsCommandWindow> GetCommandWindowAsync() => VS.GetServiceAsync<SVsCommandWindow, IVsCommandWindow>();

        /// <summary>Implemented by the environment. Used by VsPackages that want to manipulate Object Browser.</summary>
        public Task<IVsObjBrowser> GetObjectBrowserAsync() => VS.GetServiceAsync<SVsObjBrowser, IVsObjBrowser>();

        /// <summary>Manages and controls functions specific to the Output tool window that has multiple panes.</summary>
        public Task<IVsOutputWindow> GetOutputWindowAsync() => VS.GetServiceAsync<SVsOutputWindow, IVsOutputWindow>();

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
        /// If the General pane does not already exist then it will be created, but that is not
        /// the case for Build or Debug.
        /// </summary>
        /// <param name="pane">The Visual Studio pane to get.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public Task<OutputWindowPane> GetOutputWindowPaneAsync(VSOutputWindowPane pane) => OutputWindowPane.GetAsync(pane);

        /// <summary>
        /// Gets an existing Output window pane.
        /// Throws if a pane with the specified guid does not exist.
        /// </summary>
        /// <param name="guid">The pane's unique identifier.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public Task<OutputWindowPane> GetOutputWindowPaneAsync(Guid guid) => OutputWindowPane.GetAsync(guid);

        /// <summary>Manages lists of task items supplied by task providers.</summary>
        public Task<IVsTaskList> GetTaskListAsync() => VS.GetServiceAsync<SVsTaskList, IVsTaskList>();

        /// <summary>Used to manage the Toolbox.</summary>
        public Task<IVsToolbox2> GetToolboxAsync() => VS.GetServiceAsync<SVsToolbox, IVsToolbox2>();
    }
}