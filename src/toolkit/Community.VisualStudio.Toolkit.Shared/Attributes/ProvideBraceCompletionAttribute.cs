using System;

namespace Community.VisualStudio.Toolkit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ProvideBraceCompletionAttribute : RegistrationAttribute
    {
        private readonly string _languageName;
        public ProvideBraceCompletionAttribute(string languageName)
        {
            _languageName = languageName;
        }

        public override void Register(RegistrationContext context)
        {
            var keyName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", _languageName);
            using (Key langKey = context.CreateKey(keyName))
            {
                langKey.SetValue("ShowBraceCompletion", 1);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            var keyName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", _languageName);
            context.RemoveKey(keyName);
        }
    }
}
