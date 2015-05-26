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
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
    {
      [Bindable]
      public sealed class VisibilityToBoolConverter : IValueConverter
      {
        public bool Inverted { get; set; }
        public bool Not { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
          return this.Inverted ? this.BoolToVisibility( value ) : this.VisibilityToBool( value );
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
          return this.Inverted ? this.VisibilityToBool( value ) : this.BoolToVisibility( value );
        }

        private object VisibilityToBool( object value )
        {
          if( !( value is Visibility ) )
            throw new InvalidOperationException( "SuppliedValueWasNotVisibility" );

          return ( ( ( Visibility )value ) == Visibility.Visible ) ^ Not;
        }

        private object BoolToVisibility( object value )
        {
          if( !( value is bool ) )
            throw new InvalidOperationException( "SuppliedValueWasNotBool" );

          return ( ( bool )value ^ Not ) ? Visibility.Visible : Visibility.Collapsed;
        }
      }
    }
