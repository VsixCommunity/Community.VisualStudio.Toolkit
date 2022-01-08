using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A TokenTag is the only tag you need to add to your custom language implementation.
    /// </summary>
    public class TokenTag : ITag
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TokenTag(object tokenType, bool supportOutlining, Func<SnapshotPoint, Task<object>>? getTooltipAsync, params ErrorListItem[] errors)
        {
            TokenType = tokenType;
            SupportOutlining = supportOutlining;
            GetTooltipAsync = getTooltipAsync;
            Errors = errors;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TokenTag(object tokenType)
            : this(tokenType, false, null)
        { }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TokenTag(object tokenType, bool supportOutlining)
            : this(tokenType, supportOutlining, null)
        { }

        /// <summary>
        /// This can be any object you use to differentiate the type of token tags. It's used for classification.
        /// </summary>
        public virtual object TokenType { get; set; }

        /// <summary>
        /// Any tags supporting outlining will automatically get IStructure tags added.
        /// </summary>
        public virtual bool SupportOutlining { get; set; }

        /// <summary>
        /// Specify if the tag has any tooltip to show. When true, the GetTooltipAsync method will be called.
        /// </summary>
        public virtual bool HasTooltip { get; set; }

        /// <summary>
        /// A list of errors associated with the tag.
        /// </summary>
        public virtual IList<ErrorListItem> Errors { get; set; }

        /// <summary>
        /// Returns true if there are no errors in the list.
        /// </summary>
        public virtual bool IsValid => Errors?.Any() == false;

        /// <summary>
        /// A function to create custom hover tooltips (QuickInfo). Optional.
        /// </summary>
        public virtual Func<SnapshotPoint, Task<object>>? GetTooltipAsync { get; set; }

        /// <summary>
        /// A function to override the default behavior of producing collapse outlining text.
        /// </summary>
        public virtual Func<string, string> GetOutliningText { get; set; } = (text) => text.Split('\n').FirstOrDefault().Trim();
    }
}
