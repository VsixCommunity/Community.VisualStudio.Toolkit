using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to settings.</summary>
    public class Settings
    {
        internal Settings()
        { }

        /// <summary>
        /// Opens the Tools -> Options dialog to the specified page.
        /// </summary>
        public Task OpenAsync<T>() where T : DialogPage 
            => OpenAsync(typeof(T));

        /// <summary>
        /// Opens the Tools -> Options dialog to the specified page.
        /// </summary>
        public Task OpenAsync(Type dialogPageType) 
            => OpenAsync(dialogPageType.GUID);

        /// <summary>
        /// Opens the Tools -> Options dialog to the specified page.
        /// </summary>
        public Task OpenAsync(Guid dialogPageGuid) 
            => VS.Commands.ExecuteAsync(VSConstants.VSStd97CmdID.ToolsOptions, dialogPageGuid.ToString());

        /// <summary>
        /// Opens the Tools -> Options dialog to the specified page.
        /// </summary>
        public Task OpenAsync(string dialogPageGuid)
        {
            return VS.Commands.ExecuteAsync(VSConstants.VSStd97CmdID.ToolsOptions, dialogPageGuid);
        }
    }
}
