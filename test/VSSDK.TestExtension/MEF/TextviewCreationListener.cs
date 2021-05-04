using System;
using System.ComponentModel.Composition;
using Community.VisualStudio.Toolkit.Shared;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace TestExtension.MEF
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class TextViewCreationListener : IWpfTextViewCreationListener, IDisposable
    {
        private readonly ToolkitThreadHelper _threadHelper = new ToolkitThreadHelper(ThreadHelper.JoinableTaskContext);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _threadHelper.Dispose();
            }
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            // Note: RunAsync is not awaited/joined
            _threadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await _threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_threadHelper.DisposalToken);

                    // Do work, e.g.
                    // await MyMethodAsync(_threadHelper.DisposalToken);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            });
        }
    }
}
