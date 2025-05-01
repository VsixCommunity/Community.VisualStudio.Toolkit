using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace TestExtension
{
    internal class RuntimeEnumTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new RuntimeEnumProxy(value as string ?? "");
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is RuntimeEnumProxy proxy)
                {
                    return proxy.Value;
                }
                else if (value is string s)
                {
                    // This is just in case the value is still a
                    // string and wasn't converted from a string.
                    return s;
                }
            }

            throw new NotSupportedException();
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            RuntimeEnumProxy proxy = value as RuntimeEnumProxy;
            if (proxy is not null)
            {
                return GetStandardValues().Cast<string>().Contains(proxy.Value);
            }
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // Load the values from somewhere (like a database). You will want to perform some
            // sort of caching, and ideally pre-fetch the values, because any async activity
            // here (run via `ThreadHelper`, of course) will block the UI.
            return new StandardValuesCollection(
                new[] {
                    new RuntimeEnumProxy("Alpha"),
                    new RuntimeEnumProxy("Beta"),
                    new RuntimeEnumProxy("Gamma"),
                    new RuntimeEnumProxy("Delta")
                }
            );
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context) => false;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) => throw new NotSupportedException();

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => false;

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues) => throw new NotSupportedException();
    }
}
