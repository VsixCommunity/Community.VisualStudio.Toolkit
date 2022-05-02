using System;
using TestExtension.ToolWindows;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using System.Windows.Controls;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.ToolWindows
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
        public RatingPrompt(string marketplaceId, string extensionName, IRatingConfig config = null, int requestsBeforePrompt = 5)
        {
            MarketplaceId = marketplaceId ?? throw new ArgumentNullException(nameof(marketplaceId));
            ExtensionName = extensionName ?? throw new ArgumentNullException(nameof(extensionName));
            Config = config;
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
        public virtual IRatingConfig? Config { get; }

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
            if (Config == null)
            {
                throw new NullReferenceException("The Config property has not been set.");
            }

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

            if (Config != null)
            {
                Config.RatingRequests = 0;
                await Config.SaveAsync();
            }
        }

        private async Task IncrementAsync()
        {
            await Task.Yield(); // Yield to allow any shutdown procedure to continue

            if (VsShellUtilities.ShellIsShuttingDown || Config == null)
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

        /// <summary>
        /// Prompts the user to rate the extension.
        /// </summary>
        public async Task PromptAsync()
        {
            var window = new RatingPromptWindow(this);
            var result = window.ShowDialog();

            if (window.Choice == RateChoice.RateNow)
            {
                Process.Start(RatingUrl.OriginalString);
            }
            else if (window.Choice == RateChoice.RateLater)
            {
                ResetAsync().FireAndForget();
            }
        }
    }
}
