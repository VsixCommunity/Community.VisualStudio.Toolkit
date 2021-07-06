using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the window frames.
        /// </summary>
        public WindowEvents WindowEvents { get; } = new();
    }

    /// <summary>
    /// Events related to the window frames.
    /// </summary>
    public class WindowEvents : IVsWindowFrameEvents
    {
        internal WindowEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsUIShell7 svc = VS.GetRequiredService<SVsUIShell, IVsUIShell7>();
            svc.AdviseWindowFrameEvents(this);
        }

        /// <summary>
        /// Fires when a window frame is created
        /// </summary>
        public event Action<WindowFrame>? Created;

        /// <summary>
        /// Fires when a window frame is destroyed.
        /// </summary>
        public event Action<WindowFrame>? Destroyed;

        /// <summary>
        /// Fires when a changes happens to a frame's visibility.
        /// </summary>
        public event Action<FrameVisibilityEventArgs>? FrameIsVisibleChanged;

        /// <summary>
        /// Fires when a changes happens to a frames location on the screen.
        /// </summary>
        public event Action<FrameOnScreenEventArgs>? FrameIsOnScreenChanged;

        /// <summary>
        /// Fires when the active frame changes.
        /// </summary>
        public event Action<ActiveFrameChangeEventArgs>? ActiveFrameChanged;

        void IVsWindowFrameEvents.OnFrameCreated(IVsWindowFrame frame)
        {
            Created?.Invoke(new WindowFrame(frame));
        }

        void IVsWindowFrameEvents.OnFrameDestroyed(IVsWindowFrame frame)
        {
            Destroyed?.Invoke(new WindowFrame(frame));
        }

        void IVsWindowFrameEvents.OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
            FrameIsVisibleChanged?.Invoke(new FrameVisibilityEventArgs(frame, newIsVisible));
        }

        void IVsWindowFrameEvents.OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
            FrameIsOnScreenChanged?.Invoke(new FrameOnScreenEventArgs(frame, newIsOnScreen));
        }

        void IVsWindowFrameEvents.OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            ActiveFrameChanged?.Invoke(new ActiveFrameChangeEventArgs(oldFrame, newFrame));
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
            Frame = new WindowFrame(frame);
            IsOnScreen = isOnScreen;
        }

        /// <summary>The Window frame object.</summary>
        public WindowFrame Frame { get; }
        /// <summary>A value indicating if the frame is on screen.</summary>
        public bool IsOnScreen { get; }
    }

    /// <inheritdoc/>
    public class ActiveFrameChangeEventArgs : EventArgs
    {
        internal ActiveFrameChangeEventArgs(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            OldFrame = new WindowFrame(oldFrame);
            NewFrame = new WindowFrame(newFrame);
        }
        /// <summary>The frame that lost its active state.</summary>
        public WindowFrame OldFrame { get; }

        /// <summary>The frame became active.</summary>
        public WindowFrame NewFrame { get; }
    }
}
