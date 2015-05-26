using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace GraphX.Controls
{
    public static class CustomHelper
    {
        public static bool IsIntegerInput(string text)
        {
            return text != "\r" && new Regex("[^0-9]+").IsMatch(text);
        }

        public static bool IsDoubleInput(string text)
        {
            return text != "\r" && new Regex("[^0-9.]+").IsMatch(text);
        }

        public static ScaleTransform GetScaleTransform(FrameworkElement target)
        {
            var transform = target.RenderTransform as ScaleTransform;
            if (transform != null) return transform;
            var transformGroup = target.LayoutTransform as TransformGroup;
            if (transformGroup != null)
                transform = transformGroup.Children[0] as ScaleTransform;
            if (transformGroup == null || transform == null)
                transform = target.LayoutTransform as ScaleTransform;
            return transform;
        }
    }
}
