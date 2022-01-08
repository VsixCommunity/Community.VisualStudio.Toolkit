using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class to provide classification (syntax highlighting) based on a Token Tagger.
    /// </summary>
    public abstract class TokenClassificationTaggerBase : ITaggerProvider
    {
        [Import] internal IClassificationTypeRegistryService? _classificationRegistry = null;
        [Import] internal IBufferTagAggregatorFactoryService? _bufferTagAggregator = null;

        /// <summary>
        /// A map of a token value and which classification name it corresponds with.
        /// </summary>
        public abstract Dictionary<object, string> ClassificationMap { get; }

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<TokenTag>? tags = _bufferTagAggregator?.CreateTagAggregator<TokenTag>(buffer);
            return (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() => new TokenClassifier(_classificationRegistry, tags, ClassificationMap));
        }
    }

    internal class TokenClassifier : InternalTaggerBase<IClassificationTag>
    {
        private static readonly Dictionary<object, ClassificationTag> _classificationMap = new();

        internal TokenClassifier(IClassificationTypeRegistryService? registry, ITagAggregator<TokenTag>? tags, Dictionary<object, string> map) : base(tags)
        {
            foreach (object key in map.Keys)
            {
                _classificationMap[key] = new ClassificationTag(registry?.GetClassificationType(map[key]));
            }
        }

        public override IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans, bool isFullParse)
        {
            foreach (IMappingTagSpan<TokenTag> tag in Tags!.GetTags(spans))
            {
                if (_classificationMap.TryGetValue(tag.Tag.TokenType ?? "", out ClassificationTag classificationTag))
                {
                    NormalizedSnapshotSpanCollection tagSpans = tag.Span.GetSpans(tag.Span.AnchorBuffer.CurrentSnapshot);

                    foreach (SnapshotSpan tagSpan in tagSpans)
                    {
                        yield return new TagSpan<ClassificationTag>(tagSpan, classificationTag);
                    }
                }
            }
        }
    }
}
