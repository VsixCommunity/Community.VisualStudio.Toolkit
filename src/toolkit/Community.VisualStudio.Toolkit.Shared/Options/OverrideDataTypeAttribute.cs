using System;
using System.Globalization;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Apply this attribute on a get/set property in the <see cref="BaseOptionModel{T}"/> class to 
    ///             specify the type and mechanism used to store/retrieve the value of this property in the
    ///             <see cref="SettingsStore"/>. If not specified, the default mechanism is used is based on the 
    ///             property type. </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OverrideDataTypeAttribute : Attribute
    {
        /// <summary>   Alters the default type and mechanism used to store/retrieve the value of this
        ///             property in the <see cref="SettingsStore"/>. </summary>
        /// <param name="settingDataType">  Specifies the type and/or method used to store and retrieve
        ///                                 the value of the attributed property in the
        ///                                 <see cref="SettingsStore"/>. </param>
        /// <param name="useTypeConverter"> (Optional, default <see langword="false"/>) If <see langword="true"/>, 
        ///                                 and the type has a <see cref="System.ComponentModel.TypeConverterAttribute"/>
        ///                                 that allows for conversion to <paramref name="settingDataType"/>, this
        ///                                 will be used to convert and store the property value. If the 
        ///                                 <paramref name="settingDataType"/> is <c>Legacy</c> or <c>Serialized</c>
        ///                                 this has no effect. For other <see cref="SettingDataType"/> values, 
        ///                                 <see langword="false"/> will use the default conversion mechanism of
        ///                                 <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />, using
        ///                                 <see cref="CultureInfo.InvariantCulture"/>. </param>
        public OverrideDataTypeAttribute(SettingDataType settingDataType, bool useTypeConverter = false)
        {
            SettingDataType = settingDataType;
            UseTypeConverter = useTypeConverter;
        }

        /// <summary>   Specifies the type and method used to store and retrieve the value of the attributed
        ///             property in the <see cref="SettingsStore"/>.</summary>
        public SettingDataType SettingDataType { get; }

        /// <summary>   If <see langword="true"/>, and the type has a <see cref="System.ComponentModel.TypeConverterAttribute"/>
        ///  that allows for conversion to <see cref="SettingDataType"/>, this will be used to convert and store the property value. 
        ///  If the <see cref="SettingDataType"/> is <c>Legacy</c> or <c>Serialized</c> this has no effect. For other 
        ///  <see cref="SettingDataType"/>, <see langword="false"/> will use the default conversion mechanism of 
        ///  <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />, using 
        ///  <see cref="CultureInfo.InvariantCulture"/>. </summary>
        public bool UseTypeConverter { get; }
    }
}