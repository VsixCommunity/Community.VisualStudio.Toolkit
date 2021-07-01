using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    public partial class RunnerWindowControl : UserControl
    {
        public RunnerWindowControl(EnvDTE80.DTE2 dte)
        {
            InitializeComponent();

            lblHeadline.Content = $"Visual Studio v{dte.Version}";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageAsync().FireAndForget();
        }

        private async Task ShowMessageAsync()
        {
            var model = new InfoBarModel(
    new[] {
        new InfoBarTextSpan("The text in the Info Bar. "),
        new InfoBarHyperlink("Click me")
    },
    KnownMonikers.PlayStepGroup,

    true);
            var win = await VS.Windows.GetCurrentWindowAsync();
            var ib=await win.CreateInfoBarAsync(model);
            await ib.TryShowInfoBarUIAsync();
            return;
            await VS.StatusBar.ShowMessageAsync("Test");
            var text = await VS.StatusBar.GetMessageAsync();
            await VS.StatusBar.ShowMessageAsync(text + " OK");

            var ex = new Exception(nameof(TestExtension));
            await ex.LogAsync();

            VSConstants.MessageBoxResult button = await VS.MessageBox.ShowAsync("message", "title");
            Debug.WriteLine(button);
        }
    }
}