using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents an assembly reference in a project.
    /// </summary>
    public class AssemblyReference : Reference
    {
        internal AssemblyReference(IVsReference vsReference) : base(vsReference) { }

        /// <summary>
        /// Gets the full path to the assembly file.
        /// </summary>
        public string FullPath
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return VsReference.FullPath;
            }
        }
    }
}
