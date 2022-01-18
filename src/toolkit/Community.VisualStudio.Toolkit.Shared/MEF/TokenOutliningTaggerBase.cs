#if VS16 || VS17
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for providing outlining based on Token Tags.
    /// </summary>
    public abstract class TokenOutliningTaggerBase : ITaggerProvider
    {
        [Import] internal IBufferTagAggregatorFactoryService? _bufferTagAggregator = null;

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<TokenTag>? tags = _bufferTagAggregator?.CreateTagAggregator<TokenTag>(buffer);
            return (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() => new StructureTagger(tags));
        }
    }

    internal class StructureTagger : InternalTaggerBase<IStructureTag>
    {
        public StructureTagger(ITagAggregator<TokenTag>? tags) : base(tags)
        { }

        public override IEnumerable<ITagSpan<IStructureTag>> GetTags(NormalizedSnapshotSpanCollection spans, bool isFullParse)
        {
            foreach (IMappingTagSpan<TokenTag> tag in Tags!.GetTags(spans))
            {
                if (tag.Tag.GetOutliningText == null)
                {
                    continue;
                }

                NormalizedSnapshotSpanCollection tagSpans = tag.Span.GetSpans(tag.Span.AnchorBuffer.CurrentSnapshot);

                foreach (SnapshotSpan tagSpan in tagSpans)
                {
                    string text = tagSpan.GetText().TrimEnd();
                    SnapshotSpan span = new(tagSpan.Snapshot, tagSpan.Start, text.Length);
                    yield return CreateTag(span, text, tag.Tag);
                }
            }
        }

        private static TagSpan<IStructureTag> CreateTag(SnapshotSpan span, string text, TokenTag tag)
        {
            StructureTag structureTag = new(
                        span.Snapshot,
                        outliningSpan: span,
                        guideLineSpan: span,
                        guideLineHorizontalAnchor: span.Start,
                        type: PredefinedStructureTagTypes.Structural,
                        isCollapsible: true,
                        collapsedForm: tag.GetOutliningText(text),
                        collapsedHintForm: null);

            return new TagSpan<IStructureTag>(span, structureTag);
        }
    }
}
#endif