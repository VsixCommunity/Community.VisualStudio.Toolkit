namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines information about a font for a category.
    /// </summary>
    public class FontDefinition
    {
        /// <summary>
        /// The "Automatic" font which corresponds to the current "icon" system font setting in Windows.
        /// </summary>
        public static readonly FontDefinition Automatic = new("Automatic", 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="FontDefinition"/> class with the default character set.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="size">The point size of the font.</param>
        public FontDefinition(string familyName, ushort size) : this(familyName, size, 1 /* DEFAULT_CHARSET */) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontDefinition"/> class with the specified character set.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="size">The point size of the font.</param>
        /// <param name="characterSet">The character set.</param>
        public FontDefinition(string familyName, ushort size, byte characterSet)
        {
            FamilyName = familyName;
            Size = size;
            CharacterSet = characterSet;
        }

        /// <summary>
        /// The name of the font family.
        /// </summary>
        public string FamilyName { get; }

        /// <summary>
        /// The point size of the font.
        /// </summary>
        public ushort Size { get; }

        /// <summary>
        /// The character set.
        /// </summary>
        public byte CharacterSet { get; }
    }
}
