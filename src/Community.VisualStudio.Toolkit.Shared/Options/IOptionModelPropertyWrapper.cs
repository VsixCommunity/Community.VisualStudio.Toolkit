using Microsoft.VisualStudio.Settings;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>   Implementation should wrap an instance property member with public getter and setters from a <see cref="BaseOptionModel{T}"/>,
    ///             and expose the ability to load and save the value of the property to the <see cref="SettingsStore"/>.
    /// </summary>
    public interface IOptionModelPropertyWrapper
    {
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
        bool Load<TOptMdl>(BaseOptionModel<TOptMdl> baseOptionModel, SettingsStore settingsStore) where TOptMdl : BaseOptionModel<TOptMdl>, new();

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
        bool Save<TOptMdl>(BaseOptionModel<TOptMdl> baseOptionModel, WritableSettingsStore settingsStore) where TOptMdl : BaseOptionModel<TOptMdl>, new();
    }
}