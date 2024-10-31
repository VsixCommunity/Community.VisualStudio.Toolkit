using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>Contains helper methods for querying fonts and colors.</summary>
    public class FontsAndColors
    {
        private const uint _openCategoryFlags = (uint)(
            __FCSTORAGEFLAGS.FCSF_READONLY |
            __FCSTORAGEFLAGS.FCSF_NOAUTOCOLORS |
            __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS
        );

        internal FontsAndColors()
        { }

        /// <summary>
        /// Gets the configured font and colors for the given category.
        /// </summary>
        /// <typeparam name="T">The type of the category.</typeparam>
        /// <returns>The configured font and colors.</returns>
        public async Task<ConfiguredFontAndColorSet<T>> GetConfiguredFontAndColorsAsync<T>() where T : BaseFontAndColorCategory<T>, new()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsFontAndColorStorage storage = await VS.Services.GetFontAndColorStorageAsync();
            Guid categoryGuid = typeof(T).GUID;
            ErrorHandler.ThrowOnFailure(storage.OpenCategory(ref categoryGuid, _openCategoryFlags));

            try
            {
                T category = BaseFontAndColorCategory<T>.Instance;

                FontInfo[] fontInfo = new FontInfo[1];
                LOGFONTW[] logfont = new LOGFONTW[1];
                ErrorHandler.ThrowOnFailure(storage.GetFont(logfont, fontInfo));

                ConfiguredFontAndColorSet<T> set = new(
                    category,
                    ref logfont[0],
                    ref fontInfo[0],
                    await GetColorsAsync(category, categoryGuid, storage),
                    category.UnregisterChangeListener
                );

                category.RegisterChangeListener(set);

                return set;
            }
            finally
            {
                storage.CloseCategory();
            }
        }

        private async Task<Dictionary<ColorDefinition, ConfiguredColor>> GetColorsAsync(BaseFontAndColorCategory category, Guid categoryGuid, IVsFontAndColorStorage storage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsFontAndColorUtilities utilities = await VS.GetRequiredServiceAsync<SVsFontAndColorStorage, IVsFontAndColorUtilities>();
            Dictionary<ColorDefinition, ConfiguredColor> colors = new();
            foreach (ColorDefinition definition in category.GetColorDefinitions())
            {
                // Get the color info from storage.
                ColorableItemInfo[] color = new ColorableItemInfo[1];
                ErrorHandler.ThrowOnFailure(storage.GetItem(definition.Name, color));

                // Convert the color info to foreground and background colors in RGB format.
                (uint background, uint foreground) = definition.GetColors(ref categoryGuid, ref color[0], utilities);

                colors[definition] = new ConfiguredColor(background, foreground, (FontStyle)color[0].dwFontFlags);
            }
            return colors;
        }
    }
}
