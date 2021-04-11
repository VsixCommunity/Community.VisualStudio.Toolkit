using System.Windows.Controls;

namespace TestExtension
{
    public partial class MultiInstanceWindowControl : UserControl
    {
        public MultiInstanceWindowControl(int toolWindowId)
        {
            InitializeComponent();
            lblWindowId.Content = toolWindowId;
        }
    }
}