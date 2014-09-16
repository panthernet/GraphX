using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphX
{
    public static class CustomHelper
    {
        public static bool IsIntegerInput(string text)
        {
            if (text == "\r") return false;
            return new Regex("[^0-9]+").IsMatch(text); //regex that matches disallowed text
        }

        public static bool IsDoubleInput(string text)
        {
            if (text == "\r") return false;
            return new Regex("[^0-9.]+").IsMatch(text); //regex that matches disallowed text
        }

        public static ScaleTransform GetScaleTransform(FrameworkElement target)
        {
            var transform = target.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                //COMMENTED!!!
                /*var transformGroup = target.LayoutTransform as TransformGroup;
                if (transformGroup != null)
                    transform = transformGroup.Children[0] as ScaleTransform;
                if (transformGroup == null || transform == null)
                    transform = target.LayoutTransform as ScaleTransform;*/
            }
            return transform;
        }

        public static FrameworkElement FindDescendantByName(this FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name)) { return null; }

            if (name.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }
            var childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var result = (VisualTreeHelper.GetChild(element, i) as FrameworkElement).FindDescendantByName(name);
                if (result != null) { return result; }
            }
            return null;
        }
    }
}
