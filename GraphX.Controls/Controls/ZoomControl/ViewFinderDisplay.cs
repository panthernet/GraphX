using System;
using System.Windows;
using System.Windows.Media;

namespace GraphX.Controls
{
  public class ViewFinderDisplay : FrameworkElement
  {
    #region Constructors

    static ViewFinderDisplay()
    {
      ViewFinderDisplay.DefaultStyleKeyProperty.OverrideMetadata( typeof( ViewFinderDisplay ), new FrameworkPropertyMetadata( typeof( ViewFinderDisplay ) ) );
    }

    public ViewFinderDisplay()
    {
    }

    #endregion

    #region Background Property

    public static readonly DependencyProperty BackgroundProperty =
      DependencyProperty.Register("Background", typeof(Brush), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( new SolidColorBrush( Color.FromArgb( 0xC0, 0xFF, 0xFF, 0xFF ) ), FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Brush Background
    {
      get
      {
          return (Brush)this.GetValue(ViewFinderDisplay.BackgroundProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.BackgroundProperty, value);
      }
    }

    #endregion

    #region ContentBounds Property

    private static readonly DependencyPropertyKey ContentBoundsPropertyKey =
      DependencyProperty.RegisterReadOnly("ContentBounds", typeof(Rect), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( ( Rect )Rect.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender ) );

    public static readonly DependencyProperty ContentBoundsProperty = ViewFinderDisplay.ContentBoundsPropertyKey.DependencyProperty;

    internal Rect ContentBounds
    {
      get
      {
          return (Rect)this.GetValue(ViewFinderDisplay.ContentBoundsProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.ContentBoundsPropertyKey, value);
      }
    }

    #endregion

    #region ShadowBrush Property

    public static readonly DependencyProperty ShadowBrushProperty =
      DependencyProperty.Register("ShadowBrush", typeof(Brush), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( new SolidColorBrush( Color.FromArgb( 0x80, 0xFF, 0xFF, 0xFF ) ), FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Brush ShadowBrush
    {
      get
      {
          return (Brush)this.GetValue(ViewFinderDisplay.ShadowBrushProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.ShadowBrushProperty, value);
      }
    }

    #endregion

    #region ViewportBrush Property

    public static readonly DependencyProperty ViewportBrushProperty =
      DependencyProperty.Register("ViewportBrush", typeof(Brush), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Brush ViewportBrush
    {
      get
      {
          return (Brush)this.GetValue(ViewFinderDisplay.ViewportBrushProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.ViewportBrushProperty, value);
      }
    }

    #endregion

    #region ViewportPen Property

    public static readonly DependencyProperty ViewportPenProperty =
      DependencyProperty.Register("ViewportPen", typeof(Pen), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( new Pen( new SolidColorBrush( Color.FromArgb( 0x80, 0x00, 0x00, 0x00 ) ), 1d ), FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Pen ViewportPen
    {
      get
      {
          return (Pen)this.GetValue(ViewFinderDisplay.ViewportPenProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.ViewportPenProperty, value);
      }
    }

    #endregion

    #region ViewportRect Property

    public static readonly DependencyProperty ViewportRectProperty =
      DependencyProperty.Register("ViewportRect", typeof(Rect), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender ) );

    public Rect ViewportRect
    {
      get
      {
          return (Rect)this.GetValue(ViewFinderDisplay.ViewportRectProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.ViewportRectProperty, value);
      }
    }

    #endregion

    #region VisualBrush Property

    private static readonly DependencyPropertyKey VisualBrushPropertyKey =
      DependencyProperty.RegisterReadOnly("VisualBrush", typeof(VisualBrush), typeof(ViewFinderDisplay),
        new FrameworkPropertyMetadata( ( VisualBrush )null ) );

    public static readonly DependencyProperty VisualBrushProperty = ViewFinderDisplay.VisualBrushPropertyKey.DependencyProperty;

    internal VisualBrush VisualBrush
    {
      get
      {
          return (VisualBrush)this.GetValue(ViewFinderDisplay.VisualBrushProperty);
      }
      set
      {
          this.SetValue(ViewFinderDisplay.VisualBrushPropertyKey, value);
      }
    }

    #endregion

    #region AvailableSize Internal Property

    internal Size AvailableSize
    {
      get
      {
        return _availableSize;
      }
    }

    private Size _availableSize = Size.Empty;

    #endregion

    #region Scale Internal Property

    internal double Scale
    {
      get
      {
        return _scale;
      }
      set
      {
        _scale = value;
      }
    }

    private double _scale = 1d;

    #endregion

    protected override Size ArrangeOverride( Size finalSize )
    {
      // Note that we do not call the Arrange method on any children
      // because a ViewFinderDisplay has no children

      // the control's RenderSize should always match its DesiredSize
      return this.DesiredSize;
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      // Note that we do not call the Measure method on any children
      // because a ViewFinderDisplay has no children.  It is merely used 
      // as a surface for the view finder's VisualBrush.

      // store the available size for use by the Zoombox control
      _availableSize = availableSize;

      // Simulate size-to-content for the display panel by ensuring a width and height
      // based on the content bounds. Otherwise, the display panel may have no size, since it doesn't 
      // contain content.
      double width = DoubleHelper.IsNaN( this.ContentBounds.Width ) ? 0 : Math.Max( 0, this.ContentBounds.Width );
      double height = DoubleHelper.IsNaN( this.ContentBounds.Height ) ? 0 : Math.Max( 0, this.ContentBounds.Height );
      Size displayPanelSize = new Size( width, height );

      // Now ensure that the result fits within the available size while maintaining
      // the width/height ratio of the content bounds
      if( displayPanelSize.Width > availableSize.Width || displayPanelSize.Height > availableSize.Height )
      {
        double aspectX = availableSize.Width / displayPanelSize.Width;
        double aspectY = availableSize.Height / displayPanelSize.Height;
        double scale = ( aspectX < aspectY ) ? aspectX : aspectY;
        displayPanelSize = new Size( displayPanelSize.Width * scale, displayPanelSize.Height * scale );
      }

      return displayPanelSize;
    }

    protected override void OnRender( DrawingContext dc )
    {
      base.OnRender( dc );

      dc.DrawRectangle( this.Background, null, this.ContentBounds );

      dc.DrawRectangle( this.VisualBrush, null, this.ContentBounds );

      if( this.ViewportRect.IntersectsWith( new Rect( this.RenderSize ) ) )
      {
        // draw shadow rectangles over the non-viewport regions
        Rect r1 = new Rect( new Point( 0, 0 ), new Size( this.RenderSize.Width, Math.Max( 0, this.ViewportRect.Top ) ) );
        Rect r2 = new Rect( new Point( 0, this.ViewportRect.Top ), new Size( Math.Max( 0, this.ViewportRect.Left ), this.ViewportRect.Height ) );
        Rect r3 = new Rect( new Point( this.ViewportRect.Right, this.ViewportRect.Top ), new Size( Math.Max( 0, this.RenderSize.Width - this.ViewportRect.Right ), this.ViewportRect.Height ) );
        Rect r4 = new Rect( new Point( 0, this.ViewportRect.Bottom ), new Size( this.RenderSize.Width, Math.Max( 0, this.RenderSize.Height - this.ViewportRect.Bottom ) ) );
        dc.DrawRectangle( this.ShadowBrush, null, r1 );
        dc.DrawRectangle( this.ShadowBrush, null, r2 );
        dc.DrawRectangle( this.ShadowBrush, null, r3 );
        dc.DrawRectangle( this.ShadowBrush, null, r4 );

        // draw the rectangle around the viewport region
        dc.DrawRectangle( this.ViewportBrush, this.ViewportPen, this.ViewportRect );
      }
      else
      {
        // if no part of the Rect is visible, just draw a 
        // shadow over the entire content bounds
        dc.DrawRectangle( this.ShadowBrush, null, new Rect( this.RenderSize ) );
      }
    }
  }
}
