using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines the font and color categories that an extension provides.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The classes inheriting from <see cref="BaseFontAndColorCategory{T}"/> in an assembly can be provided 
    /// by defining a class that inherits from <see cref="BaseFontAndColorProvider"/> in the same assembly.
    /// </para>
    /// <para>
    /// The provider must be declared to Visual Studio using the <see cref="ProvideFontsAndColorsAttribute"/> 
    /// on your package class, and must be registered when your package is initialized by calling 
    /// <see cref="AsyncPackageExtensions.RegisterFontAndColorProvidersAsync(AsyncPackage, Assembly[])"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Guid("26442428-2cd7-xxxx-xxxx-f9b14087ca50")]
    /// public class MyFontAndColorProvider : BaseFontAndColorProvider { }
    /// 
    /// [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    /// [Guid(PackageGuids.TestExtensionString)]
    /// [ProvideFontsAndColors(typeof(MyFontAndColorProvider))]
    /// public class MyExtensionPackage : ToolkitPackage
    /// {
    ///     protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress&lt;ServiceProgressData&gt; progress)
    ///     {
    ///         await this.RegisterFontAndColorProvidersAsync();
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class BaseFontAndColorProvider : IVsFontAndColorDefaultsProvider
    {
        private readonly Dictionary<Guid, IVsFontAndColorDefaults> _categories = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFontAndColorProvider"/> class.
        /// </summary>
        protected BaseFontAndColorProvider() { }

        internal async Task InitializeAsync(AsyncPackage package)
        {
            await FindCategoriesAsync();

            // Register this provider as a service. Without doing this,
            // Visual Studio won't be able to access the categories.
            ((IServiceContainer)package).AddService(GetType(), this, true);
        }

        private async Task FindCategoriesAsync()
        {
            IVsFontAndColorUtilities utilities = await VS.GetRequiredServiceAsync<SVsFontAndColorStorage, IVsFontAndColorUtilities>();

            foreach (Type categoryType in GetCategoryTypes(GetType()))
            {
                // Create the category and initialize it. We need to give the
                // category the utilities service because there is no opportunity
                // for the category to resolve the service itself asynchronously.
                PropertyInfo instance = categoryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                BaseFontAndColorCategory category = (BaseFontAndColorCategory)instance.GetValue(null);
                category.Initialize(utilities);
                _categories.Add(categoryType.GUID, category);
            }
        }

        internal static IEnumerable<Type> GetCategoryTypes(Type providerType)
        {
            // Find all of the categories that are in the same assembly as the provider.
            Type baseCategoryType = typeof(BaseFontAndColorCategory<>);
            IEnumerable<Type> categoryTypes = providerType
                .Assembly
                .GetTypes()
                .Where((x) => !x.IsAbstract && x.IsAssignableToGenericType(baseCategoryType));

            // Verify that each category has a unique GUID.
            HashSet<Guid> guids = new();
            foreach (Type categoryType in categoryTypes)
            {
                Guid guid = categoryType.GUID;
                if (!guids.Add(guid))
                {
                    throw new InvalidOperationException(
                        $"The font and color category '{categoryType.Name}' (GUID '{guid}') has already been defined."
                    );
                }
            }

            return categoryTypes;
        }

        int IVsFontAndColorDefaultsProvider.GetObject(ref Guid rguidCategory, out object? ppObj)
        {
            if (_categories.TryGetValue(rguidCategory, out IVsFontAndColorDefaults category))
            {
                ppObj = category;
                return VSConstants.S_OK;
            }

            ppObj = null;
            return VSConstants.E_FAIL;
        }
    }
}
