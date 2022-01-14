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
        public TokenTag(object tokenType, IEnumerable<ErrorListItem> errors)
        {
            TokenType = tokenType;
            Errors = errors.ToList() ?? new();
        }

        /// <summary>
        /// This can be any object you use to differentiate the type of token tags. It's used for classification.
        /// </summary>
        public virtual object TokenType { get; set; }

        /// <summary>
        /// A list of errors associated with the tag.
        /// </summary>
        public virtual IEnumerable<ErrorListItem> Errors { get; set; }

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
        public virtual Func<string, string?>? GetOutliningText { get; set; }
    }
}
