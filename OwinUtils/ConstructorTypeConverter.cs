using System;
using System.ComponentModel;
using System.Globalization;

namespace OwinUtils
{
    public class ConstructorTypeConverter<S, T> : TypeConverter  
    {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        {

            if (sourceType == typeof(S)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, 
                                           CultureInfo culture, object value) 
        {
            if (value is S) {
                object[] args = { value };
                return Activator.CreateInstance(typeof(T), args);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context
                                         , CultureInfo culture, object value
                                         , Type destinationType) 
        {  
            if (destinationType == typeof(string)) {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}

