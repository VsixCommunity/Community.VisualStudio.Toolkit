using System.Windows.Controls;
using System.Windows.Documents;
using Community.VisualStudio.Toolkit;

namespace TestExtension
{
    public partial class FontsAndColorsWindowControl : UserControl
    {
        public FontsAndColorsWindowControl()
        {
            InitializeComponent();
        }

        private void ApplyColor(Border border, ConfiguredColor color)
        {
            // Bind the border's properties to the configured
            // color properties. This could also be done in XAML.
            border.DataContext = color;
            border.SetBinding(BackgroundProperty, nameof(ConfiguredColor.BackgroundBrush));
            border.SetBinding(TextElement.ForegroundProperty, nameof(ConfiguredColor.ForegroundBrush));
            border.SetBinding(TextElement.FontWeightProperty, nameof(ConfiguredColor.FontWeight));
        }
    }
}
