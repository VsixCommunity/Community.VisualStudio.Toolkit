using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        private DocumentEvents? _documentEvents;

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public DocumentEvents DocumentEvents => _documentEvents ??= new();
    }

    /// <summary>
    /// Events related to the editor documents.
    /// </summary>
    public class DocumentEvents : IVsRunningDocTableEvents
    {
        private readonly RunningDocumentTable _rdt;

        internal DocumentEvents()
        {
            _rdt = new RunningDocumentTable();
            _rdt.Advise(this);
        }

        /// <summary>
        /// Happens when a file is saved to disk.
        /// </summary>
        public event Action<string>? Saved;

        /// <summary>
        /// Fires after the document was opened in the editor.
        /// </summary>
        public event Action<string>? Opened;

        /// <summary>
        /// Fires after the document was closed.s
        /// </summary>
        public event Action<string>? Closed;

        /// <summary>
        /// Fires before the document takes focus.
        /// </summary>
        public event Action<DocumentView>? BeforeDocumentWindowShow;

        /// <summary>
        /// Fires after the document lost focus.
        /// </summary>
        public event Action<DocumentView>? AfterDocumentWindowHide;

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (Opened != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Opened.Invoke(file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (Closed != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;

                if (!string.IsNullOrEmpty(file))
                {
                    Closed.Invoke(file);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            if (Saved != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Saved?.Invoke(file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (BeforeDocumentWindowShow != null)
            {
                DocumentView docView = new(pFrame);
                BeforeDocumentWindowShow.Invoke(docView);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            if (AfterDocumentWindowHide != null)
            {
                DocumentView docView = new(pFrame);
                AfterDocumentWindowHide.Invoke(docView);
            }

            return VSConstants.S_OK;
        }
    }
}
