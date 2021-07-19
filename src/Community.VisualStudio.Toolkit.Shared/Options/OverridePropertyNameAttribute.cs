using System;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Apply this attribute on an individual public get/set property in your <see cref="BaseOptionModel{T}"/> 
    ///             derived class to use a specific <c>propertyName</c> to store a given property in the
    ///             <see cref="SettingsStore"/> rather than using the name of the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OverridePropertyNameAttribute : Attribute
    {
        /// <summary>   Specifies the <c>propertyName</c> in the <see cref="SettingsStore"/> where
        ///             this setting is stored rather than using the default, which is the name of 
        ///             the property. </summary>
        /// <param name="propertyName">   This value is used as the <c>propertyName</c> parameter when reading
        ///                                 and writing to the <see cref="SettingsStore"/>.  </param>
        public OverridePropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>   This value is used as the <c>propertyName</c> parameter when reading
        ///             and writing to the <see cref="SettingsStore"/>.  </summary>
        public string PropertyName { get; }
    }
}