using System;
using Windows.UI.Xaml.Data;

namespace GraphX.Converters
{
    public sealed class DoubleToLog10Converter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val = (double)value; 
            return Math.Log10(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double val = (double) value;
            return Math.Pow(10, val);
        }

        #endregion


    }
}
