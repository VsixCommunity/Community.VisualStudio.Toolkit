using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Registers a feed gallery to the extensions manager
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ProvideGalleryFeedAttribute : RegistrationAttribute
    {
        /// <summary>
        /// Registers a feed gallery
        /// </summary>
        /// <param name="guid">A unique guid to use for registering the feed.</param>
        /// <param name="name">The name of the feed as it shows up in the Extension Manager dialog.</param>
        /// <param name="url">The absolute URL to the atom feed.</param>
        public ProvideGalleryFeedAttribute(string guid, string name, string url)
        {
            Guid = guid;
            Name = name;
            Url = url;
        }

        /// <summary>
        /// A unique guid to use for registering the feed.
        /// </summary>
        public string Guid { get; }

        /// <summary>
        /// The name of the feed as it shows up in the Extension Manager dialog.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The absolute URL to the atom feed.
        /// </summary>
        public string Url { get; }

        /// <inheritdoc/>
        public override void Register(RegistrationContext context)
        {
            using (Key langKey = context.CreateKey($@"ExtensionManager\Repositories\{{{Guid}}}"))
            {
                langKey.SetValue("", Url);
                langKey.SetValue("Priority", 100);
                langKey.SetValue("Protocol", "Atom Feed");
                langKey.SetValue("DisplayName", Name);
            }
        }

        /// <inheritdoc/>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey($@"ExtensionManager\Repositories\{Guid}");
        }
    }
}
