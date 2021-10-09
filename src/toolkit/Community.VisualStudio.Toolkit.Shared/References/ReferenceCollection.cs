using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Provides access to references in a project.
    /// </summary>
    public class ReferenceCollection : IEnumerable<Reference>
    {
        private readonly Project _project;

        internal ReferenceCollection(Project project)
        {
            _project = project;
        }

        /// <summary>
        /// Adds references to one or more assembly files.
        /// </summary>
        public async Task AddAsync(params string[] assemblyFileNames)
        {
            // Note: This method does not need to be async, but it's written
            // this way to be consistent with the other `AddAsync` methods.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsReferenceManagerUser manager = GetManager();
            IVsReferenceProviderContext context;

            // Which provider context we need to use will depend on the type
            // of project style. Classic projects use assembly references,
            // and SDK-style projects use file references.
            if (TryGetProviderContext(manager, out IVsFileReferenceProviderContext fileContext))
            {
                context = fileContext;
            }
            else if (TryGetProviderContext(manager, out IVsAssemblyReferenceProviderContext assemblyContext))
            {
                context = assemblyContext;
            }
            else
            {
                throw new NotSupportedException($"The project '{_project.Name}' cannot have assembly references.");
            }

            foreach (string fileName in assemblyFileNames)
            {
                IVsReference reference = context.CreateReference();
                reference.Name = Path.GetFileNameWithoutExtension(fileName);
                reference.FullPath = fileName;
                context.AddReference(reference);
            }

            manager.ChangeReferences((uint)__VSREFERENCECHANGEOPERATION.VSREFERENCECHANGEOPERATION_ADD, context);
        }

        /// <summary>
        /// Adds references to one or more projects.
        /// </summary>
        public async Task AddAsync(params Project[] projects)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution = await VS.Services.GetSolutionAsync();
            IVsReferenceManagerUser manager = GetManager();
            Lazy<IVsProjectReferenceProviderContext> projectContext = new(() => GetProviderContext<IVsProjectReferenceProviderContext>(manager), false);
            Lazy<IVsSharedProjectReferenceProviderContext> sharedContext = new(() => GetProviderContext<IVsSharedProjectReferenceProviderContext>(manager), false);

            foreach (Project project in projects)
            {
                project.GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

                IVsSharedAssetsProject? sharedProject = hierarchy.GetSharedAssetsProject();
                if (sharedProject is not null)
                {
                    IVsSharedProjectReference reference = (IVsSharedProjectReference)sharedContext.Value.CreateReference();
                    PopulateSharedProjectReference(reference, solution, project, sharedProject);
                    sharedContext.Value.AddReference(reference);
                }
                else
                {
                    ErrorHandler.ThrowOnFailure(solution.GetUniqueNameOfProject(hierarchy, out string uniqueName));
                    ErrorHandler.ThrowOnFailure(solution.GetGuidOfProject(hierarchy, out Guid projectGuid));

                    IVsProjectReference reference = (IVsProjectReference)projectContext.Value.CreateReference();
                    reference.Name = project.Name;
                    reference.FullPath = project.FullPath;
                    reference.Identity = projectGuid.ToString("b");
                    // The reference specification is made up of the project’s GUID and the
                    // project's Visual Studio unique name, separated by a "|" character.
                    reference.ReferenceSpecification = $"{projectGuid:b}|{uniqueName}";
                    projectContext.Value.AddReference(reference);
                }
            }

            if (projectContext.IsValueCreated)
            {
                manager.ChangeReferences((uint)__VSREFERENCECHANGEOPERATION.VSREFERENCECHANGEOPERATION_ADD, projectContext.Value);
            }

            if (sharedContext.IsValueCreated)
            {
                manager.ChangeReferences((uint)__VSREFERENCECHANGEOPERATION.VSREFERENCECHANGEOPERATION_ADD, sharedContext.Value);
            }
        }

        /// <summary>
        /// Removes the given references from the project.
        /// </summary>
        /// <param name="references">The references to remove.</param>
        /// <returns>A task.</returns>
        public async Task RemoveAsync(params Reference[] references)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // There doesn't seem to be an easy way to actually remove references from a project.
            // There is no direct way to map an `IVsReference` instance to the provider that handles
            // that type of reference, and the `IVsReferenceProviderContext` has no way to remove a
            // reference from its `References` property. What we do here might seem like a bit of a hack,
            // but it appears to work fine.
            //
            // We start by creating a mapping from an `IVsReference` type to the corresponding provider
            // context by getting each provider context to create a reference object. The provider
            // context just creates a new object and doesn't seem to do anything with it, so this
            // shouldn't have any side effects. This gives us a way to get the provider context
            // that an `IVsReference` object is handled by.
            //
            // But, the provider contexts from the manager contain all of the existing references with no
            // way to remove reference objects from them, so we can't pass those contexts to the manager
            // when calling `ChangeReferences()` because that will remove all existing references!
            // What we will do is create an instance of our own implementation of a provider context
            // so that the provider context will only contain the references that we want to remove.
            Dictionary<Type, RemovingReferenceProviderContext> contextsByReferenceType = new();
            IVsReferenceManagerUser manager = GetManager();

            foreach (IVsReferenceProviderContext context in manager.GetProviderContexts().OfType<IVsReferenceProviderContext>())
            {
                contextsByReferenceType[context.CreateReference().GetType()] = new RemovingReferenceProviderContext(context.ProviderGuid);
            }

            foreach (Reference reference in references)
            {
                if (contextsByReferenceType.TryGetValue(reference.VsReference.GetType(), out RemovingReferenceProviderContext context))
                {
                    context.AddReference(reference.VsReference);
                }
            }

            foreach (IVsReferenceProviderContext context in contextsByReferenceType.Values.Where((x) => x.HasReferences))
            {
                manager.ChangeReferences((uint)__VSREFERENCECHANGEOPERATION.VSREFERENCECHANGEOPERATION_REMOVE, context);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Reference> GetEnumerator()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Not all projects can have references (for example Shared Projects),
            // so when enumerating over the references in the project, we won't throw
            // an error if the manager or provider context cannot be retrieved.
            if (TryGetManager(out IVsReferenceManagerUser manager))
            {
                IVsSharedProjectReferenceProviderContext? sharedProjectContext = null;

                foreach (IVsReferenceProviderContext context in manager.GetProviderContexts().OfType<IVsReferenceProviderContext>())
                {
                    // Remember the shared project context, because it may not actually provide the 
                    // references to shared projects, meaning we may have to create them ourselves.
                    if (context is IVsSharedProjectReferenceProviderContext shared)
                    {
                        sharedProjectContext = shared;
                    }

                    foreach (IVsReference reference in context.References.OfType<IVsReference>())
                    {
                        if (reference is IVsAssemblyReference assemblyReference)
                        {
                            yield return new AssemblyReference(assemblyReference);
                        }
                        else if (reference is IVsProjectReference projectReference)
                        {
                            yield return new ProjectReference(projectReference);
                        }
                        else
                        {
                            yield return new Reference(reference);
                        }
                    }
                }

                // Shared projects don't seem to be listed in the provider contexts, so if there is a context
                // for shared projects but it's empty, then we'll define the shared project references ourselves.
                if (sharedProjectContext is not null && sharedProjectContext.References.Length == 0)
                {
                    IVsSolution? solution = null;

                    _project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

                    foreach (IVsHierarchy sharedHierarchy in hierarchy.EnumOwningProjectsOfSharedAssets())
                    {
                        // A shared project seems to list itself as an owning project, so ignore
                        // this hierarchy if it's the same one that we got from our project.
                        if (sharedHierarchy == hierarchy)
                        {
                            continue;
                        }

                        IVsSharedAssetsProject? sharedProject = sharedHierarchy.GetSharedAssetsProject();
                        if (sharedProject is not null)
                        {
                            Project? project = SolutionItem.FromHierarchy(sharedHierarchy, VSConstants.VSITEMID_ROOT) as Project;
                            if (project is not null)
                            {
                                if (solution is null)
                                {
                                    solution = VS.GetRequiredService<SVsSolution, IVsSolution>();
                                }

                                IVsSharedProjectReference reference = (IVsSharedProjectReference)sharedProjectContext.CreateReference();
                                PopulateSharedProjectReference(reference, solution, project, sharedProject);
                                yield return new ProjectReference(reference);
                            }
                        }
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetEnumerator();
        }

        private IVsReferenceManagerUser GetManager()
        {
            if (!TryGetManager(out IVsReferenceManagerUser manager))
            {
                throw new NotSupportedException($"The project '{_project.Name}' cannot contain references.");
            }

            return manager;
        }

        private bool TryGetManager(out IVsReferenceManagerUser manager)
        {
            _project.GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

            IVsReferenceManagerUser? value = HierarchyUtilities.GetHierarchyProperty<IVsReferenceManagerUser?>(
                hierarchy,
                itemId,
                (int)__VSHPROPID5.VSHPROPID_ReferenceManagerUser
            );

            if (value is not null)
            {
                manager = value;
                return true;
            }

            manager = null!;
            return false;
        }

        private T GetProviderContext<T>(IVsReferenceManagerUser manager) where T : IVsReferenceProviderContext
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!TryGetProviderContext<T>(manager, out T context))
            {
                throw new NotSupportedException($"Could not find the {typeof(T).Name} for the project.");
            }

            return context;
        }

        private bool TryGetProviderContext<T>(IVsReferenceManagerUser manager, out T context) where T : IVsReferenceProviderContext
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            T? value = manager.GetProviderContexts().OfType<T>().FirstOrDefault();

            if (value is not null)
            {
                context = value;
                return true;
            }

            context = default!;
            return false;
        }

        private static void PopulateSharedProjectReference(IVsSharedProjectReference reference, IVsSolution solution, Project project, IVsSharedAssetsProject sharedProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);
            ErrorHandler.ThrowOnFailure(solution.GetGuidOfProject(hierarchy, out Guid projectGuid));

            reference.Name = project.Name;
            reference.FullPath = project.FullPath;
            reference.SharedMSBuildFileFullPath = sharedProject.SharedItemsImportFullPath;
            reference.SharedProjectID = projectGuid;
        }

        private class RemovingReferenceProviderContext : IVsReferenceProviderContext
        {
            private readonly List<IVsReference> _references = new();

            public RemovingReferenceProviderContext(Guid providerGuid)
            {
                ProviderGuid = providerGuid;
            }

            public void AddReference(IVsReference pReference)
            {
                _references.Add(pReference);
            }

            public IVsReference CreateReference() => throw new NotSupportedException();

            public Guid ProviderGuid { get; }

            public bool HasReferences => _references.Count > 0;

            public Array References => _references.ToArray();

            public Array ReferenceFilterPaths { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        }
    }
}
