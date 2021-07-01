using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        public async Task<DocumentView?> GetActiveDocumentViewAsync()
        {
            IVsTextView? viewAdapter = await GetActiveNativeTextViewAsync();

            if (viewAdapter != null)
            {
                return await viewAdapter.ToDocumentViewAsync();
            }

            return null;
        }

        /// <summary>
        /// Gets the document view from an open file.
        /// </summary>
        public async Task<DocumentView?> GetDocumentViewAsync(string file)
        {
            IVsTextView? nativeView = await GetNativeTextViewAsync(file);

            if (nativeView != null)
            {
                return await nativeView.ToDocumentViewAsync();
            }

            return null;
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
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public async Task<DocumentView?> OpenAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);
            IVsTextView? nativeView = VsShellUtilities.GetTextView(frame);
            return await nativeView.ToDocumentViewAsync();
        }

        /// <summary>
        /// Opens the file via the project instead of as a misc file.
        /// </summary>
        public async Task<DocumentView?> OpenViaProjectAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShellOpenDocument openDoc = await VS.GetRequiredServiceAsync<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();

            Guid viewGuid = VSConstants.LOGVIEWID_TextView;
            if (ErrorHandler.Succeeded(openDoc.OpenDocumentViaProject(file, ref viewGuid, out _, out _, out _, out IVsWindowFrame frame)))
            {
                IVsTextView? nativeView = VsShellUtilities.GetTextView(frame);

                if (nativeView != null)
                {
                    return await nativeView.ToDocumentViewAsync();
                }
            }

            return null;
        }

        /// <summary>
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public async Task<DocumentView?> OpenInPreviewTabAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE2.NDS_TryProvisional, VSConstants.NewDocumentStateReason.Navigation))
            {
                VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);
                IVsTextView? nativeView = VsShellUtilities.GetTextView(frame);
                return await nativeView.ToDocumentViewAsync();
            }
        }

        /// <summary>Gets the native text view from the currently active document.</summary>
        private async Task<IVsTextView?> GetActiveNativeTextViewAsync()
        {
            IVsTextManager textManager = await VS.GetRequiredServiceAsync<SVsTextManager, IVsTextManager>();
            textManager.GetActiveView(1, null, out IVsTextView activeView);

            return activeView;
        }

        /// <summary>Gets the native text view from the currently active document.</summary>
        private async Task<IVsTextView?> GetNativeTextViewAsync(string file)
        {
            WindowFrame? frame = await VS.Windows.FindDocumentWindowAsync(file);
            return frame != null ? VsShellUtilities.GetTextView(frame) : null;
        }
    }
}
