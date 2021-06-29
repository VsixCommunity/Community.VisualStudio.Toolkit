using System;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the window frames.
        /// </summary>
        public WindowEvents WindowEvents => new();
    }

    /// <summary>
    /// Events related to the window frames.
    /// </summary>
    public class WindowEvents : IVsWindowFrameEvents
    {
        internal WindowEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var svc = (IVsUIShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell));
            Assumes.Present(svc);
            svc.AdviseWindowFrameEvents(this);
        }

        /// <summary>
        /// Fires when a window frame is created
        /// </summary>
        public event EventHandler<IVsWindowFrame>? Created;

        /// <summary>
        /// Fires when a window frame is destroyed.
        /// </summary>
        public event EventHandler<IVsWindowFrame>? Destroyed;

        /// <summary>
        /// Fires when a changes happens to a frame's visibility.
        /// </summary>
        public event EventHandler<FrameVisibilityEventArgs>? FrameIsVisibleChanged;

        /// <summary>
        /// Fires when a changes happens to a frames location on the screen.
        /// </summary>
        public event EventHandler<FrameOnScreenEventArgs>? FrameIsOnScreenChanged;

        /// <summary>
        /// Fires when the active frame changes.
        /// </summary>
        public event EventHandler<ActiveFrameChangeEventArgs>? ActiveFrameChanged;

        void IVsWindowFrameEvents.OnFrameCreated(IVsWindowFrame frame)
        {
            Created?.Invoke(this, frame);
        }

        void IVsWindowFrameEvents.OnFrameDestroyed(IVsWindowFrame frame)
        {
            Destroyed?.Invoke(this, frame);
        }

        void IVsWindowFrameEvents.OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
            FrameIsVisibleChanged?.Invoke(this, new FrameVisibilityEventArgs(frame, newIsVisible));
        }

        void IVsWindowFrameEvents.OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
            FrameIsOnScreenChanged?.Invoke(this, new FrameOnScreenEventArgs(frame, newIsOnScreen));
        }

        void IVsWindowFrameEvents.OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            ActiveFrameChanged?.Invoke(this, new ActiveFrameChangeEventArgs(oldFrame, newFrame));
        }
    }

    /// <inheritdoc/>
    public class FrameVisibilityEventArgs : EventArgs
    {
        internal FrameVisibilityEventArgs(IVsWindowFrame frame, bool isNewVisible)
        {
            Frame = frame;
            IsNewVisible = isNewVisible;
        }

        /// <summary>The Window frame object.</summary>
        public IVsWindowFrame Frame { get; }
        /// <summary>A value indicating if the new frame is visible.</summary>
        public bool IsNewVisible { get; }
    }

    /// <inheritdoc/>
    public class FrameOnScreenEventArgs : EventArgs
    {
        internal FrameOnScreenEventArgs(IVsWindowFrame frame, bool isOnScreen)
        {
            Frame = frame;
            IsOnScreen = isOnScreen;
        }

        /// <summary>The Window frame object.</summary>
        public IVsWindowFrame Frame { get; }
        /// <summary>A value indicating if the frame is on screen.</summary>
        public bool IsOnScreen { get; }
    }

    /// <inheritdoc/>
    public class ActiveFrameChangeEventArgs : EventArgs
    {
        internal ActiveFrameChangeEventArgs(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            OldFrame = oldFrame;
            NewFrame = newFrame;
        }
        /// <summary>The frame that lost its active state.</summary>
        public IVsWindowFrame OldFrame { get; }

        /// <summary>The frame became active.</summary>
        public IVsWindowFrame NewFrame { get; }
    }
}
