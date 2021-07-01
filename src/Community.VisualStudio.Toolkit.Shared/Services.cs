using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
#if VS16 || VS17
using Microsoft.VisualStudio.TaskStatusCenter;
#endif

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A collection of services commonly used by extensions.
    /// </summary>
    public class Services
    {
        internal Services()
        { }

        #region Selection
        /// <summary>
        /// Provides access to the selection API.
        /// </summary>
        public Task<IVsMonitorSelection> GetMonitorSelectionAsync() => VS.GetRequiredServiceAsync<SVsShellMonitorSelection, IVsMonitorSelection>();
        #endregion

        #region Solution
        /// <summary>
        /// Provides top-level manipulation or maintenance of the solution.
        /// </summary>
        public Task<IVsSolution> GetSolutionAsync() => VS.GetRequiredServiceAsync<SVsSolution, IVsSolution>();

        /// <summary>
        /// Opens a Solution or Project using the standard open dialog boxes.
        /// </summary>
        public Task<IVsOpenProjectOrSolutionDlg> GetOpenProjectOrSolutionDlgAsync() => VS.GetRequiredServiceAsync<SVsOpenProjectOrSolutionDlg, IVsOpenProjectOrSolutionDlg>();
        #endregion

        #region Shell
        /// <summary>Provides access to the fundamental environment services, specifically those dealing with VSPackages and the registry.</summary>
        public Task<IVsShell> GetShellAsync() => VS.GetRequiredServiceAsync<SVsShell, IVsShell>();

        /// <summary>This interface provides access to basic windowing functionality, including access to and creation of tool windows and document windows.</summary>
        public Task<IVsUIShell> GetUIShellAsync() => VS.GetRequiredServiceAsync<SVsUIShell, IVsUIShell>();

        /// <summary>This interface is used by a package to read command-line switches entered by the user.</summary>
        public Task<IVsAppCommandLine> GetAppCommandLineAsync() => VS.GetRequiredServiceAsync<SVsAppCommandLine, IVsAppCommandLine>();

        /// <summary>Registers well-known images (such as icons) for Visual Studio.</summary>
        /// <returns>Cast return object to <see cref="IVsImageService2"/></returns>
        public Task<object> GetImageServiceAsync() => VS.GetRequiredServiceAsync<SVsImageService, object>();

        /// <summary>Controls the caching of font and color settings.</summary>
        public Task<IVsFontAndColorCacheManager> GetFontAndColorCacheManagerAsync() => VS.GetRequiredServiceAsync<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();

        /// <summary>Allows a VSPackage to retrieve or save font and color data to the registry.</summary>
        public Task<IVsFontAndColorStorage> GetFontAndColorStorageAsync() => VS.GetRequiredServiceAsync<SVsFontAndColorStorage, IVsFontAndColorStorage>();

        /// <summary>Controls the most recently used (MRU) items collection.</summary>
        /// <returns>Cast return object to <see cref="IVsMRUItemsStore"/></returns>
        public Task<object> GetMRUItemsStoreAsync() => VS.GetRequiredServiceAsync<SVsMRUItemsStore, object>();

        /// <summary>Used to retrieved services defined in the MEF catalog, such as the editor specific services like <see cref="IVsEditorAdaptersFactoryService"/>.</summary>
        public Task<IComponentModel2> GetComponentModelAsync() => VS.GetRequiredServiceAsync<SComponentModel, IComponentModel2>();
        #endregion

        #region Notifications
        /// <summary>Provides access to the environment's status bar.</summary>
        public Task<IVsStatusbar> GetStatusBarAsync() => VS.GetRequiredServiceAsync<SVsStatusbar, IVsStatusbar>();

        /// <summary>The <see cref="InfoBar"/> is often referred to as the 'yellow' or 'gold' bar.</summary>
        /// <returns>Cast return object to <see cref="IVsInfoBarUIFactory"/>.</returns>
        public Task<object> GetInfoBarUIFactoryAsync() => VS.GetRequiredServiceAsync<SVsInfoBarUIFactory, object>();

#if VS16 || VS17
        /// <summary>The Task Status Center is used to run background tasks and is located in the left-most side of the status bar.</summary>
        /// <remarks>This is only available for Visual Studio 2019 (16.0).</remarks>
        public Task<IVsTaskStatusCenterService> GetTaskStatusCenterAsync() => VS.GetRequiredServiceAsync<SVsTaskStatusCenterService, IVsTaskStatusCenterService>();
#endif

        /// <summary>Used for background tasks that needs to block the UI if they take longer than the specified seconds.</summary>
        /// <returns>Cast return object to <see cref="IVsThreadedWaitDialogFactory"/>.</returns>
        public Task<object> GetThreadedWaitDialogAsync() => VS.GetRequiredServiceAsync<SVsThreadedWaitDialogFactory, object>();

        /// <summary>Used to write log messaged to the ActivityLog.xml file.</summary>
        public Task<IVsActivityLog> GetActivityLogAsync() => VS.GetRequiredServiceAsync<SVsActivityLog, IVsActivityLog>();
        #endregion

        #region Debugger
        /// <summary>Provides access to the current debugger so that the package can listen for debugger events.</summary>
        public Task<IVsDebugger> GetDebuggerAsync() => VS.GetRequiredServiceAsync<IVsDebugger, IVsDebugger>();

        /// <summary>Allows clients to add to the debuggable protocol list.`</summary>
        public Task<IVsDebuggableProtocol> GetDebuggableProtocolAsync() => VS.GetRequiredServiceAsync<SVsDebuggableProtocol, IVsDebuggableProtocol>();
        #endregion

        #region Commands
        /// <summary>Provides methods to manage the global designer verbs and menu commands available in design mode, and to show some types of shortcut menus.</summary>
        public Task<IMenuCommandService> GetCommandServiceAsync() => VS.GetRequiredServiceAsync<IMenuCommandService, IMenuCommandService>();

        /// <summary>Used to register and unregister a command target as a high priority command handler.</summary>
        public Task<IVsRegisterPriorityCommandTarget> GetPriorityCommandTargetAsync() => VS.GetRequiredServiceAsync<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();
        #endregion

        #region Build
        /// <summary>
        /// A service for handling solution builds.
        /// </summary>
        public Task<IVsSolutionBuildManager> GetSolutionBuildManagerAsync() => VS.GetRequiredServiceAsync<SVsSolutionBuildManager, IVsSolutionBuildManager>();
        #endregion
    }
}
