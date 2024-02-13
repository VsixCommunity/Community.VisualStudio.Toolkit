using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An implementation of <see cref="ToolWindowPane"/> that allows the 
    /// <see cref="BaseToolWindow{T}"/> or the tool window's content access to this pane.
    /// </summary>
    public abstract class ToolkitToolWindowPane : ToolWindowPane
    {
        private bool _isInitialized;
        private WindowFrame? _windowFrame;
        private bool _isWindowFrameAvailable;

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();
            _isInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        internal bool IsInitialized => _isInitialized;

        internal event EventHandler? Initialized;

        /// <inheritdoc/>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
            _isWindowFrameAvailable = true;
            WindowFrameAvailable?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Indicates whether the <see cref="GetWindowFrame"/> method can be called.
        /// </summary>
        public bool IsWindowFrameAvailable => _isWindowFrameAvailable;

        /// <summary>
        /// Raised when Visual Studio creates the tool window's frame.
        /// The <see cref="WindowFrame"/> property can be accessed from this point onwards.
        /// </summary>
        public event EventHandler? WindowFrameAvailable;

        /// <summary>
        /// Gets the tool window's window frame.
        /// <para>
        /// This method can only be called after Visual Studio has created the window frame.
        /// You can detect this in various ways:
        /// <list type="bullet">
        /// <item>
        /// Override the <see cref="OnToolWindowCreated"/> method. 
        /// When this method is called, the window frame will be available.
        /// </item>
        /// <item>
        /// Listen for the <see cref="WindowFrameAvailable"/> event.
        /// When the event is raised, the window frame will be available.
        /// </item>
        /// <item>
        /// Check the <see cref="IsWindowFrameAvailable"/> property.
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">The window frame is not available.</exception>
        protected WindowFrame GetWindowFrame()
        {
            if (_windowFrame is null)
            {
                // The `Frame` property has to be set by Visual Studio, so it might
                // be null at this point. It's also typed as an `object` even though
                // internally it's stored as an `IVsWindowFrame`, so we can use
                // type matching to both cast and confirm that it's not null.
                if (Frame is IVsWindowFrame vsWindowFrame)
                {
                    // We could create the WindowFrame in `OnToolWindowCreated`,
                    // but we delay-create it for two reasons:
                    //  1. It may not ever be needed.
                    //  2. If a derived class also overrides `OnToolWindowCreated`, then the window
                    //     frame would only be available after it called `base.OnToolWindowCreated()`.
                    //     Delay-creating it means that it will be available before that call is made.
                    _windowFrame = new WindowFrame(vsWindowFrame);
                }
                else
                {
                    throw new InvalidOperationException("The tool window's frame is not available yet.");
                }
            }

            return _windowFrame;
        }
    }
}
