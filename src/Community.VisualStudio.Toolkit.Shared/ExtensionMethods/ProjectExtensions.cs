using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
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

        /// <summary>
        /// Builds the specified project asynchronously
        /// </summary>
        /// <param name="project"></param>
        /// <returns>Returns 'true' if the project builds successfully</returns>
        public async static Task<bool> BuildAsync(this Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var buildTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            DTE? dte = project.DTE;
            dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            var configuration = dte.Solution.SolutionBuild.ActiveConfiguration.Name;

            dte.Solution.SolutionBuild.BuildProject(configuration, project.UniqueName, false);
            return await buildTaskCompletionSource.Task;

            void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
            {
                dte.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;

                // Returns 'true' if the number of failed projects == 0
                buildTaskCompletionSource.TrySetResult(dte.Solution.SolutionBuild.LastBuildInfo == 0);
            }
        }

        /// <summary>
        /// Returns the <see cref="IVsHierarchy"/> for the project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<IVsHierarchy?> ToHierarchyAsync(this Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var vsSolution = await VS.Solution.GetSolutionAsync();
            if (vsSolution.GetProjectOfUniqueName(project.UniqueName, out var hierarchy) == VSConstants.S_OK)
                return hierarchy;

            return null;
        }

        /// <summary>
        /// Returns the unique project id as identified in the solution.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<Guid?> GetProjectIdAsync(this Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var hierarchy = await project.ToHierarchyAsync();
            if (hierarchy != null && hierarchy.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out var projectId) == 0)
                return projectId;

            return null;
        }

        /// <summary>
        /// Returns whether the project is an 'SDK' style project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static async Task<bool> IsSdkStyleProjectAsync(this Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var hierarchy = await project.ToHierarchyAsync();
            return hierarchy?.IsSdkStyleProject() ?? false;
        }
    }
}
