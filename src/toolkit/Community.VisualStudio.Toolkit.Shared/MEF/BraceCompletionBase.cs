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
                context = GetCompletionContext();
                return true;
            }
            else
            {
                context = null!;
                return false;
            }
        }
        /// <summary>
        /// Return object that implements the CompletionContext that you want to use
        /// </summary>
        /// <returns>an object of type IBraceCompletionContext</returns>
        /// <remarks>The default implementation returns an object that allows 'everything'</remarks>
        protected virtual IBraceCompletionContext GetCompletionContext()
        {
            return new DefaultBraceCompletionContext();
        }

        /// <summary>
        /// Determine if brace completion should be active in this context
        /// </summary>
        /// <param name="openingPoint">Point where the brace completion is triggered</param>
        /// <remarks>You can use this method to disable brace completion in comments or inside literal strings
        /// The default behavior is to disable completion inside comments and literal strings, which is determined
        /// by looking at the 
        /// </remarks>
        /// <returns>true when completion should be allowed</returns>
        protected virtual bool IsValidBraceCompletionContext(SnapshotPoint openingPoint)
        {
            IList<ClassificationSpan>? classificationSpans = GetSpans(openingPoint);
            if (classificationSpans != null)
            {
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
        /// <summary>
        /// Return a list of Classification spans for the point in the snapshot
        /// </summary>
        /// <param name="point">Point for which the list should be returned</param>
        /// <returns>A List of null when the position is invalid or when the classifier service is not available</returns>
        protected IList<ClassificationSpan>? GetSpans(SnapshotPoint point)
        {
            if (point.Position > 0 && _classifierService != null)
            {
                IList<ClassificationSpan> classificationSpans = _classifierService.GetClassifier(point.Snapshot.TextBuffer)
                                           .GetClassificationSpans(new SnapshotSpan(point - 1, 1));
                return classificationSpans;
            }
            return null;
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