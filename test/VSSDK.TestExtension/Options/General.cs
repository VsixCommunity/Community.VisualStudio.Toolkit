using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    internal partial class OptionsProvider
    {
        // Register the options with these attributes in your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "My options", "General", 0, 0, true)]
        // [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "My options", "General", 0, 0, true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("My category")]
        [DisplayName("My Options")]
        [Description("An informative description.")]
        [DefaultValue(true)]
        public bool MyOption { get; set; } = true;

        [Category("My category")]
        [DisplayName("String Value")]
        [Description("This is a string.")]
        [DefaultValue("Default")]
        public string MyString { get; set; } = "Default";

        [Category("My category")]
        [DisplayName("List of Strings")]
        [Description("This is a list of strings.")]
        public string[] MyListOfStrings { get; set; } = new string[0];

        [Category("My category")]
        [DisplayName("Color Value")]
        [Description("My Favorite Color")]
        public Color FavoriteColor { get; set; } = Color.Purple;

        [Category("My category")]
        [DisplayName("Comparison Method")]
        [Description("How to Compare Apples and Oranges")]
        [DefaultValue("IgnoreKanaType")]
        public CompareOptions MyComparisonMethod { get; set; } = CompareOptions.IgnoreKanaType;

        [Category("My category")]
        [DisplayName("MyBirthday")]
        [Description("When the Toolkit was Born")]
        [DefaultValue("2021-04-11")]
        public DateTime MyBirthday { get; set; } = new DateTime(2021, 04, 11);

        public General() : base()
        {
            Saved += delegate { VS.StatusBar.ShowMessageAsync("Options Saved").FireAndForget(); };
        }
    }
}
