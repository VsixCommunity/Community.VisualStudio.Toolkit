using System.Windows;
using System.Windows.Controls;

namespace TestExtension
{
    public class ThemedControl : Control
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(ThemedControl),
            new FrameworkPropertyMetadata("")
        );

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            "Enabled",
            typeof(object),
            typeof(ThemedControl),
            new FrameworkPropertyMetadata(null)
        );

        public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(
            "Disabled",
            typeof(object),
            typeof(ThemedControl),
            new FrameworkPropertyMetadata(null)
        );

        static ThemedControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedControl), new FrameworkPropertyMetadata(typeof(ThemedControl)));
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public object Enabled
        {
            get { return GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        public object Disabled
        {
            get { return GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }
    }
}
