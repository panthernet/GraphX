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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
  public static class VisualTreeHelperEx
  {
    public static DependencyObject FindAncestorByType( DependencyObject element, Type type, bool specificTypeOnly )
    {
      if( element == null )
        return null;

      if(  element.GetType() == type )
        return element;

      return VisualTreeHelperEx.FindAncestorByType( VisualTreeHelper.GetParent( element ), type, specificTypeOnly );
    }

    public static T FindAncestorByType<T>( DependencyObject depObj ) where T : DependencyObject
    {
      if( depObj == null )
      {
        return default( T );
      }
      if( depObj is T )
      {
        return ( T )depObj;
      }

      T parent = default( T );

      parent = VisualTreeHelperEx.FindAncestorByType<T>( VisualTreeHelper.GetParent( depObj ) );

      return parent;
    }

    public static UIElement FindDescendantByName(UIElement element, string name)
    {
      if( element != null && ( element is FrameworkElement ) && ( element as FrameworkElement ).Name == name )
        return element;

      UIElement foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).InvalidateArrange();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
          var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
        foundElement = VisualTreeHelperEx.FindDescendantByName( visual, name );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }

    public static UIElement FindDescendantByType( UIElement element, Type type )
    {
      return VisualTreeHelperEx.FindDescendantByType( element, type, true );
    }

    public static UIElement FindDescendantByType(UIElement element, Type type, bool specificTypeOnly)
    {
      if( element == null )
        return null;

      if( element.GetType() == type )
        return element;

      UIElement foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).InvalidateArrange();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
          var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
        foundElement = VisualTreeHelperEx.FindDescendantByType( visual, type, specificTypeOnly );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }

    public static T FindDescendantByType<T>(UIElement element) where T : UIElement
    {
        UIElement temp = VisualTreeHelperEx.FindDescendantByType(element, typeof(T));

      return ( T )temp;
    }

    public static UIElement FindDescendantWithPropertyValue(UIElement element,
        DependencyProperty dp, object value )
    {
      if( element == null )
        return null;

      if( element.GetValue( dp ).Equals( value ) )
        return element;

      UIElement foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).InvalidateArrange();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
          var visual = VisualTreeHelper.GetChild(element, i) as UIElement;
        foundElement = VisualTreeHelperEx.FindDescendantWithPropertyValue( visual, dp, value );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }
  }
}
