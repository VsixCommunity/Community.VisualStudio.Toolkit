using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var items = await VS.Solution.GetSelectedNodesAsync();
            var item = items.FirstOrDefault();
            
            await VS.Build.BuildProjectAsync(item);

            //var removed = await item.TryRemoveAsync();
            //Debug.Write(removed);

            //var dir = item.FileName + ".txt";

            //if (!Directory.Exists(dir))
            //{
            //    Directory.CreateDirectory(dir);
            //}
            //string file = item.FileName + ".txt";
            //if (!System.IO.File.Exists(file))
            //{
            //    System.IO.File.WriteAllText(file, "ost");
            //}


            //await item.AddItemsAsync(file);

            //await VS.Notifications.SetStatusBarTextAsync("Test");
            //var text = await VS.Notifications.GetStatusBarTextAsync();
            //await VS.Notifications.SetStatusBarTextAsync(text + " OK");

            //var ex = new Exception(nameof(TestExtension));
            //await ex.LogAsync();

            //VSConstants.MessageBoxResult button = VS.Notifications.ShowMessage("message", "title");
            //Debug.WriteLine(button);
        }
    }
}