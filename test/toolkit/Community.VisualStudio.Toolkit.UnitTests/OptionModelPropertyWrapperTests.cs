using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.Settings;
using NSubstitute;
using NSubstitute.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    public enum TestValueType
    {
        Default,
        Alternate,
    }

    public class OptionModelPropertyWrapperTests
    {
        public const string TestCollectionName = "MyCollectionName";
        public const string OverrideCollectionName = "OverrideCollectionName";
        public const string OverridePropertyName = "OverridePropertyName";

        private readonly ITestOutputHelper _output;

        public OptionModelPropertyWrapperTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PropertyTypeString()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.String));

            string defaultValueProperty = "default";
            string expectedDefaultValueInStore = defaultValueProperty;

            string expectedAlternateValueProperty = "loaded";
            string expectedAlternateValueInStore = expectedAlternateValueProperty;

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.String, s => objUt.String = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeString_Null()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.String));

            // A null string is stored as an empty string, it does not round-trip.
            string? defaultValueProperty = null;
            string expectedDefaultValueInStore = string.Empty;

            string expectedAlternateValueProperty = "loaded";
            string expectedAlternateValueInStore = expectedAlternateValueProperty;

            void OverrideAssertEquality(TestValueType valueBeingTested, string? expected, string? actual, string because)
            {
                if (valueBeingTested == TestValueType.Default)
                    actual.Should().BeEquivalentTo(string.Empty, because + " A null string should roundtrip as an empty string");
                else
                    actual.Should().BeEquivalentTo(expected, because);
            }

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.String, s => objUt.String = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore,
                OverrideAssertEquality);
        }

        [Fact]
        public void PropertyTypeFloat()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Float));

            float defaultValueProperty = float.MinValue;
            string expectedDefaultValueInStore = defaultValueProperty.ToString("G9", CultureInfo.InvariantCulture);

            float expectedAlternateValueProperty = float.MaxValue;
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString("G9", CultureInfo.InvariantCulture);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Float, s => objUt.Float = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeDouble()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Double));

            double defaultValueProperty = double.MinValue;
            string expectedDefaultValueInStore = defaultValueProperty.ToString("G17", CultureInfo.InvariantCulture);

            double expectedAlternateValueProperty = double.MaxValue;
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString("G17", CultureInfo.InvariantCulture);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Double, s => objUt.Double = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeDecimal()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Decimal));

            decimal defaultValueProperty = decimal.MinValue;
            string expectedDefaultValueInStore = defaultValueProperty.ToString(CultureInfo.InvariantCulture);

            decimal expectedAlternateValueProperty = decimal.MaxValue;
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString(CultureInfo.InvariantCulture);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Decimal, s => objUt.Decimal = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeChar()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Char));

            char defaultValueProperty = 'a';
            string expectedDefaultValueInStore = defaultValueProperty.ToString(CultureInfo.InvariantCulture);

            char expectedAlternateValueProperty = 'Z';
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString(CultureInfo.InvariantCulture);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Char, s => objUt.Char = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeGuid()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Guid));

            Guid defaultValueProperty = Guid.Parse("{3932BCBE-E660-4E08-BBE8-68812D9574C8}");
            string expectedDefaultValueInStore = defaultValueProperty.ToString();

            Guid expectedAlternateValueProperty = Guid.Parse("{605689E1-96F9-46FA-BC9C-57CAE005848B}");
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString();

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Guid, s => objUt.Guid = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeBoolean()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Bool));

            bool defaultValueProperty = false;
            int expectedDefaultValueInStore = 0;

            bool expectedAlternateValueProperty = true;
            int expectedAlternateValueInStore = 1;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Bool, s => objUt.Bool = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeSByte()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.SByte));

            sbyte defaultValueProperty = sbyte.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            sbyte expectedAlternateValueProperty = sbyte.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.SByte, s => objUt.SByte = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeByte()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Byte));

            byte defaultValueProperty = byte.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            byte expectedAlternateValueProperty = byte.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Byte, s => objUt.Byte = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeShort()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Short));

            short defaultValueProperty = short.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            short expectedAlternateValueProperty = short.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Short, s => objUt.Short = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeUShort()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.UShort));

            ushort defaultValueProperty = ushort.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            ushort expectedAlternateValueProperty = ushort.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.UShort, s => objUt.UShort = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeInt()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Int));

            int defaultValueProperty = int.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            int expectedAlternateValueProperty = int.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Int, s => objUt.Int = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeInt_OverridePropertyName()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.IntPropertyName));

            int defaultValueProperty = int.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            int expectedAlternateValueProperty = int.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, OverridePropertyName, propertyUt, () => objUt.IntPropertyName, s => objUt.IntPropertyName = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeInt_OverrideCollectionName()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.IntCollectionName));

            int defaultValueProperty = int.MinValue;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            int expectedAlternateValueProperty = int.MaxValue;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, OverrideCollectionName, propertyUt.Name, propertyUt, () => objUt.IntCollectionName, s => objUt.IntCollectionName = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeColor()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Color));

            Color defaultValueProperty = Color.FromArgb(50, 40, 30, 10);
            int expectedDefaultValueInStore = defaultValueProperty.ToArgb();

            Color expectedAlternateValueProperty = Color.FromArgb(80, 70, 60, 50);
            int expectedAlternateValueInStore = expectedAlternateValueProperty.ToArgb();

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Color, s => objUt.Color = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeUInt()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.UInt));

            uint defaultValueProperty = uint.MinValue;
            uint expectedDefaultValueInStore = defaultValueProperty;

            uint expectedAlternateValueProperty = uint.MaxValue;
            uint expectedAlternateValueInStore = expectedAlternateValueProperty;

            SettingStoreTest_UInt32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.UInt, s => objUt.UInt = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeLong()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Long));

            long defaultValueProperty = long.MinValue;
            long expectedDefaultValueInStore = defaultValueProperty;

            long expectedAlternateValueProperty = long.MaxValue;
            long expectedAlternateValueInStore = expectedAlternateValueProperty;

            SettingStoreTest_Int64(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Long, s => objUt.Long = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeDateTime()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.DateTime));

            DateTime defaultValueProperty = DateTime.UtcNow;
            long expectedDefaultValueInStore = defaultValueProperty.ToBinary();

            DateTime expectedAlternateValueProperty = defaultValueProperty + TimeSpan.FromMinutes(30);
            long expectedAlternateValueInStore = expectedAlternateValueProperty.ToBinary();

            SettingStoreTest_Int64(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.DateTime, s => objUt.DateTime = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeDateTimeOffset()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.DateTimeOffset));

            DateTimeOffset defaultValueProperty = DateTimeOffset.MinValue;
            string expectedDefaultValueInStore = defaultValueProperty.ToString("o");

            DateTimeOffset expectedAlternateValueProperty = DateTimeOffset.MaxValue;
            string expectedAlternateValueInStore = expectedAlternateValueProperty.ToString("o");

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.DateTimeOffset, s => objUt.DateTimeOffset = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeULong()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.ULong));

            ulong defaultValueProperty = ulong.MinValue;
            ulong expectedDefaultValueInStore = defaultValueProperty;

            ulong expectedAlternateValueProperty = ulong.MaxValue;
            ulong expectedAlternateValueInStore = expectedAlternateValueProperty;

            SettingStoreTest_UInt64(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.ULong, s => objUt.ULong = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeByteArray()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.ByteArray));

            byte[] defaultValueProperty = new byte[] { 0, 1, 2, 3, 4 };
            byte[] expectedDefaultValueInStore = defaultValueProperty;

            byte[] expectedAlternateValueProperty = new byte[] { 5, 6, 7, 8 };
            byte[] expectedAlternateValueInStore = expectedAlternateValueProperty;

            SettingStoreTest_MemoryStream(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.ByteArray, s => objUt.ByteArray = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeEnumeration()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.Enumeration));

            NativeSettingsType defaultValueProperty = NativeSettingsType.UInt64;
            int expectedDefaultValueInStore = (int)defaultValueProperty;

            NativeSettingsType expectedAlternateValueProperty = NativeSettingsType.Int32;
            int expectedAlternateValueInStore = (int)expectedAlternateValueProperty;

            SettingStoreTest_Int32(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.Enumeration, s => objUt.Enumeration = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void PropertyTypeBooleanStoredAsUInt64_OverrideDataType()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.BoolStoredAsUInt64));

            bool defaultValueProperty = false;
            uint expectedDefaultValueInStore = 0;

            bool expectedAlternateValueProperty = true;
            uint expectedAlternateValueInStore = 1;

            SettingStoreTest_UInt64(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.BoolStoredAsUInt64, s => objUt.BoolStoredAsUInt64 = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void CustomType()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.CustomType));

            CustomType defaultValueProperty = new() { StringValue = "default" };
            string? expectedDefaultValueInStore = objUt.SerializeValue(defaultValueProperty, typeof(CustomType), propertyUt.Name);

            CustomType expectedAlternateValueProperty = new() { StringValue = "loaded" };
            string? expectedAlternateValueInStore = objUt.SerializeValue(expectedAlternateValueProperty, typeof(CustomType), propertyUt.Name);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.CustomType, s => objUt.CustomType = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void CustomType_TypeConverterBinary()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.CustomType_TypeConverterBinary));

            CustomTypeConverter converter = new();

            CustomType defaultValueProperty = new() { StringValue = "default" };
            byte[] expectedDefaultValueInStore = (byte[])converter.ConvertTo(defaultValueProperty, typeof(byte[]));

            CustomType expectedAlternateValueProperty = new() { StringValue = "loaded" };
            byte[] expectedAlternateValueInStore = (byte[])converter.ConvertTo(expectedAlternateValueProperty, typeof(byte[]));

            SettingStoreTest_MemoryStream(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.CustomType_TypeConverterBinary, s => objUt.CustomType_TypeConverterBinary = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void CustomType_TypeConverterBinary_Null()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.CustomType_TypeConverterBinary));

            CustomTypeConverter converter = new();

            CustomType? defaultValueProperty = null;
            byte[] expectedDefaultValueInStore = (byte[])converter.ConvertTo(defaultValueProperty!, typeof(byte[]));

            CustomType expectedAlternateValueProperty = new() { StringValue = "loaded" };
            byte[] expectedAlternateValueInStore = (byte[])converter.ConvertTo(expectedAlternateValueProperty, typeof(byte[]));

            SettingStoreTest_MemoryStream(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.CustomType_TypeConverterBinary, s => objUt.CustomType_TypeConverterBinary = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void CustomType_TypeConverterString()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.CustomType_TypeConverterString));

            CustomTypeConverter converter = new();

            CustomType defaultValueProperty = new() { StringValue = "default" };
            string expectedDefaultValueInStore = (string)converter.ConvertTo(defaultValueProperty, typeof(string));

            CustomType expectedAlternateValueProperty = new() { StringValue = "loaded" };
            string expectedAlternateValueInStore = (string)converter.ConvertTo(expectedAlternateValueProperty, typeof(string));

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.CustomType_TypeConverterString, s => objUt.CustomType_TypeConverterString = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void CustomType_TypeConverterString_Null()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.CustomType_TypeConverterString));

            CustomTypeConverter converter = new();

            CustomType? defaultValueProperty = null;
            string expectedDefaultValueInStore = (string)converter.ConvertTo(defaultValueProperty, typeof(string));

            CustomType expectedAlternateValueProperty = new() { StringValue = "loaded" };
            string expectedAlternateValueInStore = (string)converter.ConvertTo(expectedAlternateValueProperty, typeof(string));

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.CustomType_TypeConverterString, s => objUt.CustomType_TypeConverterString = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void ListOfStringLegacy()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.ListOfStringLegacy));

            List<string> defaultValueProperty = new(new[] { "initial", "second" });
            string expectedDefaultValueInStore = OptionModelPropertyWrapper.LegacySerializeValue(defaultValueProperty);

            List<string> expectedAlternateValueProperty = new(new[] { "loaded", "second" });
            string expectedAlternateValueInStore = OptionModelPropertyWrapper.LegacySerializeValue(expectedAlternateValueProperty);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.ListOfStringLegacy, s => objUt.ListOfStringLegacy = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void ListOfStringLegacy_Null()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.ListOfStringLegacy));

            List<string>? defaultValueProperty = null;
            string expectedDefaultValueInStore = string.Empty;

            List<string> expectedAlternateValueProperty = new(new[] { "loaded", "second" });
            string expectedAlternateValueInStore = OptionModelPropertyWrapper.LegacySerializeValue(expectedAlternateValueProperty);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.ListOfStringLegacy, s => objUt.ListOfStringLegacy = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        [Fact]
        public void ListOfString()
        {
            TestBom objUt = new();
            PropertyInfo propertyUt = objUt.GetType().GetProperty(nameof(TestBom.ListOfString));

            List<string> defaultValueProperty = new(new[] { "initial", "second" });
            string expectedDefaultValueInStore = objUt.SerializeValue(defaultValueProperty, propertyUt.PropertyType, propertyUt.Name);

            List<string> expectedAlternateValueProperty = new(new[] { "loaded", "second" });
            string expectedAlternateValueInStore = objUt.SerializeValue(expectedAlternateValueProperty, propertyUt.PropertyType, propertyUt.Name);

            SettingStoreTest_String(objUt, TestCollectionName, propertyUt.Name, propertyUt, () => objUt.ListOfString, s => objUt.ListOfString = s,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        #region Test Helper Methods and Main Test Method

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="string"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="overrideAssertEquality">           (Optional) If non-null, this will be used to assert equality of the value of the property after
        ///                                                 <c>Load</c> operations rather than the standard assertions. Signature is <c>TestValueType valueBeingTested, 
        ///                                                 TPropertyType expected, TPropertyType actual, string because</c> </param>
        private void SettingStoreTest_String<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, string expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, string expectedAlternateValueInStore,
            Action<TestValueType, TPropertyType?, TPropertyType?, string>? overrideAssertEquality = null)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            void SetupSettingsStoreGet(string valueToReturn)
            {
                mock.GetString(collectionPath, propertyName).Returns(valueToReturn);
            }

            Action<int> verifyGet = (int callCount) =>
                 mock.Received(callCount).GetString(collectionPath, propertyName);
            Action<int, string> verifySet = (int callCount, string expectedValue) =>
                mock.Received(callCount).SetString(collectionPath, propertyName, expectedValue);

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, verifyGet, verifySet,
                             defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore, overrideAssertEquality);
        }

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="int"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        private void SettingStoreTest_Int32<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, int expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, int expectedAlternateValueInStore)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            void SetupSettingsStoreGet(int valueToReturn)
            {
                mock.GetInt32(collectionPath, propertyName).Returns(valueToReturn);
            }

            Action<int> verifyGet = (int callCount) =>
                mock.Received(callCount).GetInt32(collectionPath, propertyName);
            Action<int, int> verifySet = (int callCount, int expectedValue) =>
                mock.Received(callCount).SetInt32(collectionPath, propertyName, expectedValue);

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, verifyGet, verifySet,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="uint"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        private void SettingStoreTest_UInt32<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, uint expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, uint expectedAlternateValueInStore)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            void SetupSettingsStoreGet(uint valueToReturn)
            {
                mock.GetUInt32(collectionPath, propertyName).Returns(valueToReturn);
            }

            Action<int> verifyGet = (int callCount) =>
                mock.Received(callCount).GetUInt32(collectionPath, propertyName);
            Action<int, uint> verifySet = (int callCount, uint expectedValue) =>
                mock.Received(callCount).SetUInt32(collectionPath, propertyName, expectedValue);

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, verifyGet, verifySet,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="long"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        private void SettingStoreTest_Int64<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, long expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, long expectedAlternateValueInStore)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            void SetupSettingsStoreGet(long valueToReturn)
            {
                mock.GetInt64(collectionPath, propertyName).Returns(valueToReturn);
            }

            Action<int> verifyGet = (int callCount) =>
                mock.Received(callCount).GetInt64(collectionPath, propertyName);
            Action<int, long> verifySet = (int callCount, long expectedValue) =>
                mock.Received(callCount).SetInt64(collectionPath, propertyName, expectedValue);

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, verifyGet, verifySet,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="ulong"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        private void SettingStoreTest_UInt64<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, ulong expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, ulong expectedAlternateValueInStore)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            void SetupSettingsStoreGet(ulong valueToReturn)
            {
                mock.GetUInt64(collectionPath, propertyName).Returns(valueToReturn);
            }

            Action<int> verifyGet = (int callCount) =>
                mock.Received(callCount).GetUInt64(collectionPath, propertyName);
            Action<int, ulong> verifySet = (int callCount, ulong expectedValue) =>
                mock.Received(callCount).SetUInt64(collectionPath, propertyName, expectedValue);

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, verifyGet, verifySet,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore);
        }

        /// <summary>   Helper Test Method, used for testing properties where the underlying <see cref="SettingsStore"/> type is 
        ///             a <see cref="MemoryStream"/>.</summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="collectionPath">                   The <c>collectionPath</c> that will be used when reading/writing to the settings store.. </param>
        /// <param name="propertyName">                     The <c>propertyName</c> that will be used when reading/writing to the settings store. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="overrideAssertEquality">           (Optional) If non-null, this will be used to assert equality of the value of the property after
        ///                                                 <c>Load</c> operations rather than the standard assertions. Signature is <c>TestValueType valueBeingTested, 
        ///                                                 TPropertyType expected, TPropertyType actual, string because</c> </param>
        private void SettingStoreTest_MemoryStream<TPropertyType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, string collectionPath, string propertyName,
            PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
            TPropertyType? defaultValueProperty, byte[] expectedDefaultValueInStore,
            TPropertyType? expectedAlternateValueProperty, byte[] expectedAlternateValueInStore,
            Action<TestValueType, TPropertyType?, TPropertyType?, string>? overrideAssertEquality = null)
            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            WritableSettingsStore mock = Substitute.For<WritableSettingsStore>();
            mock.CollectionExists(collectionPath).Returns(true);
            mock.PropertyExists(collectionPath, propertyName).Returns(true);

            List<byte[]?> setInvocations = new();
            mock.When((x) => x.SetMemoryStream(collectionPath, propertyName, Arg.Any<MemoryStream>())).Do((args) =>
            {
                MemoryStream stream = args.ArgAt<MemoryStream>(2);
                if (stream == null)
                    setInvocations.Add(null);
                else
                    setInvocations.Add(stream.ToArray());
            });

            void SetupSettingsStoreGet(byte[] valueToReturn)
            {
                mock.GetMemoryStream(collectionPath, propertyName).Returns(new MemoryStream(valueToReturn));
            }

            void VerifyGet(int callCount)
            {
                mock.Received(callCount).GetMemoryStream(collectionPath, propertyName);
            }

            void VerifySet(int callCount, byte[] expectedValue)
            {
                int matchingCount = 0;
                foreach (byte[]? buffer in setInvocations)
                {
                    if (buffer == null && expectedValue == null)
                        matchingCount++;
                    else if (buffer == null || expectedValue == null)
                        continue;
                    else if (buffer.SequenceEqual(expectedValue))
                        matchingCount++;
                }
                callCount.Should().Be(matchingCount);
            }

            _output.WriteLine("expectedDefaultValueInStore (as string): {0}", System.Text.Encoding.UTF8.GetString(expectedDefaultValueInStore));
            _output.WriteLine("expectedAlternateValueInStore (as string): {0}", System.Text.Encoding.UTF8.GetString(expectedAlternateValueInStore));

            SettingStoreTest(objUt, propertyUt, getProperty, setProperty, mock, SetupSettingsStoreGet, VerifyGet, VerifySet,
                defaultValueProperty, expectedDefaultValueInStore, expectedAlternateValueProperty, expectedAlternateValueInStore, overrideAssertEquality);
        }

        /// <summary>   Main Test Method. Used by the helper methods that are based around setting up the mocks for the <paramref name="settingsStore"/>.  </summary>
        /// <typeparam name="TPropertyType">    The property type of the wrapped property, <paramref name="propertyUt"/>. </typeparam>
        /// <typeparam name="TStorageType">    The native storage type used in the settings store. </typeparam>
        /// <typeparam name="TOptMdl">    The type of the base option model class that is the target object for the test. </typeparam>
        /// <param name="objUt">                            An instance of the <see cref="BaseOptionModel{T}"/>, containing <paramref name="propertyUt"/>. </param>
        /// <param name="propertyUt">                       The property being wrapped for the test. </param>
        /// <param name="getProperty">                      Delegate to get the property value (represented by <paramref name="propertyUt"/>) from <paramref name="objUt"/>. </param>
        /// <param name="setProperty">                      Delegate to set the property (represented by <paramref name="propertyUt"/>) on <paramref name="objUt"/>. </param>
        /// <param name="settingsStore">                    The mock for the <see cref="WritableSettingsStore"/>. </param>
        /// <param name="setupSettingsStoreGet">            Method to configure the settings store to return the provided value when the get method on the <paramref name="settingsStore"/> is
        ///                                                 called with the expected parameters. Signature is <c>TStorageType valueToReturn</c> </param>
        /// <param name="verifySettingsStoreGetWasCalled">  Method to assert the settings store get method on the <paramref name="settingsStore"/> was
        ///                                                 called. Signature is <c>int expectedCallCount</c>. </param>
        /// <param name="verifySettingsStoreSetWasCalled">  Method to assert the settings store set method on the <paramref name="settingsStore"/> was 
        ///                                                 called with the expected parameters. Signature is <c>int expectedCallCount, TStorageType expectedValue</c> </param>
        /// <param name="defaultValueProperty">             Prior to test, the property will be set to this value using <paramref name="setProperty"/>.
        ///                                                 This is the expected value of the property after <c>Load</c> returns when the setting store get
        ///                                                 method is configured to return <paramref name="expectedDefaultValueInStore"/>. Must be different 
        ///                                                 from <paramref name="expectedAlternateValueProperty"/>. </param>
        /// <param name="expectedDefaultValueInStore">      The <paramref name="defaultValueProperty"/> as it is expected to exist in the store.
        ///                                                 Must be different from <paramref name="expectedAlternateValueInStore"/>. Used both to provide 
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="expectedAlternateValueProperty">   This is the expected value of the property after <c>Load</c> returns when the setting store get 
        ///                                                 method is configured to return <paramref name="expectedAlternateValueInStore"/>. Must be 
        ///                                                 different from <paramref name="defaultValueProperty"/>. </param>
        /// <param name="expectedAlternateValueInStore">    The <paramref name="expectedAlternateValueProperty"/> as it is expected to exist in the store. 
        ///                                                 Must be different from <paramref name="expectedDefaultValueInStore"/>. Used both to provide
        ///                                                 values from the store and confirm values written to the store. </param>
        /// <param name="overrideAssertEquality">           (Optional) If non-null, this will be used to assert equality of the value of the property after
        ///                                                 <c>Load</c> operations rather than the standard assertions. Signature is <c>TestValueType valueBeingTested, 
        ///                                                 TPropertyType expected, TPropertyType actual, string because</c> </param>
        private void SettingStoreTest<TPropertyType, TStorageType, TOptMdl>(BaseOptionModel<TOptMdl> objUt, PropertyInfo propertyUt, Func<TPropertyType?> getProperty, Action<TPropertyType?> setProperty,
                            WritableSettingsStore settingsStore, Action<TStorageType> setupSettingsStoreGet, Action<int> verifySettingsStoreGetWasCalled, Action<int, TStorageType> verifySettingsStoreSetWasCalled,
                            TPropertyType? defaultValueProperty, TStorageType expectedDefaultValueInStore,
                            TPropertyType? expectedAlternateValueProperty, TStorageType expectedAlternateValueInStore,
                            Action<TestValueType, TPropertyType?, TPropertyType?, string>? overrideAssertEquality = null)
                            where TOptMdl : BaseOptionModel<TOptMdl>, new()
        {
            objUt.Should().NotBeNull("because we need an object that is the target for the test");
            propertyUt.Should().NotBeNull("because we need a property info to wrap for the test");
            propertyUt.PropertyType.Should().Be<TPropertyType>("because the type of the property we are testing should match the type of the property values we are testing.");
            propertyUt.DeclaringType.Should().BeAssignableTo<BaseOptionModel<TOptMdl>>("because the type of the property we are testing should belong to the object being tested.");
            OptionModelPropertyWrapper uut = new(propertyUt);

            expectedDefaultValueInStore.Should().NotBeEquivalentTo(expectedAlternateValueInStore,
                "because for the test to be valid, the default and loaded value in setting store should be different.");

            defaultValueProperty.Should().NotBeEquivalentTo(expectedAlternateValueProperty,
                "because for the test to be valid, the default and loaded value of the property should be different.");

            setProperty(defaultValueProperty);

            // Test save of default value
            bool saveMethodResult = uut.Save(objUt, settingsStore);
            VerifySet(1, expectedDefaultValueInStore, "because Save with the defaultValueProperty should result " +
                                                                            "in the settings store set method being called with expectedDefaultValueInStore");
            saveMethodResult.Should().BeTrue("because we expect Save to return true for saving default value to store.");

            // Test Load process for alternate value. Verify property value is changed. Verify expected Settings Store Get method was called.
            setupSettingsStoreGet(expectedAlternateValueInStore);
            bool loadMethodResult = uut.Load(objUt, settingsStore);

            string propertyValueLoadMismatchBecause = "because Load should have set the property to the expectedAlternateValueProperty";
            TPropertyType? actualPropertyValue = getProperty();
            if (overrideAssertEquality == null)
                actualPropertyValue.Should().BeEquivalentTo(expectedAlternateValueProperty, propertyValueLoadMismatchBecause);
            else
                overrideAssertEquality(TestValueType.Alternate, expectedAlternateValueProperty, actualPropertyValue, propertyValueLoadMismatchBecause);
            loadMethodResult.Should().BeTrue("because we expect Load of the alternate value to report success when the property was successfully set.");
            VerifyGet(1, "because the proper Settings Store Get method should be called during load for the alternate value.");

            // Test save process. Verify expected Settings Store Set method was called with proper value.
            saveMethodResult = uut.Save(objUt, settingsStore);
            VerifySet(1, expectedAlternateValueInStore, "because Save with the expectedAlternateValueProperty should result " +
                                                                              "in the settings store set method being called with expectedAlternateValueInStore");
            saveMethodResult.Should().BeTrue("because we expect Save to return true for saving alternate value to store.");

            // Test load process for the default value
            setupSettingsStoreGet(expectedDefaultValueInStore);
            loadMethodResult = uut.Load(objUt, settingsStore);

            propertyValueLoadMismatchBecause = "because Load should have set the property to the defaultValueProperty";
            actualPropertyValue = getProperty();
            if (overrideAssertEquality == null)
                actualPropertyValue.Should().BeEquivalentTo(defaultValueProperty, propertyValueLoadMismatchBecause);
            else
                overrideAssertEquality(TestValueType.Default, defaultValueProperty, actualPropertyValue, propertyValueLoadMismatchBecause);
            VerifyGet(2, "because the proper Settings Store Get method should be called again during load for the default value.");
            loadMethodResult.Should().BeTrue("because we expect Load of the default value to report success when the property was successfully set.");

            void VerifyGet(int expectedCallCount, string because)
            {
                try
                {
                    verifySettingsStoreGetWasCalled(expectedCallCount);
                }
                catch (ReceivedCallsException ex)
                {
                    throw new ReceivedCallsException($"{ex.Message} {because}", ex);
                }
            }

            void VerifySet(int expectedCallCount, TStorageType expectedValue, string because)
            {
                try
                {
                    verifySettingsStoreSetWasCalled(expectedCallCount, expectedValue);
                }
                catch (ReceivedCallsException ex)
                {
                    throw new ReceivedCallsException($"{ex.Message} {because}", ex);
                }
            }
        }

        #endregion Test Helper Methods and Main Test Method

        #region BaseOptionModel under test

        public class TestBom : BaseOptionModel<TestBom>
        {
            /// <inheritdoc />
            protected internal override string CollectionName => TestCollectionName;

            /// <summary>   Stored as string. </summary>
            public string? String { get; set; }

            /// <summary>   Stored as string. </summary>
            public float Float { get; set; }

            /// <summary>   Stored as string. </summary>
            public double Double { get; set; }

            /// <summary>   Stored as string. </summary>
            public decimal Decimal { get; set; }

            /// <summary>   Stored as string. </summary>
            public char Char { get; set; }

            /// <summary>   Stored as string. </summary>
            public Guid Guid { get; set; }

            /// <summary>   Stored as string. </summary>
            public DateTimeOffset DateTimeOffset { get; set; }

            /// <summary>   Stored as int. </summary>
            public bool Bool { get; set; }

            /// <summary>   Stored as int. </summary>
            public sbyte SByte { get; set; }

            /// <summary>   Stored as int. </summary>
            public byte Byte { get; set; }

            /// <summary>   Stored as int. </summary>
            public short Short { get; set; }

            /// <summary>   Stored as int. </summary>
            public ushort UShort { get; set; }

            /// <summary>   Stored as int. </summary>
            public int Int { get; set; }

            /// <summary>   Stored as int. </summary>
            [OverridePropertyName(OverridePropertyName)]
            public int IntPropertyName { get; set; }

            /// <summary>   Stored as int. </summary>
            [OverrideCollectionName(OverrideCollectionName)]
            public int IntCollectionName { get; set; }

            /// <summary>   Stored as int. </summary>
            public Color Color { get; set; }

            /// <summary>   Stored as uint. </summary>
            public uint UInt { get; set; }

            /// <summary>   Stored as long. </summary>
            public long Long { get; set; }

            /// <summary>   Stored as long. </summary>
            public DateTime DateTime { get; set; }

            /// <summary>   Stored as ulong. </summary>
            public ulong ULong { get; set; }

            /// <summary>   Stored as MemoryStream. </summary>
            public byte[]? ByteArray { get; set; }

            /// <summary>   Stored as int. </summary>
            public NativeSettingsType Enumeration { get; set; }

            /// <summary>   Stored as UInt64. (OverrideDataType to UInt64) </summary>
            [OverrideDataType(SettingDataType.UInt64)]
            public bool BoolStoredAsUInt64 { get; set; }

            /// <summary>   Stored as xml serialized string. </summary>
            public CustomType? CustomType { get; set; }

            /// <summary>   Stored as string. </summary>
            [OverrideDataType(SettingDataType.String, true)]
            public CustomType? CustomType_TypeConverterString { get; set; }

            /// <summary>   Stored as binary. </summary>
            [OverrideDataType(SettingDataType.Binary, true)]
            public CustomType? CustomType_TypeConverterBinary { get; set; }

            /// <summary>   Stored as xml serialized binary. </summary>
            public List<string>? ListOfString { get; set; }

            /// <summary>   Stored as xml serialized binary. </summary>
            [OverrideDataType(SettingDataType.Legacy)]
            public List<string>? ListOfStringLegacy { get; set; }
        }
        #endregion BaseOptionModel under test

    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public class CustomType
    {
        public string? StringValue { get; set; }
    }

    /// <summary>   Supports byte[] and string conversions </summary>
    public class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(byte[]) || sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(byte[]) || destinationType == typeof(string);
        }

        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
                return FromString(stringValue);

            if (value is byte[] byteValue)
            {
                string tempStr = Encoding.UTF8.GetString(byteValue);
                return FromString(tempStr);
            }
            throw new NotSupportedException($"Cannot convert CustomType from {value?.GetType().FullName ?? "[Null Value]"}");
        }

        public override object? ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            CustomType? customType = value as CustomType;
            if (value != null && customType == null)
                throw new NotSupportedException($"Cannot convert objects of type {value.GetType().FullName} to anything");

            if (destinationType == typeof(string))
                return ToString(customType);

            if (destinationType == typeof(byte[]))
            {
                string? tmpStr = ToString(customType);
                return Encoding.UTF8.GetBytes(tmpStr);
            }

            throw new NotSupportedException($"Cannot convert to type {destinationType.FullName}.");
        }

        private string ToString(CustomType? customType)
        {
            if (customType == null)
                return string.Empty;
            if (customType.StringValue == null)
                return "\0";
            return customType.StringValue;
        }

        private CustomType? FromString(string stringValue)
        {
            if (stringValue.Length == 0)
                return null;
            if (stringValue.Length == 1 && stringValue[0] == '\0')
                return new CustomType();
            return new CustomType() { StringValue = stringValue };
        }
    }

}
