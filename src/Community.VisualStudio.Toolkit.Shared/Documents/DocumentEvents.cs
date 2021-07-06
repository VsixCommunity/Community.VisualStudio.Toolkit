using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public DocumentEvents DocumentEvents { get; } = new();
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
        public event EventHandler<string>? Saved;

        /// <summary>
        /// Fires after the document was opened in the editor.
        /// </summary>
        public event EventHandler<string>? Opened;

        /// <summary>
        /// Fires after the document was closed.s
        /// </summary>
        public event EventHandler<string>? Closed;

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (Opened != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Opened.Invoke(this, file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (Closed != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Closed!.Invoke(this, file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            if (Saved != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Saved?.Invoke(this, file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }
    }
}
