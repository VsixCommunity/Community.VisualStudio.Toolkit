using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines a font and color definitions that are shown on the <i>Fonts and Colors</i> options page.
    /// </summary>
    /// <typeparam name="T">The implementation type itself.</typeparam>
    /// <remarks>
    /// <para>
    /// Define a color category by inheriting from <see cref="BaseFontAndColorCategory{T}"/>. The color 
    /// definitions in the category are defined by declaring properties of type <see cref="ColorDefinition"/>.
    /// </para>
    /// <para>
    /// An extension that defines a font and color category must also have a class inheriting from 
    /// <see cref="BaseFontAndColorProvider"/> that will provide the category to Visual Studio.
    /// An extension can define multiple font and color categories, but only needs to define one provider.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Guid("e977c587-c06e-xxxx-xxxx-cbf9da1bdafa")]
    /// public class MyFirstFontAndColorCategory : BaseFontAndColorCategory&lt;MyFirstFontAndColorCategory&gt;
    /// {
    ///     public MyFirstFontAndColorCategory() : base(new FontDefinition("Times New Roman", 14)) { }
    /// 
    ///     public override string Name => "My First Category";
    /// 
    ///     public ColorDefinition Primary { get; } = new
    ///         "Primary",
    ///         defaultBackground: VisualStudioColor.Indexed(COLORINDEX.CI_RED),
    ///         defaultForeground: VisualStudioColor.Indexed(COLORINDEX.CI_WHITE)
    ///     );
    /// 
    ///     public ColorDefinition Secondary { get; } = new(
    ///         "Secondary",
    ///         defaultBackground: VisualStudioColor.Indexed(COLORINDEX.CI_YELLOW),
    ///         defaultForeground: VisualStudioColor.Indexed(COLORINDEX.CI_BLACK),
    ///         options: ColorOptions.AllowBackgroundChange | ColorOptions.AllowBoldChange
    ///     );
    /// }
    /// </code>
    /// </example>
    public abstract class BaseFontAndColorCategory<T> : BaseFontAndColorCategory
        where T : BaseFontAndColorCategory<T>, new()
    {
        private static BaseFontAndColorCategory<T>? _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFontAndColorCategory{T}"/> class.
        /// The default font is set to <see cref="FontDefinition.Automatic"/>.
        /// </summary>
        /// <remarks>
        /// Do not create an instance of this class directly. Instead, use the 
        /// <see cref="Instance"/> property to access a single instance of the class.
        /// </remarks>
        protected BaseFontAndColorCategory() : this(FontDefinition.Automatic) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFontAndColorCategory{T}"/> class
        /// with the given default font.
        /// </summary>
        /// <param name="defaultFont">This category's default font.</param>
        /// <remarks>
        /// Do not create an instance of this class directly. Instead, use the 
        /// <see cref="Instance"/> property to access a single instance of the class.
        /// </remarks>
        protected BaseFontAndColorCategory(FontDefinition defaultFont) : base(defaultFont)
        {
            if (_instance is not null)
            {
                throw new InvalidOperationException(
                    $"The font and color category '{typeof(T).Name}' has already been created. " +
                    $"Use the '{nameof(Instance)}' property to access a single instance instead of creating a new instance."
                );
            }

            _instance = this;
        }

        /// <summary>
        /// The instance of this class.
        /// </summary>
        /// <remarks>
        /// Always use this single instance instead of creating a new instance of the class.
        /// </remarks>
        public static T Instance => (T)(_instance ?? new T()); // Note: The constructor will assign to `_instance`.
    }
}
