using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Threading;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for providing same-word highlighting.
    /// </summary>
    public class SameWordHighlighterBase : IViewTaggerProvider
    {
        [Import] internal ITextSearchService? _textSearchService = null;

        [Import] internal ITextStructureNavigatorSelectorService? _textStructureNavigatorSelector = null;

        /// <summary>
        /// The Options that are used to find the matching words. The default implementation returns 
        /// FindOptions.WholeWord | FindOptions.MatchCase
        /// </summary>
        public virtual FindOptions FindOptions => FindOptions.WholeWord | FindOptions.MatchCase;
        /// <summary>
        /// Filter the results. 
        /// </summary>
        /// <param name="results">Collection of the results</param>
        /// <returns>Filtered list of results. The default implementation returns all the results</returns>
        public virtual IEnumerable<SnapshotSpan>? FilterResults(IEnumerable<SnapshotSpan>? results) => results;
        /// <summary>
        /// Should the Highlight code be triggered for this word
        /// </summary>
        /// <param name="text">The word to highlight</param>
        /// <returns>true to continue the highlight or false to prevent the highlight.
        /// The default implementation always returns true.</returns>
        public virtual bool ShouldHighlight(string? text) => true;

        /// <inheritdoc/>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITextStructureNavigator? navigator = _textStructureNavigatorSelector?.GetTextStructureNavigator(textView.TextBuffer);

            return (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() => 
                new SameWordHighlighterTagger(textView, buffer, _textSearchService, navigator, this));
        }
    }

    internal class HighlightWordTag : TextMarkerTag
    {
        public HighlightWordTag() : base("MarkerFormatDefinition/HighlightWordFormatDefinition") { }
    }

    internal class SameWordHighlighterTagger : ITagger<HighlightWordTag>, IDisposable
    {
        private readonly ITextView _view;
        private readonly ITextBuffer _buffer;
        private readonly ITextSearchService? _textSearchService;
        private readonly ITextStructureNavigator? _textStructureNavigator;
        private readonly SameWordHighlighterBase _tagger;
        private NormalizedSnapshotSpanCollection _wordSpans;
        private SnapshotSpan? _currentWord;
        private SnapshotPoint _requestedPoint;
        private bool _isDisposed;
        private readonly object _syncLock = new();

        public SameWordHighlighterTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService? textSearchService, 
            ITextStructureNavigator? textStructureNavigator, SameWordHighlighterBase tagger)
        {
            _view = view;
            _buffer = sourceBuffer;
            _textSearchService = textSearchService;
            _textStructureNavigator = textStructureNavigator;
            _tagger = tagger;
            _wordSpans = new NormalizedSnapshotSpanCollection();
            _currentWord = null;
            _view.Caret.PositionChanged += CaretPositionChanged;
            _view.LayoutChanged += ViewLayoutChanged;
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(_view.Caret.Position);
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        private void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            SnapshotPoint? point = caretPosition.Point.GetPoint(_buffer, caretPosition.Affinity);

            if (!point.HasValue)
            {
                return;
            }

            _requestedPoint = point.Value;
            TextExtent? word = _textStructureNavigator?.GetExtentOfWord(_requestedPoint);

            if (word.HasValue && word.Value.IsSignificant && word.Value.Span.Length > 1)
            {
                StartOnIdle(ThreadHelper.JoinableTaskFactory, () =>
                {
                    UpdateWordAdornments(word.Value);
                }, VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
            }
        }

        private void UpdateWordAdornments(TextExtent word)
        {
            SnapshotPoint currentRequest = _requestedPoint;
            List<SnapshotSpan>? wordSpans = new();

            string? text = word.Span.GetText();
            if (_tagger.ShouldHighlight(text))
            {
                FindData findData = new(text, word.Span.Snapshot)
                {
                    FindOptions = _tagger.FindOptions
                };

                System.Collections.ObjectModel.Collection<SnapshotSpan>? found = _textSearchService!.FindAll(findData);
                wordSpans.AddRange(_tagger.FilterResults(found));

                if (wordSpans.Count == 1)
                {
                    wordSpans.Clear();
                }
            }
            //If another change hasn't happened, do a real update
            if (currentRequest == _requestedPoint)
            {
                SynchronousUpdate(currentRequest, new NormalizedSnapshotSpanCollection(wordSpans), word.Span);
            }
        }

        private void SynchronousUpdate(SnapshotPoint currentRequest, NormalizedSnapshotSpanCollection newSpans, SnapshotSpan? newCurrentWord)
        {
            lock (_syncLock)
            {
                if (currentRequest != _requestedPoint)
                {
                    return;
                }

                _wordSpans = newSpans;
                _currentWord = newCurrentWord;

                SnapshotSpan span = new(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }

        public IEnumerable<ITagSpan<HighlightWordTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_currentWord == null)
            {
                yield break;
            }

            // Hold on to a "snapshot" of the word spans and current word, so that we maintain the same
            // collection throughout
            SnapshotSpan currentWord = _currentWord.Value;
            NormalizedSnapshotSpanCollection wordSpans = _wordSpans;

            if (spans.Count == 0 || _wordSpans.Count == 0)
            {
                yield break;
            }

            // If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot
            if (spans[0].Snapshot != wordSpans[0].Snapshot)
            {
                wordSpans = new NormalizedSnapshotSpanCollection(
                    wordSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));

                currentWord = currentWord.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
            }

            // First, yield back the word the cursor is under (if it overlaps)
            // Note that we'll yield back the same word again in the wordspans collection;
            // the duplication here is expected.
            if (spans.OverlapsWith(new NormalizedSnapshotSpanCollection(currentWord)))
            {
                yield return new TagSpan<HighlightWordTag>(currentWord, new HighlightWordTag());
            }

            // Second, yield all the other words in the file
            foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(spans, wordSpans))
            {
                yield return new TagSpan<HighlightWordTag>(span, new HighlightWordTag());
            }
        }

        //Schedules a delegate for background execution on the UI thread without inheriting any claim to the UI thread from its caller.
        private static JoinableTask StartOnIdle(JoinableTaskFactory joinableTaskFactory, Action action, VsTaskRunContext priority = VsTaskRunContext.UIThreadBackgroundPriority)
        {
            using (joinableTaskFactory.Context.SuppressRelevance())
            {
                return joinableTaskFactory.RunAsync(priority, async delegate
                {
                    await System.Threading.Tasks.Task.Yield();
                    await joinableTaskFactory.SwitchToMainThreadAsync();
                    action();
                });
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _view.Caret.PositionChanged -= CaretPositionChanged;
                _view.LayoutChanged -= ViewLayoutChanged;
            }

            _isDisposed = true;
        }

        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
    }
}
