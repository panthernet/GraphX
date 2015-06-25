using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GraphX.Controls.Models;

namespace GraphX.Controls
{
    [TemplatePart(Name = PART_PRESENTER, Type = typeof(ZoomContentPresenter))]
    public class ZoomControl : ContentControl, IZoomControl, INotifyPropertyChanged
    {

        #region Viewfinder (minimap)

        #region Properties & Commands

        #region Center Command

        public static RoutedUICommand Center = new RoutedUICommand("Center Content", "Center", typeof(ZoomControl));

        private void CenterContent(object sender, ExecutedRoutedEventArgs e)
        {
            CenterContent();
        }

        #endregion

        #region Fill Command

        public static RoutedUICommand Fill = new RoutedUICommand("Fill Bounds with Content", "FillToBounds", typeof(ZoomControl));

        private void FillToBounds(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomToFill();
        }

        #endregion

        #region Fit Command

        public static RoutedUICommand Fit = new RoutedUICommand("Fit Content within Bounds", "FitToBounds", typeof(ZoomControl));

        private void FitToBounds(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        #endregion

        #region Refocus Command

        public static RoutedUICommand Refocus = new RoutedUICommand("Refocus View", "Refocus", typeof(ZoomControl));

        private void CanRefocusView(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void RefocusView(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void CanExecuteTrue(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        #region ResizeEdge Nested Type

        private enum ResizeEdge
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Left,
            Top,
            Right,
            Bottom,
        }

        #endregion

        #region CacheBits Nested Type

        private enum CacheBits
        {
            IsUpdatingView = 0x00000001,
            IsUpdatingViewport = 0x00000002,
            IsDraggingViewport = 0x00000004,
            IsResizingViewport = 0x00000008,
            IsMonitoringInput = 0x00000010,
            IsContentWrapped = 0x00000020,
            HasArrangedContentPresenter = 0x00000040,
            HasRenderedFirstView = 0x00000080,
            RefocusViewOnFirstRender = 0x00000100,
            HasUiPermission = 0x00000200,
        }

        #endregion

        #region Viewport Property

        private static readonly DependencyPropertyKey ViewportPropertyKey =
          DependencyProperty.RegisterReadOnly("Viewport", typeof(Rect), typeof(ZoomControl),
            new FrameworkPropertyMetadata(Rect.Empty,
              OnViewportChanged));

        public static readonly DependencyProperty ViewportProperty = ViewportPropertyKey.DependencyProperty;

        public Rect Viewport
        {
            get
            {
                return (Rect)GetValue(ViewportProperty);
            }
        }

        private static void OnViewportChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // keep the Position property in sync with the Viewport
            //var zoombox = (ZoomControl)o;
            //var pt = new Point(-zoombox.Viewport.Left * zoombox.Zoom / zoombox._viewboxFactor, -zoombox.Viewport.Top * zoombox.Zoom / zoombox._viewboxFactor);
            //zoombox.TranslateX = pt.X;
            //zoombox.TranslateY = pt.Y;
        }

        #endregion

        #region ViewFinder Property

        private static readonly DependencyPropertyKey ViewFinderPropertyKey =
          DependencyProperty.RegisterReadOnly("ViewFinder", typeof(FrameworkElement), typeof(ZoomControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ViewFinderProperty = ViewFinderPropertyKey.DependencyProperty;

        public FrameworkElement ViewFinder
        {
            get
            {
                return (FrameworkElement)GetValue(ViewFinderProperty);
            }
        }

        #endregion

        #region ViewFinderVisibility Attached Property

        public static readonly DependencyProperty ViewFinderVisibilityProperty =
          DependencyProperty.RegisterAttached("ViewFinderVisibility", typeof(Visibility), typeof(ZoomControl),
            new FrameworkPropertyMetadata(Visibility.Visible));

        public static Visibility GetViewFinderVisibility(DependencyObject d)
        {
            return (Visibility)(d.GetValue(ViewFinderVisibilityProperty));
        }

        public static void SetViewFinderVisibility(DependencyObject d, Visibility value)
        {
            d.SetValue(ViewFinderVisibilityProperty, value);
        }

        #endregion

        // the view finder display panel
        // this is used to show the current viewport
        private ViewFinderDisplay _viewFinderDisplay;
        private double _viewboxFactor = 1.0;

        #endregion

        #region Attach / Detach
        private void AttachToVisualTree()
        {      // detach from the old tree
            DetachFromVisualTree();
            // set a reference to the ViewFinder element, if present
            SetValue(ViewFinderPropertyKey, Template.FindName("ViewFinder", this) as FrameworkElement);

            // locate the view finder display panel
            _viewFinderDisplay = VisualTreeHelperEx.FindDescendantByType(this, typeof(ViewFinderDisplay)) as ViewFinderDisplay;

            // if a ViewFinder was specified but no display panel is present, throw an exception
            if (ViewFinder != null && _viewFinderDisplay == null)
                throw new Exception("ZoomControlHasViewFinderButNotDisplay");

            // set up the VisualBrush and adorner for the display panel
            if (_viewFinderDisplay != null)
            {
                // create VisualBrush for the view finder display panel
                CreateVisualBrushForViewFinder(Content as Visual);
                _viewFinderDisplay.Background = this.Background;

                // hook up event handlers for dragging and resizing the viewport
                _viewFinderDisplay.MouseMove += ViewFinderDisplayMouseMove;
                _viewFinderDisplay.MouseLeftButtonDown += ViewFinderDisplayBeginCapture;
                _viewFinderDisplay.MouseLeftButtonUp += ViewFinderDisplayEndCapture;
            }
        }

        private void CreateVisualBrushForViewFinder(Visual visual)
        {
            _viewFinderDisplay.VisualBrush = new VisualBrush(visual) {Stretch = Stretch.Uniform, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top};
        }

        private void DetachFromVisualTree()
        {
            // remove the view finder display panel's visual brush and adorner
            _viewFinderDisplay = null;
        }
        #endregion

        #region Mouse Events

        #region IsResizingViewport Private Property
        private bool IsResizingViewport
        {
            get
            {
                return _cacheBits[(int)CacheBits.IsResizingViewport];
            }
            set
            {
                _cacheBits[(int)CacheBits.IsResizingViewport] = value;
            }
        }

        #endregion

        #region IsDraggingViewport Private Property

        private bool IsDraggingViewport
        {
            get
            {
                return _cacheBits[(int)CacheBits.IsDraggingViewport];
            }
            set
            {
                _cacheBits[(int)CacheBits.IsDraggingViewport] = value;
            }
        }

        #endregion

        // state variables used during drag and select operations
        private Rect _resizeViewportBounds = Rect.Empty;
        private Point _resizeAnchorPoint = new Point(0, 0);
        private Point _resizeDraggingPoint = new Point(0, 0);
        private Point _originPoint = new Point(0, 0);
        private BitVector32 _cacheBits = new BitVector32(0);

        private void ViewFinderDisplayBeginCapture(object sender, MouseButtonEventArgs e)
        {
            const double arbitraryLargeValue = 10000000000;

            // if we need to acquire capture, the Tag property of the view finder display panel
            // will be a ResizeEdge value.
            if (_viewFinderDisplay.Tag is ResizeEdge)
            {
                // if the Tag is ResizeEdge.None, then its a drag operation; otherwise, its a resize
                if ((ResizeEdge)_viewFinderDisplay.Tag == ResizeEdge.None)
                {
                    IsDraggingViewport = true;
                }
                else
                {
                    IsResizingViewport = true;
                    var direction = new Vector();
                    switch ((ResizeEdge)_viewFinderDisplay.Tag)
                    {
                        case ResizeEdge.TopLeft:
                            _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.TopLeft;
                            _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.BottomRight;
                            direction = new Vector(-1, -1);
                            break;

                        case ResizeEdge.TopRight:
                            _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.TopRight;
                            _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.BottomLeft;
                            direction = new Vector(1, -1);
                            break;

                        case ResizeEdge.BottomLeft:
                            _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.BottomLeft;
                            _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.TopRight;
                            direction = new Vector(-1, 1);
                            break;

                        case ResizeEdge.BottomRight:
                            _resizeDraggingPoint = _viewFinderDisplay.ViewportRect.BottomRight;
                            _resizeAnchorPoint = _viewFinderDisplay.ViewportRect.TopLeft;
                            direction = new Vector(1, 1);
                            break;
                        case ResizeEdge.Left:
                            _resizeDraggingPoint = new Point(_viewFinderDisplay.ViewportRect.Left,
                                _viewFinderDisplay.ViewportRect.Top + (_viewFinderDisplay.ViewportRect.Height / 2));
                            _resizeAnchorPoint = new Point(_viewFinderDisplay.ViewportRect.Right,
                                _viewFinderDisplay.ViewportRect.Top + (_viewFinderDisplay.ViewportRect.Height / 2));
                            direction = new Vector(-1, 0);
                            break;
                        case ResizeEdge.Top:
                            _resizeDraggingPoint = new Point(_viewFinderDisplay.ViewportRect.Left + (_viewFinderDisplay.ViewportRect.Width / 2),
                                _viewFinderDisplay.ViewportRect.Top);
                            _resizeAnchorPoint = new Point(_viewFinderDisplay.ViewportRect.Left + (_viewFinderDisplay.ViewportRect.Width / 2),
                                _viewFinderDisplay.ViewportRect.Bottom);
                            direction = new Vector(0, -1);
                            break;
                        case ResizeEdge.Right:
                            _resizeDraggingPoint = new Point(_viewFinderDisplay.ViewportRect.Right,
                                _viewFinderDisplay.ViewportRect.Top + (_viewFinderDisplay.ViewportRect.Height / 2));
                            _resizeAnchorPoint = new Point(_viewFinderDisplay.ViewportRect.Left,
                                _viewFinderDisplay.ViewportRect.Top + (_viewFinderDisplay.ViewportRect.Height / 2));
                            direction = new Vector(1, 0);
                            break;
                        case ResizeEdge.Bottom:
                            _resizeDraggingPoint = new Point(_viewFinderDisplay.ViewportRect.Left + (_viewFinderDisplay.ViewportRect.Width / 2),
                                _viewFinderDisplay.ViewportRect.Bottom);
                            _resizeAnchorPoint = new Point(_viewFinderDisplay.ViewportRect.Left + (_viewFinderDisplay.ViewportRect.Width / 2),
                                _viewFinderDisplay.ViewportRect.Top);
                            direction = new Vector(0, 1);
                            break;
                    }
                    var contentRect = _viewFinderDisplay.ContentBounds;
                    var minVector = new Vector(direction.X * arbitraryLargeValue, direction.Y * arbitraryLargeValue);
                    var maxVector = new Vector(direction.X * contentRect.Width / MaxZoom, direction.Y * contentRect.Height / MaxZoom);
                    _resizeViewportBounds = new Rect(_resizeAnchorPoint + minVector, _resizeAnchorPoint + maxVector);
                }

                // store the origin of the operation and acquire capture
                _originPoint = e.GetPosition(_viewFinderDisplay);
                _viewFinderDisplay.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ViewFinderDisplayEndCapture(object sender, MouseButtonEventArgs e)
        {
            // if a drag or resize is in progress, end it and release capture
            if (IsDraggingViewport || IsResizingViewport)
            {
                // call the DragDisplayViewport method to end the operation
                // and store the current position on the stack
                DragDisplayViewport(new DragDeltaEventArgs(0, 0));

                // reset the dragging state variables and release capture
                IsDraggingViewport = false;
                IsResizingViewport = false;
                _originPoint = new Point();
                _viewFinderDisplay.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void ViewFinderDisplayMouseMove(object sender, MouseEventArgs e)
        {
            // if a drag operation is in progress, update the operation
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed
                && (IsDraggingViewport || IsResizingViewport))
            {
                var pos = e.GetPosition(_viewFinderDisplay);
                var delta = pos - _originPoint;
                if (IsDraggingViewport)
                {
                    DragDisplayViewport(new DragDeltaEventArgs(delta.X, delta.Y));
                }
                else
                {
                    ResizeDisplayViewport(new DragDeltaEventArgs(delta.X, delta.Y), (ResizeEdge)_viewFinderDisplay.Tag);
                }
                e.Handled = true;
            }
            else
            {
                // update the cursor based on the nearest corner
                var mousePos = e.GetPosition(_viewFinderDisplay);
                var viewportRect = _viewFinderDisplay.ViewportRect;
                var cornerDelta = viewportRect.Width * viewportRect.Height > 100 ? 5.0
                    : Math.Sqrt(viewportRect.Width * viewportRect.Height) / 2;

                // if the mouse is within the Rect and the Rect does not encompass the entire content, set the appropriate cursor
                if (viewportRect.Contains(mousePos)
                    && !DoubleHelper.AreVirtuallyEqual(Rect.Intersect(viewportRect, _viewFinderDisplay.ContentBounds), _viewFinderDisplay.ContentBounds))
                {
                    if (PointHelper.DistanceBetween(mousePos, viewportRect.TopLeft) < cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.TopLeft;
                        _viewFinderDisplay.Cursor = Cursors.SizeNWSE;
                    }
                    else if (PointHelper.DistanceBetween(mousePos, viewportRect.BottomRight) < cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.BottomRight;
                        _viewFinderDisplay.Cursor = Cursors.SizeNWSE;
                    }
                    else if (PointHelper.DistanceBetween(mousePos, viewportRect.TopRight) < cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.TopRight;
                        _viewFinderDisplay.Cursor = Cursors.SizeNESW;
                    }
                    else if (PointHelper.DistanceBetween(mousePos, viewportRect.BottomLeft) < cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.BottomLeft;
                        _viewFinderDisplay.Cursor = Cursors.SizeNESW;
                    }
                    else if (mousePos.X <= viewportRect.Left + cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.Left;
                        _viewFinderDisplay.Cursor = Cursors.SizeWE;
                    }
                    else if (mousePos.Y <= viewportRect.Top + cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.Top;
                        _viewFinderDisplay.Cursor = Cursors.SizeNS;
                    }
                    else if (mousePos.X >= viewportRect.Right - cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.Right;
                        _viewFinderDisplay.Cursor = Cursors.SizeWE;
                    }
                    else if (mousePos.Y >= viewportRect.Bottom - cornerDelta)
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.Bottom;
                        _viewFinderDisplay.Cursor = Cursors.SizeNS;
                    }
                    else
                    {
                        _viewFinderDisplay.Tag = ResizeEdge.None;
                        _viewFinderDisplay.Cursor = Cursors.SizeAll;
                    }
                }
                else
                {
                    _viewFinderDisplay.Tag = null;
                    _viewFinderDisplay.Cursor = Cursors.Arrow;
                }
            }
        }
        #endregion

        #region Drag and resize
        private void DragDisplayViewport(DragDeltaEventArgs e)
        {
           // UpdateViewFinderDisplayContentBounds();
            // get the scale of the view finder display panel, the selection rect, and the VisualBrush rect

            var scale = _viewFinderDisplay.Scale;
            var viewportRect = _viewFinderDisplay.ViewportRect;
            var vbRect = _viewFinderDisplay.ContentBounds;

            // if the entire content is visible, do nothing
            if (viewportRect.Contains(vbRect))
                return;

            // ensure that we stay within the bounds of the VisualBrush
            var dx = e.HorizontalChange;
            var dy = e.VerticalChange;

            // check left boundary
            if (viewportRect.Left < vbRect.Left)
                dx = Math.Max(0, dx);
            else if (viewportRect.Left + dx < vbRect.Left)
                    dx = vbRect.Left - viewportRect.Left;

            // check right boundary
            if (viewportRect.Right > vbRect.Right)
                dx = Math.Min(0, dx);
            else if (viewportRect.Right + dx > vbRect.Left + vbRect.Width)
                    dx = vbRect.Left + vbRect.Width - viewportRect.Right;
    
            // check top boundary
            if (viewportRect.Top < vbRect.Top)
                dy = Math.Max(0, dy);
            else if (viewportRect.Top + dy < vbRect.Top)
                    dy = vbRect.Top - viewportRect.Top;

            // check bottom boundary
            if (viewportRect.Bottom > vbRect.Bottom)
                dy = Math.Min(0, dy);
            else if (viewportRect.Bottom + dy > vbRect.Top + vbRect.Height)
                    dy = vbRect.Top + vbRect.Height - viewportRect.Bottom;

            // call the main OnDrag handler that is used when dragging the content directly
            OnDrag(new DragDeltaEventArgs(-dx / scale / _viewboxFactor, -dy / scale / _viewboxFactor));

            // for a drag operation, update the origin with each delta
            _originPoint = _originPoint + new Vector(dx, dy);
        }

        private void ResizeDisplayViewport(DragDeltaEventArgs e, ResizeEdge relativeTo)
        {
            return;// NOT IMPLEMENTED
            // get the existing viewport rect and scale
/*
            var viewportRect = _viewFinderDisplay.ViewportRect;
            var scale = _viewFinderDisplay.Scale;

            // ensure that we stay within the bounds of the VisualBrush
            var x = Math.Max(_resizeViewportBounds.Left, Math.Min(_resizeDraggingPoint.X + e.HorizontalChange, _resizeViewportBounds.Right));
            var y = Math.Max(_resizeViewportBounds.Top, Math.Min(_resizeDraggingPoint.Y + e.VerticalChange, _resizeViewportBounds.Bottom));

            // get the selected region in the coordinate space of the content
            var anchorPoint = new Point(_resizeAnchorPoint.X / scale, _resizeAnchorPoint.Y / scale);
            var newRegionVector = new Vector((x - _resizeAnchorPoint.X) / scale / _viewboxFactor, (y - _resizeAnchorPoint.Y) / scale / _viewboxFactor);
            var region2 = new Rect(anchorPoint, newRegionVector);

            // now translate the region from the coordinate space of the content 
            // to the coordinate space of the content presenter
            var region =
              new Rect(
                (Content as UIElement).TranslatePoint(region2.TopLeft, _presenter),
                (Content as UIElement).TranslatePoint(region2.BottomRight, _presenter));

            // calculate actual scale value
            var aspectX = RenderSize.Width / region.Width;
            var aspectY = RenderSize.Height / region.Height;
            scale = aspectX < aspectY ? aspectX : aspectY;

            // scale relative to the anchor point
            ZoomToInternal(region2);
*/
        }

        private void OnDrag(DragDeltaEventArgs e)
        {
           /* Point relativePosition = _relativePosition;
            double scale = this.Scale;
            Point newPosition = relativePosition + (this.ContentOffset * scale) + new Vector(e.HorizontalChange * scale, e.VerticalChange * scale);*/
            var dd = new Vector(e.HorizontalChange * Zoom * _viewboxFactor, e.VerticalChange * Zoom * _viewboxFactor);
            TranslateX += dd.X;
            TranslateY += dd.Y;
            UpdateViewport();
        }

        #endregion

        #region Updates & Refreshes

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // when the size is changing, the viewbox factor must be updated before updating the view
            UpdateViewboxFactor();
        }

        //private Rect _prevAreaSize;
        internal void UpdateViewport()
        {
            // if we haven't attached to the visual tree yet or we don't have content, just return
            if (ContentVisual == null || _viewFinderDisplay == null)
                return;

            var size = IsContentTrackable ? TrackableContent.ContentSize : new Rect(0,0, ContentVisual.DesiredSize.Width, ContentVisual.DesiredSize.Height);
            if (double.IsInfinity(size.X) || double.IsInfinity(size.Y)) return;

            // calculate the current viewport
            var viewport =
                new Rect(
                    TranslatePoint(new Point(0, 0), ContentVisual),
                    TranslatePoint(new Point(ActualWidth, ActualHeight), ContentVisual));
            viewport.X -= size.X; 
            viewport.Y -= size.Y;


            // if the viewport has changed, set the Viewport dependency property
            if (!DoubleHelper.AreVirtuallyEqual(viewport, Viewport))
            {
                SetValue(ViewportPropertyKey, viewport);
            }

            if (viewport.IsEmpty)
                _viewFinderDisplay.ViewportRect = viewport;
            else
            {
                // adjust the viewport from the coordinate space of the Content element
                // to the coordinate space of the view finder display panel
                var scale = _viewFinderDisplay.Scale;// *_viewboxFactor;
                _viewFinderDisplay.ViewportRect = new Rect(viewport.Left * scale, viewport.Top * scale, viewport.Width * scale, viewport.Height * scale);
            }
        }

        private void UpdateViewFinderDisplayContentBounds()
        {
            if (ContentVisual == null || _viewFinderDisplay == null)
                return;

            /*if (DesignerProperties.GetIsInDesignMode(this))
            {
                _viewFinderDisplay.ContentBounds = new Rect(new Size(100, 100));
                return;
            }*/

            UpdateViewboxFactor();
            
            // ensure the display panel has a size
            var contentSize = IsContentTrackable ? TrackableContent.ContentSize.Size : ContentVisual.DesiredSize;

            var viewFinderSize = _viewFinderDisplay.AvailableSize;
            if (viewFinderSize.Width > 0d && DoubleHelper.AreVirtuallyEqual(viewFinderSize.Height, 0d))
            {
                // update height to accomodate width, while keeping a ratio equal to the actual content
                viewFinderSize = new Size(viewFinderSize.Width, contentSize.Height * viewFinderSize.Width / contentSize.Width);
            }
            else if (viewFinderSize.Height > 0d && DoubleHelper.AreVirtuallyEqual(viewFinderSize.Width, 0d))
            {
                // update width to accomodate height, while keeping a ratio equal to the actual content
                viewFinderSize = new Size(contentSize.Width * viewFinderSize.Height / contentSize.Height, viewFinderSize.Width);
            }

            // determine the scale of the view finder display panel
            var aspectX = viewFinderSize.Width / contentSize.Width;
            var aspectY = viewFinderSize.Height / contentSize.Height;
            var scale = aspectX < aspectY ? aspectX : aspectY;
            if (DesignerProperties.GetIsInDesignMode(this)) scale = 0.8;

            // determine the rect of the VisualBrush
            var vbWidth = contentSize.Width * scale;
            var vbHeight = contentSize.Height * scale;

            // set the ContentBounds and Scale properties on the view finder display panel
            _viewFinderDisplay.Scale = scale;
            _viewFinderDisplay.ContentBounds = new Rect(new Size(vbWidth, vbHeight));
        }

        private void UpdateViewboxFactor()
        {
            if (ContentVisual == null) return;
            var contentWidth = ActualWidth;
            var trueContentWidth = IsContentTrackable ? TrackableContent.ContentSize.Width : ContentVisual.DesiredSize.Width;
            if (contentWidth <=1  || trueContentWidth <= 1) _viewboxFactor = 1d;
            else _viewboxFactor = contentWidth / trueContentWidth;
        }
        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if animation should be disabled
        /// </summary>
        public bool IsAnimationDisabled { get; set; }

        /// <summary>
        /// Use Ctrl key to zoom with mouse wheel or without it
        /// </summary>
        public bool UseCtrlForMouseWheel { get; set; }

        /// <summary>
        /// Gets or sets mousewheel zooming mode. Positional: depend on mouse position. Absolute: center area.
        /// </summary>
        public MouseWheelZoomingMode MouseWheelZoomingMode { get; set; }

        /// <summary>
        /// Fires when area has been selected using SelectionModifiers 
        /// </summary>
        public event AreaSelectedEventHandler AreaSelected;

        private void OnAreaSelected(Rect selection)
        {
            if (AreaSelected != null)
                AreaSelected(this, new AreaSelectedEventArgs(selection));
        }

        private const string PART_PRESENTER = "PART_Presenter";

        public static readonly DependencyProperty HideZoomProperty =
            DependencyProperty.Register("HideZoom", typeof(Visibility), typeof(ZoomControl),
                                        new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty AnimationLengthProperty =
            DependencyProperty.Register("AnimationLength", typeof(TimeSpan), typeof(ZoomControl),
                                        new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        public static readonly DependencyProperty MaximumZoomStepProperty =
            DependencyProperty.Register("MaximumZoomValueValue", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(5.0));

        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomControl), new PropertyMetadata(100.0));

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomControl), new PropertyMetadata(0.01));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ZoomControlModes), typeof(ZoomControl),
                                        new PropertyMetadata(ZoomControlModes.Custom, Mode_PropertyChanged));

        private static void Mode_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            var mode = (ZoomControlModes)e.NewValue;
            switch (mode)
            {
                case ZoomControlModes.Fill:
                    zc.DoZoomToFill();
                    break;
                case ZoomControlModes.Original:
                    zc.DoZoomToOriginal();
                    break;
                case ZoomControlModes.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly DependencyProperty ModifierModeProperty =
            DependencyProperty.Register("ModifierMode", typeof(ZoomViewModifierMode), typeof(ZoomControl),
                                        new PropertyMetadata(ZoomViewModifierMode.None));

        #region TranslateX TranslateY
        public static readonly DependencyProperty TranslateXProperty =
            DependencyProperty.Register("TranslateX", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(0.0, TranslateX_PropertyChanged, TranslateX_Coerce));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(0.0, TranslateY_PropertyChanged, TranslateY_Coerce));

        private static object TranslateX_Coerce(DependencyObject d, object basevalue)
        {
            var zc = (ZoomControl)d;
            return zc.GetCoercedTranslateX((double)basevalue, zc.Zoom);
        }

        private double GetCoercedTranslateX(double baseValue, double zoom)
        {
            return _presenter == null ? 0.0 : baseValue;
        }

        private static object TranslateY_Coerce(DependencyObject d, object basevalue)
        {
            var zc = (ZoomControl)d;
            return zc.GetCoercedTranslateY((double)basevalue, zc.Zoom);
        }

        private double GetCoercedTranslateY(double baseValue, double zoom)
        {
            return _presenter == null ? 0.0 : baseValue;
        }

        private static void TranslateX_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            if (zc._translateTransform == null)
                return;
            zc._translateTransform.X = (double)e.NewValue;
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
            zc.OnPropertyChanged("Presenter");
            zc.Presenter.OnPropertyChanged("RenderTransform");
        }

        private static void TranslateY_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;
            if (zc._translateTransform == null)
                return;
            zc._translateTransform.Y = (double)e.NewValue;
            if (!zc._isZooming)
                zc.Mode = ZoomControlModes.Custom;
            zc.OnPropertyChanged("Presenter");
            zc.Presenter.OnPropertyChanged("RenderTransform");

        }

        #endregion

        public static readonly DependencyProperty ZoomBoxBackgroundProperty =
            DependencyProperty.Register("ZoomBoxBackground", typeof(Brush), typeof(ZoomControl),
                                        new PropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxBorderBrushProperty =
            DependencyProperty.Register("ZoomBoxBorderBrush", typeof(Brush), typeof(ZoomControl),
                                        new PropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxBorderThicknessProperty =
            DependencyProperty.Register("ZoomBoxBorderThickness", typeof(Thickness), typeof(ZoomControl),
                                        new PropertyMetadata(null));


        public static readonly DependencyProperty ZoomBoxOpacityProperty =
            DependencyProperty.Register("ZoomBoxOpacity", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(0.5));


        public static readonly DependencyProperty ZoomBoxProperty =
            DependencyProperty.Register("ZoomBox", typeof(Rect), typeof(ZoomControl),
                                        new PropertyMetadata(new Rect()));

        public static readonly DependencyProperty ZoomSensitivityProperty =
            DependencyProperty.Register("ZoomSensitivity", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(100.0));

        #region Zoom
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(1.0, Zoom_PropertyChanged));

        private static void Zoom_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zc = (ZoomControl)d;

            if (zc._scaleTransform == null)
                return;

            var zoom = (double)e.NewValue;
            zc._scaleTransform.ScaleX = zoom;
            zc._scaleTransform.ScaleY = zoom;
            if (!zc._isZooming)
            {
                var delta = (double)e.NewValue / (double)e.OldValue;
                zc.TranslateX *= delta;
                zc.TranslateY *= delta;
                zc.Mode = ZoomControlModes.Custom;
            }
            zc.OnPropertyChanged("Presenter");
            zc.Presenter.OnPropertyChanged("RenderTransform");
            zc.OnPropertyChanged("Zoom");
            zc.UpdateViewport();
        }
        #endregion

        private Point _mouseDownPos;
        private ZoomContentPresenter _presenter;

        /// <summary>
        /// Applied to the presenter.
        /// </summary>
        private ScaleTransform _scaleTransform;
        private Vector _startTranslate;
        private TransformGroup _transformGroup;

        /// <summary>
        /// Applied to the scrollviewer.
        /// </summary>
        private TranslateTransform _translateTransform;

        private bool _isZooming;

        public Brush ZoomBoxBackground
        {
            get { return (Brush)GetValue(ZoomBoxBackgroundProperty); }
            set { SetValue(ZoomBoxBackgroundProperty, value); }
        }

        public Brush ZoomBoxBorderBrush
        {
            get { return (Brush)GetValue(ZoomBoxBorderBrushProperty); }
            set { SetValue(ZoomBoxBorderBrushProperty, value); }
        }

        public Thickness ZoomBoxBorderThickness
        {
            get { return (Thickness)GetValue(ZoomBoxBorderThicknessProperty); }
            set { SetValue(ZoomBoxBorderThicknessProperty, value); }
        }

        public double ZoomBoxOpacity
        {
            get { return (double)GetValue(ZoomBoxOpacityProperty); }
            set { SetValue(ZoomBoxOpacityProperty, value); }
        }

        public Rect ZoomBox
        {
            get { return (Rect)GetValue(ZoomBoxProperty); }
            set { SetValue(ZoomBoxProperty, value); }
        }

        /// <summary>
        /// Gets origo (area center) position
        /// </summary>
        public Point OrigoPosition
        {
            get { return new Point(ActualWidth / 2, ActualHeight / 2); }
        }

        /// <summary>
        /// Gets or sets translation value for X property
        /// </summary>
        public double TranslateX
        {
            get 
            {
                var value = (double)GetValue(TranslateXProperty);
                return double.IsNaN(value) ? 0 : value; 
            }
            set
            {
                BeginAnimation(TranslateXProperty, null);
                SetValue(TranslateXProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets translation value for Y property
        /// </summary>
        public double TranslateY
        {
            get {
                var value = (double)GetValue(TranslateYProperty);
                return double.IsNaN(value) ? 0 : value;
            }
            set
            {
                BeginAnimation(TranslateYProperty, null);
                SetValue(TranslateYProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets animation length
        /// </summary>
        public TimeSpan AnimationLength
        {
            get { return (TimeSpan)GetValue(AnimationLengthProperty); }
            set { SetValue(AnimationLengthProperty, value); }
        }

        /// <summary>
        /// Minimum zoom distance (Zoom out)
        /// </summary>
        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }

        /// <summary>
        /// Maximum zoom distance (Zoom in)
        /// </summary>
        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }

        /// <summary>
        /// Maximum value for zoom step (how fast the zoom can do)
        /// </summary>
        public double MaximumZoomStep
        {
            get { return (double)GetValue(MaximumZoomStepProperty); }
            set { SetValue(MaximumZoomStepProperty, value); }
        }

        /// <summary>
        /// Gets or sets zoom sensitivity. Lower the value - smoother the zoom.
        /// </summary>
        public double ZoomSensitivity
        {
            get { return (double)GetValue(ZoomSensitivityProperty); }
            set { SetValue(ZoomSensitivityProperty, value); }
        }

        /// <summary>
        /// Gets or sets current zoom value
        /// </summary>
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set
            {
                if (value == (double)GetValue(ZoomProperty))
                    return;
                BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, value);
            }
        }

        /// <summary>
        /// Gets content object as UIElement
        /// </summary>
        public UIElement ContentVisual
        {
            get
            {
                return Content as UIElement;
            }
        }
        /// <summary>
        /// Gets content as ITrackableContent like GraphArea
        /// </summary>
        public ITrackableContent TrackableContent
        {
            get
            {
                return Content as ITrackableContent;
            }
        }

        bool _isga;
        /// <summary>
        /// Is loaded content represents ITrackableContent object
        /// </summary>
        public bool IsContentTrackable
        {
            get { return _isga; }
        }


        public ZoomContentPresenter Presenter
        {
            get { return _presenter; }
            set
            {
                _presenter = value;
                if (_presenter == null)
                    return;

                //add the ScaleTransform to the presenter
                _transformGroup = new TransformGroup();
                _scaleTransform = new ScaleTransform();
                _translateTransform = new TranslateTransform();
                _transformGroup.Children.Add(_scaleTransform);
                _transformGroup.Children.Add(_translateTransform);
                _presenter.RenderTransform = _transformGroup;
                _presenter.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public UIElement PresenterVisual
        {
            get { return Presenter; }
        }

        /// <summary>
        /// Gets or sets the active modifier mode.
        /// </summary>
        public ZoomViewModifierMode ModifierMode
        {
            get { return (ZoomViewModifierMode)GetValue(ModifierModeProperty); }
            set { SetValue(ModifierModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the mode of the zoom control.
        /// </summary>
        public ZoomControlModes Mode
        {
            get { return (ZoomControlModes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        protected RoutedUICommand CommandZoomIn = new RoutedUICommand("Zoom In", "ZoomIn", typeof(ZoomControl));
        protected RoutedUICommand CommandZoomOut = new RoutedUICommand("Zoom Out", "ZoomOut", typeof(ZoomControl));

        #endregion

        static ZoomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomControl), new FrameworkPropertyMetadata(typeof(ZoomControl)));
        }

        public ZoomControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                //Mode = ZoomControlModes.Fill;
                //Zoom = 0.5;             
            }
            else
            {
                PreviewMouseWheel += ZoomControl_MouseWheel;
                PreviewMouseDown += ZoomControl_PreviewMouseDown;
                MouseDown += ZoomControl_MouseDown;
                MouseUp += ZoomControl_MouseUp;
                UseCtrlForMouseWheel = true;

                AddHandler(SizeChangedEvent, new SizeChangedEventHandler(OnSizeChanged), true);

                BindCommand(Refocus, RefocusView, CanRefocusView);
                BindCommand(Center, CenterContent);
                BindCommand(Fill, FillToBounds);
                BindCommand(Fit, FitToBounds);

                BindKey(CommandZoomIn, Key.Up, ModifierKeys.Control, 
                    (sender, args) => MouseWheelAction(120, OrigoPosition));
                BindKey(CommandZoomOut, Key.Down, ModifierKeys.Control, 
                    (sender, args) => MouseWheelAction(-120, OrigoPosition));

                this.PreviewKeyDown += ZoomControl_PreviewKeyDown;

                Loaded += ZoomControl_Loaded;
            }
        }

        void ZoomControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        protected void BindCommand(RoutedUICommand command, ExecutedRoutedEventHandler execute, CanExecuteRoutedEventHandler canExecute = null)
        {
            var binding = new CommandBinding(command, execute, canExecute);
            CommandBindings.Add(binding);
        }

        protected void BindKey(RoutedUICommand command, Key key, ModifierKeys modifier, ExecutedRoutedEventHandler execute)
        {
            var binding = new CommandBinding(command, execute);
            CommandBindings.Add(binding);
            InputBindings.Add(new KeyBinding(command, key, modifier));
        }


        void ZoomControl_Loaded(object sender, RoutedEventArgs e)
        {
            //FakeZoom();
        }

        #region ContentChanged
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (oldContent != null)
            {
                var old = oldContent as ITrackableContent;
                if (old != null) old.ContentSizeChanged -= Content_ContentSizeChanged;
            }
            if (newContent != null)
            {
                UpdateViewFinderDisplayContentBounds();
                UpdateViewport();
                var newc = newContent as ITrackableContent;
                if (newc != null)
                {
                    _isga = true;
                    newc.ContentSizeChanged += Content_ContentSizeChanged;
                }
                else _isga = false;
            }
                
            base.OnContentChanged(oldContent, newContent);
        }

        void Content_ContentSizeChanged(object sender, ContentSizeChangedEventArgs e)
        {
            UpdateViewFinderDisplayContentBounds();
            UpdateViewport();
        }
        #endregion

        #region Mouse controls

        /// <summary>
        /// Converts screen rectangle area to rectangle in content coordinate space according to scale and translation
        /// </summary>
        /// <param name="screenRectangle">Screen rectangle data</param>
        public Rect ToContentRectangle(Rect screenRectangle)
        {
            var tl = TranslatePoint(new Point(screenRectangle.X, screenRectangle.Y), ContentVisual);
            //var br = TranslatePoint(new Point(screenRectangle.Right, screenRectangle.Bottom), ContentVisual);
            //return new Rect(tl.X, tl.Y, Math.Abs(Math.Abs(br.X) - Math.Abs(tl.X)), Math.Abs(Math.Abs(br.Y) - Math.Abs(tl.Y)));

            return new Rect(tl.X, tl.Y, screenRectangle.Width / Zoom, screenRectangle.Height / Zoom);
        }

        private void ZoomControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var handle = ((Keyboard.Modifiers & ModifierKeys.Control) > 0 && ModifierMode == ZoomViewModifierMode.None) || UseCtrlForMouseWheel;
            if (!handle) return;

            e.Handled = true;
            MouseWheelAction(e);
        }

        private void MouseWheelAction(MouseWheelEventArgs e)
        {
            MouseWheelAction(e.Delta, e.GetPosition(this));
        }

        /// <summary>
        /// Defines action on mousewheel
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="mousePosition"></param>
        protected virtual void MouseWheelAction(int delta, Point mousePosition)
        {
            var origoPosition = OrigoPosition;

            DoZoom(
                Math.Max(1 / MaximumZoomStep, Math.Min(MaximumZoomStep, (Math.Abs(delta) / 10000.0 * ZoomSensitivity + 1))),
                delta < 0 ? -1 : 1,
                origoPosition,
                MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? origoPosition : mousePosition,
                MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? origoPosition : mousePosition);
        }

        private void ZoomControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (ModifierMode)
            {
                case ZoomViewModifierMode.None:
                    return;
                case ZoomViewModifierMode.Pan:
                    break;
                case ZoomViewModifierMode.ZoomIn:
                    break;
                case ZoomViewModifierMode.ZoomOut:
                    break;
                case ZoomViewModifierMode.ZoomBox:
                    if (_startedAsAreaSelection)
                    {
                        _startedAsAreaSelection = false;

                        OnAreaSelected(ToContentRectangle(ZoomBox));
                        ZoomBox = Rect.Empty;
                    }
                    else ZoomToInternal(ZoomBox);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ModifierMode = ZoomViewModifierMode.None;
            PreviewMouseMove -= ZoomControl_PreviewMouseMove;
            ReleaseMouseCapture();
        }

        private void ZoomControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            switch (ModifierMode)
            {
                case ZoomViewModifierMode.None:
                    return;
                case ZoomViewModifierMode.Pan:
                    var translate = _startTranslate + (e.GetPosition(this) - _mouseDownPos);
                    TranslateX = translate.X;
                    TranslateY = translate.Y;
                    UpdateViewport();
                    break;
                case ZoomViewModifierMode.ZoomIn:
                    break;
                case ZoomViewModifierMode.ZoomOut:
                    break;
                case ZoomViewModifierMode.ZoomBox:
                    var pos = e.GetPosition(this);
                    var x = Math.Min(_mouseDownPos.X, pos.X);
                    var y = Math.Min(_mouseDownPos.Y, pos.Y);
                    var sizeX = Math.Abs(_mouseDownPos.X - pos.X);
                    var sizeY = Math.Abs(_mouseDownPos.Y - pos.Y);
                    ZoomBox = new Rect(x, y, sizeX, sizeY);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ZoomControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseDown(e, false);
        }

        private void ZoomControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseDown(e, true);
            e.Handled = false;
        }

        private bool _startedAsAreaSelection;
        private void OnMouseDown(MouseButtonEventArgs e, bool isPreview)
        {
            if (ModifierMode != ZoomViewModifierMode.None)
                return;
            _startedAsAreaSelection = false;
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.None:
                    if (!isPreview)
                        ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Alt | ModifierKeys.Control:
                    _startedAsAreaSelection = true;
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;
                case ModifierKeys.Alt:
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;
                case ModifierKeys.Control:
                    break;
                case ModifierKeys.Shift:
                    ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case ModifierKeys.Windows:
                    break;
                default:
                    return;
            }

            if (ModifierMode == ZoomViewModifierMode.None)
                return;

            _mouseDownPos = e.GetPosition(this);
            _startTranslate = new Vector(TranslateX, TranslateY);
            Mouse.Capture(this);
            PreviewMouseMove += ZoomControl_PreviewMouseMove;
        }
        #endregion

        #region Animation

        public event EventHandler ZoomAnimationCompleted;

        private void OnZoomAnimationCompleted()
        {
            if (ZoomAnimationCompleted != null)
                ZoomAnimationCompleted(this, EventArgs.Empty);
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY, bool isZooming = true)
        {
            if (targetZoom == 0d && double.IsNaN(transformX) && double.IsNaN(transformY)) return;
            _isZooming = isZooming;
            var duration = !IsAnimationDisabled ? new Duration(AnimationLength) : new Duration(new TimeSpan(0,0,0,0,100));
            var value = (double)GetValue(TranslateXProperty);
            if (double.IsNaN(value) || double.IsInfinity(value)) SetValue(TranslateXProperty, 0d);
            value = (double)GetValue(TranslateYProperty);
            if (double.IsNaN(value) || double.IsInfinity(value)) SetValue(TranslateYProperty, 0d);
            StartAnimation(TranslateXProperty, transformX, duration);
            if (double.IsNaN(transformY) || double.IsInfinity(transformY)) transformY = 0;
            StartAnimation(TranslateYProperty, transformY, duration);
            if (double.IsNaN(targetZoom) || double.IsInfinity(targetZoom)) targetZoom = 1;
            StartAnimation(ZoomProperty, targetZoom, duration);
        }

        private void StartAnimation(DependencyProperty dp, double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                if (dp == ZoomProperty)
                {
                    _isZooming = false;
                }
                return;
            }
            var animation = new DoubleAnimation(toValue, duration);
            if (dp == ZoomProperty)
            {
                _zoomAnimCount++;
                animation.Completed += ZoomCompleted;
            }
            BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        private int _zoomAnimCount;

        void ZoomCompleted(object sender, EventArgs e)
        {
            _zoomAnimCount--;
            if (_zoomAnimCount > 0)
                return;
            var zoom = Zoom;          
            BeginAnimation(ZoomProperty, null);
            SetValue(ZoomProperty, zoom);
            _isZooming = false;
            UpdateViewport();
            OnZoomAnimationCompleted();
        }

        #endregion

        /// <summary>
        /// Zoom to rectangle area (MAY BE DEPRECATED). Use ZoomToContent method instead.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="setDelta"></param>
        public void ZoomTo(Rect rect, bool setDelta = false)
        {
            ZoomToInternal(rect, setDelta);
            UpdateViewFinderDisplayContentBounds();
            UpdateViewport();
        }

        /// <summary>
        /// Zoom to rectangle area of the content
        /// </summary>
        /// <param name="rectangle">Rectangle area</param>
        /// <param name="usingContentCoordinates">Sets if content coordinates or screen coordinates was specified</param>
        public void ZoomToContent(Rect rectangle, bool usingContentCoordinates = true)
        {
            //if content isn't UIElement - return
            if (ContentVisual == null) return;
            // translate the region from the coordinate space of the content 
            // to the coordinate space of the content presenter
            var region = usingContentCoordinates ?
              new Rect(
                ContentVisual.TranslatePoint(rectangle.TopLeft, _presenter),
                ContentVisual.TranslatePoint(rectangle.BottomRight, _presenter)) : rectangle;
            
            // calculate actual zoom, which must fit the entire selection 
            // while maintaining a 1:1 ratio
            var aspectX = ActualWidth / region.Width;
            var aspectY = ActualHeight / region.Height;
            var newRelativeScale = aspectX < aspectY ? aspectX : aspectY;
            // ensure that the scale value alls within the valid range
            if (newRelativeScale > MaxZoom)
                newRelativeScale = MaxZoom;
            else if (newRelativeScale < MinZoom)
                newRelativeScale = MinZoom;

            var center = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            var newRelativePosition = new Point((ActualWidth / 2 - center.X) * Zoom, (ActualHeight / 2 - center.Y) * Zoom);

            TranslateX = newRelativePosition.X;
            TranslateY = newRelativePosition.Y;
            Zoom = newRelativeScale;
        }

        /// <summary>
        /// Zoom to original size
        /// </summary>
        public void ZoomToOriginal()
        {
            if (Mode == ZoomControlModes.Original)
                DoZoomToOriginal();
            else Mode = ZoomControlModes.Original;
        }

        /// <summary>
        /// Centers content on the screen
        /// </summary>
        public void CenterContent()
        {
            if (_presenter == null)
                return;

            var initialTranslate = GetTrackableTranslate();
            DoZoomAnimation(Zoom, initialTranslate.X*Zoom, initialTranslate.Y*Zoom);
        }

        /// <summary>
        /// Zoom to fill screen area with the content
        /// </summary>
        public void ZoomToFill()
        {
            if(Mode == ZoomControlModes.Fill)
                DoZoomToFill();
            else Mode = ZoomControlModes.Fill;
        }

        private void ZoomToInternal(Rect rect, bool setDelta = false)
        {
            var deltaZoom = Math.Min(ActualWidth / rect.Width, ActualHeight / rect.Height);
            var startHandlePosition = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            DoZoom(deltaZoom, 1, OrigoPosition, startHandlePosition, OrigoPosition, setDelta);
            ZoomBox = new Rect();
        }

        /// <summary>
        /// Returns initial translate depending on container graph settings (to deal with optinal new coord system)
        /// </summary>
        private Vector GetTrackableTranslate()
        {
            if (!IsContentTrackable) return new Vector();
            return DesignerProperties.GetIsInDesignMode(this) ? GetInitialTranslate(200,100) : GetInitialTranslate(TrackableContent.ContentSize.Width, TrackableContent.ContentSize.Height, TrackableContent.ContentSize.X, TrackableContent.ContentSize.Y);
        }

        private void DoZoomToOriginal()
        {
            if (_presenter == null)
                return;

            var initialTranslate = GetTrackableTranslate();
            DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y);
        }

        private Vector GetInitialTranslate(double contentWidth, double contentHeight, double offsetX = 0, double offsetY = 0)
        {
            if (_presenter == null)
                return new Vector(0.0, 0.0);
            var w = contentWidth - ActualWidth;
            var h = contentHeight - ActualHeight;
            var tX = -(w / 2.0 + offsetX);
            var tY = -(h / 2.0 + offsetY);

            return new Vector(tX, tY);
        }

        private void DoZoomToFill()
        {
            if (_presenter == null)
                return;
            var c = IsContentTrackable ? TrackableContent.ContentSize.Size : ContentVisual.DesiredSize;
            if (c.Width == 0 || double.IsNaN(c.Width) || double.IsInfinity(c.Width)) return;

            var deltaZoom = Math.Min(MaxZoom,Math.Min( ActualWidth / (c.Width), ActualHeight / (c.Height)));
            var initialTranslate = IsContentTrackable ? GetTrackableTranslate() : GetInitialTranslate(c.Width, c.Height);
            DoZoomAnimation(deltaZoom, initialTranslate.X * deltaZoom, initialTranslate.Y * deltaZoom);
        }

        private void DoZoom(double deltaZoom, int mod, Point origoPosition, Point startHandlePosition, Point targetHandlePosition, bool setDelta = false)
        {
            var startZoom = Zoom;
            var currentZoom = setDelta ? deltaZoom : (mod == -1 ? (startZoom / deltaZoom) : (startZoom * deltaZoom));
            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            var startTranslate = new Vector(TranslateX, TranslateY);

            var v = (startHandlePosition - origoPosition);
            var vTarget = (targetHandlePosition - origoPosition);

            var targetPoint = (v - startTranslate) / startZoom;
            var zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
            var endTranslate = vTarget - zoomedTargetPointPos;


            if (setDelta)
            {
                var transformX = GetCoercedTranslateX(endTranslate.X, currentZoom);
                var transformY = GetCoercedTranslateY(endTranslate.Y, currentZoom);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            else
            {
                var transformX = GetCoercedTranslateX(TranslateX + endTranslate.X, currentZoom);
                var transformY = GetCoercedTranslateY(TranslateY + endTranslate.Y, currentZoom);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            Mode = ZoomControlModes.Custom;
        }

        /*private void FakeZoom()
        {
            var startZoom = Zoom;
            var currentZoom = startZoom;
            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            var startTranslate = new Vector(TranslateX, TranslateY);

            var v = (OrigoPosition - OrigoPosition);
            var vTarget = (OrigoPosition - OrigoPosition);

            var targetPoint = (v - startTranslate) / startZoom;
            var zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
            var endTranslate = vTarget - zoomedTargetPointPos;

            var transformX = GetCoercedTranslateX(TranslateX + endTranslate.X, currentZoom);
            var transformY = GetCoercedTranslateY(TranslateY + endTranslate.Y, currentZoom);
            DoZoomAnimation(currentZoom, transformX, transformY);
        }*/

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                if (ViewFinder != null)
                    ViewFinder.Visibility = Visibility.Collapsed;
                return;
            }

            //get the presenter, and initialize
            Presenter = GetTemplateChild(PART_PRESENTER) as ZoomContentPresenter;
            if (Presenter != null)
            {
                Presenter.SizeChanged += (s, a) =>
                                             {
                                                 //UpdateViewFinderDisplayContentBounds();
                                                 UpdateViewport();
                                                 if (Mode == ZoomControlModes.Fill)
                                                     DoZoomToFill();
                                             };
                Presenter.ContentSizeChanged += (s, a) =>
                {
                    //UpdateViewFinderDisplayContentBounds();
                    if (Mode == ZoomControlModes.Fill)
                    {
                        DoZoomToFill();
                        //IsAnimationDisabled = false;
                    }
                };
            }
            ZoomToFill();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public delegate void ValueChangedEventArgs(object sender, DependencyPropertyChangedEventArgs args);
}
