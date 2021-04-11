using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the command system.</summary>
    public class Shell
    {
        internal Shell()
        { }

        /// <summary>Provides access to the fundamental environment services, specifically those dealing with VSPackages and the registry.</summary>
        public Task<IVsShell> GetShellAsync() => VS.GetServiceAsync<SVsShell, IVsShell>();

        /// <summary>This interface provides access to basic windowing functionality, including access to and creation of tool windows and document windows.</summary>
        public Task<IVsUIShell> GetUIShellAsync() => VS.GetServiceAsync<SVsUIShell, IVsUIShell>();

        /// <summary>This interface is used by a package to read command-line switches entered by the user.</summary>
        public Task<IVsAppCommandLine> GetAppCommandLineAsync() => VS.GetServiceAsync<SVsAppCommandLine, IVsAppCommandLine>();

        /// <summary>Registers well-known images (such as icons) for Visual Studio.</summary>
        /// <returns>Cast return object to <see cref="IVsImageService2"/></returns>
        public Task<object> GetImageServiceAsync() => VS.GetServiceAsync<SVsImageService, object>();

        /// <summary>Controls the caching of font and color settings.</summary>
        public Task<IVsFontAndColorCacheManager> GetFontAndColorCacheManagerAsync() => VS.GetServiceAsync<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();

        /// <summary>Allows a VSPackage to retrieve or save font and color data to the registry.</summary>
        public Task<IVsFontAndColorStorage> GetFontAndColorStorageAsync() => VS.GetServiceAsync<SVsFontAndColorStorage, IVsFontAndColorStorage>();

        /// <summary>Manages a Tools Options dialog box. The environment implements this interface.</summary>
        public Task<IVsToolsOptions> GetToolsOptionsAsync() => VS.GetServiceAsync<SVsToolsOptions, IVsToolsOptions>();

        /// <summary>Controls the most recently used (MRU) items collection.</summary>
        /// <returns>Cast return object to <see cref="IVsMRUItemsStore"/></returns>
        public Task<object> GetMRUItemsStoreAsync() => VS.GetServiceAsync<SVsMRUItemsStore, object>();

        /// <summary>Used to retrieved services defined in the MEF catalog, such as the editor specific services like <see cref="IVsEditorAdaptersFactoryService"/>.</summary>
        public Task<IComponentModel2> GetComponentModelAsync() => VS.GetServiceAsync<SComponentModel, IComponentModel2>();

        /// <summary>
        /// Opens the file via the project instead of as a misc file.
        /// </summary>
        public async Task OpenDocumentViaProjectAsync(string fileName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShellOpenDocument openDoc = await VS.GetServiceAsync<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();

            System.Guid viewGuid = VSConstants.LOGVIEWID_TextView;
            if (ErrorHandler.Succeeded(openDoc.OpenDocumentViaProject(fileName, ref viewGuid, out _, out _, out _, out IVsWindowFrame frame)))
            {
                if (frame != null)
                {
                    frame.Show();
                }
            }
        }
    }
}
