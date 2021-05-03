namespace Community.VisualStudio.Toolkit
{
        /// <summary>A list of content types for known languages.</summary>
        public class ContentTypes
        {
            /// <summary>Applies to all languages.</summary>
            public const string Any = "any";
            /// <summary>The base content type of all text documents including 'code'.</summary>
            public const string Text = "text";
            /// <summary>The base content type of all coding text documents and languages.</summary>
            public const string Code = "code";


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public const string CSharp = "CSharp";
            public const string VisualBasic = "Basic";
            public const string FSharp = "F#";
            public const string CPlusPlus = "C/C++";
            public const string Css = "CSS";
            public const string Less = "LESS";
            public const string Scss = "SCSS";
            public const string HTML = "HTMLX";
            public const string WebForms = "HTML";
            public const string Json = "JSON";
            public const string Xaml = "XAML";
            public const string Xml = "XML";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
