using System.Collections.Generic;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents items that go in the Error List
    /// </summary>
    public class ErrorListItem
    {
        /// <summary>
        /// Project name of the error item.
        /// </summary>
        public string? ProjectName { get; set; }

        /// <summary>
        /// File name of the error item.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 0-based line of code on the error item.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// 0-based column of the error item
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Error code for the error item.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Error code tool tip.
        /// </summary>
        public string? ErrorCodeToolTip { get; set; }

        /// <summary>
        /// Error category.
        /// </summary>
        public string? ErrorCategory { get; set; }

        /// <summary>
        /// Severity of the error item.
        /// </summary>
        public __VSERRORCATEGORY Severity { get; set; } = __VSERRORCATEGORY.EC_WARNING;

        /// <summary>
        /// Error help link.
        /// </summary>
        public string? HelpLink { get; set; }

        /// <summary>
        /// Column used to display the build tool that generated the error (e.g. "FxCop").
        /// </summary>
        public string? BuildTool { get; set; }

        /// <summary>
        /// The image icon moniker to use for the error list item.
        /// </summary>
        public ImageMoniker Icon { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ErrorListItem item &&
                   ProjectName == item.ProjectName &&
                   FileName == item.FileName &&
                   Line == item.Line &&
                   Column == item.Column &&
                   Message == item.Message &&
                   ErrorCode == item.ErrorCode &&
                   ErrorCodeToolTip == item.ErrorCodeToolTip &&
                   ErrorCategory == item.ErrorCategory &&
                   Severity == item.Severity &&
                   HelpLink == item.HelpLink &&
                   BuildTool == item.BuildTool;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 1638003424;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectName!);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName!);
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message!);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ErrorCode!);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ErrorCodeToolTip!);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ErrorCategory!);
            hashCode = hashCode * -1521134295 + Severity.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HelpLink!);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BuildTool!);
            return hashCode;
        }
    }
}
