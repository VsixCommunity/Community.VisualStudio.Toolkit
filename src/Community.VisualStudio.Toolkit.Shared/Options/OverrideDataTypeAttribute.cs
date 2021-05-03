using System;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Apply this attribute on a get/set property in the <see cref="BaseOptionModel{T}"/> class to 
    ///             alter the default mechanism used to store/retrieve the value of this property from the
    ///             <see cref="WritableSettingsStore"/>. If not specified, the <see cref="SettingDataType"/><c>.Serialized</c>
    ///             mechanism is used.  </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OverrideDataTypeAttribute : Attribute
    {
        /// <summary>   Alters the default mechanism used to store/retrieve the value of this property from the setting store. </summary>
        /// <param name="settingDataType">  Specifies the type and/or method used to store and retrieve the value of the attributed 
        ///                                 property in the <see cref="WritableSettingsStore"/>. </param>
        public OverrideDataTypeAttribute(SettingDataType settingDataType)
        {
            SettingDataType = settingDataType;
        }

        /// <summary>   Specifies the type and/or method used to store and retrieve the value of the attributed
        ///             property in the <see cref="WritableSettingsStore"/>.</summary>
        public SettingDataType SettingDataType { get; }
    }
}