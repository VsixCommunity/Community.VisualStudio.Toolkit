using System;
using System.ComponentModel.Composition;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.Shared;
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
            base.FileActionOccurred(e);
        }
    }
}
