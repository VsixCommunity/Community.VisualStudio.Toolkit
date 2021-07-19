using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Threading;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Wraps an instance property member with public getter and setters from a <see cref="BaseOptionModel{T}"/>,
    ///             and exposes the ability to load and save the value of the property to the <see cref="SettingsStore"/>.
    /// </summary>
    /// <remarks>
    ///             The instance of the <see cref="BaseOptionModel{T}"/> provides the default collection path that is used 
    ///             to store the property values. Adding <see cref="OverrideCollectionNameAttribute"/> to the property will
    ///             override this only for the attributed property. This also will infer the proper data type to store 
    ///             the property value as for common types to avoid serialization. See remarks at
    ///             <see cref="ConvertPropertyTypeToStorageType{T}"/> for specifics.
    ///             <para/> 
    ///             For types not supported by default, the overridable serialization methods in <see cref="BaseOptionModel{T}"/> 
    ///             will be used, and the output of that will be stored. Alternatively, you can apply the <see cref="OverrideDataTypeAttribute"/>
    ///             specifying the storage type/methodology. By default, that will use <see cref="Convert.ChangeType(object,Type, IFormatProvider)"/> 
    ///             with <see cref="CultureInfo.InvariantCulture"/>, though that attribute can also include a flag to use the type's 
    ///             <see cref="System.ComponentModel.TypeConverterAttribute"/> instead.
    ///             <para/>
    ///             The implementation of this class uses reflection, only in the ctor, to create an open delegate that is used
    ///             to get and set the property values. This initial hit using reflection happens once, and subsequent load
    ///             and saves of the value are therefore as performant as possible. This is the technique used by
    ///             <a href="https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/">Jon Skeet
    ///             in Google's Protocol Buffers</a>.
    /// </remarks>
    public class OptionModelPropertyWrapper : IOptionModelPropertyWrapper
    {
        #region Static Initialization

        /// <summary>   (Immutable) Dictionary of types, limited to the types available in <see cref="SettingsStore"/>, to a
        ///             delegate with a signature of <c>WritableSettingsStore targetSettingsStore, string collectionPath,
        ///             string propertyPath, object value</c>. This is initialized in the static ctor. </summary>
        protected static IReadOnlyDictionary<NativeSettingsType, Action<WritableSettingsStore, string, string, object>> SettingStoreSetMethodsDict { get; }

        /// <summary>   (Immutable) Dictionary of types, limited to the types available in <see cref="SettingsStore"/>, to a 
        ///             delegate with a signature of <c>SettingsStore targetSettingsStore, string collectionPath,
        ///             string propertyPath</c>, which returns the value of the setting as an object. This is initialized 
        ///             in the static ctor. </summary>
        protected static IReadOnlyDictionary<NativeSettingsType, Func<SettingsStore, string, string, object>> SettingStoreGetMethodsDict { get; }

        /// <summary>   One-time static initialization of delegates to interact with the <see cref="SettingsStore"/> and <see cref="WritableSettingsStore"/>. </summary>
        static OptionModelPropertyWrapper()
        {
            Dictionary<NativeSettingsType, Action<WritableSettingsStore, string, string, object>> settingStoreSetMethodsDict = new(7);
            SettingStoreSetMethodsDict = settingStoreSetMethodsDict;

            Dictionary<NativeSettingsType, Func<SettingsStore, string, string, object>> settingStoreGetMethodsDict = new(7);
            SettingStoreGetMethodsDict = settingStoreGetMethodsDict;

            Type typeOfWritableSettingsStore = typeof(WritableSettingsStore);
            settingStoreSetMethodsDict[NativeSettingsType.String] = CreateSettingsStoreSetMethod<string>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetString), new[] { typeof(string), typeof(string), typeof(string) }));
            settingStoreSetMethodsDict[NativeSettingsType.Int32] = CreateSettingsStoreSetMethod<int>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetInt32), new[] { typeof(string), typeof(string), typeof(int) }));
            settingStoreSetMethodsDict[NativeSettingsType.UInt32] = CreateSettingsStoreSetMethod<uint>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetUInt32), new[] { typeof(string), typeof(string), typeof(uint) }));
            settingStoreSetMethodsDict[NativeSettingsType.Int64] = CreateSettingsStoreSetMethod<long>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetInt64), new[] { typeof(string), typeof(string), typeof(long) }));
            settingStoreSetMethodsDict[NativeSettingsType.UInt64] = CreateSettingsStoreSetMethod<ulong>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetUInt64), new[] { typeof(string), typeof(string), typeof(ulong) }));
            settingStoreSetMethodsDict[NativeSettingsType.Binary] = CreateSettingsStoreSetMethod<System.IO.MemoryStream>(typeOfWritableSettingsStore.GetMethod(nameof(WritableSettingsStore.SetMemoryStream), new[] { typeof(string), typeof(string), typeof(System.IO.MemoryStream) }));

            Type typeOfSettingsStore = typeof(SettingsStore);
            settingStoreGetMethodsDict[NativeSettingsType.String] = CreateSettingsStoreGetMethod<string>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetString), new[] { typeof(string), typeof(string) }));
            settingStoreGetMethodsDict[NativeSettingsType.Int32] = CreateSettingsStoreGetMethod<int>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetInt32), new[] { typeof(string), typeof(string) }));
            settingStoreGetMethodsDict[NativeSettingsType.UInt32] = CreateSettingsStoreGetMethod<uint>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetUInt32), new[] { typeof(string), typeof(string) }));
            settingStoreGetMethodsDict[NativeSettingsType.Int64] = CreateSettingsStoreGetMethod<long>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetInt64), new[] { typeof(string), typeof(string) }));
            settingStoreGetMethodsDict[NativeSettingsType.UInt64] = CreateSettingsStoreGetMethod<ulong>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetUInt64), new[] { typeof(string), typeof(string) }));
            settingStoreGetMethodsDict[NativeSettingsType.Binary] = CreateSettingsStoreGetMethod<System.IO.MemoryStream>(typeOfSettingsStore.GetMethod(nameof(SettingsStore.GetMemoryStream), new[] { typeof(string), typeof(string) }));
        }

        /// <summary>   Creates a <c>delegate</c> to the settings store that sets a value in the settings store. The delegate
        ///             exposes a common signature that intentionally makes the type to be set an <c>object</c> to simplify code. </summary>
        /// <typeparam name="T">    The actual type being stored in the <see cref="WritableSettingsStore"/>. </typeparam>
        /// <param name="mi">   The method info of the typed set method for the <see cref="WritableSettingsStore"/>. </param>
        /// <returns>   The delegate to set a value, as described above. </returns>
        private static Action<WritableSettingsStore, string, string, object> CreateSettingsStoreSetMethod<T>(MethodInfo mi)
        {
            Action<WritableSettingsStore, string, string, T> action = (Action<WritableSettingsStore, string, string, T>)Delegate.CreateDelegate(typeof(Action<WritableSettingsStore, string, string, T>), mi, true)!;
            return delegate (WritableSettingsStore settingsStore, string collectionName, string propertyName, object value)
            {
                action(settingsStore, collectionName, propertyName, (T)value);
            };
        }

        /// <summary>   Creates a <c>delegate</c> to the settings store that gets a value from the settings store. The delegate
        ///             exposes a common signature that intentionally makes the return type an <c>object</c> to simplify code. </summary>
        /// <typeparam name="T">    The actual type that is stored in the <see cref="SettingsStore"/>. </typeparam>
        /// <param name="mi">   The method info of the typed get method for the <see cref="SettingsStore"/>. </param>
        /// <returns>   The delegate to get a value, as described above. </returns>
        private static Func<SettingsStore, string, string, object> CreateSettingsStoreGetMethod<T>(MethodInfo mi)
        {
            Func<SettingsStore, string, string, T> func = (Func<SettingsStore, string, string, T>)Delegate.CreateDelegate(typeof(Func<SettingsStore, string, string, T>), mi, true)!;
            return (settingsStore, collectionName, propertyName) => func(settingsStore, collectionName, propertyName)!;
        }

        #endregion

        #region Construction and initialization

        /// <summary>   Initializes a new instance of the class. </summary>
        /// <param name="propertyInfo"> The property being wrapped. </param>
        public OptionModelPropertyWrapper(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            PropertyName = propertyInfo.Name;
            WrappedPropertySetMethod = CreateWrappedPropertySetDelegate(propertyInfo);
            WrappedPropertyGetMethod = CreateWrappedPropertyGetDelegate(propertyInfo);

            OverrideDataTypeAttribute? overrideDataTypeAttribute = null;
            foreach (Attribute attribute in propertyInfo.GetCustomAttributes())
            {
                if (attribute is OverrideCollectionNameAttribute collectionNameAttribute)
                {
                    string collectionName = collectionNameAttribute.CollectionName.Trim();
                    if (collectionName.Length > 0)
                        OverrideCollectionName = collectionName;
                }
                else if (attribute is OverridePropertyNameAttribute overridePropertyNameAttribute)
                {
                    string propertyName = overridePropertyNameAttribute.PropertyName.Trim();
                    if (propertyName.Length > 0)
                        PropertyName = propertyName;
                }
                else if (attribute is OverrideDataTypeAttribute odt)
                {
                    overrideDataTypeAttribute = odt;
                }
            }

            if (overrideDataTypeAttribute != null)
            {
                DataType = overrideDataTypeAttribute.SettingDataType;
                if (overrideDataTypeAttribute.UseTypeConverter && DataType != SettingDataType.Legacy && DataType != SettingDataType.Serialized)
                    TypeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
            }
            else
            {
                DataType = InferDataType(propertyInfo.PropertyType);
            }

            NativeStorageType = GetNativeSettingsType(DataType);
            SettingStoreSetMethod = SettingStoreSetMethodsDict[NativeStorageType];
            SettingStoreGetMethod = SettingStoreGetMethodsDict[NativeStorageType];
        }

        /// <summary>   Creates a delegate that can get the value of a property with object signatures. This is
        ///             for both performance reasons and ease of implementation as types are not known until runtime. </summary>
        /// <param name="propertyInfo">   The property for which to create the delegate. </param>
        /// <returns>   A delegate as described above. </returns>
        protected static Func<object, object?> CreateWrappedPropertyGetDelegate(PropertyInfo propertyInfo)
        {
            // First fetch the generic form
            MethodInfo? genericHelper = typeof(OptionModelPropertyWrapper).GetMethod(nameof(PropertyGetHelper), BindingFlags.Static | BindingFlags.NonPublic);
            if (genericHelper == null)
                throw new InvalidOperationException($"Could not get method {nameof(PropertyGetHelper)}");

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { propertyInfo.GetGetMethod(false) });

            // Cast the result to the right kind of delegate and return it
            return (Func<object, object?>)ret;
        }

        /// <summary>   Gets a delegate that ultimately will get value of a property. The real types are not known at compile-time,
        ///             so this is called via reflection. The returned delegate has the signature using objects, which at runtime 
        ///             are cast to the proper types. </summary>
        /// <typeparam name="TTarget">  Type for the target object on which the property get method will be called. </typeparam>
        /// <typeparam name="TReturn">  Type returned by the property get method. </typeparam>
        /// <param name="method">   The property get method. </param>
        /// <returns>   A delegate as described above. </returns>
        private static Func<object, object?> PropertyGetHelper<TTarget, TReturn>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TReturn> func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            return (object target) => func((TTarget)target);
        }

        /// <summary>   Creates a delegate that can set the value of a property with object signatures. This is
        ///             for both performance reasons and ease of implementation as types are not known until runtime. </summary>
        /// <param name="propertyInfo">   The property for which to create the delegate. </param>
        /// <returns>   A delegate as described above. </returns>
        protected static Action<object, object?> CreateWrappedPropertySetDelegate(PropertyInfo propertyInfo)
        {
            // First fetch the generic form
            MethodInfo? genericHelper = typeof(OptionModelPropertyWrapper).GetMethod(nameof(PropertySetHelper), BindingFlags.Static | BindingFlags.NonPublic);
            if (genericHelper == null)
                throw new InvalidOperationException($"Could not get method {nameof(PropertySetHelper)}");

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { propertyInfo.GetSetMethod(false) });

            // Cast the result to the right kind of delegate and return it
            return (Action<object, object?>)ret;
        }

        /// <summary>   Gets a delegate that ultimately will set value of a property. The real types are not known at compile-time,
        ///             so this is called via reflection. The returned delegate has the signature using objects, which at runtime 
        ///             are cast to the proper types. </summary>
        /// <typeparam name="TTarget">  Type for the target object on which the property set method will be called. </typeparam>
        /// <typeparam name="TParam">  Type expected by the property set method. </typeparam>
        /// <param name="method">   The property set method. </param>
        /// <returns>   A delegate as described above. </returns>
        private static Action<object, object?> PropertySetHelper<TTarget, TParam>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Action<TTarget, TParam?> action = (Action<TTarget, TParam?>)Delegate.CreateDelegate
                (typeof(Action<TTarget, TParam?>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            return (object target, object? value) => action((TTarget)target, (TParam?)value);
        }

        #endregion

        /// <summary>   (Immutable) The property being wrapped. </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>   (Immutable) Either specified via <see cref="OverrideDataTypeAttribute"/> or inferred from the
        ///             <see cref="System.Reflection.PropertyInfo.PropertyType"/> of the wrapped property in the 
        ///             <see cref="InferDataType"/> method. This serves a dual purpose - it specifies how the 
        ///             wrapped value is converted to the storage type as well as the native type that is stored. </summary>
        protected SettingDataType DataType { get; }

        /// <summary>   (Immutable) A delegate to the method to set a value in <see cref="WritableSettingsStore"/>.
        ///             This delegate signature is <c>WritableSettingsStore settingsStore, string collectionPath, 
        ///             string propertyPath, object value</c>. </summary>
        protected Action<WritableSettingsStore, string, string, object> SettingStoreSetMethod { get; }

        /// <summary>   (Immutable) A delegate to the method to get a value from the <see cref="SettingsStore"/>.
        ///             This delegate signature is <c>SettingsStore settingsStore, string collectionPath, 
        ///             string propertyPath</c> and returns the value stored as an object. </summary>
        protected Func<SettingsStore, string, string, object> SettingStoreGetMethod { get; }

        /// <summary>   (Immutable) A delegate to set the value of the wrapped property from the <see cref="BaseOptionModel{T}"/> instance.
        ///             These are explicitly object types in the signature but must be of the proper types when they are called. The 
        ///             signature is <c>BaseOptionModel{T} targetObject, object value</c>, where the type of <c>value</c> must be  
        ///             assignable to the <see cref="System.Reflection.PropertyInfo.PropertyType"/> of the wrapped property. </summary>
        protected Action<object, object?> WrappedPropertySetMethod { get; }

        /// <summary>   (Immutable) A delegate to get the value of the wrapped property from the <see cref="BaseOptionModel{T}"/> instance.
        ///             These are explicitly object types in the signature but must be of the proper types when they are called. The
        ///             signature is <c>BaseOptionModel{T} targetObject</c>, where the type that is returned will be the
        ///             <see cref="System.Reflection.PropertyInfo.PropertyType"/> of the wrapped property. </summary>
        protected Func<object, object?> WrappedPropertyGetMethod { get; }

        /// <summary>   (Immutable) If not null the <c>CollectionPath</c> the value of this property should be loaded/saved to,
        ///             which is set via the optional <see cref="OverrideCollectionNameAttribute"/> on the property.
        ///             If null, the <see cref="BaseOptionModel{T}.CollectionName"/> should be used instead. </summary>
        protected string? OverrideCollectionName { get; }

        /// <summary>   (Immutable) The <c>PropertyName</c> in the <see cref="SettingsStore"/> where the value of this property 
        ///             is stored. By default, this is the actual name of the property that this instance wraps. This can be 
        ///             overridden via the optional <see cref="OverridePropertyNameAttribute"/> on the property. </summary>
        protected string PropertyName { get; }

        /// <summary>   (Immutable) The data type the property will be stored as, which is limited to the types available
        ///             in <see cref="SettingsStore"/>. Set via <see cref="GetNativeSettingsType"/>. See also the summary of 
        ///             <see cref="DataType"/>. </summary>
        protected NativeSettingsType NativeStorageType { get; }

        /// <summary>   (Immutable) If <see cref="OverrideDataTypeAttribute"/> is applied, and the <see cref="PropertyInfo"/> 
        ///             <c>PropertyType</c> has a <see cref="TypeConverterAttribute"/> applied that is compatible with its 
        ///             declared storage data type, this <see cref="System.ComponentModel.TypeConverter"/> will be non-null and used to convert
        ///             the property value to and from the <see cref="SettingsStore"/> <see cref="NativeStorageType"/>.</summary>
        protected TypeConverter? TypeConverter { get; }

        /// <summary> Serialize using <see cref="BinaryFormatter"/>, then convert to a base64 string for storage. Returning an 
        ///           empty string represents a null object. </summary>
        /// <param name="value">        The object that is to be serialized. Can Be Null. </param>
        internal static string LegacySerializeValue(object? value)
        {
            if (value == null)
                return string.Empty;
            using (MemoryStream stream = new())
            {
                BinaryFormatter formatter = new();
                formatter.Serialize(stream, value);
                stream.Flush();
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary> Convert base64 encoded string, then deserialize using <see cref="BinaryFormatter"/>. </summary>
        /// <param name="serializedString">        The base64 encoded string that was serialized by <see cref="BinaryFormatter"/>.
        ///                                        An empty string represents a null object.</param>
        /// <param name="conversionType">          The type to deserialize as.</param>
        internal static object? LegacyDeserializeValue(string serializedString, Type conversionType)
        {
            if (serializedString.Length == 0)
            {
                if (conversionType.IsValueType)
                    return Activator.CreateInstance(conversionType);
                return null;
            }
            byte[] b = Convert.FromBase64String(serializedString);
            using (MemoryStream stream = new(b))
            {
                BinaryFormatter formatter = new();
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>   The value of the wrapped property is retrieved by calling the property get method on <paramref name="baseOptionModel"/>.
        ///             This value is converted or serialized to a native type supported by the <paramref name="settingsStore"/>, 
        ///             then persisted to the store, assuring the collection exists first. No exceptions should be thrown from
        ///             this method. </summary>
        /// <typeparam name="TOptMdl">  Type of the base option model. </typeparam>
        /// <param name="baseOptionModel">  The base option model which is used as the target object from which the property 
        ///                                 value will be retrieved. It also can be used for serialization of stored data.  </param>
        /// <param name="settingsStore">    The settings store to set the setting value in. </param>
        /// <returns>   True if we were able to persist the value in the store. However, if the serialization results in a null value,
        ///             it cannot be persisted in the settings store and false will be returned. False is also returned if any step 
        ///             of the process failed, and these are logged. </returns>
        public virtual bool Save<TOptMdl>(BaseOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            string collectionName = OverrideCollectionName ?? baseOptionModel.CollectionName;
            object? value = null;
            try
            {
                value = WrappedPropertyGetMethod(baseOptionModel);

                value = ConvertPropertyTypeToStorageType(value, baseOptionModel);

                if (value == null)
                {
                    Exception ex = new("Cannot store null in settings store.");
                    ex.LogAsync("BaseOptionModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}",
                        baseOptionModel.GetType().FullName, nameof(Load), collectionName, PropertyName, DataType, PropertyInfo.PropertyType,
                        value ?? "[NULL]").Forget();
                    return false;
                }

                // Rather than if ! CollectionExists then CreateCollection this is likely more efficient.
                settingsStore.CreateCollection(collectionName);
                SettingStoreSetMethod(settingsStore, collectionName, PropertyName, value);

                return true;
            }
            catch (Exception ex)
            {
                ex.Log("BaseOptionModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}",
                    baseOptionModel.GetType().FullName, nameof(Load), collectionName, PropertyName, DataType, PropertyInfo.PropertyType,
                    value ?? "[NULL]");
            }

            return false;
        }

        /// <summary>   If the setting is found in the <paramref name="settingsStore"/>, retrieves the value of the setting, 
        ///             converts or deserializes it to the type of the wrapped property, and calls the property set method on 
        ///             the <paramref name="baseOptionModel"/>. No exceptions should be thrown from
        ///             this method. No changes to the property will be made if the setting does not exist. </summary>
        /// <typeparam name="TOptMdl">  Type of the base option model. </typeparam>
        /// <param name="baseOptionModel">  The base option model which is used as the target object on which the property 
        ///                                 will be set. It also can be used for deserialization of stored data.  </param>
        /// <param name="settingsStore">    The settings store to retrieve the setting value from. </param>
        /// <returns>   True if the value exists in the <paramref name="settingsStore"/>, and the property was updated in
        ///             <paramref name="baseOptionModel"/>, false if setting does not exist or any step of the process 
        ///             failed. </returns>
        public virtual bool Load<TOptMdl>(BaseOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            string collectionName = OverrideCollectionName ?? baseOptionModel.CollectionName;

            object? value = null;
            try
            {
                if (!settingsStore.PropertyExists(collectionName, PropertyName))
                    return false;

                value = SettingStoreGetMethod(settingsStore, collectionName, PropertyName);
                value = ConvertStorageTypeToPropertyType(value, baseOptionModel);
                WrappedPropertySetMethod(baseOptionModel, value);
                return true;
            }
            catch (Exception ex)
            {
                ex.Log("BaseOptionModel<{0}>.{1} CollectionName:{2} PropertyName:{3} dataType:{4} PropertyType:{5} Value:{6}",
                    baseOptionModel.GetType().FullName, nameof(Load), collectionName, PropertyName, DataType, PropertyInfo.PropertyType,
                    value ?? "[NULL]");
            }

            return false;
        }

        #region Type inference and conversion

        /// <summary>   Gets the native data type that the property value will be stored as in the <see cref="SettingsStore"/>. </summary>
        /// <param name="settingDataType">  The type/mechanism by which the value will be converted for storage. </param>
        /// <returns>   The native data type that the property value will be stored as in the <see cref="SettingsStore"/>. </returns>
        protected virtual NativeSettingsType GetNativeSettingsType(SettingDataType settingDataType)
        {
            switch (settingDataType)
            {
                case SettingDataType.Legacy:
                case SettingDataType.String:
                case SettingDataType.Serialized:
                    return NativeSettingsType.String;
                case SettingDataType.Int32:
                    return NativeSettingsType.Int32;
                case SettingDataType.UInt32:
                    return NativeSettingsType.UInt32;
                case SettingDataType.Int64:
                    return NativeSettingsType.Int64;
                case SettingDataType.UInt64:
                    return NativeSettingsType.UInt64;
                case SettingDataType.Binary:
                    return NativeSettingsType.Binary;
                default:
                    throw new InvalidOperationException($"GetNativeDataType for SettingDataType {settingDataType} is not supported.");
            }
        }

        /// <summary>   Infers the underlying type (or mechanism for unknown types) by which we will set the native data type. See remarks at 
        /// <see cref="ConvertPropertyTypeToStorageType{T}"/> </summary>
        /// <param name="propertyType">   The type of the property being wrapped. </param>
        /// <returns>   A SettingDataType. </returns>
        protected virtual SettingDataType InferDataType(Type propertyType)
        {
            // Enums can be any integral type, so if we are an enum, get the integral type and use that.
            if (propertyType.IsEnum)
                propertyType = propertyType.GetEnumUnderlyingType();

            if (propertyType == typeof(string) || propertyType == typeof(float) || propertyType == typeof(double) ||
                propertyType == typeof(decimal) || propertyType == typeof(char) || propertyType == typeof(Guid) ||
                propertyType == typeof(DateTimeOffset))
                return SettingDataType.String;
            if (propertyType == typeof(bool) || propertyType == typeof(sbyte) || propertyType == typeof(byte) ||
                propertyType == typeof(short) || propertyType == typeof(ushort) ||
                propertyType == typeof(int) || propertyType == typeof(Color))
                return SettingDataType.Int32;
            if (propertyType == typeof(uint))
                return SettingDataType.UInt32;
            if (propertyType == typeof(long) || propertyType == typeof(DateTime))
                return SettingDataType.Int64;
            if (propertyType == typeof(ulong))
                return SettingDataType.UInt64;
            if (propertyType == typeof(byte[]) || propertyType == typeof(MemoryStream))
                return SettingDataType.Binary;

            // Use the serializer from BaseOptionModel for types unknown to us.
            return SettingDataType.Serialized;
        }

        /// <summary>   Convert the <paramref name="propertyValue"/> retrieved from the property to the type it will be stored as in the
        ///             <see cref="SettingsStore"/>. </summary>
        /// <typeparam name="TOptMdl">  Type of <see cref="BaseOptionModel{TOptMdl}"/>. </typeparam>
        /// <param name="propertyValue">        The value retrieved from the wrapped property, as an object. </param>
        /// <param name="baseOptionModel">      Instance of <see cref="BaseOptionModel{TOptMdl}"/>. For types requiring serialization, methods in this object are used. </param>
        /// <returns>   <paramref name="propertyValue"/>, converted to one of the types supported by <see cref="SettingsStore"/>. </returns>
        /// <remarks>
        /// The methods <see cref="ConvertPropertyTypeToStorageType{T}" />, <see cref="ConvertStorageTypeToPropertyType{T}" />, and <see cref="InferDataType"/> are designed to
        /// work in tandem, and are therefore tightly coupled. The <see cref="SettingsStore"/> cannot store null values, therefore any property that is converted 
        /// to a reference type cannot round-trip successfully if that conversion yields <see cref="string"/>, <see cref="MemoryStream"/>, and arrays of 
        /// <see cref="byte"/> - in these cases the equivalent of <c>empty</c> is stored, therefore when loaded the result will not match.
        /// <para />
        /// The method <see cref="InferDataType"/> returns an enumeration that identifies both the native storage type, and method of conversion, that 
        /// will be used when storing the property value. These defaults can be overridden via the <see cref="OverrideDataTypeAttribute"/>.
        /// <para />
        /// The method <see cref="ConvertPropertyTypeToStorageType{T}" /> is provided the current value of the property. It's job is to convert this value to
        /// the native storage type based on <see cref="DataType"/> which is set via <see cref="InferDataType"/>.
        /// <para />
        /// The method <see cref="ConvertStorageTypeToPropertyType{T}" /> is the reverse of the above. Given an instance of the native storage type,
        /// it's job is to convert it to an instance the property type.
        /// <para />
        /// The conversions between types in the default implementation follows this:
        /// <list type="bullet">
        ///  <item> <description>A property with a setting data type of <see cref="SettingDataType.Legacy"/> uses <see cref="BinaryFormatter"/> and stores it as a base64 encoded string. <see langword="null"/> values are stored as an empty string. </description></item>
        ///  <item> <description>Array of <see cref="byte"/> is wrapped in a <see cref="MemoryStream"/>. <see langword="null"/> values are converted to an empty <see cref="MemoryStream"/>.</description></item>
        ///  <item> <description><see cref="Color"/>, with setting data type <see cref="SettingDataType.Int32"/> uses To[From]Argb to store it as an Int32.</description></item>
        ///  <item> <description><see cref="Guid"/>, with setting data type <see cref="SettingDataType.String"/> uses <see cref="Guid.ToString()"/> and <see cref="Guid.Parse"/> to convert to and from a string.</description></item>
        ///  <item> <description><see cref="DateTime"/>, with setting data type <see cref="SettingDataType.Int64"/> uses To[From]Binary to store it as an Int64.</description></item>
        ///  <item> <description><see cref="DateTimeOffset"/>, with setting data type <see cref="SettingDataType.String"/> uses the round-trip 'o' specifier to store as a string.</description></item>
        ///  <item> <description><see cref="float"/> and <see cref="double"/>, with setting data type <see cref="SettingDataType.String"/> uses the round-trip 'G9' and 'G17' specifier to store as a string, and is parsed via the standard Convert method.</description></item>
        ///  <item> <description><see cref="string"/>, if null, is stored as an empty string.</description></item>
        ///  <item> <description>Enumerations are converted to/from their underlying type.</description></item>
        ///  <item> <description><a href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types">Integral numeric types</a>,
        ///                      <see cref="float"/>, <see cref="double"/>, <see cref="decimal"/>, and <see cref="char"/>
        ///                      use <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />, using <see cref="CultureInfo.InvariantCulture"/>. Enumerations are
        ///                      stored as their underlying integral numeric type.</description></item>
        ///  <item> <description>Any type not described above, or a property with a setting data type of <see cref="SettingDataType.Serialized"/>
        ///                      uses <see cref="BaseOptionModel{T}.SerializeValue"/> and <see cref="BaseOptionModel{T}.DeserializeValue"/> and stores it as binary,
        ///                      refer to those overridable methods for details.</description></item>
        /// </list>
        /// </remarks>
        protected virtual object ConvertPropertyTypeToStorageType<TOptMdl>(object? propertyValue, BaseOptionModel<TOptMdl> baseOptionModel) where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            switch (DataType)
            {
                case SettingDataType.Serialized:
                    if (NativeStorageType != NativeSettingsType.String)
                        throw new InvalidOperationException($"The SettingDataType of Serialized is not capable of supporting native storage type {NativeStorageType}");
                    string serializedString = baseOptionModel.SerializeValue(propertyValue, PropertyInfo.PropertyType, PropertyName);
                    if (serializedString == null)
                        throw new InvalidOperationException($"The SerializeValue method of {baseOptionModel.GetType().FullName} returned " +
                            " a null value. This method cannot return null.");
                    return serializedString;
                case SettingDataType.Legacy:
                    if (NativeStorageType != NativeSettingsType.String)
                        throw new InvalidOperationException($"The SettingDataType of Legacy is not capable of supporting native storage type {NativeStorageType}");
                    return LegacySerializeValue(propertyValue);
            }

            Type conversionType = NativeStorageType.GetDotNetType();
            if (TypeConverter != null)
            {
                bool returnMemoryStream = false;
                if (NativeStorageType == NativeSettingsType.Binary)
                {
                    // For binary, the type conversion should be to byte[], then we return a memory stream.
                    returnMemoryStream = true;
                    conversionType = typeof(byte[]);
                }

                if (!TypeConverter.CanConvertTo(conversionType))
                    throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} can not convert {PropertyInfo.PropertyType.FullName} to {NativeStorageType} ({conversionType.Name})");

                object? convertedObj = TypeConverter.ConvertTo(null, CultureInfo.InvariantCulture, propertyValue, conversionType);
                if (convertedObj == null)
                    throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} returned null converting from {PropertyInfo.PropertyType.FullName} to {NativeStorageType} ({conversionType.Name}), which is not supported.");
                if (!conversionType.IsInstanceOfType(convertedObj))
                    throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} returned type {convertedObj.GetType().FullName} when converting from {PropertyInfo.PropertyType.FullName} to {NativeStorageType} ({conversionType.Name}).");
                if (returnMemoryStream)
                    return new MemoryStream((byte[])convertedObj);
                return convertedObj;
            }

            switch (NativeStorageType)
            {
                case NativeSettingsType.Int32:
                    if (propertyValue is Color color)
                        return color.ToArgb();
                    break;
                case NativeSettingsType.Int64:
                    if (propertyValue is DateTime dt)
                        return dt.ToBinary();
                    break;
                case NativeSettingsType.String:
                    if (propertyValue is Guid guid)
                        return guid.ToString();
                    if (propertyValue is DateTimeOffset dtOffset)
                        return dtOffset.ToString("o", CultureInfo.InvariantCulture);
                    if (propertyValue is float floatVal)
                        return floatVal.ToString("G9", CultureInfo.InvariantCulture);
                    if (propertyValue is double doubleVal)
                        return doubleVal.ToString("G17", CultureInfo.InvariantCulture);
                    if (propertyValue == null)
                        return string.Empty;
                    break;
                case NativeSettingsType.Binary:
                    if (propertyValue is byte[] bytes)
                        return new MemoryStream(bytes);
                    if (propertyValue is MemoryStream memStream)
                        return memStream;
                    if (propertyValue == null)
                        return new MemoryStream();
                    throw new InvalidOperationException($"Can not convert NativeStorageType of Binary to {propertyValue.GetType().FullName} - property type must be byte[] or MemoryStream.");
            }

            if (propertyValue == null)
                throw new InvalidOperationException($"A null property value with SettingDataType of {DataType} is not supported.");

            if (conversionType.IsInstanceOfType(propertyValue))
                return propertyValue;

            return Convert.ChangeType(propertyValue, conversionType, CultureInfo.InvariantCulture);
        }

        /// <summary>   Convert the <paramref name="settingsStoreValue"/> retrieved from the settings store to the type of the
        /// property we are wrapping. See remarks at <see cref="ConvertPropertyTypeToStorageType{T}"/></summary>
        /// <typeparam name="TOptMdl">  Type of <see cref="BaseOptionModel{TOptMdl}"/>. </typeparam>
        /// <param name="settingsStoreValue">                The value retrieved from the settings store, as an object. This will not be null. </param>
        /// <param name="baseOptionModel">      Instance of <see cref="BaseOptionModel{TOptMdl}"/>. For types requiring deserialization, methods in this object are used. </param>
        /// <returns>   <paramref name="settingsStoreValue"/>, converted to the property type. </returns>
        protected virtual object? ConvertStorageTypeToPropertyType<TOptMdl>(object settingsStoreValue, BaseOptionModel<TOptMdl> baseOptionModel) where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            Type typeOfWrappedProperty = PropertyInfo.PropertyType;
            if (typeOfWrappedProperty.IsEnum)
                typeOfWrappedProperty = typeOfWrappedProperty.GetEnumUnderlyingType();

            switch (DataType)
            {
                case SettingDataType.Serialized:
                    if (NativeStorageType != NativeSettingsType.String)
                        throw new InvalidOperationException($"The SettingDataType of Serialized must be SettingsType.String. Was: {NativeStorageType}");
                    return baseOptionModel.DeserializeValue((string)settingsStoreValue, typeOfWrappedProperty, PropertyName);
                case SettingDataType.Legacy:
                    if (NativeStorageType != NativeSettingsType.String)
                        throw new InvalidOperationException($"The SettingDataType of Legacy must be SettingsType.String. Was: {NativeStorageType}");
                    return LegacyDeserializeValue((string)settingsStoreValue, typeOfWrappedProperty);
            }

            if (TypeConverter != null)
            {
                Type valueType = settingsStoreValue.GetType();
                if (NativeStorageType == NativeSettingsType.Binary)
                {
                    // Type converter uses byte[] so extract byte array and set the conversion type.
                    valueType = typeof(byte[]);
                    settingsStoreValue = ((MemoryStream)settingsStoreValue).ToArray();
                }
                if (!TypeConverter.CanConvertFrom(valueType))
                    throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} can not convert from {valueType.Name} to {typeOfWrappedProperty.FullName}.");

                object? returnObject = TypeConverter.ConvertFrom(null!, CultureInfo.InvariantCulture, settingsStoreValue);
                if (returnObject == null)
                {
                    if (typeOfWrappedProperty.IsValueType)
                        throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} attempt to convert from {valueType.Name} to {typeOfWrappedProperty.FullName} returned null for a value type.");
                    return returnObject;
                }
                if (!typeOfWrappedProperty.IsInstanceOfType(returnObject))
                    throw new InvalidOperationException($"TypeConverter {TypeConverter.GetType().FullName} attempt to convert from {valueType.Name} to {typeOfWrappedProperty.FullName} returned incompatible type {returnObject.GetType().FullName}.");
                return returnObject;
            }

            switch (NativeStorageType)
            {
                case NativeSettingsType.Int32:
                    if (typeOfWrappedProperty == typeof(Color))
                        return Color.FromArgb((int)settingsStoreValue);
                    break;
                case NativeSettingsType.Int64:
                    if (typeOfWrappedProperty == typeof(DateTime))
                        return DateTime.FromBinary((long)settingsStoreValue);
                    break;
                case NativeSettingsType.String:
                    if (typeOfWrappedProperty == typeof(Guid))
                        return Guid.Parse((string)settingsStoreValue);
                    if (typeOfWrappedProperty == typeof(DateTimeOffset))
                        return DateTimeOffset.Parse((string)settingsStoreValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    break;
                case NativeSettingsType.Binary:
                    if (typeOfWrappedProperty == typeof(MemoryStream))
                        return (MemoryStream)settingsStoreValue;
                    if (typeOfWrappedProperty == typeof(byte[]))
                        return ((MemoryStream)settingsStoreValue).ToArray();
                    throw new InvalidCastException($"Can not convert SettingsType.Binary to {typeOfWrappedProperty.FullName} - property type must be byte[] or MemoryStream.");
            }

            if (typeOfWrappedProperty.IsInstanceOfType(settingsStoreValue))
                return settingsStoreValue;

            return Convert.ChangeType(settingsStoreValue, typeOfWrappedProperty, CultureInfo.InvariantCulture);
        }

        #endregion Type inference and conversion
    }
}
