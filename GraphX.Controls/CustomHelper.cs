using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

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

        public static ScaleTransform? GetScaleTransform(FrameworkElement target)
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

        public static FrameworkElement? FindDescendantByName(this FrameworkElement? element, string name)
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

        public static bool IsInDesignMode(DependencyObject? ctrl = null)
        {
            return ctrl != null && DesignerProperties.GetIsInDesignMode(ctrl);
        }
    }
}