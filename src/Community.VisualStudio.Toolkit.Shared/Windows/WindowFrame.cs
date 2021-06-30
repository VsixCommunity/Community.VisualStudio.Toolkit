// ================================================================================================
// WindowFrame.cs
//
// Created: 2008.07.02, by Istvan Novak (DeepDiver)
// ================================================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// This class encapsulates an IVsWindowFrame instance and build functionality around.
    /// </summary>
    public class WindowFrame : IVsWindowFrame, IVsWindowFrameNotify3
    {
        private readonly IVsWindowFrame _frame;

        /// <summary>
        /// Creates a new instance from the specified frame
        /// </summary>
        /// <param name="frame"></param>
        public WindowFrame(IVsWindowFrame frame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _frame = frame ?? throw new ArgumentNullException("frame");
            
            ErrorHandler.ThrowOnFailure(_frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this));
        }

        /// <summary>
        /// Gets or sets the caption of the window frame.
        /// </summary>
        public string Caption
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                ErrorHandler.ThrowOnFailure(_frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out var result));
                return result.ToString();
            }
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                ErrorHandler.ThrowOnFailure(_frame.SetProperty((int)__VSFPROPID.VSFPROPID_Caption, value));
            }
        }

        /// <summary>
        /// Gets the GUID of this window frame.
        /// </summary>
        public Guid Guid
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot, out Guid guid);
                return guid;
            }
        }

        /// <summary>
        /// The editor guid associated with this frame.
        /// </summary>
        public Guid Editor
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_guidEditorType, out Guid guid);
                return guid;
            }
        }

        /// <summary>
        /// Event raised when the show state of the window frame changes.
        /// </summary>
        public event EventHandler<WindowFrameShowEventArgs>? OnShow;

        /// <summary>
        /// Event raised when the window frame is being closed.
        /// </summary>
        public event EventHandler<WindowFrameCloseEventArgs>? OnClose;

        /// <summary>
        /// Event raised when the window frame is being resized.
        /// </summary>
        public event EventHandler<WindowFramePositionChangedEventArgs>? OnResize;

        /// <summary>
        /// Event raised when the window frame is being moved.
        /// </summary>
        public event EventHandler<WindowFramePositionChangedEventArgs>? OnMove;

        /// <summary>
        /// Event raised when the window frame's dock state is being changed.
        /// </summary>
        public event EventHandler<WindowFrameDockChangedEventArgs>? OnDockChange;

        /// <summary>
        /// Event raised when the window frame's state is being changed.
        /// </summary>
        public event EventHandler? OnStatusChange;

        /// <summary>
        /// Renders this window visible, brings the window to the top, and activates the window.
        /// </summary>
        public async Task<bool> ShowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((IVsWindowFrame)this).Show() == VSConstants.S_OK;
        }

        /// <summary>
        /// Hides a window.
        /// </summary>
        public async Task<bool> HideAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((IVsWindowFrame)this).Hide() == VSConstants.S_OK;
        }

        /// <summary>
        /// Determines whether or not the window is visible.
        /// </summary>
        public async Task<bool> IsVisibleAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((IVsWindowFrame)this).Hide() == VSConstants.S_OK;
        }

        /// <summary>
        /// Shows or makes a window visible and brings it to the top, but does not make it the 
        /// active window.
        /// </summary>
        public async Task<bool> ShowNoActivateAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((IVsWindowFrame)this).ShowNoActivate() == VSConstants.S_OK;
        }

        /// <summary>
        /// Closes a window.
        /// </summary>
        /// <param name="option">Save options</param>
        public async Task<bool> CloseFrameAsync(FrameCloseOption option)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return ((IVsWindowFrame)this).CloseFrame((uint)option) == VSConstants.S_OK;
        }

        /// <summary>
        /// Sets the size of the window frame
        /// </summary>
        /// <param name="size">Size of the frame.</param>
        public async Task<bool> SetFrameSizeAsync(Size size)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Guid guid = Guid.Empty;
            return ((IVsWindowFrame)this).SetFramePos(VSSETFRAMEPOS.SFP_fSize, ref guid, 0, 0, size.Width, size.Height) == VSConstants.S_OK;
        }

        /// <summary>
        /// Sets the position of the window frame
        /// </summary>
        /// <param name="rec">Rectangle defining the size of the frame.</param>
        public async Task<bool> SetWindowPositionAsync(Rectangle rec)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Guid guid = Guid.Empty;
            return ((IVsWindowFrame)this).SetFramePos(VSSETFRAMEPOS.SFP_fMove, ref guid, rec.Left, rec.Top, rec.Width, rec.Height) == VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the position of the window frame.
        /// </summary>
        /// <param name="position">Window frame coordinates.</param>
        /// <returns>
        /// General position of the frame (docked, tabbed, floating, etc.)
        /// </returns>
        public FramePosition GetWindowPosition(out Rectangle position)
        {
            //TODO: Make this async
            ThreadHelper.ThrowIfNotOnUIThread();

            var pdwSFP = new VSSETFRAMEPOS[1];
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).GetFramePos(pdwSFP, out _, out var left, out var top, out var width, out var height));
            position = new Rectangle(left, top, width, height);

            return pdwSFP[0] switch
            {
                VSSETFRAMEPOS.SFP_fDock => FramePosition.Docked,
                VSSETFRAMEPOS.SFP_fTab => FramePosition.Tabbed,
                VSSETFRAMEPOS.SFP_fFloat => FramePosition.Float,
                VSSETFRAMEPOS.SFP_fMdiChild => FramePosition.MdiChild,
                _ => FramePosition.Unknown,
            };
        }

        /// <summary>
        /// Checks if the window frame is on the screen.
        /// </summary>
        /// <returns>True, if the frame is on the screen; otherwise, false.</returns>
        public async Task<bool> IsOnScreenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this).IsOnScreen(out var onScreen));
            return onScreen != 0;
        }

        /// <summary>
        /// Obtains all tool window frames.
        /// </summary>
        /// <value>All available tool window frames.</value>
        public static IEnumerable<WindowFrame> ToolWindowFrames
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
                Assumes.Present(uiShell);

                ErrorHandler.ThrowOnFailure(uiShell!.GetToolWindowEnum(out IEnumWindowFrames windowEnumerator));
                var frame = new IVsWindowFrame[1];
                var hr = VSConstants.S_OK;
                while (hr == VSConstants.S_OK)
                {
                    hr = windowEnumerator.Next(1, frame, out var fetched);
                    ErrorHandler.ThrowOnFailure(hr);
                    if (fetched == 1)
                    {
                        yield return new WindowFrame(frame[0]);
                    }
                }
            }
        }

        #region IVsWindowFrame members

        int IVsWindowFrame.Show()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.Show();
        }

        int IVsWindowFrame.Hide()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.Hide();
        }

        int IVsWindowFrame.IsVisible()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.IsVisible();
        }
        int IVsWindowFrame.ShowNoActivate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.ShowNoActivate();
        }

        int IVsWindowFrame.CloseFrame(uint grfSaveOptions)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.CloseFrame(grfSaveOptions);
        }

        int IVsWindowFrame.SetFramePos(VSSETFRAMEPOS dwSFP, ref Guid rguidRelativeTo, int x, int y, int cx, int cy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.SetFramePos(dwSFP, ref rguidRelativeTo, x, y, cx, cy);
        }

        int IVsWindowFrame.GetFramePos(VSSETFRAMEPOS[] pdwSFP, out Guid pguidRelativeTo, out int px, out int py, out int pcx, out int pcy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.GetFramePos(pdwSFP, out pguidRelativeTo, out px, out py, out pcx, out pcy);
        }

        int IVsWindowFrame.GetProperty(int propid, out object pvar)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.GetProperty(propid, out pvar);
        }

        int IVsWindowFrame.SetProperty(int propid, object var)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.SetProperty(propid, var);
        }

        int IVsWindowFrame.GetGuidProperty(int propid, out Guid pguid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.GetGuidProperty(propid, out pguid);
        }

        int IVsWindowFrame.SetGuidProperty(int propid, ref Guid rguid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.SetGuidProperty(propid, ref rguid);
        }

        int IVsWindowFrame.QueryViewInterface(ref Guid riid, out IntPtr ppv)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.QueryViewInterface(ref riid, out ppv);
        }

        int IVsWindowFrame.IsOnScreen(out int pfOnScreen)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _frame.IsOnScreen(out pfOnScreen);
        }

        #endregion

        #region IVsWindowFrameNotify3 members

        int IVsWindowFrameNotify3.OnShow(int fShow)
        {
            if (OnShow != null)
            {
                var e = new WindowFrameShowEventArgs((FrameShow)fShow);
                OnShow(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h)
        {
            if (OnMove != null)
            {
                var e = new WindowFramePositionChangedEventArgs(new Rectangle(x, y, w, h));
                OnMove(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h)
        {
            if (OnResize != null)
            {
                var e = new WindowFramePositionChangedEventArgs(new Rectangle(x, y, w, h));
                OnResize(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            if (OnDockChange != null)
            {
                var e = new WindowFrameDockChangedEventArgs(new Rectangle(x, y, w, h), fDockable != 0);
                OnDockChange(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions)
        {
            if (OnClose != null)
            {
                var e = new WindowFrameCloseEventArgs((FrameCloseOption)pgrfSaveOptions);
                OnClose(this, e);
            }
            InvokeStatusChanged();
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// Invokes the event handler for the status changed event.
        /// </summary>
        private void InvokeStatusChanged()
        {
            if (OnStatusChange != null)
            {
                var e = new EventArgs();
                OnStatusChange(this, e);
            }
        }
    }

    /// <summary>
    /// Specifies close options when closing a window frame.
    /// </summary>
    public enum FrameCloseOption
    {
        /// <summary>Do not save the document.</summary>
        NoSave = __FRAMECLOSE.FRAMECLOSE_NoSave,

        /// <summary>Save the document if it is dirty.</summary>
        SaveIfDirty = __FRAMECLOSE.FRAMECLOSE_SaveIfDirty,

        /// <summary>Prompt for document save.</summary>
        PromptSave = __FRAMECLOSE.FRAMECLOSE_PromptSave
    }

    /// <summary>
    /// Specifies the window frame positions.
    /// </summary>
    public enum FramePosition
    {
        /// <summary>Window frame has unknown position.</summary>
        Unknown = 0,
        /// <summary>Window frame is docked.</summary>
        Docked = VSSETFRAMEPOS.SFP_fDock,
        /// <summary>Window frame is tabbed.</summary>
        Tabbed = VSSETFRAMEPOS.SFP_fTab,
        /// <summary>Window frame floats.</summary>
        Float = VSSETFRAMEPOS.SFP_fFloat,
        /// <summary>Window frame is currently within the MDI space.</summary>
        MdiChild = VSSETFRAMEPOS.SFP_fMdiChild
    }

    /// <summary>
    /// Specifies options when the show state of a window frame changes.
    /// </summary>
    [Flags]
    public enum FrameShow
    {
        /// <summary>Reason unknown</summary>
        Unknown = 0,
        /// <summary>Obsolete; use WinHidden.</summary>
        Hidden = __FRAMESHOW.FRAMESHOW_Hidden,
        /// <summary>Window (tabbed or otherwise) is hidden.</summary>
        WinHidden = __FRAMESHOW.FRAMESHOW_WinHidden,
        /// <summary>A nontabbed window is made visible.</summary>
        Shown = __FRAMESHOW.FRAMESHOW_WinShown,
        /// <summary>A tabbed window is activated (made visible).</summary>
        TabActivated = __FRAMESHOW.FRAMESHOW_TabActivated,
        /// <summary>A tabbed window is deactivated.</summary>
        TabDeactivated = __FRAMESHOW.FRAMESHOW_TabDeactivated,
        /// <summary>Window is restored to normal state.</summary>
        Restored = __FRAMESHOW.FRAMESHOW_WinRestored,
        /// <summary>Window is minimized.</summary>
        Minimized = __FRAMESHOW.FRAMESHOW_WinMinimized,
        /// <summary>Window is maximized.</summary>
        Maximized = __FRAMESHOW.FRAMESHOW_WinMaximized,
        /// <summary>Multi-instance tool window destroyed.</summary>
        DestroyMultipleInstance = __FRAMESHOW.FRAMESHOW_DestroyMultInst,
        /// <summary>Autohidden window is about to slide into view.</summary>
        AutoHideSlideBegin = __FRAMESHOW.FRAMESHOW_AutoHideSlideBegin
    }

    /// <summary>
    /// Event arguments for the event raised when the show state of a window frame changes.
    /// </summary>
    public class WindowFrameShowEventArgs : EventArgs
    {
        /// <summary>
        /// Reason of the event (why the show state is changed)?
        /// </summary>
        public FrameShow Reason { get; private set; }

        /// <summary>
        /// Creates an event argument instance with the initial reason.
        /// </summary>
        /// <param name="reason">Event reason.</param>
        public WindowFrameShowEventArgs(FrameShow reason)
        {
            Reason = reason;
        }
    }

    /// <summary>
    /// Event arguments for the event raised when the window frame is closed.
    /// </summary>
    public class WindowFrameCloseEventArgs : EventArgs
    {
        /// <summary>
        /// Options used to close the window frame.
        /// </summary>
        public FrameCloseOption CloseOption { get; private set; }

        /// <summary>
        /// Creates an event argument instance with the initial close option.
        /// </summary>
        /// <param name="closeOption">Close option.</param>
        public WindowFrameCloseEventArgs(FrameCloseOption closeOption)
        {
            CloseOption = closeOption;
        }
    }

    /// <summary>
    /// Event arguments for the events raised when the window frame position is changed.
    /// </summary>
    public class WindowFramePositionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New window frame position.
        /// </summary>
        public Rectangle Position { get; private set; }

        /// <summary>
        /// Creates an event argument instance with the new frame position.
        /// </summary>
        /// <param name="position">New frame position.</param>
        public WindowFramePositionChangedEventArgs(Rectangle position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Event arguments for the event raised when the dock state of the window frame is changed.
    /// </summary>
    public class WindowFrameDockChangedEventArgs : WindowFramePositionChangedEventArgs
    {
        /// <inheritdoc />
        public bool Docked { get; private set; }

        /// <summary>
        /// Creates an event argument instance with the new position and dock state..
        /// </summary>
        /// <param name="position">New position of the window frame.</param>
        /// <param name="docked">True, if the frame is docked; otherwise, false.</param>
        public WindowFrameDockChangedEventArgs(Rectangle position, bool docked)
          : base(position)
        {
            Docked = docked;
        }
    }
}