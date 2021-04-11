using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An API wrapper that makes it easy to work with the status bar.
    /// </summary>
    public partial class Notifications
    {
        /// <summary>Provides access to the environment's status bar.</summary>
        public Task<IVsStatusbar> GetStatusbarAsync() => VS.GetServiceAsync<SVsStatusbar, IVsStatusbar>();

        /// <summary>Gets the current text from the status bar.</summary>
        public async Task<string?> GetStatusbarTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                IVsStatusbar statusBar = await GetStatusbarAsync();

                statusBar.GetText(out var pszText);
                return pszText;
            }
            catch (Exception ex)
            {
                VsShellUtilities.LogError(ex.Source, ex.ToString());
                return null;
            }
        }

        /// <summary>Sets the text in the status bar.</summary>
        public async Task SetStatusbarTextAsync(string text)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusbarAsync();

                statusBar.FreezeOutput(0);
                statusBar.SetText(text);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>Clears all text from the status bar.</summary>
        public async Task ClearStatusbarAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusbarAsync();

                statusBar.FreezeOutput(0);
                statusBar.Clear();
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>Starts the animation on the status bar.</summary>
        public async Task StartStatusbarAnimationAsync(StatusAnimation animation)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusbarAsync();

                statusBar.FreezeOutput(0);
                statusBar.Animation(1, animation);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>Ends the animation on the status bar.</summary>
        public async Task EndStatusbarAnimationAsync(StatusAnimation animation)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusbarAsync();

                statusBar.FreezeOutput(0);
                statusBar.Animation(0, animation);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

        }
    }

    /// <summary>A list of built-in animation visuals for the status bar.</summary>
    public enum StatusAnimation
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        General = 0,
        Print = 1,
        Save = 2,
        Deploy = 3,
        Sync = 4,
        Build = 5,
        Find = 6
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}