using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Default implementation of brace completion.
    /// </summary>
    public abstract class BraceCompletionBase : IBraceCompletionContextProvider
    {
        [Import] internal IClassifierAggregatorService? _classifierService = null;

        /// <inheritdoc/>
        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            if (IsValidBraceCompletionContext(openingPoint))
            {
                context = new DefaultBraceCompletionContext();
                return true;
            }
            else
            {
                context = null!;
                return false;
            }
        }

        private bool IsValidBraceCompletionContext(SnapshotPoint openingPoint)
        {
            if (openingPoint.Position > 0 && _classifierService != null)
            {
                IList<ClassificationSpan> classificationSpans = _classifierService.GetClassifier(openingPoint.Snapshot.TextBuffer)
                                                           .GetClassificationSpans(new SnapshotSpan(openingPoint - 1, 1));

                foreach (ClassificationSpan span in classificationSpans)
                {
                    if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Comment))
                    {
                        return false;
                    }
                    if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.String))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    internal class DefaultBraceCompletionContext : IBraceCompletionContext
    {
        public bool AllowOverType(IBraceCompletionSession session) => true;

        public void Finish(IBraceCompletionSession session)
        {
        }

        public void OnReturn(IBraceCompletionSession session)
        {
        }

        public void Start(IBraceCompletionSession session)
        {
        }
    }
}