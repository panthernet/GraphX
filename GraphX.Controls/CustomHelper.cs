using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

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
                var transformGroup = target.LayoutTransform as TransformGroup;
                if (transformGroup != null)
                    transform = transformGroup.Children[0] as ScaleTransform;
                if (transformGroup == null || transform == null)
                    transform = target.LayoutTransform as ScaleTransform;
            }
            return transform;
        }
    }
}
