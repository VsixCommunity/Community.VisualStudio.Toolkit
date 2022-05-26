using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An implementation of <see cref="ToolWindowPane"/> that allows the 
    /// <see cref="BaseToolWindow{T}"/> or the tool window's content access to this pane.
    /// </summary>
    public abstract class ToolkitToolWindowPane : ToolWindowPane
    {
        private bool _isInitialized;

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();
            _isInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        internal bool IsInitialized => _isInitialized;

        internal event EventHandler? Initialized;
    }
}
