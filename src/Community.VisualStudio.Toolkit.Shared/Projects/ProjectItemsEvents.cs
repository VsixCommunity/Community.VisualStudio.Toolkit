using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit.Shared.Projects
{
    public partial class Events
    {
        private ProjectItemsEvents? _projectItemsEvents;

        /// <summary>
        /// Events related to project items
        /// </summary>
        public ProjectItemsEvents ProjectItemsEvents => _projectItemsEvents ??= new();
    }

    /// <summary>
    /// Events related to project items.
    /// </summary>
    public class ProjectItemsEvents : IVsTrackProjectDocumentsEvents2
    {
        internal ProjectItemsEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsTrackProjectDocuments2 tpd = VS.GetRequiredService<SVsTrackProjectDocuments, IVsTrackProjectDocuments2>();
            tpd.AdviseTrackProjectDocumentsEvents(this, out _);
        }

        /// <summary>
        /// Fires after project items was renamed
        /// </summary>
        public event Action<AfterRenameProjectItemEventArgs?>? OnAfterRenameProjectItems;

        /// <summary>
        /// Fires after project items was removed
        /// </summary>
        public event Action<IEnumerable<SolutionItem>>? OnAfterRemoveProjectItems;

        /// <summary>
        /// Fires after project items was added
        /// </summary>
        public event Action<IEnumerable<SolutionItem>>? OnAfterAddProjectItems;

        int IVsTrackProjectDocumentsEvents2.OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
        {
            HandleAddItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags)
        {
            HandleAddItems(cProjects, cDirectories, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            HandleRemoveItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            HandleRemoveItems(cProjects, cDirectories, rgpProjects, rgFirstIndices, rgpszMkDocuments);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            HandleRenamedItems(cProjects, cFiles, rgpProjects, rgFirstIndices, rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags)
        {
            HandleRenamedItems(cProjects, cDirs, rgpProjects, rgFirstIndices, rgszMkOldNames, rgszMkNewNames);
            return VSConstants.S_OK;
        }

        int IVsTrackProjectDocumentsEvents2.OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults) => VSConstants.S_OK;

        int IVsTrackProjectDocumentsEvents2.OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus) => VSConstants.S_OK;

        private void HandleRenamedItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnAfterRenameProjectItems != null)
            {
                List<ProjectItemRename> renameParams = new List<ProjectItemRename>();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {
                        string newName = rgszMkNewNames[itemIndex];
                        string oldName = rgszMkOldNames[itemIndex];
                        vsProject.IsDocumentInProject(newName, out _, new VSDOCUMENTPRIORITY[1], out uint itemid);
                        SolutionItem? projectFile = SolutionItem.FromHierarchy(vsHierarchy, itemid);
                        renameParams.Add(new ProjectItemRename(projectFile, oldName));
                    }
                }

                OnAfterRenameProjectItems?.Invoke(new AfterRenameProjectItemEventArgs(renameParams.ToArray()));
            }
        }

        private void HandleRemoveItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnAfterRemoveProjectItems != null)
            {
                List<SolutionItem> removedItems = new List<SolutionItem>();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {   //do I need the item after the removal or just the string??
                        //string itemName = rgpszMkDocuments[itemIndex];
                        //vsProject.IsDocumentInProject(itemName, out _, new VSDOCUMENTPRIORITY[1], out uint itemid);
                        //SolutionItem? projectFile = SolutionItem.FromHierarchy(vsHierarchy, itemid);
                        //if (projectFile != null)
                        //    removedItems.Add(projectFile);
                    }
                }

                OnAfterRemoveProjectItems?.Invoke(removedItems);
            }
        }

        private void HandleAddItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnAfterAddProjectItems != null)
            {
                List<SolutionItem> addedItems = new List<SolutionItem>();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {
                        string itemName = rgpszMkDocuments[itemIndex];
                        vsProject.IsDocumentInProject(itemName, out _, new VSDOCUMENTPRIORITY[1], out uint itemid);
                        SolutionItem? projectFile = SolutionItem.FromHierarchy(vsHierarchy, itemid);
                        if (projectFile != null)
                            addedItems.Add(projectFile);
                    }
                }

                OnAfterAddProjectItems?.Invoke(addedItems);
            }
        }
    }

    public class AfterRenameProjectItemEventArgs : EventArgs
    {
        public AfterRenameProjectItemEventArgs(ProjectItemRename[]? projectItemsRename)
        {
            ProjectItemsRename = projectItemsRename;
        }

        public ProjectItemRename[]? ProjectItemsRename { get; }
    }

    public class ProjectItemRename
    {
        public ProjectItemRename(SolutionItem? solutionItem, string? oldName)
        {
            SolutionItem = solutionItem;
            OldName = oldName;
        }

        public SolutionItem? SolutionItem { get; }

        public string? OldName { get; }
    }
}
