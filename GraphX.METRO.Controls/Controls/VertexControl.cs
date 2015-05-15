using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using GraphX.METRO.Controls.Models;
using GraphX.PCL.Common.Enums;

namespace GraphX.METRO.Controls
{
	/// <summary>
	/// Visual vertex control
	/// </summary>
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerLeave")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    public class VertexControl : Control, IGraphControl
    {
        #region Properties
        /// <summary>
        /// Provides settings for event calls within single vertex control
        /// </summary>
        public VertexEventOptions EventOptions { get; private set; }

        private double _labelAngle;
        /// <summary>
        /// Gets or sets vertex label angle
        /// </summary>
        public double LabelAngle
        {
            get
            {
                return _vertexLabelControl != null ? _vertexLabelControl.Angle : _labelAngle;
            }
            set
            {
                _labelAngle = value;
                if (_vertexLabelControl != null) _vertexLabelControl.Angle = _labelAngle;
            }
        }

        /// <summary>
        /// Gets or sets actual shape form of vertex control (affects mostly math calculations such edges connectors)
        /// </summary>
        public VertexShape VertexShape
        {
            get { return (VertexShape)GetValue(VertexShapeProperty); }
            set { SetValue(VertexShapeProperty, value); }
        }

        public static readonly DependencyProperty VertexShapeProperty =
            DependencyProperty.Register("VertexShape", typeof(VertexShape), typeof(VertexControl), new PropertyMetadata(VertexShape.Rectangle));

        /// <summary>
        /// Gets or sets vertex data object
        /// </summary>
		public object Vertex
		{
			get { return GetValue( VertexProperty ); }
			set { SetValue( VertexProperty, value ); }
		}

		public static readonly DependencyProperty VertexProperty =
            DependencyProperty.Register("Vertex", typeof(object), typeof(VertexControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets vertex control parent GraphArea object (don't need to be set manualy)
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(VertexControl), new PropertyMetadata(null));

        private bool _showLabel;
        public bool ShowLabel
        {
            get { return _showLabel; }
            set
            {
                _showLabel = value;
                if (_vertexLabelControl != null)
                    if(_showLabel) _vertexLabelControl.Show(); else _vertexLabelControl.Hide();
            }
        }

        #endregion

        #region Position trace feature

        #region Attached property tracer
        private static readonly DependencyProperty TestXProperty =
            DependencyProperty.Register("TestX", typeof(double), typeof(VertexControl), new PropertyMetadata(0, testxchanged));

        private static readonly DependencyProperty TestYProperty =
            DependencyProperty.Register("TestY", typeof(double), typeof(VertexControl), new PropertyMetadata(0, testychanged));

        private static void testychanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vc = d as IPositionChangeNotify;
            if (vc != null)
                vc.OnPositionChanged();
        }

        private static void testxchanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vc = d as IPositionChangeNotify;
            if (vc != null)
                vc.OnPositionChanged();
        }
        #endregion

        /// <summary>
        /// Fires when IsPositionTraceEnabled property set and object changes its coordinates.
        /// </summary>
        public event VertexPositionChangedEH PositionChanged;

        protected void OnPositionChanged(Point offset, Point pos)
        {
            if (PositionChanged != null)
                PositionChanged.Invoke(this, new VertexPositionEventArgs(offset, pos, this));
        }

        void IPositionChangeNotify.OnPositionChanged()
        {
            if (ShowLabel && _vertexLabelControl != null)
                _vertexLabelControl.UpdatePosition();
            OnPositionChanged(new Point(), GetPosition());
        }

        #endregion

        #region Position methods

	    /// <summary>
	    /// Set attached coordinates X and Y
	    /// </summary>
	    /// <param name="pt"></param>
	    /// <param name="alsoFinal"></param>
        public void SetPosition(Point pt, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, pt.X, alsoFinal);
            GraphAreaBase.SetY(this, pt.Y, alsoFinal);
        }

        public void SetPosition(double x, double y, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, x, alsoFinal);
            GraphAreaBase.SetY(this, y, alsoFinal);
        }

	    /// <summary>
	    /// Get control position on the GraphArea panel in attached coords X and Y
	    /// </summary>
	    /// <param name="final"></param>
	    /// <param name="round"></param>
        public Point GetPosition(bool final = false, bool round = false)
        {
            return round ? new Point(final ? (int)GraphAreaBase.GetFinalX(this) : (int)GraphAreaBase.GetX(this), final ? (int)GraphAreaBase.GetFinalY(this) : (int)GraphAreaBase.GetY(this)) : new Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
        }
        /// <summary>
        /// Get control position on the GraphArea panel in attached coords X and Y (GraphX type version)
        /// </summary>
        /// <param name="final"></param>
        /// <param name="round"></param>
        internal Measure.Point GetPositionGraphX(bool final = false, bool round = false)
        {
            return round ? new Measure.Point(final ? (int)GraphAreaBase.GetFinalX(this) : (int)GraphAreaBase.GetX(this), final ? (int)GraphAreaBase.GetFinalY(this) : (int)GraphAreaBase.GetY(this)) : new Measure.Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
        }

	    #endregion

        /// <summary>
        /// Create vertex visual control
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        /// <param name="bindToDataObject">Bind DataContext to the Vertex data. True by default. </param>
        public VertexControl(object vertexData,  bool bindToDataObject = true)
        {
            DefaultStyleKey = typeof (VertexControl);
            if (bindToDataObject) DataContext = vertexData;
            Vertex = vertexData;

            EventOptions = new VertexEventOptions(this);
            foreach(var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);

            IsEnabledChanged += VertexControl_IsEnabledChanged;

            var xBinding = new Binding
            {
                Path = new PropertyPath("(Canvas.Left)"),
                Source = this
            };
            SetBinding(TestXProperty, xBinding);
            var yBinding = new Binding
            {
                Path = new PropertyPath("(Canvas.Top)"),
                Source = this
            };
            SetBinding(TestYProperty, yBinding);
        }

        void VertexControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        private IVertexLabelControl _vertexLabelControl;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null) return;
            _vertexLabelControl = this.FindDescendantByName("PART_vertexLabel") as IVertexLabelControl;
            if (_vertexLabelControl != null)
            {
                if(ShowLabel) _vertexLabelControl.Show(); else _vertexLabelControl.Hide();
                UpdateLayout();
                _vertexLabelControl.UpdatePosition();
            }
        }

        #region Events handling

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) PointerPressed += VertexControl_Down;
                    else PointerPressed -= VertexControl_Down;
                    break;
                case EventType.MouseDoubleClick:
                   // if (EventOptions.MouseDoubleClickEnabled) Poi += VertexControl_MouseDoubleClick;
                   // else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;
                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) PointerMoved += VertexControl_MouseMove;
                    else PointerMoved -= VertexControl_MouseMove;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) PointerEntered += VertexControl_MouseEnter;
                    else PointerEntered -= VertexControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) PointerExited += VertexControl_MouseLeave;
                    else PointerExited -= VertexControl_MouseLeave;
                    break;
            }
        }

        void VertexControl_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this, e);
            VisualStateManager.GoToState(this, "PointerLeave", true);

            //e.Handled = true;
        }

        void VertexControl_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseEnter(this, e);
            }
            VisualStateManager.GoToState(this, "PointerOver", true);

            // e.Handled = true;
        }

        void VertexControl_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this, e);

        }

        void VertexControl_MouseDoubleClick(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexDoubleClick(this, e);
            e.Handled = true;
        }

        void VertexControl_Down(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e);
            //e.Handled = true;
            VisualStateManager.GoToState(this, "Pressed", true);
        }
        #endregion

        /// <summary>
        /// Cleans all potential memory-holding code
        /// </summary>
        public void Clean()
        {
            Vertex = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            _vertexLabelControl = null;
            if (EventOptions != null)
            {
                EventOptions.Clean();
            }

        }

        /// <summary>
        /// Get vertex center position
        /// </summary>
        public Point GetCenterPosition(bool final = false)
        {
            var pos = GetPosition();
            return new Point(pos.X + ActualWidth * .5, pos.Y + ActualHeight * .5);
        }

    }
}