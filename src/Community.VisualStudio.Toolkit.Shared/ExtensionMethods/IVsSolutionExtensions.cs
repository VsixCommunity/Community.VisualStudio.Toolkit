using System;
using System.Collections.Generic;
using EnvDTE;

namespace Microsoft.VisualStudio.Shell.Interop
{
    /// <summary>
    /// Extension methods for the IVsSolution interface.
    /// </summary>
    public static class IVsSolutionExtensions
    {
        /// <summary>
        /// Checks if a solution is open.
        /// </summary>
        /// <returns><c>true</c> if a solution file is open.</returns>
        public static bool IsOpen(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out var value);
            return value is bool isOpen && isOpen;
        }

        /// <summary>
        /// Checks if a solution is opening.
        /// </summary>
        /// <returns><c>true</c> if a solution file is opening.</returns>
        public static bool IsOpening(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpening, out var value);
            return value is bool isOpening && isOpening;
        }

        /// <summary>
        /// Retrieves an array of all projects in the solution.
        /// </summary>
        public static IEnumerable<Project> GetAllProjects(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return GetAllProjects(solution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        /// <summary>
        /// Retrieves an array of all projects in the solution.
        /// </summary>
        public static IEnumerable<Project> GetAllProjects(this IVsSolution solution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (IVsHierarchy hier in GetAllProjectHierarchys(solution, flags))
            {
                var project = hier.ToProject();

                if (project != null)
                {
                    yield return project;
                }
            }
        }

        /// <summary>
        /// Gets all projects in the solution as IVsHierarchy items.
        /// </summary>
        public static IEnumerable<IVsHierarchy> GetAllProjectHierarchys(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetAllProjectHierarchys(solution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        /// <summary>
        /// Gets all projects in the solution as IVsHierarchy items.
        /// </summary>
        public static IEnumerable<IVsHierarchy> GetAllProjectHierarchys(this IVsSolution solution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (solution == null)
            {
                yield break;
            }

            Guid guid = Guid.Empty;
            solution.GetProjectEnum((uint)flags, ref guid, out IEnumHierarchies enumHierarchies);
            if (enumHierarchies == null)
            {
                yield break;
            }

            var hierarchy = new IVsHierarchy[1];
            while (enumHierarchies.Next(1, hierarchy, out var fetched) == VSConstants.S_OK && fetched == 1)
            {
                if (hierarchy.Length > 0 && hierarchy[0] != null)
                {
                    yield return hierarchy[0];
                }
            }
        }

        /// <summary>
        /// Gets the directory of the currently loaded solution.
        /// </summary>
        public static string? GetDirectory(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(solution.GetSolutionInfo(out var dir, out _, out _));
            return dir;
        }

        /// <summary>
        /// Gets the file path of the .sln file.
        /// </summary>
        public static string? GetFilePath(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.ThrowOnFailure(solution.GetSolutionInfo(out _, out var file, out _));
            return file;
        }
    }
}
