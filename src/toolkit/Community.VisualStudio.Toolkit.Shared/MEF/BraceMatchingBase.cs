using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for creating brace matching highlights.
    /// </summary>
    public abstract class BraceMatchingBase : IViewTaggerProvider
    {
        /// <summary>
        /// A dictionary with open and closing braces to highlight.
        /// </summary>
        public virtual Dictionary<char, char> BraceList { get; } = new()
        {
            { '{', '}' },
            { '(', ')' },
            { '[', ']' },
        };

        /// <summary>
        /// Other predefined types include "bracehightligh", "blue", "vivid".
        /// </summary>
        public virtual string TextMarketTagType => "bracehighlight";

        /// <inheritdoc />
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag =>
            (ITagger<T>)buffer.Properties.GetOrCreateSingletonProperty(() => new BraceMatchingTagger(textView, BraceList, TextMarketTagType));
    }

    internal class BraceMatchingTagger : ITagger<TextMarkerTag>
    {
        private readonly ITextView _view;
        private readonly ITextBuffer _buffer;
        private readonly Dictionary<char, char> _braceList;
        private SnapshotPoint? _currentChar;
        private readonly TextMarkerTag _tag;

        internal BraceMatchingTagger(ITextView view, Dictionary<char, char> braceList, string textMarkerTagType)
        {
            _view = view;
            _buffer = view.TextBuffer;
            _braceList = braceList;
            _tag = new(textMarkerTagType);

            _view.Caret.PositionChanged += CaretPositionChanged;
            _view.LayoutChanged += ViewLayoutChanged;
            _view.Closed += ViewClosed;
        }

        private void ViewClosed(object sender, EventArgs e)
        {
            _view.Closed -= ViewClosed;
            _view.Caret.PositionChanged -= CaretPositionChanged;
            _view.LayoutChanged -= ViewLayoutChanged;
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
            _currentChar = caretPosition.Point.GetPoint(_buffer, caretPosition.Affinity);

            if (_currentChar.HasValue)
            {
                SnapshotSpan snapshot = new(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(snapshot));
            }
        }

        public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;

        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (!_currentChar.HasValue || _currentChar.Value.Position > _currentChar.Value.Snapshot.Length)
            {
                yield break;
            }

            SnapshotPoint currentChar = _currentChar.Value;

            if (spans[0].Snapshot != currentChar.Snapshot)
            {
                currentChar = currentChar.TranslateTo(spans[0].Snapshot, PointTrackingMode.Positive);
            }

            char currentText = currentChar.Position == currentChar.Snapshot.Length ? '\0' : currentChar.GetChar();
            SnapshotPoint lastChar = currentChar == 0 ? currentChar : currentChar - 1;
            char lastText = lastChar.Position == lastChar.Snapshot.Length ? '\0' : lastChar.GetChar();

            if (_braceList.TryGetValue(currentText, out char closeChar))
            {
                if (FindMatchingCloseChar(currentChar, currentText, closeChar, out SnapshotSpan pairSpan))
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(currentChar, 1), _tag);
                    yield return new TagSpan<TextMarkerTag>(pairSpan, _tag);
                }
            }
            else if (_braceList.ContainsValue(lastText))
            {
                char open = _braceList.FirstOrDefault(b => b.Value == lastText).Key;

                if (FindMatchingOpenChar(lastChar, open, lastText, out SnapshotSpan pairSpan))
                {
                    yield return new TagSpan<TextMarkerTag>(new SnapshotSpan(lastChar, 1), _tag);
                    yield return new TagSpan<TextMarkerTag>(pairSpan, _tag);
                }
            }

        }

        private bool FindMatchingCloseChar(SnapshotPoint startPoint, char open, char close, out SnapshotSpan pairSpan)
        {
            int maxLines = _view.TextViewLines.Count;
            pairSpan = new SnapshotSpan(startPoint.Snapshot, 1, 1);
            ITextSnapshotLine line = startPoint.GetContainingLine();
            string? lineText = line.GetText();
            int lineNumber = line.LineNumber;
            int offset = startPoint.Position - line.Start.Position + 1;

            int stopLineNumber = startPoint.Snapshot.LineCount - 1;
            if (maxLines > 0)
            {
                stopLineNumber = Math.Min(stopLineNumber, lineNumber + maxLines);
            }

            int openCount = 0;
            while (true)
            {
                while (offset < line.Length)
                {
                    char currentChar = lineText[offset];
                    if (currentChar == close)
                    {
                        if (openCount > 0)
                        {
                            openCount--;
                        }
                        else
                        {
                            pairSpan = new SnapshotSpan(startPoint.Snapshot, line.Start + offset, 1);
                            return true;
                        }
                    }
                    else if (currentChar == open)
                    {
                        openCount++;
                    }
                    offset++;
                }
                if (++lineNumber > stopLineNumber)
                {
                    break;
                }

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = 0;
            }

            return false;
        }

        private bool FindMatchingOpenChar(SnapshotPoint startPoint, char open, char close, out SnapshotSpan pairSpan)
        {
            int maxLines = _view.TextViewLines.Count;
            pairSpan = new SnapshotSpan(startPoint, startPoint);

            ITextSnapshotLine line = startPoint.GetContainingLine();

            int lineNumber = line.LineNumber;
            int offset = startPoint - line.Start - 1;

            if (offset < 0)
            {
                if (--lineNumber < 0)
                {
                    return false;
                }

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                offset = line.Length - 1;
            }

            string? lineText = line.GetText();

            int stopLineNumber = 0;
            if (maxLines > 0)
            {
                stopLineNumber = Math.Max(stopLineNumber, lineNumber - maxLines);
            }

            int closeCount = 0;

            while (true)
            {
                while (offset >= 0)
                {
                    char currentChar = lineText[offset];

                    if (currentChar == open)
                    {
                        if (closeCount > 0)
                        {
                            closeCount--;
                        }
                        else
                        {
                            pairSpan = new SnapshotSpan(line.Start + offset, 1);
                            return true;
                        }
                    }
                    else if (currentChar == close)
                    {
                        closeCount++;
                    }
                    offset--;
                }

                if (--lineNumber < stopLineNumber)
                {
                    break;
                }

                line = line.Snapshot.GetLineFromLineNumber(lineNumber);
                lineText = line.GetText();
                offset = line.Length - 1;
            }
            return false;
        }
    }
}
