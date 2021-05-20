using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for easily specifying options that are stored in the <c>UserSettings</c> registry using 
    /// the <c>collectionPath</c> of <see cref="CollectionName"/>. All properties with getters and setters with
    /// types that are <see cref="Type.IsSerializable"/> and <c>public</c> will be loaded and saved using the
    /// name of the property as the key. See <c>Remarks</c>.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="CollectionName"/> is used as the <c>collectionPath</c> for all settings 
    /// in this class, and unless overridden is set to <c>typeof(</c><typeparamref name="T"/><c>).FullName</c>.
    /// This can also be overridden for an individual property in <typeparamref name="T"/> by adding the
    /// <see cref="OverrideCollectionNameAttribute"/> to the property.
    /// <para/>
    /// Also by default, the property values are stored as <see cref="string"/> types, using the <see cref="BinaryFormatter"/>
    /// to serialize the property value and storing this as a <c>Base64</c> encoded string. This mechanism can also
    /// be overridden via <see cref="SerializeValue"/> and <see cref="DeserializeValue"/>. In lieu of the serialization
    /// mechanism, since the <see cref="SettingsStore"/> provides the means of storing the values using a set of native 
    /// types (see <see cref="SettingDataType"/>), you can opt-in to using these by applying the 
    /// <see cref="OverrideDataTypeAttribute"/> to the property. The property's <see cref="Type"/> must be convertible 
    /// to the specified <see cref="SettingDataType"/>.
    /// </remarks>
    public abstract class BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private static readonly AsyncLazy<T> _liveModel = new(CreateAsync, ThreadHelper.JoinableTaskFactory);
        private static readonly AsyncLazy<ShellSettingsManager> _settingsManager = new(GetSettingsManagerAsync, ThreadHelper.JoinableTaskFactory);

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
                ThreadHelper.ThrowIfNotOnUIThread();
                return ThreadHelper.JoinableTaskFactory.Run(GetLiveInstanceAsync);
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
            var instance = new T();
            await instance.LoadAsync();
            return instance;
        }

        /// <summary>
        /// The name of the options collection as stored in the registry. By default this is <c>typeof(</c><typeparamref name="T"/><c>).FullName</c>
        /// unless overridden. This can also be overridden for an individual property in <typeparamref name="T"/> by adding the 
        /// <see cref="OverrideCollectionNameAttribute"/> to the property.
        /// </summary>
        protected virtual string CollectionName { get; } = typeof(T).FullName;

        /// <summary>
        /// Hydrates the properties from the registry.
        /// </summary>
        public virtual void Load()
        {
            ThreadHelper.JoinableTaskFactory.Run(LoadAsync);
        }

        /// <summary>
        /// Hydrates the properties from the registry asynchronously.
        /// </summary>
        public virtual async Task LoadAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            SettingsStore settingsStore = manager.GetReadOnlySettingsStore(scope);
            var testedCollections = new HashSet<string>();

            bool DoesCollectionExist(string collectionName)
            {
                if (testedCollections.Contains(collectionName))
                {
                    return true;
                }

                if (settingsStore.CollectionExists(collectionName))
                {
                    testedCollections.Add(collectionName);
                    return true;
                }
                return false;
            }

            foreach (PropertyInfo property in GetOptionProperties())
            {
                object? value = null;
                var collectionName = CollectionName;
                SettingDataType dataType = SettingDataType.Serialized;
                string? serializedValue = null;
                try
                {
                    OverrideCollectionNameAttribute? collectionNameAttribute = property.GetCustomAttribute<OverrideCollectionNameAttribute>();
                    collectionName = collectionNameAttribute?.CollectionName ?? collectionName;
                    if (!DoesCollectionExist(collectionName))
                    {
                        continue;
                    }

                    if (!settingsStore.PropertyExists(collectionName, property.Name))
                    {
                        continue;
                    }

                    OverrideDataTypeAttribute? overrideDataTypeAttribute = property.GetCustomAttribute<OverrideDataTypeAttribute>();
                    dataType = overrideDataTypeAttribute?.SettingDataType ?? dataType;

                    switch (dataType)
                    {
                        case SettingDataType.Serialized:
                            serializedValue = settingsStore.GetString(collectionName, property.Name, default);
                            value = DeserializeValue(serializedValue, property.PropertyType);
                            break;
                        case SettingDataType.Bool:
                            value = settingsStore.GetBoolean(collectionName, property.Name, false);
                            break;
                        case SettingDataType.Int32:
                            value = settingsStore.GetInt32(collectionName, property.Name, default);
                            break;
                        case SettingDataType.UInt32:
                            value = settingsStore.GetUInt32(collectionName, property.Name, default(int));
                            break;
                        case SettingDataType.Int64:
                            value = settingsStore.GetInt64(collectionName, property.Name, default);
                            break;
                        case SettingDataType.UInt64:
                            value = settingsStore.GetUInt64(collectionName, property.Name, default(long));
                            break;
                        case SettingDataType.String:
                            value = settingsStore.GetString(collectionName, property.Name, default);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "The specified datatype is not supported.");
                    }
                    property.SetValue(this, value);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync("BaseOptionModel<{0}>.{1} Scope:{2} CollectionName:{3} PropertyName:{4} dataType:{5} PropertyType:{6} Value:{7} SerializedValue:{8}",
                        typeof(T).FullName, nameof(LoadAsync), scope, collectionName, property.Name, dataType, property.PropertyType, value ?? "[NULL]",
                        serializedValue ?? "[NULL]");
                }
            }
        }

        /// <summary>
        /// Saves the properties to the registry.
        /// </summary>
        public virtual void Save()
        {
            ThreadHelper.JoinableTaskFactory.Run(SaveAsync);
        }

        /// <summary>
        /// Saves the properties to the registry asynchronously.
        /// </summary>
        public virtual async Task SaveAsync()
        {
            ShellSettingsManager manager = await _settingsManager.GetValueAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            WritableSettingsStore settingsStore = manager.GetWritableSettingsStore(scope);
            var testedCollections = new HashSet<string>();

            foreach (PropertyInfo property in GetOptionProperties())
            {
                OverrideCollectionNameAttribute? collectionNameAttribute = property.GetCustomAttribute<OverrideCollectionNameAttribute>();
                var collectionName = collectionNameAttribute?.CollectionName ?? CollectionName;

                OverrideDataTypeAttribute? overrideDataTypeAttribute = property.GetCustomAttribute<OverrideDataTypeAttribute>();
                SettingDataType dataType = overrideDataTypeAttribute?.SettingDataType ?? SettingDataType.Serialized;
                object? value = null;

                try
                {
                    value = property.GetValue(this);

                    if (testedCollections.Add(collectionName))
                    {
                        if (!settingsStore.CollectionExists(collectionName))
                        {
                            settingsStore.CreateCollection(collectionName);
                        }
                    }

                    switch (dataType)
                    {
                        case SettingDataType.Serialized:
                            var serializedValue = SerializeValue(property.GetValue(this));
                            settingsStore.SetString(collectionName, property.Name, serializedValue);
                            break;
                        case SettingDataType.Bool:
                            var boolValue = Convert.ToBoolean(property.GetValue(this));
                            settingsStore.SetBoolean(collectionName, property.Name, boolValue);
                            break;
                        case SettingDataType.Int32:
                            var intValue = Convert.ToInt32(property.GetValue(this));
                            settingsStore.SetInt32(collectionName, property.Name, intValue);
                            break;
                        case SettingDataType.UInt32:
                            var uintValue = Convert.ToUInt32(property.GetValue(this));
                            settingsStore.SetUInt32(collectionName, property.Name, uintValue);
                            break;
                        case SettingDataType.Int64:
                            var int64Value = Convert.ToInt64(property.GetValue(this));
                            settingsStore.SetInt64(collectionName, property.Name, int64Value);
                            break;
                        case SettingDataType.UInt64:
                            var uint64Value = Convert.ToUInt64(property.GetValue(this));
                            settingsStore.SetUInt64(collectionName, property.Name, uint64Value);
                            break;
                        case SettingDataType.String:
                            var stringValue = Convert.ToString(property.GetValue(this));
                            settingsStore.SetString(collectionName, property.Name, stringValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "The specified datatype is not supported.");
                    }
                }
                catch (Exception ex)
                {
                    await ex.LogAsync("BaseOptionModel<{0}>.{1} Scope:{2} CollectionName:{3} PropertyName:{4} dataType:{5} PropertyType:{6} Value:{7} ValueType:{8}",
                        typeof(T).FullName, nameof(SaveAsync), scope, collectionName, property.Name, dataType, property.PropertyType, value ?? "[NULL]",
                        value?.GetType().FullName ?? "[NULL]");
                }
            }

            T liveModel = await GetLiveInstanceAsync();

            if (this != liveModel)
            {
                await liveModel.LoadAsync();
            }

            Saved?.Invoke(this, liveModel);
        }

        /// <summary>
        /// Serializes an object value to a string using the binary serializer.
        /// </summary>
        protected virtual string SerializeValue(object value)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, value);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes a string to an object using the binary serializer.
        /// </summary>
        protected virtual object DeserializeValue(string value, Type type)
        {
            var b = Convert.FromBase64String(value);

            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
#if VS14
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new ShellSettingsManager(ServiceProvider.GlobalProvider);
#else
            IVsSettingsManager? svc = await VS.GetRequiredServiceAsync<SVsSettingsManager, IVsSettingsManager>();

            return new ShellSettingsManager(svc);
#endif
        }

        /// <summary>   Returns an enumerable of <see cref="PropertyInfo"/> for the properties of <typeparamref name="T"/>
        /// that will be loaded and saved. </summary>
        protected IEnumerable<PropertyInfo> GetOptionProperties()
        {
            return GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsSerializable && p.PropertyType.IsPublic && p.CanRead && p.CanWrite);
        }

        /// <summary>
        /// The Saved event is fired after the options have been persisted.
        /// </summary>
        public static event EventHandler<T>? Saved;
    }
}