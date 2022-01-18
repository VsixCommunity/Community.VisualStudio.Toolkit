using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An error tagger based on TokenTag.
    /// </summary>
    public abstract class TokenErrorTaggerBase : ITaggerProvider
    {
        [Import] internal IBufferTagAggregatorFactoryService? _bufferTagAggregator = null;

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<TokenTag>? tags = _bufferTagAggregator?.CreateTagAggregator<TokenTag>(buffer);
            return (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() => new ErrorTagger(tags));
        }
    }

    internal class ErrorTagger : InternalTaggerBase<IErrorTag>
    {
        private readonly TableDataSource _dataSource;

        public ErrorTagger(ITagAggregator<TokenTag>? tags) : base(tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            _dataSource = new TableDataSource(tags.BufferGraph.TopBuffer.ContentType.DisplayName);
        }

        public override IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans, bool isFullParse)
        {
            IEnumerable<IMappingTagSpan<TokenTag>> tags = Tags!.GetTags(spans).Where(t => !t.Tag.IsValid);

            foreach (IMappingTagSpan<TokenTag> tag in tags)
            {
                NormalizedSnapshotSpanCollection tagSpans = tag.Span.GetSpans(tag.Span.AnchorBuffer.CurrentSnapshot);
                string tooltip = string.Join(Environment.NewLine, tag.Tag.Errors);
                ErrorTag errorTag = new(GetErrorType(tag.Tag.Errors), tooltip);

                foreach (SnapshotSpan span in tagSpans)
                {
                    yield return new TagSpan<IErrorTag>(span, errorTag);
                }
            }

            if (isFullParse)
            {
                PopulateErrorList(tags);
            }
        }

        private static string GetErrorType(IEnumerable<ErrorListItem> errors)
        {
            return errors.FirstOrDefault()?.ErrorCategory ?? PredefinedErrorTypeNames.SyntaxError;
        }

        private void PopulateErrorList(IEnumerable<IMappingTagSpan<TokenTag>> tags)
        {
            IEnumerable<ErrorListItem> errors = tags.SelectMany(t => t.Tag.Errors);

            if (!errors.Any())
            {
                _dataSource?.CleanAllErrors();
            }
            else
            {
                _dataSource?.AddErrors(errors);
            }
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dataSource?.CleanAllErrors();
            }

            base.Dispose(disposing);
        }
    }
}
