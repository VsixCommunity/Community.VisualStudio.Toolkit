using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
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
        public event Action<AfterRenameProjectItemEventArgs?>? AfterRenameProjectItems;

        /// <summary>
        /// Fires after project items was removed
        /// </summary>
        public event Action<AfterRemoveProjectItemEventArgs?>? AfterRemoveProjectItems;

        /// <summary>
        /// Fires after project items was added
        /// </summary>
        public event Action<IEnumerable<SolutionItem>>? AfterAddProjectItems;

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
            if (AfterRenameProjectItems != null)
            {
                List<ProjectItemRenameDetails> renameParams = new();
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
                        renameParams.Add(new ProjectItemRenameDetails(projectFile, oldName));
                    }
                }

                AfterRenameProjectItems?.Invoke(new AfterRenameProjectItemEventArgs(renameParams.ToArray()));
            }
        }

        private void HandleRemoveItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (AfterRemoveProjectItems != null)
            {
                List<ProjectItemRemoveDetails> removedItems = new();
                for (int projectIndex = 0; projectIndex < cProjects; projectIndex++)
                {
                    int firstIndex = rgFirstIndices[projectIndex];
                    IVsProject vsProject = rgpProjects[projectIndex];
                    IVsHierarchy vsHierarchy = (IVsHierarchy)vsProject;
                    Project? project = SolutionItem.FromHierarchy(vsHierarchy, VSConstants.VSITEMID_ROOT) as Project;

                    int nextProjectIndex = cItems;
                    if (rgFirstIndices.Length > projectIndex + 1)
                    {
                        nextProjectIndex = rgFirstIndices[projectIndex + 1];
                    }

                    for (int itemIndex = firstIndex; itemIndex < nextProjectIndex; itemIndex++)
                    {   
                        string itemName = rgpszMkDocuments[itemIndex];
                        removedItems.Add(new ProjectItemRemoveDetails(project, itemName));
                    }
                }

                AfterRemoveProjectItems?.Invoke(new AfterRemoveProjectItemEventArgs(removedItems.ToArray()));
            }
        }

        private void HandleAddItems(int cProjects, int cItems, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (AfterAddProjectItems != null)
            {
                List<SolutionItem> addedItems = new();
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

                AfterAddProjectItems?.Invoke(addedItems);
            }
        }
    }

    /// <inheritdoc/>
    public class AfterRenameProjectItemEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of the event args
        /// </summary>
        public AfterRenameProjectItemEventArgs(ProjectItemRenameDetails[]? projectItemRenames)
        {
            ProjectItemRenames = projectItemRenames;
        }

        /// <summary>
        /// ProjectItem details that was renamed
        /// </summary>
        public ProjectItemRenameDetails[]? ProjectItemRenames { get; }
    }

    /// <summary>
    /// ProjectItem rename details 
    /// </summary>
    public class ProjectItemRenameDetails
    {
        public ProjectItemRenameDetails(SolutionItem? solutionItem, string? oldName)
        {
            SolutionItem = solutionItem;
            OldName = oldName;
        }

        /// <summary>
        /// The rename solution item
        /// </summary>
        public SolutionItem? SolutionItem { get; }

        /// <summary>
        /// The name before the rename
        /// </summary>
        public string? OldName { get; }
    }

    /// <inheritdoc/>
    public class AfterRemoveProjectItemEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance of the event args
        /// </summary>
        public AfterRemoveProjectItemEventArgs(ProjectItemRemoveDetails[]? projectItemRemoves)
        {
            ProjectItemRemoves = projectItemRemoves;
        }

        /// <summary>
        /// ProjectItem details that was removed
        /// </summary>
        public ProjectItemRemoveDetails[]? ProjectItemRemoves { get; }
    }

    /// <summary>
    /// Removed ProjectItem details
    /// </summary>
    public class ProjectItemRemoveDetails
    {
        public ProjectItemRemoveDetails(Project? project, string? itemName)
        {
            Project = project;
            RemovedItemName = itemName;
        }

        /// <summary>
        /// The project that removed the item
        /// </summary>
        public Project? Project { get; }

        /// <summary>
        /// The item name that was removed
        /// </summary>
        public string? RemovedItemName { get; }
    }
}
