using System.Diagnostics;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace TestExtension.ToolWindows
{
    public partial class RatingPromptWindow : DialogWindow
    {
        private readonly RatingPrompt _prompt;

        public RatingPromptWindow(RatingPrompt prompt)
        {
            InitializeComponent();

            lblText.Text = $"Are you enjoying the {prompt.ExtensionName} extension? Help spread the word by leaving a review.";
            _prompt = prompt;
        }

        public RateChoice Choice { get; private set; }

        private void btnRate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Choice = RateChoice.RateNow;
            Close();
        }

        private void btnPostpone_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Choice = RateChoice.RateLater;
            Close();
        }

        private void btnNoThanks_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Choice = RateChoice.NoRate;
            Close();
        }


    }

    public enum RateChoice
    {
        RateNow,
        RateLater,
        NoRate,
    }
}
