using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A helper class that can automatically theme any XAML control or window using the VS theme properties.
    /// </summary>
    /// <remarks>Should only be referenced from within .xaml files.</remarks>
    /// <example>
    /// <code>
    /// &lt;UserControl x:Class="MyClass"
    /// xmlns:toolkit="clr-namespace:Community.VisualStudio.Toolkit;assembly=Community.VisualStudio.Toolkit"
    /// toolkit:Themes.UseVsTheme="True"&gt;
    /// &lt;/UserControl&gt;
    /// </code>
    /// </example>
    public static class Themes
    {
        private static readonly DependencyProperty _originalBackgroundProperty = DependencyProperty.RegisterAttached("OriginalBackground", typeof(object), typeof(Themes));
        private static readonly DependencyProperty _originalForegroundProperty = DependencyProperty.RegisterAttached("OriginalForeground", typeof(object), typeof(Themes));

        /// <summary>
        /// The property to add to your XAML control.
        /// </summary>
        public static readonly DependencyProperty UseVsThemeProperty = DependencyProperty.RegisterAttached("UseVsTheme", typeof(bool), typeof(Themes), new PropertyMetadata(false, UseVsThemePropertyChanged));

        /// <summary>
        /// Sets the UseVsTheme property.
        /// </summary>
        public static void SetUseVsTheme(UIElement element, bool value) => element.SetValue(UseVsThemeProperty, value);

        /// <summary>
        /// Gets the UseVsTheme property from the specified element.
        /// </summary>
        public static bool GetUseVsTheme(UIElement element) => (bool)element.GetValue(UseVsThemeProperty);

        private static void UseVsThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d))
            {
                if (d is FrameworkElement element)
                {
                    if ((bool)e.NewValue)
                    {
                        OverrideProperty(element, Control.BackgroundProperty, _originalBackgroundProperty, EnvironmentColors.StartPageTabBackgroundBrushKey);
                        OverrideProperty(element, Control.ForegroundProperty, _originalForegroundProperty, EnvironmentColors.StartPageTextBodyBrushKey);
                        ThemedDialogStyleLoader.SetUseDefaultThemedDialogStyles(element, true);
                        ImageThemingUtilities.SetThemeScrollBars(element, true);

                        // Only merge the styles after the element has been initialized.
                        // If the element hasn't been initialized yet, add an event handler
                        // so that we can merge the styles once it has been initialized.
                        if (!element.IsInitialized)
                        {
                            element.Initialized += OnElementInitialized;
                        }
                        else
                        {
                            MergeStyles(element);
                        }
                    }
                    else
                    {
                        element.Resources.MergedDictionaries.Remove(ThemeResources);
                        ImageThemingUtilities.SetThemeScrollBars(element, null);
                        ThemedDialogStyleLoader.SetUseDefaultThemedDialogStyles(element, false);
                        RestoreProperty(element, Control.ForegroundProperty, _originalForegroundProperty);
                        RestoreProperty(element, Control.BackgroundProperty, _originalBackgroundProperty);
                    }
                }
            }
        }

        private static void OverrideProperty(FrameworkElement element, DependencyProperty property, DependencyProperty backup, object value)
        {
            if (element is Control control)
            {
                var original = control.ReadLocalValue(property);

                if (!ReferenceEquals(value, DependencyProperty.UnsetValue))
                {
                    control.SetValue(backup, original);
                }

                control.SetResourceReference(property, value);
            }
        }

        private static void RestoreProperty(FrameworkElement element, DependencyProperty property, DependencyProperty backup)
        {
            if (element is Control control)
            {
                var value = control.ReadLocalValue(backup);

                if (!ReferenceEquals(value, DependencyProperty.UnsetValue))
                {
                    control.SetValue(property, value);
                }
                else
                {
                    control.ClearValue(property);
                }

                control.ClearValue(backup);
            }
        }

        private static void OnElementInitialized(object sender, EventArgs args)
        {
            var element = (FrameworkElement)sender;
            MergeStyles(element);
            element.Initialized -= OnElementInitialized;
        }

        private static void MergeStyles(FrameworkElement element)
        {
            Collection<ResourceDictionary> dictionaries = element.Resources.MergedDictionaries;
            if (!dictionaries.Contains(ThemeResources))
            {
                dictionaries.Add(ThemeResources);
            }
        }

        private static ResourceDictionary ThemeResources { get; } = BuildThemeResources();

        private static ResourceDictionary BuildThemeResources()
        {
            var resources = new ResourceDictionary();

            try
            {
                var inputPadding = new Thickness(6, 8, 6, 8); // This is the same padding used by WatermarkedTextBox.

                resources[ToolkitResourceKeys.InputPaddingKey] = inputPadding;

                resources[typeof(TextBox)] = new Style
                {
                    TargetType = typeof(TextBox),
                    BasedOn = (Style)Application.Current.FindResource(VsResourceKeys.TextBoxStyleKey),
                    Setters =
                    {
                        new Setter(Control.PaddingProperty, new DynamicResourceExtension(ToolkitResourceKeys.InputPaddingKey))
                    }
                };

                resources[typeof(ComboBox)] = new Style
                {
                    TargetType = typeof(ComboBox),
                    BasedOn = (Style)Application.Current.FindResource(VsResourceKeys.ComboBoxStyleKey),
                    Setters =
                    {
                        new Setter(Control.PaddingProperty, new DynamicResourceExtension(ToolkitResourceKeys.InputPaddingKey))
                    }
                };
            }
            catch (Exception ex) when (!ErrorHandler.IsCriticalException(ex))
            {
                ex.Log();
            }

            return resources;
        }
    }
}