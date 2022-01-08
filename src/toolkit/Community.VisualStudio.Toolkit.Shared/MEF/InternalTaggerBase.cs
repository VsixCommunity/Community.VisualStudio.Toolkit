using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for creating token taggers for your custom language implementation.
    /// </summary>
    /// <typeparam name="TTag"></typeparam>
    internal abstract class InternalTaggerBase<TTag> : ITagger<TTag>, IDisposable where TTag : ITag
    {
        private bool _isDisposed;

        /// <summary>
        /// Creates a new instance of the base class.
        /// </summary>
        /// <param name="tags"></param>
        public InternalTaggerBase(ITagAggregator<TokenTag>? tags)
        {
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            Tags.TagsChanged += TokenTagsChanged;
        }

        /// <summary>
        /// The collection of Token Tags.
        /// </summary>
        public ITagAggregator<TokenTag> Tags { get; }

        private void TokenTagsChanged(object sender, TagsChangedEventArgs e)
        {
            ITextBuffer buffer = e.Span.BufferGraph.TopBuffer;
            SnapshotSpan span = new(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length);

            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        /// <inheritdoc/>
        public IEnumerable<ITagSpan<TTag>>? GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans[0].IsEmpty)
            {
                return null;
            }

            bool isFullParse = spans.First().Start == 0 && spans.Last().End == spans[0].Snapshot.Length;
            return GetTags(spans, isFullParse);
        }

        /// <summary>
        /// Override to provide custom tags
        /// </summary>
        public abstract IEnumerable<ITagSpan<TTag>> GetTags(NormalizedSnapshotSpanCollection spans, bool isFullParse);

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing && Tags != null)
                {
                    Tags.TagsChanged -= TokenTagsChanged;
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
    }
}
