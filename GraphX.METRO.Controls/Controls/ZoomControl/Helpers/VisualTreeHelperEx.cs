/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public static class VisualTreeHelperEx
    {
        public static DependencyObject FindAncestorByType(DependencyObject element, Type type, bool specificTypeOnly)
        {
            if (element == null)
                return null;

            if (element.GetType() == type)
                return element;

            return FindAncestorByType(VisualTreeHelper.GetParent(element), type, specificTypeOnly);
        }

        public static T FindAncestorByType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return default(T);
            }
            if (depObj is T)
            {
                return (T) depObj;
            }

            T parent = default(T);

            parent = FindAncestorByType<T>(VisualTreeHelper.GetParent(depObj));

            return parent;
        }

        public static UIElement FindDescendantByName(UIElement element, string name)
        {
            if (element != null && (element is FrameworkElement) && (element as FrameworkElement).Name == name)
                return element;

            UIElement foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).InvalidateArrange();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantByName(visual, name);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }

        public static UIElement FindDescendantByType(UIElement element, Type type)
        {
            return FindDescendantByType(element, type, true);
        }

        public static UIElement FindDescendantByType(UIElement element, Type type, bool specificTypeOnly)
        {
            if (element == null)
                return null;

            if (element.GetType() == type)
                return element;

            UIElement foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).InvalidateArrange();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantByType(visual, type, specificTypeOnly);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }

        public static T FindDescendantByType<T>(UIElement element) where T : UIElement
        {
            UIElement temp = FindDescendantByType(element, typeof (T));

            return (T) temp;
        }

        public static UIElement FindDescendantWithPropertyValue(UIElement element,
            DependencyProperty dp, object value)
        {
            if (element == null)
                return null;

            if (element.GetValue(dp).Equals(value))
                return element;

            UIElement foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).InvalidateArrange();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantWithPropertyValue(visual, dp, value);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }

        #region Find descendants of type

        public static IEnumerable<T> FindDescendantsOfType<T>(this UIElement element) where T : class
        {
            if (element == null) yield break;
            if (element is T)
                yield return element as T;

            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null)
                frameworkElement.InvalidateArrange();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                if (visual == null) continue;
                foreach (var item in visual.FindDescendantsOfType<T>())
                    yield return item;
            }
        }

        #endregion
    }
}
