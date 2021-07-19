using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>Exposes WPF resource keys for WPF controls.</summary>
    public static class ToolkitResourceKeys
    {
        private const string _prefix = "Toolkit";

        /// <summary>Gets the key that can be used to get the <see cref="Thickness"/> to use for input controls.</summary>
        public static object InputPaddingKey { get; } = _prefix + nameof(InputPaddingKey);

        /// <summary>
        /// Gets the key that can be used to get the <see cref="double"/> to use for a progress bar with a thick height
        /// (similar to the height used in the <see cref="IVsThreadedWaitDialog"/>).
        /// </summary>
        public static object ThickProgressBarHeight { get; } = _prefix + nameof(ThickProgressBarHeight);
    }
}
