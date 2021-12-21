using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for <c>IWpfTextViewCreationListener</c>
    /// </summary>
    public abstract class WpfTextViewCreationListener : IWpfTextViewCreationListener
    {
        private readonly Lazy<ToolkitThreadHelper> _threadHelper = new(() => ToolkitThreadHelper.Create());

        [Import] internal ITextDocumentFactoryService? _documentService = null;

        /// <summary>
        /// Gets a <see cref="CancellationToken"/> that can be used to check if the class has been disposed.
        /// </summary>
        public CancellationToken DisposalToken => _threadHelper.Value.DisposalToken;

        /// <summary>
        /// The JoinableTaskFactory instance.
        /// </summary>
        public JoinableTaskFactory JoinableTaskFactory => _threadHelper.Value.JoinableTaskFactory;

        /// <summary>
        /// The text document associated with the text view.
        /// </summary>
        public ITextDocument? Document { get; private set; }

        /// <summary>
        /// The text view passed to this <see cref="Created"/> method.
        /// </summary>
        public IWpfTextView? TextView { get; private set; }

        /// <inheritdoc />
        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            TextView = textView;
            textView.Closed += OnViewClosed;

            if (_documentService != null && _documentService.TryGetTextDocument(textView.TextBuffer, out ITextDocument document))
            {
                Document = document;

                DocumentView docView = textView.ToDocumentView();

                Created(docView);

                document.FileActionOccurred += OnFileActionOccurred;
                document.DirtyStateChanged += OnDirtyStateChanged;
                document.EncodingChanged += OnEncodingChanged;
            }
            else
            {
                DocumentView docView = textView.ToDocumentView();
                Created(docView);
            }
        }

        private void OnEncodingChanged(object sender, EncodingChangedEventArgs e)
        {

            EncodingChanged(e);
        }

        private void OnDirtyStateChanged(object sender, EventArgs e)
        {
            DirtyStateChanged();
        }

        private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            FileActionOccurred(e);
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;
            textView.Closed -= OnViewClosed;

            if (Document != null)
            {
                Document.FileActionOccurred -= OnFileActionOccurred;
                Document.DirtyStateChanged -= OnDirtyStateChanged;
                Document.EncodingChanged -= OnEncodingChanged;
            }

            try
            {
                Closed(textView);
            }
            finally
            {
                if (_threadHelper.IsValueCreated)
                {
                    _threadHelper.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// </summary>
        protected virtual void Created(DocumentView docView)
        {
            CreatedAsync(docView).FireAndForget();
        }

        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// </summary>
        protected virtual Task CreatedAsync(DocumentView docView)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is called when the view closes. Use this to clean up references.
        /// </summary>
        protected virtual void Closed(IWpfTextView textView)
        { }

        /// <summary>
        /// Is called when the document encoding changes.
        /// </summary>
        protected virtual void EncodingChanged(EncodingChangedEventArgs e)
        { }

        /// <summary>
        /// Is called when the dirty state of the document changes.
        /// </summary>
        protected virtual void DirtyStateChanged()
        { }

        /// <summary>
        /// Is called when a file action on the document occurs.
        /// </summary>
        protected virtual void FileActionOccurred(TextDocumentFileActionEventArgs e)
        { }
    }
}
