﻿/*************************************************************************************

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
using System.Windows;
using System.Windows.Media;

namespace GraphX.Controls
{
    public static class VisualTreeHelperEx
    {
        public static DependencyObject? FindAncestorByType(DependencyObject? element, Type type, bool specificTypeOnly)
        {
            if (element == null)
                return null;

            if (specificTypeOnly ? (element.GetType() == type)
                : (element.GetType() == type) || (element.GetType().IsSubclassOf(type)))
                return element;

            return VisualTreeHelperEx.FindAncestorByType(VisualTreeHelper.GetParent(element), type, specificTypeOnly);
        }

        public static T? FindAncestorByType<T>(DependencyObject? depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return default(T);
            }
            if (depObj is T)
            {
                return (T) depObj;
            }

            T? parent = default(T);

            parent = VisualTreeHelperEx.FindAncestorByType<T>(VisualTreeHelper.GetParent(depObj));

            return parent;
        }

        public static Visual? FindDescendantByName(Visual? element, string name)
        {
            if (element is FrameworkElement frameworkElement && frameworkElement.Name == name)
                return element;

            Visual? foundElement = null;
            if (element is FrameworkElement frameworkElementWithTemplate)
                frameworkElementWithTemplate.ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual? visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = VisualTreeHelperEx.FindDescendantByName(visual, name);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }

        public static Visual? FindDescendantByType(Visual element, Type type)
        {
            return VisualTreeHelperEx.FindDescendantByType(element, type, true);
        }

        public static Visual? FindDescendantByType(Visual? element, Type type, bool specificTypeOnly)
        {
            if (element == null)
                return null;

            if (specificTypeOnly ? (element.GetType() == type)
                : (element.GetType() == type) || (element.GetType().IsSubclassOf(type)))
                return element;

            Visual? foundElement = null;
            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual? visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = VisualTreeHelperEx.FindDescendantByType(visual, type, specificTypeOnly);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }

        #region Find descendants of type

        public static IEnumerable<T> FindDescendantsOfType<T>(this Visual? element) where T: class
        {
            if (element == null)  yield break;
            if (element is T)
                yield return (T)(object)element;

            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null)
                frameworkElement.ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                if(visual == null) continue;
                foreach (var item in visual.FindDescendantsOfType<T>())
                    yield return item;
            }
        }

        #endregion

        public static T? FindDescendantByType<T>(Visual element) where T : Visual
        {
            Visual? temp = VisualTreeHelperEx.FindDescendantByType(element, typeof (T));

            return (T?) temp;
        }

        public static Visual? FindDescendantWithPropertyValue(Visual? element, DependencyProperty dp, object value)
        {
            if (element == null)
                return null;

            if (element.GetValue(dp).Equals(value))
                return element;

            Visual? foundElement = null;
            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual? visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = VisualTreeHelperEx.FindDescendantWithPropertyValue(visual, dp, value);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }
    }
}
