using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for easily specifying options that are stored in the <see cref="SettingsStore"/>
    /// (Visual Studio's private registry hive) as per-user settings. The <see cref="SettingsStore"/>
    /// the <c>collectionPath</c> is determined via <see cref="CollectionName"/>. All properties with <c>public</c> 
    /// getters and <c>public</c> setters are loaded and saved using the name of the property as the key. 
    /// See <c>Remarks</c>.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="CollectionName"/> is used as the <see cref="SettingsStore"/>'s <c>collectionPath</c>
    /// for all settings in this class, and is easily overridden. An individual property can be overridden by adding
    /// <see cref="OverrideCollectionNameAttribute"/> to the property.
    /// <para/>
    /// Also by default, the <see cref="SettingsStore"/>'s <c>propertyName</c> is set to the name of the property,
    /// which can be overridden by adding <see cref="OverridePropertyNameAttribute"/> to the property.
    /// <para/>
    /// Property values are stored using the most appropriate native storage type. Support exists to store for all 
    /// integral numeric types, enumerations, <see cref="float" />, <see cref="double" />, 
    /// <see cref="decimal" />, <see cref="bool" />, <see cref="char" />, <see cref="string" />, 
    /// <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="System.Drawing.Color"/>, 
    /// <see cref="Guid"/>, <see cref="MemoryStream"/>, and arrays of <see cref="byte"/>.
    /// <para/>
    /// For property types not mentioned above, they are stored after passing through the <see cref="SerializeValue"/> 
    /// and <see cref="DeserializeValue"/> methods, which use <see cref="XmlSerializer"/> by default. If this mechanism is not
    /// viable for your desired property type, you may derive from these methods to implement your own serialization mechanism.
    /// <para/>
    /// When migrating an existing extension, you can use the <see cref="OverrideDataTypeAttribute"/> to specify the 
    /// native type it should use instead. See documentation on the <see cref="SettingDataType"/> enumeration. With this
    /// attribute you can specify the type conversion to use the <see cref="System.ComponentModel.TypeConverterAttribute"/>,
    /// or the default <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />.
    /// </remarks>
    public abstract class BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private static readonly AsyncLazy<T> _liveModel = new(CreateAsync, ThreadHelper.JoinableTaskFactory);
        private static readonly AsyncLazy<ShellSettingsManager> _settingsManager = new(GetSettingsManagerAsync, ThreadHelper.JoinableTaskFactory);

        /// <summary>   Use <see cref="GetPropertyWrappers"/>. This is an implementation detail of base class. </summary>
        private static IReadOnlyList<IOptionModelPropertyWrapper> _propertyWrappers = new List<IOptionModelPropertyWrapper>();
        /// <summary>   (Immutable) Use <see cref="GetPropertyWrappers"/>. This is an implementation detail of base class. </summary>
        private static readonly object _propertyWrapperLock = new();
        /// <summary>   (Immutable) Use <see cref="GetPropertyWrappers"/>. This is an implementation detail of base class. </summary>
        private static bool _propertyWrappersLoaded;

        /// <summary>
        /// Creates a new instance of the option model.
        /// </summary>
        protected BaseOptionModel()
        { }

        /// <summary>
        /// A singleton instance of the options. MUST be called from UI thread only.
        /// </summary>
        /// <remarks>
        /// Call <see cref="GetLiveInstanceAsync()" /> instead if on a background thread or in an async context on the main thread.
        /// </remarks>
        public static T Instance
        {
            get
            {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable VSTHRD104 // Offer async methods
                return ThreadHelper.JoinableTaskFactory.Run(GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
#pragma warning restore IDE0079 // Remove unnecessary suppression
            }
        }

        /// <summary>
        /// Get the singleton instance of the options. Thread safe.
        /// </summary>
        public static Task<T> GetLiveInstanceAsync()
        {
            return _liveModel.GetValueAsync();
        }

        /// <summary>
        /// Creates a new instance of the options class and loads the values from the store. For internal use only
        /// </summary>
        /// <returns></returns>
        public static async Task<T> CreateAsync()
        {
            T instance = new();
            await instance.LoadAsync();
            return instance;
        }

        /// <summary>
        /// The name of the options collection as stored in the registry. By default this is <c>typeof(</c><typeparamref name="T"/><c>).FullName</c>
        /// unless overridden. This can also be overridden for an individual property in <typeparamref name="T"/> by adding the 
        /// <see cref="OverrideCollectionNameAttribute"/> to the property.
        /// </summary>
        protected internal virtual string CollectionName { get; } = typeof(T).FullName;

        /// <summary>
        /// Hydrates the properties from the registry, via <see cref="LoadAsync"/>.
        /// </summary>
        public virtual void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        /// <summary>
        /// Hydrates the values of the properties returned by <see cref="GetOptionProperties"/> from the User <see cref="SettingsStore"/>
        /// (Visual Studio Private Registry) asynchronously.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            SettingsStore settingsStore = manager.GetReadOnlySettingsStore(scope);

            foreach (IOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
            {
                propertyWrapper.Load(this, settingsStore);
            }
        }

        /// <summary>
        /// Saves the properties to the registry, via <see cref="SaveAsync"/>.
        /// </summary>
        public virtual void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        /// <summary>
        /// Saves the values of the properties returned by <see cref="GetOptionProperties"/> to the User <see cref="SettingsStore"/>
        /// (Visual Studio Private Registry) asynchronously. After the values are saved, the live instance will be refreshed and 
        /// <see cref="Saved"/> will then be raised.
        /// </summary>
        public virtual async Task SaveAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            WritableSettingsStore settingsStore = manager.GetWritableSettingsStore(scope);

            foreach (IOptionModelPropertyWrapper propertyWrapper in GetPropertyWrappers())
            {
                propertyWrapper.Save(this, settingsStore);
            }

            T liveModel = await GetLiveInstanceAsync();

            if (this != liveModel)
            {
                await liveModel.LoadAsync();
            }

            Saved?.Invoke(liveModel);
        }

        /// <summary>   For properties with types that cannot be stored natively, this method is given a
        ///             the raw <paramref name="value"/> from the property and its declared
        ///             <paramref name="type"/>. This must be serialized into a non-null
        ///             <see cref="string"/> that will be stored as such in the
        ///             <see cref="SettingsStore"/>. This method should throw an exception on failure. </summary>
        /// <remarks>   The base class implementation utilizes the <see cref="XmlSerializer"/> to avoid
        ///             reliance on 3rd party libraries. If overriding this method, you must also override the related
        ///             <see cref="DeserializeValue"/>, as these are two interrelated methods. <para />
        ///             If you wish to represent <c>null</c> for reference types, be aware that the
        ///             <see cref="string"/> that is stored must be non-null. <para /> </remarks>
        /// <param name="value">        The object that is to be serialized. Can Be Null. </param>
        /// <param name="type">         The type of the property. <paramref name="value"/> would be an instance of something assignable to this type.  </param>
        /// <param name="propertyName"> The <c>PropertyName</c> in the <see cref="SettingsStore"/> where the value of this property is stored. </param>
        /// <returns>   The string containing the data necessary to deserialize the object. Not Null. </returns>
        protected internal virtual string SerializeValue(object? value, Type type, string propertyName)
        {
            if (value == null)
                return string.Empty;

            XmlSerializer xmlSerializer = new(value.GetType());
            using (StringWriter textWriter = new())
            {
                xmlSerializer.Serialize(textWriter, value);
                return textWriter.ToString();
            }
        }

        /// <summary>   For properties with types that cannot be stored natively, this method is given the
        ///             <paramref name="serializedData"/> from the <see cref="SettingsStore"/> containing
        ///             serialized data (from <see cref="SerializeValue"/>) and its <paramref name="type"/>. This
        ///             method should deserialize the data or throw an exception on failure. </summary>
        /// <remarks>   The base class implementation utilizes the <see cref="XmlSerializer"/> to avoid
        ///             reliance on 3rd party libraries. If overriding this method, you must also override the related
        ///             <see cref="SerializeValue"/>, as these are two interrelated methods. <para />
        ///             If you wish to represent <c>null</c> for reference types, be aware that the
        ///             <see cref="string"/> that is stored must be non-null. <para />
        ///             For value types you must always return an instance of the
        ///             <paramref name="type"/>. </remarks>
        /// <param name="serializedData"> The string representing the serialized object. By convention, an
        ///                             empty string indicates a value of null. </param>
        /// <param name="type">         The type of the property. The return value should be an instance of something assignable to this type.  </param>
        /// <param name="propertyName"> The <c>PropertyName</c> in the <see cref="SettingsStore"/> where the value of this property is stored. </param>
        /// <returns>   The deserialized object, which should be an instance of something assignable to <paramref name="type"/> (if a value type, cannot be null)  </returns>
        protected internal virtual object? DeserializeValue(string serializedData, Type type, string propertyName)
        {
            if (serializedData.Length == 0)
            {
                if(type.IsValueType)
                    return Activator.CreateInstance(type);
                return null;
            }

            XmlSerializer xmlSerializer = new(type);
            return xmlSerializer.Deserialize(new StringReader(serializedData));
        }

        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new ShellSettingsManager(ServiceProvider.GlobalProvider);
        }

        /// <summary>   Returns an enumerable of <see cref="PropertyInfo"/> for the properties of <typeparamref name="T"/>
        /// that will be loaded and saved. Base implementation utilizes reflection. </summary>
        protected virtual IEnumerable<PropertyInfo> GetOptionProperties()
        {
            return GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsPublic && p.CanRead && p.CanWrite);
        }

        /// <summary>   Uses <see cref="GetOptionProperties"/> to retrieve a list of properties to persist. 
        ///             For each of those properties, a wrapper is created that implements the 
        ///             logic necessary to get and set values in the properties and in the 
        ///             <see cref="SettingsStore"/>. Once performed, the results should be 
        ///             cached and returned in subsequent calls. </summary>
        /// <remarks> This implementation only performs this once and caches the results 
        ///           statically. After this is called once per type, reflection is no longer 
        ///           necessary for any operations performed by this class. </remarks>
        protected virtual IEnumerable<IOptionModelPropertyWrapper> GetPropertyWrappers()
        {
            if(_propertyWrappersLoaded)
                return _propertyWrappers;
            lock (_propertyWrapperLock)
            {
                if (_propertyWrappersLoaded)
                    return _propertyWrappers;
                List<IOptionModelPropertyWrapper> propertyWrappers = new();
                _propertyWrappers = propertyWrappers.AsReadOnly();
                foreach (PropertyInfo property in GetOptionProperties())
                {
                    try
                    {
                        propertyWrappers.Add(new OptionModelPropertyWrapper(property));
                    }
                    catch (Exception ex)
                    {
                        ex.Log("BaseOptionModel<{0}>.{1} Property:{2} PropertyType:{3} is not a valid property.",
                            typeof(T).FullName, nameof(GetPropertyWrappers), property.Name, property.PropertyType);
                    }
                }
                _propertyWrappersLoaded = true;
            }
            return _propertyWrappers;
        }

        /// <summary>
        /// The Saved event is fired after the options have been persisted.
        /// </summary>
        public static event Action<T>? Saved;
    }
}