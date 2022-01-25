using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A standardized way to prompt the user to rate and review the extension
    /// </summary>
    public class RatingPrompt
    {
        private const string _urlFormat = "https://marketplace.visualstudio.com/items?itemName={0}&ssr=false#review-details";
        private const int _minutesVisible = 2;
        private static readonly ConcurrentDictionary<string, bool> _hasAlreadyRequested = new();

        /// <summary>
        /// Creates a new instance of the rating prompt.
        /// </summary>
        /// <param name="marketplaceId">The unique Marketplace id found at the end of the Marketplace URL. For instance: "MyName.MyExtensions".</param>
        /// <param name="extensionName">The name of the extension to show in the prompt.</param>
        /// <param name="config">Likely a options page that implements the <see cref="IRatingConfig"/> interface. This is used to keep track of how many times the prompt was requested and if the user has already rated.</param>
        /// <param name="requestsBeforePrompt">Indicates how many successful requests it takes before the user is prompted to rate.</param>
        /// <exception cref="ArgumentNullException">None of the parameters passed in can be null.</exception>
        /// <exception cref="ArgumentException">The Marketplace ID has to be valid so an absolute URI can be constructed.</exception>
        public RatingPrompt(string marketplaceId, string extensionName, IRatingConfig config, int requestsBeforePrompt = 5)
        {
            MarketplaceId = marketplaceId ?? throw new ArgumentNullException(nameof(marketplaceId));
            ExtensionName = extensionName ?? throw new ArgumentNullException(nameof(extensionName));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            RequestsBeforePrompt = requestsBeforePrompt;

            string ratingUrl = string.Format(CultureInfo.InvariantCulture, _urlFormat, MarketplaceId);

            if (!Uri.TryCreate(ratingUrl, UriKind.Absolute, out Uri parsedUrl))
            {
                throw new ArgumentException($"{RatingUrl} is not a valid URL", nameof(marketplaceId));
            }

            RatingUrl = parsedUrl;
        }

        /// <summary>
        /// The Marketplace ID is the unique last part of the URL. For instance: "MyName.MyExtension".
        /// </summary>
        public virtual string MarketplaceId { get; }

        /// <summary>
        /// The name of the extension. It's shown in the prompt so the user knows which extension to rate.
        /// </summary>
        public virtual string ExtensionName { get; }

        /// <summary>
        /// The configuration/options object used to store the information related to the rating prompt.
        /// </summary>
        public virtual IRatingConfig Config { get; }

        /// <summary>
        /// The Marketplace URL the users are taken to when prompted.
        /// </summary>
        public virtual Uri RatingUrl { get; }

        /// <summary>
        /// Indicates how many successful requests it takes before the user is prompted to rate.
        /// </summary>
        public virtual int RequestsBeforePrompt { get; set; }

        /// <summary>
        /// Registers successful usage of the extension. Only one is registered per Visual Studio session.
        /// When the number of usages matches <see cref="RequestsBeforePrompt"/>, the prompt will be shown.
        /// </summary>
        public virtual void RegisterSuccessfulUsage()
        {
            if (_hasAlreadyRequested.TryAdd(MarketplaceId, true) && Config.RatingRequests < RequestsBeforePrompt)
            {
                IncrementAsync().FireAndForget();
            }
        }

        /// <summary>
        /// Resets the count of successful usages and starts over.
        /// </summary>
        public virtual async Task ResetAsync()
        {
            _hasAlreadyRequested.TryRemove(MarketplaceId, out _);
            Config.RatingRequests = 0;
            await Config.SaveAsync();
        }

        private async Task IncrementAsync()
        {
            await Task.Yield(); // Yield to allow any shutdown procedure to continue

            if (VsShellUtilities.ShellIsShuttingDown)
            {
                return;
            }

            Config.RatingRequests += 1;
            await Config.SaveAsync();

            if (Config.RatingRequests == RequestsBeforePrompt)
            {
                PromptAsync().FireAndForget();
            }
        }

        private async Task PromptAsync()
        {
            InfoBar? infoBar = await CreateInfoBarAsync();

            if (infoBar == null)
            {
                return;
            }

            if (await infoBar.TryShowInfoBarUIAsync())
            {
                if (infoBar.TryGetWpfElement(out Control? control))
                {
                    control?.SetResourceReference(Control.BackgroundProperty, EnvironmentColors.SearchBoxBackgroundBrushKey);
                }

                // Automatically close the InfoBar after a period of time
                await Task.Delay(_minutesVisible * 60 * 1000);

                if (infoBar.IsVisible)
                {
                    await ResetAsync();
                    infoBar.Close();
                }
            }
        }

        private async Task<InfoBar?> CreateInfoBarAsync()
        {
            InfoBarModel model = new(
                    new[] {
                        new InfoBarTextSpan("Are you enjoying the "),
                        new InfoBarTextSpan(ExtensionName, true),
                        new InfoBarTextSpan(" extension? Help spread the word by leaving a review.")
                    },
                    new[] {
                        new InfoBarHyperlink("Rate it now"),
                        new InfoBarHyperlink("Remind me later"),
                        new InfoBarHyperlink("Don't show again"),
                    },
                    KnownMonikers.Extension,
                    true);

            InfoBar? infoBar = await VS.InfoBar.CreateAsync(model);

            if (infoBar != null)
            {
                infoBar.ActionItemClicked += ActionItemClicked;
            }

            return infoBar;
        }

        private void ActionItemClicked(object sender, InfoBarActionItemEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (e.ActionItem.Text == "Rate it now")
            {
                Process.Start(RatingUrl.OriginalString);
            }
            else if (e.ActionItem.Text == "Remind me later")
            {
                ResetAsync().FireAndForget();
            }

            e.InfoBarUIElement.Close();

            ((InfoBar)sender).ActionItemClicked -= ActionItemClicked;
        }
    }
}
