using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Associates an <c>ImageMoniker</c> icon to a file extension in Solution Explorer
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ProvideFileIconAttribute : RegistrationAttribute
    {
        /// <summary>
        /// Associates an icon with a file extension.
        /// </summary>
        /// <param name="fileExtension">Any file extension. Must start with a dot.</param>
        /// <param name="monikerName">Could be "KnownMonikers.Save" or "guid:id".</param>
        public ProvideFileIconAttribute(string fileExtension, string monikerName)
        {
            FileExtension = fileExtension;
            MonikerName = monikerName;
        }

        /// <summary>
        /// The file extension that's the target of the icon.
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// The <c>ImageMoniker</c> identifier. It's either a KnownMonker or a guid:id pair.
        /// </summary>
        public string MonikerName { get; }

        /// <inheritdoc/>
        public override void Register(RegistrationContext context)
        {
            using (Key langKey = context.CreateKey($@"ShellFileAssociations\{FileExtension}"))
            {
                langKey.SetValue("DefaultIconMoniker", MonikerName);
            }
        }

        /// <inheritdoc/>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey($@"ShellFileAssociations\{FileExtension}");
        }
    }
}
