using System.Collections.Generic;
using System.Windows.Controls;

namespace TestExtension
{
    public partial class ThemeWindowDemo : UserControl
    {
        public ThemeWindowDemo()
        {
            InitializeComponent();
        }

        public IEnumerable<string> ListItems { get; } = new string[] {
                "First",
                "Second",
                "Third",
                "Fourth",
                "Fifth",
                "Sixth"
       };

    }
}
