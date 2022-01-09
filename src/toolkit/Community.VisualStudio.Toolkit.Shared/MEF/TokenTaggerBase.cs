using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for tagging up a document using <see cref="TokenTag"/>.
    /// </summary>
    public abstract class TokenTaggerBase : ITagger<TokenTag>
    {
        /// <summary>
        /// Creates a new instance of the tagger base class.
        /// </summary>
        /// <param name="buffer">The text buffer.</param>
        /// <param name="runFirstTokenizationImmediately">Determines the thread priority of the first tokenization run.</param>
        public TokenTaggerBase(ITextBuffer buffer, bool runFirstTokenizationImmediately = true)
        {
            Buffer = buffer;
            Tokenize(runFirstTokenizationImmediately);
        }

        /// <summary>
        /// The current text buffer.
        /// </summary>
        public ITextBuffer Buffer { get; }

        /// <summary>
        /// The cache of all the tag spans. This cache must be updated from the <see cref="TokenizeAsync"/> method.
        /// </summary>
        public List<ITagSpan<TokenTag>> TagsCache { get; private set; } = new();

        private void Tokenize(bool runFirstTokenizationImmediately)
        {
            VsTaskRunContext context = runFirstTokenizationImmediately ? VsTaskRunContext.UIThreadNormalPriority : VsTaskRunContext.UIThreadIdlePriority;

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            _ = ThreadHelper.JoinableTaskFactory.StartOnIdleShim(async () =>
            {
                await TaskScheduler.Default;
                await TokenizeAsync();
            }, context);
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
        }

        /// <summary>
        /// Call this method from a background thread to update the <see cref="TagsCache"/>.
        /// </summary>
        /// <returns></returns>
        public abstract Task TokenizeAsync();

        /// <inheritdoc/>
        public IEnumerable<ITagSpan<TokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return TagsCache;
        }

        /// <summary>
        /// Creates a <see cref="TokenTag"/>. When all tags are created, invoke <see cref="OnTagsUpdated"/> to update the <see cref="TagsCache"/>.
        /// </summary>
        /// <param name="tokenType">An object (could be a string or type) that identifies the type of token. This used for syntax highlighting.</param>
        /// <param name="hasTooltip">If true, the <see cref="GetTooltipAsync"/> method must be overridden.</param>
        /// <param name="supportOutlining">If true, the <see cref="GetOutliningText"/> method is assigned the token. Override it to customize outlining.</param>
        /// <param name="errors">Optional. List of errors associated with the token. This is used by the error tagger and will Error List.</param>
        public virtual TokenTag CreateToken(object tokenType, bool hasTooltip, bool supportOutlining, IEnumerable<ErrorListItem> errors)
        {
            return new TokenTag(tokenType, errors.ToArray())
            {
                GetTooltipAsync = hasTooltip ? GetTooltipAsync : null,
                GetOutliningText = supportOutlining ? GetOutliningText : null,
            };
        }

        /// <summary>
        /// A callback for when the user hovers the mouse over a token in the document.
        /// </summary>
        /// <param name="triggerPoint">The point where the mouse is that caused the tooltip (QuickInfo) to be invoked.</param>
        /// <returns><see langword="null"/>, or any text or WPF content to show in the tooltip (QuickInfo).</returns>
        public virtual Task<object> GetTooltipAsync(SnapshotPoint triggerPoint)
        {
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// A callback for getting the text shown in the collapsed state of the outlining tag.
        /// </summary>
        /// <param name="text">The entirety of the text to collapse.</param>
        public virtual string? GetOutliningText(string text)
        {
            return text?.Split('\n').FirstOrDefault().Trim();
        }

        /// <summary>
        /// Call this when tokenization is complete and <see cref="TagsCache"/> should be updated and <see cref="TagsChanged"/> invoked.
        /// </summary>
        /// <param name="tags"></param>
        protected void OnTagsUpdated(List<ITagSpan<TokenTag>> tags)
        {
            TagsCache = tags;
            SnapshotSpan span = new(Buffer.CurrentSnapshot, 0, Buffer.CurrentSnapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        /// <inheritdoc/>
        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
    }
}
