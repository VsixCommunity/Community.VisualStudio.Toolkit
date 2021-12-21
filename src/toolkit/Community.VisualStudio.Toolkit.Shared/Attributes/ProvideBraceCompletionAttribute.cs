using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Adds support for brace completion to the specified language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ProvideBraceCompletionAttribute : RegistrationAttribute
    {
        private readonly string _languageName;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="languageName">The language name to add brace completion to.</param>
        public ProvideBraceCompletionAttribute(string languageName)
        {
            _languageName = languageName;
        }

        /// <inheritdoc/>
        public override void Register(RegistrationContext context)
        {
            string keyName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", _languageName);
            using (Key langKey = context.CreateKey(keyName))
            {
                langKey.SetValue("ShowBraceCompletion", 1);
            }
        }

        /// <inheritdoc/>
        public override void Unregister(RegistrationContext context)
        {
            string keyName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", _languageName);
            context.RemoveKey(keyName);
        }
    }
}
