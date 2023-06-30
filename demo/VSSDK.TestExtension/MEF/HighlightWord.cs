using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Community.VisualStudio.Toolkit;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;
namespace TestExtension.MEF
{
    
    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/HighlightWordFormatDefinition")]
    [UserVisible(true)]
    internal class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }

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
