using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the editor.</summary>
    public class Editor
    {
        internal Editor()
        { }

        /// <summary>Gets an instance of <see cref="TextDocument"/> from the currently active document.</summary>
        public async Task<TextDocument?> GetActiveTextDocumentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 dte = await VS.GetDTEAsync();
            return dte.ActiveDocument.Object("TextDocument") as TextDocument;
        }

        /// <summary>Gets the WPF text view from the currently active document.</summary>
        public async Task<IWpfTextView?> GetCurrentWpfTextViewAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IComponentModel2 compService = await VS.GetServiceAsync<SComponentModel, IComponentModel2>();
            IVsEditorAdaptersFactoryService? editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            IVsTextView viewAdapter = await GetCurrentNativeTextViewAsync();

            return editorAdapter.GetWpfTextView(viewAdapter);
        }

        /// <summary>Gets the native text view from the currently active document.</summary>
        public async Task<IVsTextView> GetCurrentNativeTextViewAsync()
        {
            IVsTextManager textManager = await VS.GetServiceAsync<SVsTextManager, IVsTextManager>();
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));

            return activeView;
        }

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
}
