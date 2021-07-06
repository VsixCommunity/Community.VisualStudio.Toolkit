using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a physical file in the solution hierarchy.
    /// </summary>
    public class PhysicalFile : SolutionItem
    {
        internal PhysicalFile(IVsHierarchyItem item) : base(item)
        { }

        /// <summary>
        /// Opens the item in the editor window.
        /// </summary>
        /// <returns><see langword="null"/> if the item was not succesfully opened.</returns>
        public async Task<WindowFrame?> OpenAsync()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                await VS.Documents.OpenViaProjectAsync(FileName!);
            }

            return null;
        }
    }
}
