using System;
using System.Globalization;
using System.Windows.Data;
using ShowcaseApp.WPF.Models;

namespace ShowcaseApp.WPF
{
    
    public sealed class ValueToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return null;
            return ImageLoader.GetImageById((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Image to Id conversion is not supported!");
        }

        #endregion
    }

    public sealed class ValueToPersonImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return null;
            return ThemedDataStorage.GetImageById((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Image to Id conversion is not supported!");
        }

        #endregion

        
    }

    public sealed class ValueToEditorImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return null;
            return ThemedDataStorage.GetEditorImageById((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Image to Id conversion is not supported!");
        }

        #endregion
    }
}
