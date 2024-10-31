using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines a font and color definitions that are shown on the <i>Fonts and Colors</i> options page.
    /// </summary>
    /// <remarks>
    /// Inherit from <see cref="BaseFontAndColorCategory{T}"/> instead of this class.
    /// </remarks>
    public abstract class BaseFontAndColorCategory : IVsFontAndColorDefaults, IVsFontAndColorEvents
    {
        private readonly FontDefinition _defaultFont;
        private readonly Guid _categoryGuid;
        private readonly List<IFontAndColorChangeListener> _changeListeners;
        private protected IVsFontAndColorUtilities? _utilities;
        private protected ColorDefinition[]? _colorDefinitions;

        internal BaseFontAndColorCategory(FontDefinition defaultFont)
        {
            _defaultFont = defaultFont;
            _categoryGuid = GetType().GUID;
            _changeListeners = new List<IFontAndColorChangeListener>();
        }

        internal void Initialize(IVsFontAndColorUtilities utilities)
        {
            _utilities = utilities;

            // The available colors are defined by declaring properties on the derived
            // class. Find all `ColorDefinition` properties on this type (we don't
            // have any, so the only ones we will find will be on the derived class).
            //
            // The color definitions are found here instead of in the constructor because we
            // should not access the properties on the derived type from our constructor (at
            // the point that our constructor runs, the constructor of the derived class has
            // not run, so the properties may not have been initialized at that point).
            _colorDefinitions ??= GetType()
                .GetProperties()
                .Where((x) => x.CanRead && x.PropertyType == typeof(ColorDefinition))
                .Select((x) => x.GetValue(this))
                .Cast<ColorDefinition>().ToArray();
        }

        internal IEnumerable<ColorDefinition> GetColorDefinitions()
        {
            ThrowIfNotInitialized();
            return _colorDefinitions;
        }

        internal void RegisterChangeListener(IFontAndColorChangeListener listener)
        {
            // Lock when accessing the list because we cannot guarantee
            // that a set will be unregistered on the main thread.
            lock (_changeListeners)
            {
                _changeListeners.Add(listener);
            }
        }

        internal void UnregisterChangeListener(IFontAndColorChangeListener listener)
        {
            // Lock when accessing the list because we cannot guarantee
            // that a set will be unregistered on the main thread.
            lock (_changeListeners)
            {
                _changeListeners.Remove(listener);
            }
        }

        [MemberNotNull(nameof(_colorDefinitions), nameof(_utilities))]
        private protected void ThrowIfNotInitialized()
        {
            if (_utilities is null || _colorDefinitions is null)
            {
                throw new InvalidOperationException(
                    $"The font and color category '{GetType().FullName}' has not been initialized."
                );
            }
        }

        /// <summary>
        /// The name of the category. This appears in the drop-down on the <i>Fonts and Colors</i> page.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The priority of this category. This determines the order of
        /// the category in the drop-down on the <i>Fonts and Colors</i> page.
        /// </summary>
        /// <remarks>
        /// The default priority is <c>257</c> which places this category after
        /// the <i>Environment</i> category (which has a priority of <c>256</c>).
        /// </remarks>
        public virtual ushort Priority => 257;

        /// <summary>
        /// The GUID of the category to use as the base category when resetting to defaults.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For more information, see <see cref="IVsFontAndColorDefaults.GetBaseCategory(out Guid)"/>.
        /// </para>
        /// <para>
        /// Some known categories can be found in <see cref="FontsAndColorsCategory"/>.
        /// </para>
        /// </remarks>
        public virtual Guid BaseCategory => Guid.Empty;

        /// <summary>
        /// Visual Studio must be restarted for changes to the fonts or colors to take effect.
        /// </summary>
        /// <remarks>Corresponds to <see cref="__FONTCOLORFLAGS.FCF_MUSTRESTART"/>.</remarks>
        protected virtual bool MustRestart => false;

        /// <summary>
        /// Restricts the Font drop-down box to only display TrueType fonts.
        /// </summary>
        /// <remarks>Corresponds to <see cref="__FONTCOLORFLAGS.FCF_ONLYTTFONTS"/>.</remarks>
        protected virtual bool OnlyTrueTypeFonts => true;

        /// <summary>
        /// Visual Studio will save all customizable Display Item attributes if any of them have been modified.
        /// Normally only attributes that have changed from their defaults are saved.
        /// </summary>
        /// <remarks>Corresponds to <see cref="__FONTCOLORFLAGS.FCF_SAVEALL"/>.</remarks>
        protected virtual bool SaveAllItemsIfAnyModified => false;

        /// <summary>
        /// Generates a warning that changes will take effect only for new instance of the UI components that use the font or color.
        /// </summary>
        /// <remarks>Corresponds to <see cref="__FONTCOLORFLAGS.FCF_ONLYNEWINSTANCES"/>.</remarks>
        protected virtual bool ChangesAffectNewInstancesOnly => false;

        int IVsFontAndColorDefaults.GetFlags(out uint dwFlags)
        {
            dwFlags = 0;
            if (MustRestart)
            {
                dwFlags |= (uint)__FONTCOLORFLAGS.FCF_MUSTRESTART;
            }
            if (OnlyTrueTypeFonts)
            {
                dwFlags |= (uint)__FONTCOLORFLAGS.FCF_ONLYTTFONTS;
            }
            if (SaveAllItemsIfAnyModified)
            {
                dwFlags |= (uint)__FONTCOLORFLAGS.FCF_SAVEALL;
            }
            if (ChangesAffectNewInstancesOnly)
            {
                dwFlags |= (uint)__FONTCOLORFLAGS.FCF_ONLYNEWINSTANCES;
            }
            // If the default font is "Automatic", then the `FCF_AUTOFONT` needs
            // to be specified so that "Automatic" is listed in the font drop down.
            if (string.Equals(_defaultFont.FamilyName, FontDefinition.Automatic.FamilyName))
            {
                dwFlags |= (uint)__FONTCOLORFLAGS.FCF_AUTOFONT;
            }
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetPriority(out ushort pPriority)
        {
            pPriority = Priority;
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetCategoryName(out string pbstrName)
        {
            pbstrName = Name;
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetBaseCategory(out Guid pguidBase)
        {
            pguidBase = BaseCategory;
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetFont(FontInfo[] pInfo)
        {
            pInfo[0] = new FontInfo
            {
                bstrFaceName = _defaultFont.FamilyName,
                wPointSize = _defaultFont.Size,
                iCharSet = _defaultFont.CharacterSet,
                bFaceNameValid = 1,
                bPointSizeValid = 1,
                bCharSetValid = 1
            };
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetItemCount(out int pcItems)
        {
            ThrowIfNotInitialized();
            pcItems = _colorDefinitions.Length;
            return VSConstants.S_OK;
        }

        int IVsFontAndColorDefaults.GetItem(int iItem, AllColorableItemInfo[] pInfo)
        {
            ThrowIfNotInitialized();
            ThreadHelper.ThrowIfNotOnUIThread();

            if (iItem >= 0 && iItem < _colorDefinitions.Length)
            {
                pInfo[0] = _colorDefinitions[iItem].ToAllColorableItemInfo(_utilities);
                return VSConstants.S_OK;
            }

            return VSConstants.E_FAIL;
        }

        int IVsFontAndColorDefaults.GetItemByName(string szItem, AllColorableItemInfo[] pInfo)
        {
            ThrowIfNotInitialized();
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (ColorDefinition definition in _colorDefinitions)
            {
                if (definition.Name == szItem)
                {
                    pInfo[0] = definition.ToAllColorableItemInfo(_utilities);
                    return VSConstants.S_OK;
                }
            }

            return VSConstants.E_FAIL;
        }

#if VS17
        int IVsFontAndColorEvents.OnFontChanged(ref Guid rguidCategory, FontInfo[] pInfo, LOGFONTW[] pLOGFONT, IntPtr HFONT)
#else
        int IVsFontAndColorEvents.OnFontChanged(ref Guid rguidCategory, FontInfo[] pInfo, LOGFONTW[] pLOGFONT, uint HFONT)
#endif
        {
            // We get notified about _all_ font changes, but we
            // only want to handle the changes for this category.
            if (rguidCategory.Equals(_categoryGuid))
            {
                EmitChange((x) => x.SetFont(ref pLOGFONT[0], ref pInfo[0]));
            }
            return VSConstants.S_OK;
        }

        int IVsFontAndColorEvents.OnItemChanged(ref Guid rguidCategory, string szItem, int iItem, ColorableItemInfo[] pInfo, uint crLiteralForeground, uint crLiteralBackground)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // We get notified about _all_ color changes, but we
            // only want to handle the changes for this category.
            if (rguidCategory.Equals(_categoryGuid))
            {
                // We should have been initialized by this point, but since this
                // is an event handler, we won't throw an error if we haven't
                // been initialized. We'll just ignore the event instead.
                if ((_colorDefinitions is not null) && (_utilities is not null))
                {
                    // Ignore the change if any part of it is invalid.
                    if ((pInfo[0].bBackgroundValid != 0) && (pInfo[0].bForegroundValid != 0) && (pInfo[0].bFontFlagsValid != 0))
                    {
                        if ((iItem >= 0) && (iItem < _colorDefinitions.Length))
                        {
                            ColorDefinition definition = _colorDefinitions[iItem];

                            // The `crLiteralBackground` and `crLiteralForeground` parameters
                            // do not contain the correct color when the color is "Automatic",
                            // so we need to use the utilities service to get the RGB values.
                            (uint background, uint foreground) = definition.GetColors(
                                ref rguidCategory,
                                ref pInfo[0],
                                _utilities
                            );

                            EmitChange(
                                (x) => x.SetColor(definition, background, foreground, (FontStyle)pInfo[0].dwFontFlags)
                            );
                        }
                    }
                }
            }
            return VSConstants.S_OK;
        }

        int IVsFontAndColorEvents.OnReset(ref Guid rguidCategory)
        {
            return VSConstants.S_OK;
        }

        int IVsFontAndColorEvents.OnResetToBaseCategory(ref Guid rguidCategory)
        {
            return VSConstants.S_OK;
        }

        int IVsFontAndColorEvents.OnApply()
        {
            return VSConstants.S_OK;
        }

        private void EmitChange(Action<IFontAndColorChangeListener> action)
        {
            // We can't control what thread a listener is unregistered from, so we
            // need to lock around accessing that list. Take a copy so that we don't
            // hold the lock while we emit the changes. Taking a copy also prevents
            // the collection from being changed while we are iterating through it
            // (because a listener could be unregistered in response to the change).
            IFontAndColorChangeListener[] listeners;
            lock (_changeListeners)
            {
                listeners = _changeListeners.ToArray();
            }
            foreach (IFontAndColorChangeListener listener in listeners)
            {
                action(listener);
            }
        }
    }
}
