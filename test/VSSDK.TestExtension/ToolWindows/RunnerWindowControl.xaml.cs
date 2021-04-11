using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
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
            await VS.Notifications.SetStatusbarTextAsync("Test");
            var text = await VS.Notifications.GetStatusbarTextAsync();
            await VS.Notifications.SetStatusbarTextAsync(text + " OK");

            var ex = new Exception(nameof(TestExtension));
            await ex.LogAsync();

            VSConstants.MessageBoxResult button = VS.Notifications.ShowMessage("message", "title");
            Debug.WriteLine(button);
        }
    }
}