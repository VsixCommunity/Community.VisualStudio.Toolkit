using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        private SolutionEvents? _solutionEvents;

        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public SolutionEvents SolutionEvents => _solutionEvents ??= new();
    }

    /// <summary>
    /// Events related to solutions.
    /// </summary>
    public class SolutionEvents :
        IVsSolutionEvents,
        IVsSolutionEvents2,
        IVsSolutionEvents4,
        IVsSolutionEvents5,
#if !VS14
        IVsSolutionEvents7,
#endif
        IVsSolutionLoadEvents
    {
        internal SolutionEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolution svc = VS.GetRequiredService<SVsSolution, IVsSolution>();
            svc.AdviseSolutionEvents(this, out _);
        }

        /// <summary></summary>
        public event Action? OnAfterCloseSolution;

        /// <summary>Notifies listening clients that the project has been loaded.</summary>
        public event Action<Project?>? OnAfterLoadProject;

        /// <summary>Notifies listening clients that the project has been opened.</summary>
        public event Action<Project?>? OnAfterOpenProject;

        /// <summary>Notifies listening clients that the a project with the specified file name is about to open.</summary>
        public event Action<string?>? OnBeforeOpenProject;

        /// <summary>Notifies listening clients that the solution is about to be opened.</summary>
        public event Action<string>? OnBeforeOpenSolution;

        /// <summary>Notifies listening clients that the solution has been opened.</summary>
        public event Action<Solution?>? OnAfterOpenSolution;

        /// <summary>Notifies listening clients that the project is about to be closed.</summary>
        public event Action<Project?>? OnBeforeCloseProject;

        /// <summary>Notifies listening clients that a solution has been closed.</summary>
        public event Action? OnBeforeCloseSolution;

        /// <summary>Notifies listening clients that all projects have been merged into the open solution.</summary>
        public event Action? OnAfterMergeSolution;

        /// <summary>Notifies listening clients that the project is about to be unloaded.</summary>
        public event Action<Project?>? OnBeforeUnloadProject;

        /// <summary>Notifies listening clients that a project has been renamed.</summary>
        public event Action<Project?>? OnAfterRenameProject;

#if !VS14
        /// <summary>Notifies listening clients that the folder is being closed.</summary>
        public event Action<string>? OnBeforeCloseFolder;

        /// <summary>Notifies listening clients that the folder has been closed.</summary>
        public event Action<string>? OnAfterCloseFolder;

        /// <summary>Notifies listening clients that the folder has been opened.</summary>
        public event Action<string>? OnAfterOpenFolder;
#endif

        /// <summary> Fired when the solution load process is fully complete, including all background loading of projects.</summary>
        public event Action? OnAfterBackgroundSolutionLoadComplete;

        #region IVsSolutionEvents
        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnAfterOpenProject != null)
            {
                Project? project = SolutionItem.FromHierarchy(pHierarchy, VSConstants.VSITEMID_ROOT) as Project;
                OnAfterOpenProject?.Invoke(project);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnBeforeCloseProject != null)
            {
                Project? project = SolutionItem.FromHierarchy(pHierarchy, 1) as Project;
                OnBeforeCloseProject?.Invoke(project);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (OnAfterLoadProject != null)
            {
                Project? project = SolutionItem.FromHierarchy(pRealHierarchy, VSConstants.VSITEMID_ROOT) as Project;
                OnAfterLoadProject?.Invoke(project);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (OnBeforeUnloadProject != null)
            {
                Project? project = SolutionItem.FromHierarchy(pRealHierarchy, VSConstants.VSITEMID_ROOT) as Project;
                OnBeforeUnloadProject?.Invoke(project);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (OnAfterOpenSolution != null)
            {
                Solution? solution = VS.Solutions.GetCurrentSolution();
                OnAfterOpenSolution?.Invoke(solution);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (OnBeforeCloseSolution != null)
            {
                SolutionItem? solution = VS.Solutions.GetCurrentSolution();
                OnBeforeCloseSolution?.Invoke();
            }
            return VSConstants.S_OK;

        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            OnAfterCloseSolution?.Invoke();
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsSolutionEvents2
        int IVsSolutionEvents2.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnAfterOpenProject(pHierarchy, fAdded);
        }

        int IVsSolutionEvents2.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnQueryCloseProject(pHierarchy, fRemoving, ref pfCancel);
        }

        int IVsSolutionEvents2.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnBeforeCloseProject(pHierarchy, fRemoved);
        }

        int IVsSolutionEvents2.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnAfterLoadProject(pStubHierarchy, pRealHierarchy);
        }

        int IVsSolutionEvents2.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnQueryUnloadProject(pRealHierarchy, ref pfCancel);
        }

        int IVsSolutionEvents2.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnBeforeUnloadProject(pRealHierarchy, pStubHierarchy);
        }

        int IVsSolutionEvents2.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnAfterOpenSolution(pUnkReserved, fNewSolution);
        }

        int IVsSolutionEvents2.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnQueryCloseSolution(pUnkReserved, pfCancel);
        }

        int IVsSolutionEvents2.OnBeforeCloseSolution(object pUnkReserved)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnBeforeCloseSolution(pUnkReserved);
        }

        int IVsSolutionEvents2.OnAfterCloseSolution(object pUnkReserved)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ((IVsSolutionEvents)this).OnAfterCloseSolution(pUnkReserved);
        }

        int IVsSolutionEvents2.OnAfterMergeSolution(object pUnkReserved)
        {
            OnAfterMergeSolution?.Invoke();
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsSolutionEvents4
        int IVsSolutionEvents4.OnAfterRenameProject(IVsHierarchy pHierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (OnAfterRenameProject != null)
            {
                Project? project = SolutionItem.FromHierarchy(pHierarchy, VSConstants.VSITEMID_ROOT) as Project;
                OnAfterRenameProject?.Invoke(project);
            }
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents4.OnQueryChangeProjectParent(IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents4.OnAfterChangeProjectParent(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents4.OnAfterAsynchOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsSolutionEvents5
        void IVsSolutionEvents5.OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName)
        {
            OnBeforeOpenProject?.Invoke(pszFileName);
        }
        #endregion

        #region IVsSolutionEvents7
#if !VS14
        void IVsSolutionEvents7.OnAfterOpenFolder(string folderPath)
        {
            OnAfterOpenFolder?.Invoke(folderPath);
        }

        void IVsSolutionEvents7.OnBeforeCloseFolder(string folderPath)
        {
            OnBeforeCloseFolder?.Invoke(folderPath);
        }

        void IVsSolutionEvents7.OnQueryCloseFolder(string folderPath, ref int pfCancel)
        { }

        void IVsSolutionEvents7.OnAfterCloseFolder(string folderPath)
        {
            OnAfterCloseFolder?.Invoke(folderPath);
        }

        void IVsSolutionEvents7.OnAfterLoadAllDeferredProjects()
        { }
#endif
        #endregion

        #region IVsSolutionLoadEvents
        int IVsSolutionLoadEvents.OnBeforeOpenSolution(string solutionFilename)
        {
            OnBeforeOpenSolution?.Invoke(solutionFilename);
            return VSConstants.S_OK;
        }

        int IVsSolutionLoadEvents.OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionLoadEvents.OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        int IVsSolutionLoadEvents.OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionLoadEvents.OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionLoadEvents.OnAfterBackgroundSolutionLoadComplete()
        {
            OnAfterBackgroundSolutionLoadComplete?.Invoke();
            return VSConstants.S_OK;
        }
        #endregion
    }
}