﻿using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text
{
    /// <summary>
    /// Extension methods for <see cref="ITextView" />.
    /// </summary>
    public static class ITextViewExtensions
    {
        /// <summary>
        /// Creates an instance of an <see cref="InfoBar"/> in the text view.
        /// </summary>
        public static InfoBar? CreateInfoBar(this ITextView textView, InfoBarModel model)
        {
            var fileName = textView.TextBuffer.GetFileName();

            if (!string.IsNullOrEmpty(fileName))
            {
                return VS.Notifications.CreateInfoBar(fileName!, model);
            }

            return null;
        }
    }
}