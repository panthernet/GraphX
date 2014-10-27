using System;
using Windows.UI.Xaml.Data;

namespace GraphX.Converters
{
    public sealed class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return object.Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return parameter;

            //it's false, so don't bind it back
            return null;
        }
    }
}
