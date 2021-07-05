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
        public RunnerWindowControl(Version vsVersion)
        {
            InitializeComponent();

            lblHeadline.Content = $"Visual Studio v{vsVersion}";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageAsync().FireAndForget();
        }

        private async Task ShowMessageAsync()
        {
            await VS.StatusBar.ShowMessageAsync("Test");
            string text = await VS.StatusBar.GetMessageAsync();
            await VS.StatusBar.ShowMessageAsync(text + " OK");

            Exception ex = new Exception(nameof(TestExtension));
            await ex.LogAsync();

            VSConstants.MessageBoxResult button = await VS.MessageBox.ShowAsync("message", "title");
            Debug.WriteLine(button);
        }
    }
}