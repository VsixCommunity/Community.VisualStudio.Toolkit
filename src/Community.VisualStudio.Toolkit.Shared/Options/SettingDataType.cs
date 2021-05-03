using System;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Enumeration that specifies the underlying type that is to be stored/retrieved from the
    ///             <see cref="WritableSettingsStore"/>.  </summary>
    public enum SettingDataType
    {
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetString"/>
        /// to update the value of the attributed property (of any <see cref="Type.IsSerializable"/> type), using 
        /// an underlying string type in the settings store. The raw value of the property is first serialized 
        /// to a string for storage, using the means specified in <see cref="BaseOptionModel{T}"/>. This differs
        /// from <see cref="String"/> because with this option the underlying type IS ALWAYS serialized prior 
        /// to storage. </summary>
        Serialized,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetString"/>
        /// to update the value of the attributed <see cref="string"/> property, using an underlying string type
        /// in the settings store. This differs from <see cref="Serialized"/> because with this option the raw 
        /// string is stored and IS NEVER serialized. </summary>
        String,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetBoolean"/>
        /// to update the value of the attributed <see cref="bool"/> property, using an underlying Int32 type 
        /// in the settings store. </summary>
        Bool,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetInt32"/>
        /// to update the value of the attributed <see cref="Int32"/> property, using an underlying Int32 type
        /// in the settings store. </summary>
        Int32,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetUInt32"/>
        /// to update the value of the attributed <see cref="UInt32"/> property, using an underlying UInt32 type
        /// in the settings store. </summary>
        UInt32,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetInt64"/>
        /// to update the value of the attributed <see cref="Int64"/> property, using an underlying Int64 type
        /// in the settings store. </summary>
        Int64,
        /// <summary>   Uses the <see cref="WritableSettingsStore"/>.<see cref="WritableSettingsStore.SetUInt64"/>
        /// to update the value of the attributed <see cref="UInt64"/> property, using an underlying UInt64 type
        /// in the settings store. </summary>
        UInt64,
    }
}