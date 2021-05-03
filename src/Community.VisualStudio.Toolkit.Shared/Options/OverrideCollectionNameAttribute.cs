using System;
using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Apply this attribute on an individual get/set property in your <see cref="BaseOptionModel{T}"/> 
    ///             derived class to use a specific <c>CollectionName</c> to store a given property in the 
    ///             <see cref="WritableSettingsStore"/> rather than using the <see cref="BaseOptionModel{T}.CollectionName"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class OverrideCollectionNameAttribute : Attribute
    {
        /// <summary>   Specifies the <c>CollectionName</c> in the <see cref="WritableSettingsStore"/> where
        ///             this setting is stored rather than using the default, which is the <c>FullName</c>
        ///             of the typeparam <c>T</c>. </summary>
        /// <param name="collectionName">   This value is used as the <c>collectionPath</c> parameter when reading 
        ///                                 and writing using the <see cref="WritableSettingsStore"/>.  </param>
        public OverrideCollectionNameAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }

        /// <summary>   This value is used as the <c>collectionPath</c> parameter when reading
        ///             and writing using the <see cref="WritableSettingsStore"/>.  </summary>
        public string CollectionName { get; }
    }
}