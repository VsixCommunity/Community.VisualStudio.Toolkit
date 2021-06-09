using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to settings.</summary>
    public class Settings
    {
        internal Settings()
        { }

        /// <summary>Provides access to the settings manager.</summary>
        /// /// <returns>Cast return object to <see cref="IVsSettingsManager"/></returns>
        public Task<object> GetSettingsManagerAsync() => VS.GetRequiredServiceAsync<SVsSettingsManager, object>();

        /// <summary>Manages a Tools Options dialog box. The environment implements this interface.</summary>
        public Task<IVsToolsOptions> GetToolsOptionsAsync() => VS.GetRequiredServiceAsync<SVsToolsOptions, IVsToolsOptions>();
    }
}
