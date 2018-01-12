using System;
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public sealed class DoubleToLog10Converter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = Math.Log10((double)value);
            return double.IsNegativeInfinity(val) ? 0 : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var val = Math.Pow(10, (double)value);
            return double.IsNegativeInfinity(val) ? 0 : val;
        }

        #endregion


    }
}
