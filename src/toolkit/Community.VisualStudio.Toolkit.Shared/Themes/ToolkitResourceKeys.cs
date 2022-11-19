using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>Exposes WPF resource keys for WPF controls.</summary>
    public static class ToolkitResourceKeys
    {
        private const string _prefix = "Toolkit";

        /// <summary>
        /// Gets the URI of the <see cref="ResourceDictionary"/> that contains the theme resources.
        /// </summary>
        /// <remarks>
        /// This can be used to load the <see cref="ResourceDictionary"/> when you would like to customize the styles.
        /// <code>
        /// &lt;ResourceDictionary&gt;
        ///     &lt;ResourceDictionary.MergedDictionaries&gt;
        ///         &lt;ResourceDictionary Source="{x:Static toolkit:ToolkitResourceKeys.ThemeResourcesUri}"/&gt;
        ///     &lt;/ResourceDictionary.MergedDictionaries&gt;
        /// &lt;/ResourceDictionary&gt;
        /// </code>
        /// </remarks>
        public static Uri ThemeResourcesUri { get; } = BuildPackUri("Themes/ThemeResources.xaml");

        /// <summary>Gets the key that can be used to get the <see cref="Thickness"/> to use for input controls.</summary>
        public static object InputPaddingKey { get; } = _prefix + nameof(InputPaddingKey);

        /// <summary>
        /// Gets the key that can be used to get the <see cref="double"/> to use for a progress bar with a thick height
        /// (similar to the height used in the <see cref="IVsThreadedWaitDialog"/>).
        /// </summary>
        public static object ThickProgressBarHeight { get; } = _prefix + nameof(ThickProgressBarHeight);

        /// <summary>Gets the key that defines the resource for a Visual Studio-themed <see cref="TextBox"/> style.</summary>
        public static object TextBoxStyleKey { get; } = _prefix + nameof(TextBoxStyleKey);

        /// <summary>Gets the key that defines the resource for a Visual Studio-themed <see cref="ComboBox"/> style.</summary>
        public static object ComboBoxStyleKey { get; } = _prefix + nameof(ComboBoxStyleKey);

        /// <summary>Gets the key that defines the resource for a Visual Studio-themed <see cref="PasswordBox"/> style.</summary>
        public static object PasswordBoxStyleKey { get; } = _prefix + nameof(PasswordBoxStyleKey);

        /// <summary>Gets the key that defines the resource for the <see cref="ControlTemplate"/> of a Visual Studio-themed <see cref="PasswordBox"/> style.</summary>
        public static object PasswordBoxControlTemplateKey { get; } = _prefix + nameof(PasswordBoxControlTemplateKey);

        /// <summary>Gets the key that defines the resource for a Visual Studio-themed <see cref="RichTextBox"/> style.</summary>
        public static object RichTextBoxStyleKey { get; } = _prefix + nameof(RichTextBoxStyleKey);

        /// <summary>Gets the key that defines the resource for the <see cref="ControlTemplate"/> of a Visual Studio-themed <see cref="RichTextBox"/> style.</summary>
        public static object RichTextBoxControlTemplateKey { get; } = _prefix + nameof(RichTextBoxControlTemplateKey);

        private static Uri BuildPackUri(string resource)
        {
            // Multiple versions of the toolkit assembly might be loaded, so when
            // loading a resource, we need to include the version number of this
            // assembly to ensure that the resource is loaded from the correct assembly.
            AssemblyName assemblyName = typeof(ToolkitResourceKeys).Assembly.GetName();
            string version = assemblyName.Version.ToString();
            return new Uri($"pack://application:,,,/{assemblyName.Name};v{version};component/{resource}");
        }
    }
}
