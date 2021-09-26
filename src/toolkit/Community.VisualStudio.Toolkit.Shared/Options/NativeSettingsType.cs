using System;
using System.IO;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Data types of the properties that are stored inside the collections. This mostly mirror
    /// <see cref="SettingsType"/>, but adds <c>UInt32</c> and <c>UInt64</c>
    /// </summary>
    public enum NativeSettingsType
    {
        /// <summary>
        /// Data type used to store 4 byte (32 bits) properties which are Boolean and Int32. Note
        /// that Boolean is stored 1 byte in the .NET environment but as a property inside the SettingsStore,
        /// it is kept as 4 byte value and any value other than 0 is converted to true and 0 is converted to
        /// false.
        /// NOTE: In .NET we need to explicitly use the unsigned methods to successfully store unsigned types.
        /// This enumeration adds <see cref="UInt32"/> for that purpose.
        /// </summary>
        Int32 = 1,
        /// <summary>
        /// Data type used to store 8 byte (64 bit) properties which are Int64.
        /// NOTE: In .NET we need to explicitly use the unsigned methods to successfully store unsigned types. 
        /// This enumeration adds <see cref="UInt64"/> for that purpose.
        /// </summary>
        Int64 = 2,
        /// <summary>Data type used to store the strings.</summary>
        String = 3,
        /// <summary>Data type used to store byte streams (arrays).</summary>
        Binary = 4,
        /// <summary>
        /// Data type used to store 4 byte (32 bits) properties which is UInt32.
        /// NOTE: This value is not in <see cref="SettingsType"/>, but is necessary so we can use the 
        /// appropriate methods to successfully store unsigned types.
        /// </summary>
        UInt32 = 5,
        /// <summary>
        /// Data type used to store 8 byte (64 bit) properties which is UInt64.
        /// NOTE: This value is not in <see cref="SettingsType"/>, but is necessary so we can use the 
        /// appropriate methods to successfully store unsigned types.
        /// </summary>
        UInt64 = 6,
    }

    /// <summary>   Extension methods for <see cref="NativeSettingsType"/>. </summary>
    public static class NativeSettingsTypeExtensions
    {
        /// <summary>   Get the .NET <see cref="Type"/> based on the method signature for the <see cref="SettingsStore"/>
        ///             necessary to retrieve and store data. </summary>
        /// <exception cref="ArgumentOutOfRangeException">  Thrown when one or more arguments are outside
        ///                                                 the required range. </exception>
        /// <param name="nativeSettingsType">   The nativeSettingsType to act on. </param>
        /// <returns>   The .NET <see cref="Type"/>. Not Null. </returns>
        public static Type GetDotNetType(this NativeSettingsType nativeSettingsType)
        {
            switch (nativeSettingsType)
            {
                case NativeSettingsType.Int32:
                    return typeof(int);
                case NativeSettingsType.Int64:
                    return typeof(long);
                case NativeSettingsType.String:
                    return typeof(string);
                case NativeSettingsType.Binary:
                    return typeof(MemoryStream);
                case NativeSettingsType.UInt32:
                    return typeof(uint);
                case NativeSettingsType.UInt64:
                    return typeof(ulong);
                default:
                    throw new ArgumentOutOfRangeException(nameof(nativeSettingsType), nativeSettingsType, null);
            }
        }
    }
}
