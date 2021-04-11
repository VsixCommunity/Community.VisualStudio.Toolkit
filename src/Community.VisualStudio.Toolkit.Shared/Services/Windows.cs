using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to windows.</summary>
    public class Windows
    {
        internal Windows()
        { }

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

        /// <summary>Manages lists of task items supplied by task providers.</summary>
        public Task<IVsTaskList> GetTaskListAsync() => VS.GetServiceAsync<SVsTaskList, IVsTaskList>();

        /// <summary>Used to manage the Toolbox.</summary>
        public Task<IVsToolbox2> GetToolboxAsync() => VS.GetServiceAsync<SVsToolbox, IVsToolbox2>();
    }
}