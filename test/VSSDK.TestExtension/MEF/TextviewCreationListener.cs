using System.ComponentModel.Composition;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.MEF
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class TextViewCreationListener : WpfTextViewCreationListener
    {
        protected override Task CreatedAsync(IWpfTextView textView, ITextDocument document)
        {
            // Do your async work here
            return Task.CompletedTask;
        }

        protected override void FileActionOccurred(TextDocumentFileActionEventArgs e)
        {
            VS.Notifications.SetStatusbarTextAsync($"File Action: {e.FileActionType}").FireAndForget();
        }

        protected override void DirtyStateChanged()
        {
            VS.Notifications.SetStatusbarTextAsync($"Dirty state changed").FireAndForget();
        }

        protected override void EncodingChanged(EncodingChangedEventArgs e)
        {
            VS.Notifications.SetStatusbarTextAsync($"Encoding chaged from {e.OldEncoding} to: {e.OldEncoding}").FireAndForget();
        }

        protected override void Closed(IWpfTextView textView)
        {
            VS.Notifications.SetStatusbarTextAsync("Document closed").FireAndForget();
        }
    }
}
