using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for a DialogPage to show in Tools -> Options.
    /// </summary>
    public class BaseOptionPage<T> : DialogPage where T : BaseOptionModel<T>, new()
    {
        private readonly BaseOptionModel<T> _model;

        /// <summary>
        /// Creates a new instance of the options page.
        /// </summary>
        public BaseOptionPage()
        {
            _model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<T>.CreateAsync);
        }

        /// <summary>The model object to load and store.</summary>
        public override object AutomationObject => _model;

        /// <summary>Loads the settings from the internal storage.</summary>
        public override void LoadSettingsFromStorage()
        {
            _model.Load();
        }

        /// <summary>Saves settings to the internal storage.</summary>
        public override void SaveSettingsToStorage()
        {
            _model.Save();
        }
    }
}