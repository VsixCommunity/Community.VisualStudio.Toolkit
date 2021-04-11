using System.Windows;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>Exposes WPF resource keys for WPF controls.</summary>
    public static class ToolkitResourceKeys
    {
        /// <summary>Gets the key that can be used to get the <see cref="Thickness"/> to use for input controls.</summary>
        public static object InputPaddingKey { get; } = "Toolkit" + nameof(InputPaddingKey);
    }
}
