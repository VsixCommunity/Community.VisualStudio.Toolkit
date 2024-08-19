using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.FontsAndColorsWindow)]
    internal sealed class FontsAndColorsWindowCommand : BaseCommand<FontsAndColorsWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) =>
            await FontsAndColorsWindow.ShowAsync();
    }
}
