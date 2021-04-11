using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to debugging.</summary>
    public class Debugger
    {
        internal Debugger()
        { }

        /// <summary>Provides access to the current debugger so that the package can listen for debugger events.</summary>
        public Task<IVsDebugger> GetDebuggerAsync() => VS.GetServiceAsync<SVsShell, IVsDebugger>();

        /// <summary>Used to launch the debugger.</summary>
        public Task<IVsDebugLaunch> GetDebugLaunchAsync() => VS.GetServiceAsync<SVsDebugLaunch, IVsDebugLaunch>();

        /// <summary>Allows clients to add to the debuggable protocol list.`</summary>
        public Task<IVsDebuggableProtocol> GetDebuggableProtocolAsync() => VS.GetServiceAsync<SVsDebuggableProtocol, IVsDebuggableProtocol>();
    }
}
