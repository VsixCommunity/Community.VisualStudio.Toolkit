using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Services related to the selection of windows and nodes.
    /// </summary>
    public class Selection
    {
        internal Selection()
        { }

        /// <summary>
        /// Provides access to the selection API.
        /// </summary>
        public Task<IVsMonitorSelection> GetMonitorSelectionAsync() => VS.GetRequiredServiceAsync<SVsShellMonitorSelection, IVsMonitorSelection>();
    }
}
