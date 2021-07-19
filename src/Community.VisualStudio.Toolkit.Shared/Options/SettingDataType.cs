using System;
using System.IO;
using System.Globalization;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Enumeration that specifies both the underlying type that is to be stored/retrieved from the
    ///             <see cref="SettingsStore"/> and method of type conversion.  </summary>
    public enum SettingDataType
    {
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as a <see cref="string"/>.
        /// <see langword="null"/> strings are converted to an empty string, therefore will not round-trip.
        /// Type conversions, if needed, are performed via <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />, 
        /// using <see cref="CultureInfo.InvariantCulture"/>. Types such as <see cref="float"/>, <see cref="double"/>, 
        /// <see cref="decimal"/>, and <see cref="char"/> are stored this way. </summary>
        String,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as an <see cref="int"/>.
        /// Type conversions, if needed, are performed via <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />,
        /// using <see cref="CultureInfo.InvariantCulture"/>. <see cref="System.Drawing.Color"/> is converted using To[From]Argb. </summary>
        Int32,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as an <see cref="uint"/>.
        /// Type conversions, if needed, are performed via <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />,
        /// using <see cref="CultureInfo.InvariantCulture"/>. </summary>
        UInt32,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as an <see cref="long"/>.
        /// Type conversions, if needed, are performed via <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />,
        /// using <see cref="CultureInfo.InvariantCulture"/>. <see cref="DateTime"/> is converted via To[From]Binary, and 
        /// <see cref="DateTimeOffset"/> is converted via To[From]UnixTimeMilliseconds. </summary>
        Int64,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as an <see cref="ulong"/>.
        /// Type conversions, if needed, are performed via <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />,
        /// using <see cref="CultureInfo.InvariantCulture"/>. </summary>
        UInt64,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as a <see cref="MemoryStream"/>.
        /// Array of <see cref="byte"/> is wrapped in a <see cref="MemoryStream"/>. <see langword="null"/> values are converted 
        /// to an empty <see cref="MemoryStream"/>, therefore will not round-trip. </summary>
        Binary,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as a <see cref="string"/>.
        /// Conversion uses <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/>, with the bytes converted
        /// to/from a Base64 encoded string, the string value is what is stored. <see langword="null"/> values are stored 
        /// as an empty string. </summary>
        Legacy,
        /// <summary>   Value of the property is persisted in the <see cref="SettingsStore"/> as a <see cref="string"/>.
        /// The methods <see cref="BaseOptionModel{T}.SerializeValue"/> and <see cref="BaseOptionModel{T}.DeserializeValue"/> are
        /// used to convert to and from storage. </summary>
        Serialized,
    }
}