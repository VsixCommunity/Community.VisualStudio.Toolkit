using System.Collections.Generic;
using System.Windows.Input;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.ExpandSelectedItems)]
    internal sealed class ExpandSelectedItemsCommand : BaseCommand<ExpandSelectedItemsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                IEnumerable<SolutionItem> items = await solutionExplorer.GetSelectionAsync();
                SolutionItemExpansionMode mode = SolutionItemExpansionMode.None;

                ModifierKeys modifiers = Keyboard.Modifiers;
                if (modifiers == ModifierKeys.None)
                {
                    mode = SolutionItemExpansionMode.Single;
                }
                else
                {
                    if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        mode |= SolutionItemExpansionMode.Recursive;
                    }

                    if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        // To expand to the selected items, theose items and their ancestors
                        // all need to be collapsed, so collapse them first, then pause for a
                        // moment so that you can see that we start from a collapsed state.
                        foreach (SolutionItem item in items)
                        {
                            CollapseRecursively(solutionExplorer, item);
                        }
                        await Task.Delay(2000);
                        mode |= SolutionItemExpansionMode.Ancestors;
                    }
                }

                solutionExplorer.Expand(items, mode);
            }
        }

        private void CollapseRecursively(SolutionExplorerWindow solutionExplorer, SolutionItem item)
        {
            while (item != null)
            {
                solutionExplorer.Collapse(item);
                item = item.Parent;
            }
        }
    }
}
