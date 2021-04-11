using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace EnvDTE
{
    /// <summary>Extension methods for the ProjectItem class.</summary>
    public static class ProjectItemExtensions
    {
        /// <summary>
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public static void OpenInPreviewTab(this ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OpenInPreviewTab(item.FileNames[1]);
        }

        /// <summary>
        /// Opens a file in the Preview Tab (provisional tab) if supported by the editor factory.
        /// </summary>
        public static void OpenInPreviewTab(string file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE2.NDS_TryProvisional, VSConstants.NewDocumentStateReason.Navigation))
            {
                VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, file);
            }
        }

        /// <summary>
        /// Gets the full file/directory path of the project item
        /// </summary>
        public static string? GetFullPath(this ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (item?.Properties != null)
            {
                return item.Properties.Item("FullPath").Value.ToString();
            }

            return item?.FileNames[1];
        }

        /// <summary>
        /// Sets the item type in the project file for the item.
        /// </summary>
        public static void SetItemType(this ProjectItem item, string itemType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (item == null || item.ContainingProject == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(itemType) || item.ContainingProject.IsKind(ProjectTypes.WEBSITE_PROJECT, ProjectTypes.UNIVERSAL_APP))
                {
                    return;
                }

                item.Properties.Item("ItemType").Value = itemType;
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
}
