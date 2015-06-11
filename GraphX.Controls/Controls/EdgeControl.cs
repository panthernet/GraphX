using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using SysRect = System.Windows.Rect;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
    [Serializable]
    public class EdgeControl : EdgeControlBase
    {
        #region Dependency Properties

       public static readonly DependencyProperty StrokeThicknessProperty = Shape.StrokeThicknessProperty.AddOwner(typeof(EdgeControl),
                                                                                                                    new UIPropertyMetadata(5.0));


       /// <summary>
       /// Custom edge thickness
       /// </summary>
       public double StrokeThickness
       {
           get { return (double)GetValue(StrokeThicknessProperty); }
           set { SetValue(StrokeThicknessProperty, value); }
       }


        private static readonly DependencyPropertyKey IsSelfLoopedPropertyKey
    = DependencyProperty.RegisterReadOnly("IsSelfLooped", typeof(bool), typeof(EdgeControl),
    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty IsSelfLoopedPropProperty
            = IsSelfLoopedPropertyKey.DependencyProperty;

        private bool _isSelfLooped { get { return Source != null && Target != null && Source.Vertex == Target.Vertex; } }
        /// <summary>
        /// Gets if this edge is self looped (have same Source and Target)
        /// </summary>
        public override bool IsSelfLooped
        {
            get { return _isSelfLooped; }
            protected set { SetValue(IsSelfLoopedPropertyKey, value); }
        }

        #endregion

        protected override void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            ((EdgeControl)d).ActivateSourceListener();
        }

        protected override void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            ((EdgeControl)d).ActivateTargetListener();
        }


        #region public Clean()
        public override void Clean()
        {
            if(_sourceListener != null)
                _sourceListener.Dispose();
            if(_targetListener != null)
                _targetListener.Dispose();
            if (Source != null)
                Source.PositionChanged -= source_PositionChanged;
            if (Target != null)
                Target.PositionChanged -= source_PositionChanged;
            _oldSource = _oldTarget = null;
            Source = null;
            Target = null;
            Edge = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            _linegeometry = null;
            _linePathObject = null;
            SelfLoopIndicator = null;
            if (_edgeLabelControl != null)
            {
                _edgeLabelControl.Dispose();
                _edgeLabelControl = null;
            }

            if (_edgePointerForSource != null)
            {
                _edgePointerForSource.Dispose();
                _edgePointerForSource = null;
            }
            if (_edgePointerForTarget != null)
            {
                _edgePointerForTarget.Dispose();
                _edgePointerForTarget = null;
            }
            if (EventOptions != null)
                EventOptions.Clean();
        }
        #endregion

        public EdgeControl()
            : this(null, null, null)
        {
        }

        public EdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
        {
            DataContext = edge;
            Source = source; Target = target;
            Edge = edge; DataContext = edge;
            SetCurrentValue(ShowArrowsProperty, showArrows);
            ShowLabel = showLabels;
            IsHiddenEdgesUpdated = true;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                EventOptions = new EdgeEventOptions(this);
                foreach (var item in Enum.GetValues(typeof (EventType)).Cast<EventType>())
                    UpdateEventhandling(item);

                ActivateSourceListener();
                ActivateTargetListener();

            }
            /*var dpd = DependencyPropertyDescriptor.FromProperty(SourceProperty, typeof(EdgeControl));
            if (dpd != null) dpd.AddValueChanged(this, SourceChanged);
            dpd = DependencyPropertyDescriptor.FromProperty(TargetProperty, typeof(EdgeControl));
            if (dpd != null) dpd.AddValueChanged(this, TargetChanged);*/

            IsSelfLooped = _isSelfLooped;
        }

        internal void ActivateSourceListener()
        {
            if (Source != null && !_posTracersActivatedS)
            {
                _sourceTrace = Source.EventOptions.PositionChangeNotification;
                Source.EventOptions.PositionChangeNotification = true;
                Source.PositionChanged += source_PositionChanged;
                _sourceListener = new PropertyChangeNotifier(this, SourceProperty);
                _sourceListener.ValueChanged += SourceChanged;
                _posTracersActivatedS = true;
            } 
        }


        internal void ActivateTargetListener()
        {
            if (Target != null && !_posTracersActivatedT)
            {
                _targetTrace = Target.EventOptions.PositionChangeNotification;
                Target.EventOptions.PositionChangeNotification = true;
                Target.PositionChanged += source_PositionChanged;
                _targetListener = new PropertyChangeNotifier(this, TargetProperty);
                _targetListener.ValueChanged += TargetChanged;
                _posTracersActivatedT = true;
            }
        }

        private bool _posTracersActivatedS;
        private bool _posTracersActivatedT;

        static EdgeControl()
        {
            //override the StyleKey
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EdgeControl), new FrameworkPropertyMetadata(typeof(EdgeControl)));
        }


        #region Vertex position tracing
        private void SourceChanged(object sender, EventArgs e)
        {
            if (_oldSource != null)
            {
                _oldSource.PositionChanged -= source_PositionChanged;
                _oldSource.EventOptions.PositionChangeNotification = _sourceTrace;
            }
            _oldSource = Source;
            if (Source != null)
            {
                _sourceTrace = Source.EventOptions.PositionChangeNotification;
                Source.EventOptions.PositionChangeNotification = true;
                Source.PositionChanged += source_PositionChanged;
            }
            IsSelfLooped = _isSelfLooped;
            UpdateSelfLoopedEdgeData();
        }
        private void TargetChanged(object sender, EventArgs e)
        {
            if (_oldTarget != null)
            {
                _oldTarget.PositionChanged -= source_PositionChanged;
                _oldTarget.EventOptions.PositionChangeNotification = _targetTrace;
            }
            _oldTarget = Target;
            if (Target != null)
            {
                _targetTrace = Target.EventOptions.PositionChangeNotification;
                Target.EventOptions.PositionChangeNotification = true;
                Target.PositionChanged += source_PositionChanged;
            }
            IsSelfLooped = _isSelfLooped;
            UpdateSelfLoopedEdgeData();
        }

        private void source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge();
        }

        private bool _sourceTrace;
        private bool _targetTrace;
        private VertexControl _oldSource;
        private VertexControl _oldTarget;
        #endregion

        #region Event handlers

        internal void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) MouseDown += GraphEdge_MouseDown;
                    else MouseDown -= GraphEdge_MouseDown;
                    break;
                case EventType.MouseDoubleClick:
                    if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += EdgeControl_MouseDoubleClick;
                    else MouseDoubleClick -= EdgeControl_MouseDoubleClick;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) MouseEnter += EdgeControl_MouseEnter;
                    else MouseEnter -= EdgeControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) MouseLeave += EdgeControl_MouseLeave;
                    else MouseLeave -= EdgeControl_MouseLeave;
                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) MouseMove += EdgeControl_MouseMove;
                    else MouseMove -= EdgeControl_MouseMove;
                    break;
            }
        }

        void EdgeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseLeave(this, null, Keyboard.Modifiers);
            // e.Handled = true;
        }

        void EdgeControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseEnter(this, null, Keyboard.Modifiers);
            // e.Handled = true;
        }

        void EdgeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseMove(this, null, Keyboard.Modifiers);
            e.Handled = true;
        }

        void EdgeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeDoubleClick(this, e, Keyboard.Modifiers);
            e.Handled = true;
        }

        void GraphEdge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeSelected(this, e, Keyboard.Modifiers);
            e.Handled = true;
        }

        #endregion

        public override void Dispose()
        {
            Clean();
        }
    }
}