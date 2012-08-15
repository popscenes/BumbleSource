using System;
using System.ComponentModel;

namespace WebSite.Application.Extension.TypeConverter
{
    public class FromStringTypeConverter : System.ComponentModel.TypeConverter
    {
        private readonly Func<string, object> _fromString;

        public FromStringTypeConverter(Func<string,object> fromString)
        {
            _fromString = fromString;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
           System.Globalization.CultureInfo culture, object value)
        {
            var stringVal = value as string;
            if (stringVal != null)
            {
                return _fromString(stringVal);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
