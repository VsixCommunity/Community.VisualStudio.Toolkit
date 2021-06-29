using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the editor.</summary>
    public partial class Editor
    {
        internal Editor()
        { }

        /// <summary>Gets the WPF text view from the currently active document.</summary>
        public async Task<IWpfTextView?> GetCurrentWpfTextViewAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsEditorAdaptersFactoryService? editorAdapter = await VS.GetMefServiceAsync<IVsEditorAdaptersFactoryService>();
            IVsTextView viewAdapter = await GetCurrentNativeTextViewAsync();

            return editorAdapter?.GetWpfTextView(viewAdapter);
        }

        /// <summary>Gets the native text view from the currently active document.</summary>
        public async Task<IVsTextView> GetCurrentNativeTextViewAsync()
        {
            IVsTextManager textManager = await VS.GetRequiredServiceAsync<SVsTextManager, IVsTextManager>();
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));

            return activeView;
        }
    }
}
