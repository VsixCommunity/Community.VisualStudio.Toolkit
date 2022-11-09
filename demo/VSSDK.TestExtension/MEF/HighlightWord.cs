using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Community.VisualStudio.Toolkit;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;

namespace TestExtension.MEF
{
    /// <summary>
    /// This class demonstrates a HighlightWord tagger for text files
    /// and it only highlights whole words starting with a Letter
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("Text")]
    [TagType(typeof(TextMarkerTag))]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class HighlightWordTaggerProvider : SameWordHighlighterBase
    {
        public override FindOptions FindOptions => FindOptions.WholeWord;
        public override bool ShouldHighlight(string text)
        {
            if (text?.Length > 0)
                return char.IsLetter(text[0]);
            return false;
        }

    }
}
