using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
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

        /// <summary>Gets an instance of <see cref="TextDocument"/> from the currently active document.</summary>
        public async Task<TextDocument?> GetActiveTextDocumentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 dte = await VS.GetDTEAsync();
            return dte.ActiveDocument.Object("TextDocument") as TextDocument;
        }

        /// <summary>Gets the WPF text view from the currently active document.</summary>
        public async Task<IWpfTextView?> GetCurrentWpfTextViewAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IComponentModel2 compService = await VS.GetServiceAsync<SComponentModel, IComponentModel2>();
            IVsEditorAdaptersFactoryService? editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            IVsTextView viewAdapter = await GetCurrentNativeTextViewAsync();

            return editorAdapter.GetWpfTextView(viewAdapter);
        }

        /// <summary>Gets the native text view from the currently active document.</summary>
        public async Task<IVsTextView> GetCurrentNativeTextViewAsync()
        {
            IVsTextManager textManager = await VS.GetServiceAsync<SVsTextManager, IVsTextManager>();
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));

            return activeView;
        }
    }
}
