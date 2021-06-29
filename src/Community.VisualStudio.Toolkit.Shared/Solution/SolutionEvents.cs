using System;
using Microsoft.VisualStudio.Shell.Events;
using e = Microsoft.VisualStudio.Shell.Events.SolutionEvents;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public SolutionEvents SolutionEvents => new();
    }

    /// <summary>
    /// Events related to the editor documents.
    /// </summary>
    public class SolutionEvents
    {
        internal SolutionEvents()
        { }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public event EventHandler OnAfterCloseSolution
        {
            add { e.OnAfterCloseSolution += value; }
            remove { e.OnAfterCloseSolution -= value; }
        }

        public event EventHandler<LoadProjectEventArgs> OnAfterLoadProject
        {
            add { e.OnAfterLoadProject += value; }
            remove { e.OnAfterLoadProject -= value; }
        }

        public event EventHandler<OpenProjectEventArgs> OnAfterOpenProject
        {
            add { e.OnAfterOpenProject += value; }
            remove { e.OnAfterOpenProject -= value; }
        }

        public event EventHandler<OpenSolutionEventArgs> OnAfterOpenSolution
        {
            add { e.OnAfterOpenSolution += value; }
            remove { e.OnAfterOpenSolution -= value; }
        }

        public event EventHandler<CloseProjectEventArgs> OnBeforeCloseProject
        {
            add { e.OnBeforeCloseProject += value; }
            remove { e.OnBeforeCloseProject -= value; }
        }

        public event EventHandler OnBeforeCloseSolution
        {
            add { e.OnBeforeCloseSolution += value; }
            remove { e.OnBeforeCloseSolution -= value; }
        }

        public event EventHandler<LoadProjectEventArgs> OnBeforeUnloadProject
        {
            add { e.OnBeforeUnloadProject += value; }
            remove { e.OnBeforeUnloadProject -= value; }
        }

        public event EventHandler<HierarchyEventArgs> OnAfterRenameProject
        {
            add { e.OnAfterRenameProject += value; }
            remove { e.OnAfterRenameProject -= value; }
        }

        public event EventHandler<BeforeOpenProjectEventArgs> OnBeforeOpenProject
        {
            add { e.OnBeforeOpenProject += value; }
            remove { e.OnBeforeOpenProject -= value; }
        }

        public event EventHandler<BeforeOpenSolutionEventArgs> OnBeforeOpenSolution
        {
            add { e.OnBeforeOpenSolution += value; }
            remove { e.OnBeforeOpenSolution -= value; }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}