using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Contains helper methods for dealing with documents.
    /// </summary>
    public class Documents
    {
        /// <summary>
        /// Gets the current text document.
        /// </summary>
        public async Task<ITextDocument?> GetCurrentDocumentAsync()
        {
            IWpfTextView? view = await GetCurrentTextViewAsync();
            return view?.TextBuffer?.GetTextDocument();
        }

        /// <summary>
        /// Gets the text document from an open file.
        /// </summary>
        public async Task<ITextDocument?> GetOpenDocumentAsync(string file)
        {
            WindowFrame? frame = await FindFrameAsync(file);

            if (frame != null)
            {
                IVsEditorAdaptersFactoryService? editorAdapter = await VS.GetMefServiceAsync<IVsEditorAdaptersFactoryService>();
                IVsTextView? viewAdapter = VsShellUtilities.GetTextView(frame);
                IWpfTextView? view = editorAdapter?.GetWpfTextView(viewAdapter);
                return view?.TextBuffer?.GetTextDocument();
            }

            return null;
        }

        /// <summary>Gets the WPF text view from the currently active document.</summary>
        public async Task<IWpfTextView?> GetCurrentTextViewAsync()
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

        /// <summary>
        ///  <see langword="true"/> if the document is open with the given logical view
        /// </summary>
        /// <param name="fileName">The absolute file path.</param>
        public async Task<bool> IsOpenAsync(string fileName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return VsShellUtilities.IsDocumentOpen(ServiceProvider.GlobalProvider, fileName, Guid.Empty, out _, out _, out _);
        }

        /// <summary>
        /// Find the open window frame hosting the specified file.
        /// </summary>
        /// <returns><see langword="null"/> if the file isn't open.</returns>
        public async Task<WindowFrame?> FindFrameAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.IsDocumentOpen(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);

            return ToWindowFrame(frame);
        }

        /// <summary>
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public async Task<WindowFrame?> OpenAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);
            return ToWindowFrame(frame);
        }

        /// <summary>
        /// Opens the file via the project instead of as a misc file.
        /// </summary>
        public async Task<WindowFrame?> OpenViaProjectAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShellOpenDocument openDoc = await VS.GetRequiredServiceAsync<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();

            Guid viewGuid = VSConstants.LOGVIEWID_TextView;
            if (ErrorHandler.Succeeded(openDoc.OpenDocumentViaProject(file, ref viewGuid, out _, out _, out _, out IVsWindowFrame frame)))
            {
                if (frame != null)
                {
                    frame.Show();
                    return new WindowFrame(frame);
                }
            }

            return null;
        }

        /// <summary>
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public async Task<WindowFrame?> OpenInPreviewTabAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE2.NDS_TryProvisional, VSConstants.NewDocumentStateReason.Navigation))
            {
                VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);
                return ToWindowFrame(frame);
            }
        }

        private static WindowFrame? ToWindowFrame(IVsWindowFrame frame)
        {
            return frame != null ? new WindowFrame(frame) : null;
        }
    }
}
