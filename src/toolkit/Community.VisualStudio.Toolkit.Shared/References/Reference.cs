using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a reference in a project.
    /// </summary>
    public class Reference
    {
        internal Reference(IVsReference vsReference)
        {
            VsReference = vsReference;
        }

        /// <summary>
        /// Gets the underlying <see cref="IVsReference"/> object.
        /// </summary>
        public IVsReference VsReference { get; }

        /// <summary>
        /// Gets the name of the reference.
        /// </summary>
        public virtual string Name
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return VsReference.Name;
            }
        }
    }
}
