using System;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Apply this attribute on an individual get/set property in your <see cref="BaseOptionModel{T}"/> 
    ///             derived class to use a specific <c>CollectionName</c> to store a given property in the 
    ///             <see cref="SettingsStore"/> rather than using the <see cref="BaseOptionModel{T}.CollectionName"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OverrideCollectionNameAttribute : Attribute
    {
        /// <summary>   Specifies the <c>CollectionName</c> in the <see cref="SettingsStore"/> where
        ///             this setting is stored rather than using the default. </summary>
        /// <param name="collectionName">   This value is used as the <c>collectionPath</c> parameter when reading 
        ///                                 and writing using the <see cref="SettingsStore"/>.  </param>
        public OverrideCollectionNameAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }

        /// <summary>   This value is used as the <c>collectionPath</c> parameter when reading
        ///             and writing using the <see cref="SettingsStore"/>.  </summary>
        public string CollectionName { get; }
    }
}