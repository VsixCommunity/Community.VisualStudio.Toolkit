using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestExtension.Commands
{
    [Command(PackageIds.SplitButton)]
    internal sealed class SplitButtonMenuCommand : BaseCommand<SplitButtonMenuCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowAsync("The main split button was pressed.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }

    [Command(PackageIds.SplitButtonChild1)]
    internal sealed class SplitButton1Command : BaseCommand<SplitButton1Command>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowAsync("The first split button child was pressed.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }

    [Command(PackageIds.SplitButtonChild2)]
    internal sealed class SplitButton2Command : BaseCommand<SplitButton2Command>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowAsync("The second split button child was pressed.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);

            // Toggle the checked state to demonstrate having a check mark on the button.
            Command.Checked = !Command.Checked;
        }
    }
}
