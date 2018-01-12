using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using GraphX.Measure;
using GraphX.Controls.Models;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
using Thickness = Windows.UI.Xaml.Thickness;

namespace GraphX.Controls
{
    [TemplatePart(Name = PART_PRESENTER, Type = typeof(ZCP))]
    public class ZoomControl : ContentControl, IZoomControl, INotifyPropertyChanged
    {

        #region Properties

        #region ViewFinderVisibility Attached Property

        public static readonly DependencyProperty ViewFinderVisibilityProperty =
          DependencyProperty.RegisterAttached("ViewFinderVisibility", typeof(Visibility), typeof(ZoomControl),
            new PropertyMetadata(Visibility.Visible));

        public static Visibility GetViewFinderVisibility(DependencyObject d)
        {
            return (Visibility)(d.GetValue(ViewFinderVisibilityProperty));
        }

        public static void SetViewFinderVisibility(DependencyObject d, Visibility value)
        {
            d.SetValue(ViewFinderVisibilityProperty, value);
        }

        #endregion

        public DelegateCommand<object> ZoomToFillCommand { get { return new DelegateCommand<object>((o) => ZoomToFill());} }

        public DelegateCommand<object> CenterToContentCommand { get { return  new DelegateCommand<object>((o) => CenterContent());} }

        /// <summary>
        /// Gets or sets if animation should be disabled
        /// </summary>
        public bool IsAnimationDisabled { get; set; }

        /// <summary>
        /// Use Ctrl key to zoom with mouse wheel or without it
        /// </summary>
        public bool UseCtrlForMouseWheel { get; set; }

        /// <summary>
        /// Gets or sets absolute zooming on mouse wheel which doesn't depend on mouse position
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
            DependencyProperty.Register("MaximumZoomStep", typeof(double), typeof(ZoomControl),
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
                                        new PropertyMetadata(0.0, TranslateX_PropertyChanged));

        public static readonly DependencyProperty TranslateYProperty =
            DependencyProperty.Register("TranslateY", typeof(double), typeof(ZoomControl),
                                        new PropertyMetadata(0.0, TranslateY_PropertyChanged));


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
           /* if (!zc._isZooming)
            {
                var delta = (double)e.NewValue / (double)e.OldValue;
                zc.TranslateX *= delta;
                zc.TranslateY *= delta;
                zc.Mode = ZoomControlModes.Custom;
            }*/
            zc.OnPropertyChanged("Presenter");
            zc.Presenter.OnPropertyChanged("RenderTransform");
            zc.OnPropertyChanged("Zoom");

            //VF zc.UpdateViewport();
        }
        #endregion

        private Point _mouseDownPos;
        private ZCP _presenter;

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

        private Storyboard _currentZoomAnimation;

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

        public Point OrigoPosition
        {
            get { return new Point(ActualWidth / 2, ActualHeight / 2); }
        }

        private Storyboard _lastTranslateXAnimation;
        public double TranslateX
        {
            get 
            {
                var value = (double)GetValue(TranslateXProperty);
                return double.IsNaN(value) ? 0 : value; 
            }
            set
            {
                if (_lastTranslateXAnimation != null)
                {
                    //_lastTranslateXAnimation.SkipToFill();
                    _lastTranslateXAnimation.Stop();
                    //SetValue(TranslateXProperty, TranslateX);
                }
                _lastTranslateXAnimation = AnimationHelper.CreateDoubleAnimation(TranslateX, value, 0, "TranslateX", this, null, (o, e) => SetValue(TranslateXProperty, value));
               // ((DoubleAnimation)_lastTranslateXAnimation.Children[0]).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
                _lastTranslateXAnimation.Begin();
                //SetValue(TranslateXProperty, value);
            }
        }

        private Storyboard _lastTranslateYAnimation;
        public double TranslateY
        {
            get {
                var value = (double)GetValue(TranslateYProperty);
                return double.IsNaN(value) ? 0 : value;
            }
            set
            {
                if (_lastTranslateYAnimation != null)
                {
                    //_lastTranslateYAnimation.SkipToFill();
                    _lastTranslateYAnimation.Stop();
                    //SetValue(TranslateYProperty, TranslateY);
                }

                _lastTranslateYAnimation = AnimationHelper.CreateDoubleAnimation(TranslateY, value, 0, "TranslateY", this, null, (o, e) => SetValue(TranslateYProperty, value));
                //((DoubleAnimation)_lastTranslateYAnimation.Children[0]).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
                _lastTranslateYAnimation.Begin();
                //SetValue(TranslateYProperty, value);
            }
        }

        public TimeSpan AnimationLength
        {
            get { return (TimeSpan)GetValue(AnimationLengthProperty); }
            set { SetValue(AnimationLengthProperty, value); }
        }

        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }

        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }

        public double MaximumZoomStep
        {
            get { return (double)GetValue(MaximumZoomStepProperty); }
            set { SetValue(MaximumZoomStepProperty, value); }
        }

        public double ZoomSensitivity
        {
            get { return (double)GetValue(ZoomSensitivityProperty); }
            set { SetValue(ZoomSensitivityProperty, value); }
        }

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set
            {
                if (value == (double)GetValue(ZoomProperty))
                    return;
                //TODO BeginAnimation(ZoomProperty, null);
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


        public ZCP Presenter
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

        #endregion

        public ZoomControl()
        {
            DefaultStyleKey = typeof (ZoomControl);
            if (DesignMode.DesignModeEnabled)
            {
                //Mode = ZoomControlModes.Fill;

                Loaded += ZoomControl_DesignerLoaded;
            }
            else
            {
                PointerWheelChanged += ZoomControl_MouseWheel;
                PointerPressed += ZoomControl_PreviewMouseDown;
                PointerReleased += ZoomControl_MouseUp;
                UseCtrlForMouseWheel = true;
                Loaded += ZoomControl_Loaded;
            }

        }

        void ZoomControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetValue(ZoomProperty, Zoom);
        }

        void ZoomControl_DesignerLoaded(object sender, RoutedEventArgs e)
        {
            Zoom = 1.0;
        }

        #region ContentChanged
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent != null)
            {
                var old = oldContent as ITrackableContent;
                if (old != null) old.ContentSizeChanged -= Content_ContentSizeChanged;
            }
            if (newContent != null)
            {
                //VF UpdateViewFinderDisplayContentBounds();
                //VF UpdateViewport();
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
            //VF UpdateViewFinderDisplayContentBounds();
            //VF UpdateViewport();
        }
        #endregion

        #region Mouse controls

        /// <summary>
        /// Converts screen rectangle area to rectangle in content coordinate space according to scale and translation
        /// </summary>
        /// <param name="screenRectangle">Screen rectangle data</param>
        public Rect ToContentRectangle(Rect screenRectangle)
        {
            var transformer = TransformToVisual(ContentVisual);
            var tl = transformer.TransformPoint(new Point(screenRectangle.X, screenRectangle.Y));
            var br = transformer.TransformPoint(new Point(screenRectangle.Right, screenRectangle.Bottom));
            return new Rect(tl.X, tl.Y, Math.Abs(Math.Abs(br.X) - Math.Abs(tl.X)), Math.Abs(Math.Abs(br.Y) - Math.Abs(tl.Y)));
        }

        private void ZoomControl_MouseWheel(object sender, PointerRoutedEventArgs e)
        {
            
            var handle = (e.KeyModifiers == VirtualKeyModifiers.Control && ModifierMode == ZoomViewModifierMode.None) || UseCtrlForMouseWheel;
            if (!handle) return;

            e.Handled = true;
            //var origoPosition = new Point(ActualWidth / 2, ActualHeight / 2);
            //var mousePosition = e.GetCurrentPoint(this).Position;
            MouseWheelAction(e.GetCurrentPoint(this).Properties.MouseWheelDelta, e.GetCurrentPoint(this).Position);
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
                 MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? OrigoPosition : mousePosition,
                 MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? OrigoPosition : mousePosition);
        }


        private void ZoomControl_MouseUp(object sender, PointerRoutedEventArgs e)
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
            PointerMoved -= ZoomControl_PreviewMouseMove;
            ReleasePointerCapture(e.Pointer);
        }

        private void ZoomControl_PreviewMouseMove(object sender, PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(this).Position;
            switch (ModifierMode)
            {
                case ZoomViewModifierMode.None:
                    return;
                case ZoomViewModifierMode.Pan:
                    var pps = pos.Subtract(_mouseDownPos);
                    var translatex = _startTranslate.X + pps.X;
                    var translatey = _startTranslate.Y + pps.Y;
                    TranslateX = translatex;
                    TranslateY = translatey;
                    //VF UpdateViewport();
                    break;
                case ZoomViewModifierMode.ZoomIn:
                    break;
                case ZoomViewModifierMode.ZoomOut:
                    break;
                case ZoomViewModifierMode.ZoomBox:
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

       /* private void ZoomControl_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            OnMouseDown(e, false);
        }*/

        private void ZoomControl_PreviewMouseDown(object sender, PointerRoutedEventArgs e)
        {
            OnMouseDown(e, false);
            e.Handled = false;
        }

        private bool _startedAsAreaSelection;
        private void OnMouseDown(PointerRoutedEventArgs e, bool isPreview)
        {
            if (ModifierMode != ZoomViewModifierMode.None)
                return;
            _startedAsAreaSelection = false;
            switch (e.KeyModifiers)
            {
                case VirtualKeyModifiers.None:
                    if (!isPreview)
                        ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                case VirtualKeyModifiers.Windows | VirtualKeyModifiers.Control:
                    _startedAsAreaSelection = true;
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;
                case VirtualKeyModifiers.Windows:
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;
                case VirtualKeyModifiers.Control:
                    break;
                case VirtualKeyModifiers.Shift:
                    ModifierMode = ZoomViewModifierMode.Pan;
                    break;
                default:
                    return;
            }

            if (ModifierMode == ZoomViewModifierMode.None)
                return;

            _mouseDownPos = e.GetCurrentPoint(this).Position;
            _startTranslate = new Vector(TranslateX, TranslateY);
            CapturePointer(e.Pointer);
            PointerMoved += ZoomControl_PreviewMouseMove;
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
            StartAnimation(TranslateXProperty, "TranslateX", transformX, duration);
            if (double.IsNaN(transformY) || double.IsInfinity(transformY)) transformY = 0;
            StartAnimation(TranslateYProperty, "TranslateY", transformY, duration);
            if (double.IsNaN(targetZoom) || double.IsInfinity(targetZoom)) targetZoom = 1;
            StartAnimation(ZoomProperty, "Zoom", targetZoom, duration);
        }

        private void StartAnimation(DependencyProperty dp, string dpName, double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                if (dp == ZoomProperty)
                {
                    _isZooming = false;
                }
                return;
            }

            _currentZoomAnimation = AnimationHelper.CreateDoubleAnimation(null, toValue, duration.TimeSpan.TotalMilliseconds, dpName, this);
            if (dp == ZoomProperty)
            {
                _zoomAnimCount++;
                _currentZoomAnimation.Completed += (s, args) =>
                {
                    _zoomAnimCount--;
                    if (_zoomAnimCount > 0 && _currentZoomAnimation != s)
                        return;
                    var zoom = Zoom;
                    SetValue(ZoomProperty, zoom);
                    _isZooming = false;
                    //VF UpdateViewport();
                    OnZoomAnimationCompleted();
                };
            }
            _currentZoomAnimation.Begin();
        }

        private int _zoomAnimCount;


        #endregion

        /// <summary>
        /// Zoom to rectangle area (MAY BE DEPRECATED). Use ZoomToContent method instead.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="setDelta"></param>
        public void ZoomTo(Rect rect, bool setDelta = false)
        {
            ZoomToInternal(rect, setDelta);
            //VF UpdateViewFinderDisplayContentBounds();
            //VF UpdateViewport();
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
            var transformer = ContentVisual.TransformToVisual(_presenter);
            var region = usingContentCoordinates ?
              new Rect(
                transformer.TransformPoint(new Point(rectangle.Top, rectangle.Left)),
                transformer.TransformPoint(new Point(rectangle.Bottom, rectangle.Right))) : rectangle;
            
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
            return DesignMode.DesignModeEnabled ? GetInitialTranslate(200,100) : GetInitialTranslate(TrackableContent.ContentSize.Width, TrackableContent.ContentSize.Height, TrackableContent.ContentSize.X, TrackableContent.ContentSize.Y);
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
            var c = IsContentTrackable ? TrackableContent.ContentSize.Size() : ContentVisual.DesiredSize;
            var deltaZoom = Math.Min(MaxZoom,Math.Min( ActualWidth / (c.Width), ActualHeight / (c.Height)));
            var initialTranslate = IsContentTrackable ? GetTrackableTranslate() : GetInitialTranslate(c.Width, c.Height);
            DoZoomAnimation(deltaZoom, initialTranslate.X * deltaZoom, initialTranslate.Y * deltaZoom);
        }

        private double GetCoercedTranslate(double baseValue)
        {
            return _presenter == null ? 0.0 : baseValue;
        }

        private void DoZoom(double deltaZoom, int mod, Point origoPosition, Point startHandlePosition, Point targetHandlePosition, bool setDelta = false)
        {
            var startZoom = Zoom;
            var currentZoom = setDelta ? deltaZoom : (mod == -1 ? (startZoom / deltaZoom) : (startZoom * deltaZoom));
            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            var startTranslate = new Point(TranslateX, TranslateY);

            var v = startHandlePosition.Subtract(origoPosition);
            var vTarget = targetHandlePosition.Subtract(origoPosition);

            var targetPoint = v.Subtract(startTranslate).Div(startZoom);
            var zoomedTargetPointPos = targetPoint.Mul(currentZoom).Sum(startTranslate);
            var endTranslate = vTarget.Subtract(zoomedTargetPointPos);


            if (setDelta)
            {
                var transformX = GetCoercedTranslate(endTranslate.X);
                var transformY = GetCoercedTranslate(endTranslate.Y);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            else
            {
                var transformX = GetCoercedTranslate(TranslateX + endTranslate.X);
                var transformY = GetCoercedTranslate(TranslateY + endTranslate.Y);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            Mode = ZoomControlModes.Custom;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //VF AttachToVisualTree();

            //get the presenter, and initialize
            Presenter = GetTemplateChild(PART_PRESENTER) as ZCP;
            if (Presenter != null)
            {
                Presenter.SizeChanged += (s, a) =>
                                             {
                                                 //VF UpdateViewport();
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
}
