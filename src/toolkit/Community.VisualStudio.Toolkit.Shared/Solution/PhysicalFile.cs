using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a physical file in the solution hierarchy.
    /// </summary>
    public class PhysicalFile : SolutionItem
    {
        private static readonly Dictionary<PhysicalFileAttribute, string> _knownFileAttributeNames = new()
        {
            { PhysicalFileAttribute.BuildAction, "{ItemType}" },
            { PhysicalFileAttribute.CopyToOutputDirectory, "CopyToOutputDirectory" },
            { PhysicalFileAttribute.CustomToolNamespace, "CustomToolNamespace" },
            { PhysicalFileAttribute.Generator, "Generator" },
            { PhysicalFileAttribute.SubType, "SubType" },
            { PhysicalFileAttribute.Visible, "Visible" },
            { PhysicalFileAttribute.DependentUpon, "DependentUpon" },
            { PhysicalFileAttribute.DesignTimeSharedInput, "DesignTimeSharedInput" },
            { PhysicalFileAttribute.LastGenOutput, "LastGenOutput" },
            { PhysicalFileAttribute.DesignTime, "DesignTime" },
            { PhysicalFileAttribute.CustomTool, "CustomTool" },
            { PhysicalFileAttribute.AutoGen, "AutoGen" }
        };

        internal PhysicalFile(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// The containing folder of the file.
        /// </summary>
        public string Folder => Path.GetDirectoryName(FullPath);

        /// <summary>
        /// The file extension starting with a dot.
        /// </summary>
        public string Extension => Path.GetExtension(FullPath);

        /// <summary>
        /// The project containing this file, or <see langword="null"/>.
        /// </summary>
        public Project? ContainingProject => FindParent(SolutionItemType.Project) as Project;

        /// <summary>
        /// Opens the item in the editor window.
        /// </summary>
        /// <returns><see langword="null"/> if the item was not successfully opened.</returns>
        public async Task<WindowFrame?> OpenAsync()
        {
            if (!string.IsNullOrEmpty(FullPath))
            {
                await VS.Documents.OpenViaProjectAsync(FullPath!);
            }

            return null;
        }

#if VS16 || VS17
        /// <summary>
        /// Gets the content type associated with this type of physical files.
        /// </summary>
        public async Task<IContentType> GetContentTypeAsync()
        {
            IFileToContentTypeService fileToContentTypeService = await VS.GetMefServiceAsync<IFileToContentTypeService>();
            return fileToContentTypeService.GetContentTypeForFileNameOrExtension(Text);
        }
#endif

        /// <summary>
        /// Tries to remove the file from the project or solution folder.
        /// </summary>
        public async Task<bool> TryRemoveAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SolutionItem? parent = FindParent(SolutionItemType.Project) ?? FindParent(SolutionItemType.SolutionFolder);

            if (parent != null)
            {
                GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

                if (hierarchy is IVsProject2 project)
                {
                    project.RemoveItem(0, itemId, out int result);
                    return result == 1;
                }
            }

            return false;
        }

        /// <summary>
        /// Nests a file under this file by setting its <c>DependentUpon</c> property..
        /// </summary>
        public Task AddNestedFileAsync(PhysicalFile fileToNest)
            => fileToNest.TrySetAttributeAsync(PhysicalFileAttribute.DependentUpon, Name);

        /// <summary>
        /// Tries to set an attribute in the project file for the item.
        /// </summary>
        public Task<bool> TrySetAttributeAsync(PhysicalFileAttribute attribute, object value)
        {
            return TrySetAttributeAsync(_knownFileAttributeNames[attribute], value);
        }

        /// <summary>
        /// Tries to set an attribute in the project file for the item.
        /// </summary>
        public async Task<bool> TrySetAttributeAsync(string name, object value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);
            int hr = hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_BrowseObject, out object? browseObject);

            // First try if the attribute name exist in the Property Descriptor Collection
            if (ErrorHandler.Succeeded(hr))
            {
                // Inspired by this sample: https://stackoverflow.com/a/24538728

                string cleanName = Regex.Replace(name, @"\s+", ""); // remove whitespace
                PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(browseObject);
                PropertyDescriptor? customToolDescriptor = propertyDescriptors?.Find(cleanName, true);

                if (customToolDescriptor != null)
                {
                    string? invariantValue = customToolDescriptor.Converter.ConvertToInvariantString(value);
                    customToolDescriptor.SetValue(browseObject, invariantValue);
                    IVsUIShell? shell = await VS.Services.GetUIShellAsync();

                    // Refresh the Property window
                    if (customToolDescriptor.Attributes[typeof(DispIdAttribute)] is DispIdAttribute dispId)
                    {
                        ErrorHandler.ThrowOnFailure(shell.RefreshPropertyBrowser(dispId.Value));
                    }

                    return true;
                }
            }
            // Then write straight to project file
            else if (hierarchy is IVsBuildPropertyStorage storage)
            {
                ErrorHandler.ThrowOnFailure(storage.SetItemAttribute(itemId, name, value?.ToString()));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to retrieve an attribute value from the project file for the item.
        /// </summary>
        /// <returns><see langword="null"/> if the attribute doesn't exist.</returns>
        public async Task<string?> GetAttributeAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

            if (hierarchy is IVsBuildPropertyStorage storage)
            {
                storage.GetItemAttribute(itemId, name, out string value);
                return value;
            }

            return null;
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePath">The absolute file path of a file that exists in the solution.</param>
        /// <returns><see langword="null"/> if the file wasn't found in the solution.</returns>
        public static async Task<PhysicalFile?> FromFileAsync(string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IEnumerable<IVsHierarchy> projects = await VS.Solutions.GetAllProjectHierarchiesAsync();

            VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];

            foreach (IVsHierarchy hierarchy in projects)
            {
                IVsProject proj = (IVsProject)hierarchy;
                proj.IsDocumentInProject(filePath, out int isFound, priority, out uint itemId);

                if (isFound == 1)
                {
                    return await FromHierarchyAsync(hierarchy, itemId) as PhysicalFile;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePaths">The absolute file paths of files that exist in the solution.</param>
        public static async Task<IEnumerable<PhysicalFile>> FromFilesAsync(params string[] filePaths)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            List<PhysicalFile> items = new();

            foreach (string filePath in filePaths)
            {
                PhysicalFile? item = await FromFileAsync(filePath);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }

    /// <summary>
    /// Known attributes of a <see cref="PhysicalFile"/>. 
    /// This can be used to set an attribute of a file using the <see cref="PhysicalFile.TrySetAttributeAsync(PhysicalFileAttribute, string)"/> method.
    /// </summary>
    public enum PhysicalFileAttribute
    {
        /// <summary>
        /// Sets the <c>AutoGen</c> attribute.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        AutoGen,
        /// <summary>
        /// How the file relates to the build and deployment processes.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        BuildAction,
        /// <summary>
        /// Specifies the source file will be copied to the output directory.
        /// <para>
        /// Type: <see cref="CopyToOutputDirectory"/>
        /// </para>
        /// </summary>
        CopyToOutputDirectory,
        /// <summary>
        /// The name of the single-file generator.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        CustomTool,
        /// <summary>
        /// The namespace into which the output of the custom tool is placed.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        CustomToolNamespace,
        /// <summary>
        /// The other file that this file is dependent upon.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        DependentUpon,
        /// <summary>
        /// Sets the <c>DesignTime</c> attribute.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        DesignTime,
        /// <summary>
        /// Sets the <c>DesignTimeSharedInput</c> attribute.
        /// <para>
        /// Type: <see cref="bool"/>
        /// </para>
        /// </summary>
        DesignTimeSharedInput,
        /// <summary>
        /// Specifies the tool that transforms a file at design time and places the output of that transformation into another file. For example, a dataset (.xsd) file comes with a default custom tool.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        Generator,
        /// <summary>
        /// Sets the <c>LastGenOutput</c> attribute.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        LastGenOutput,
        /// <summary>
        /// File sub-type.
        /// <para>
        /// Type: <see cref="string"/>
        /// </para>
        /// </summary>
        SubType,
        /// <summary>
        /// Whether to show the file in Solution Explorer.
        /// <para>
        /// Type: <see cref="bool"/>
        /// </para>
        /// </summary>
        Visible
    }

    /// <summary>
    /// Defines whether a file will be copied to the build's output directory.
    /// </summary>
    /// <remarks>
    /// Equivalent to the <c>Microsoft.VisualStudio.ProjectFlavoring.CopyToOutputDirectory</c> enum.
    /// </remarks>
    public enum CopyToOutputDirectory
    {
        /// <summary>
        /// The file will never be copied.
        /// </summary>
        DoNotCopy,
        /// <summary>
        /// The file will always be copied.
        /// </summary>
        Always,
        /// <summary>
        /// The file will be copied, but only if it is newer that the file that is already in the build's output directory.
        /// </summary>
        PreserveNewest
    }
}
