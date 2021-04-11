using System;
using System.IO;
using System.Linq;
using Community.VisualStudio.Toolkit;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace EnvDTE
{
    /// <summary>Extension methods for the Project class.</summary>
    public static class ProjectExtensions
    {
        /// <summary>Casts the Project to a SolutionFolder.</summary>
        public static SolutionFolder AsSolutionFolder(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (SolutionFolder)project.Object;
        }

        /// <summary>Gets the root folder of any Visual Studio project.</summary>
        public static string? GetDirectory(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string? path = null;
            var properties = new[] { "FullPath", "ProjectPath", "ProjectDirectory" };

            foreach (var name in properties)
            {
                try
                {
                    if (project?.Properties.Item(name)?.Value is string fullPath)
                    {
                        path = fullPath;
                        break;
                    }
                }
                catch (Exception)
                { }
            }

            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            return path;
        }

        /// <summary>Adds one or more files to the project.</summary>
        public static async Task AddFilesToProjectAsync(this Project project, params string[] files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (project == null || project.IsKind(ProjectTypes.ASPNET_Core, ProjectTypes.DOTNET_Core, ProjectTypes.SSDT))
            {
                return;
            }

            DTE2? dte = await VS.GetDTEAsync();

            if (project.IsKind(ProjectTypes.WEBSITE_PROJECT))
            {
                Command command = dte.Commands.Item("SolutionExplorer.Refresh");

                if (command.IsAvailable)
                {
                    dte.ExecuteCommand(command.Name);
                }

                return;
            }

            IVsSolution? solutionService = await VS.GetServiceAsync<SVsSolution, IVsSolution>();
            solutionService.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy? hierarchy);

            if (hierarchy == null)
            {
                return;
            }

            var ip = (IVsProject)hierarchy;
            var result = new VSADDRESULT[files.Count()];

            ip.AddItem(VSConstants.VSITEMID_ROOT,
                       VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE,
                       string.Empty,
                       (uint)files.Count(),
                       files.ToArray(),
                       IntPtr.Zero,
                       result);
        }

        /// <summary>Check what kind the project is.</summary>
        /// <param name="project">The project to check.</param>
        /// <param name="kindGuids">Use the <see cref="ProjectTypes"/> list of strings</param>
        public static bool IsKind(this Project project, params string[] kindGuids)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var guid in kindGuids)
            {
                if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the default namespace or asembly name of the project.
        /// </summary>
        /// <remarks>
        /// It tries to get the DefaultNamespace first, and will then fallback to RootNamespace
        /// and then AssemblyName. The different project types in Visual Studio differ in how
        /// they support similar concepts, so the code tries different properties to compensate.
        /// </remarks>
        public static string GetProjectNamespace(this Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var ns = project.Properties.Item("DefaultNamespace").Value.ToString();

            if (string.IsNullOrEmpty(ns))
            {
                ns = project.Properties.Item("RootNamespace").Value.ToString();
            }
            if (string.IsNullOrEmpty(ns))
            {
                ns = project.Properties.Item("AssemblyName").Value.ToString();
            }

            return ns;
        }

        /// <summary>
        /// Kicks off a build of the project.
        /// </summary>
        public static vsBuildState Build(this Project project, bool waitForBuildToFinish = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE? dte = project.DTE;
            var configuration = dte.Solution.SolutionBuild.ActiveConfiguration.Name;

            dte.Solution.SolutionBuild.BuildProject(configuration, project.UniqueName, waitForBuildToFinish);

            return dte.Solution.SolutionBuild.BuildState;
        }
    }
}
