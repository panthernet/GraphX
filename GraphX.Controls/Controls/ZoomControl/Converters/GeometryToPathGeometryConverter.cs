using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GraphX.Controls
{
    public class GeometryToPathGeometryConverter : IValueConverter
    {
        #region Inverted Property

        public bool Inverted
        {
            get
            {
                return _inverted;
            }
            set
            {
                _inverted = value;
            }
        }

        private bool _inverted; //false

        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EllipseGeometry) return null;
            return _inverted ? (Geometry)value : (PathGeometry)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EllipseGeometry) return null;
            return _inverted ? (PathGeometry)value : (Geometry)value;
        }
    }
}
