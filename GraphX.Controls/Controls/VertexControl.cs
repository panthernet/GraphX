using System.Windows;
using System.Windows.Controls;
using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using GraphX.Controls.Models.Interfaces;
using GraphX.Models;

namespace GraphX
{
	/// <summary>
	/// Visual vertex control
	/// </summary>
    [Serializable]
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(VertexLabelControl))]
    public class VertexControl: Control, IGraphControl
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


        public static readonly DependencyProperty VertexShapeProperty =
            DependencyProperty.Register("VertexShape", typeof(VertexShape), typeof(VertexControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets actual shape form of vertex control (affects mostly math calculations such edges connectors)
        /// </summary>
        public VertexShape VertexShape
        {
            get { return (VertexShape)GetValue(VertexShapeProperty); }
            set { SetValue(VertexShapeProperty, value); }
        }

        /// <summary>
        /// Gets or sets vertex data object
        /// </summary>
		public object Vertex
		{
            get { return GetValue(VertexProperty); }
			set { SetValue( VertexProperty, value ); }
		}

		public static readonly DependencyProperty VertexProperty =
            DependencyProperty.Register("Vertex", typeof(object), typeof(VertexControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets vertex control parent GraphArea object (don't need to be set manualy)
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(VertexControl), new UIPropertyMetadata(null));



        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(VertexControl), new UIPropertyMetadata(false, ShowLabelChanged));

        private static void ShowLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as VertexControl;
            if (obj._vertexLabelControl != null)
                obj._vertexLabelControl.Visibility = ((bool)e.NewValue) ? Visibility.Visible : Visibility.Collapsed;
        }


        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value);}
        }

		static VertexControl()
		{
			//override the StyleKey Property
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VertexControl), new FrameworkPropertyMetadata(typeof(VertexControl)));
		}

        #endregion

        #region Position trace feature

        /// <summary>
        /// Fires when IsPositionTraceEnabled property set and object changes its coordinates.
        /// </summary>
        public event VertexPositionChangedEH PositionChanged;

        protected void OnPositionChanged(Point offset, Point pos)
        {
            if (PositionChanged != null)
                PositionChanged.Invoke(this, new VertexPositionEventArgs(offset, pos, this));
        }

        private DependencyPropertyDescriptor _sxDescriptor;
        private DependencyPropertyDescriptor _syDescriptor;
        internal void UpdatePositionTraceState()
        {
            if (EventOptions.PositionChangeNotification)
            {
                _sxDescriptor = DependencyPropertyDescriptor.FromProperty(GraphAreaBase.XProperty, typeof(VertexControl));
                _sxDescriptor.AddValueChanged(this, source_PositionChanged);
                _syDescriptor = DependencyPropertyDescriptor.FromProperty(GraphAreaBase.YProperty, typeof(VertexControl));
                _syDescriptor.AddValueChanged(this, source_PositionChanged);
            }
            else
            {
                if (_sxDescriptor != null)
                    _sxDescriptor.RemoveValueChanged(this, source_PositionChanged);
                if (_syDescriptor != null)
                    _syDescriptor.RemoveValueChanged(this, source_PositionChanged);
            }
        }

        private void source_PositionChanged(object sender, EventArgs e)
        {
            if(ShowLabel && _vertexLabelControl != null)
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
        /// <param name="tracePositionChange">Listen for the vertex position changed events and fire corresponding event</param>
        /// <param name="bindToDataObject">Bind DataContext to the Vertex data. True by default. </param>
        public VertexControl(object vertexData, bool tracePositionChange = true, bool bindToDataObject = true)
        {
            if (bindToDataObject) DataContext = vertexData;
            Vertex = vertexData;

            EventOptions = new VertexEventOptions(this) { PositionChangeNotification = tracePositionChange };
            foreach(var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);
        }

	    private VertexLabelControl _vertexLabelControl;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                _vertexLabelControl = Template.FindName("PART_vertexLabel", this) as VertexLabelControl;

                if (_vertexLabelControl != null)
                {
                    _vertexLabelControl.Visibility = ShowLabel ? Visibility.Visible : Visibility.Collapsed;
                    UpdateLayout(); 
                    _vertexLabelControl.UpdatePosition();
                }
            }

        }

        #region Events handling

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) MouseDown += VertexControl_Down;
                    else MouseDown -= VertexControl_Down;
                    break;
                case EventType.MouseDoubleClick:
                    if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += VertexControl_MouseDoubleClick;
                    else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;
                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) MouseMove += VertexControl_MouseMove;
                    else MouseMove -= VertexControl_MouseMove;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) MouseEnter += VertexControl_MouseEnter;
                    else MouseEnter -= VertexControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) MouseLeave += VertexControl_MouseLeave;
                    else MouseLeave -= VertexControl_MouseLeave;
                    break;
                case EventType.PositionChangeNotify:
                    UpdatePositionTraceState();
                    break;
            }
            MouseUp += VertexControl_MouseUp;
        }

        void VertexControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseUp(this, e, Keyboard.Modifiers);
            e.Handled = true;
        }

        void VertexControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseLeave(this);
            //e.Handled = true;
        }

        void VertexControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexMouseEnter(this);
           // e.Handled = true;
        }

        void VertexControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null)
                RootArea.OnVertexMouseMove(this);
        }

        void VertexControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexDoubleClick(this);
            e.Handled = true;
        }

        void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnVertexSelected(this, e, Keyboard.Modifiers);
            e.Handled = true;
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
            if (EventOptions != null)
            {
                EventOptions.PositionChangeNotification = false;
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