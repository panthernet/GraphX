using System;
using System.Linq;
#if WPF
using SysRect = System.Windows.Rect;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
#elif METRO
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using SysRect =Windows.Foundation.Rect;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
#if WPF
    [Serializable]
#endif
    public class EdgeControl : EdgeControlBase
    {
        #region Dependency Properties

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double),
                                                                                              typeof(EdgeControl),
                                                                                              new PropertyMetadata(5.0));


       /// <summary>
       /// Custom edge thickness
       /// </summary>
       public double StrokeThickness
       {
           get { return (double)GetValue(StrokeThicknessProperty); }
           set { SetValue(StrokeThicknessProperty, value); }
       }


       private static readonly DependencyProperty IsSelfLoopedProperty = DependencyProperty.Register("IsSelfLooped", typeof(bool), typeof(EdgeControl), new PropertyMetadata(false));

       private bool IsSelfLoopedInternal { get { return Source != null && Target != null && Source.Vertex == Target.Vertex; } }
       /// <summary>
       /// Gets if this edge is self looped (have same Source and Target)
       /// </summary>
       public override bool IsSelfLooped
       {
           get { return IsSelfLoopedInternal; }
           protected set { SetValue(IsSelfLoopedProperty, value); }
       }

        #endregion

        public event EdgeLabelEventHandler LabelMouseDown;
        protected void OnLabelMouseDown(MouseButtonEventArgs mArgs, ModifierKeys keys)
        {
            if (LabelMouseDown != null)
                LabelMouseDown(this, new EdgeLabelSelectedEventArgs(EdgeLabelControl, this, mArgs, keys));
        }

        protected override void OnEdgeLabelUpdated()
        {
            if (EdgeLabelControl is Control)
            {
                var ctrl = (Control)EdgeLabelControl;
#if WPF
                MouseButtonEventHandler func = (sender, args) => OnLabelMouseDown(args, Keyboard.Modifiers);
                ctrl.MouseDown -= func;
                ctrl.MouseDown += func;
#elif METRO
                PointerEventHandler func = (sender, args) => OnLabelMouseDown(args, null);
                ctrl.PointerPressed -= func;
                ctrl.PointerPressed += func;
#endif                
            }
        }        

        #region public Clean()
        public override void Clean()
        {
            //TODO rename to _sourceWatcher _targetWatcher
            if (_sourceWatcher != null)
                _sourceWatcher.Dispose();
            if (_targetWatcher != null)
                _targetWatcher.Dispose();
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
            LinePathObject = null;
            SelfLoopIndicator = null;
            if (EdgeLabelControl != null)
            {
                EdgeLabelControl.Dispose();
                EdgeLabelControl = null;
            }

            if (EdgePointerForSource != null)
            {
                EdgePointerForSource.Dispose();
                EdgePointerForSource = null;
            }
            if (EdgePointerForTarget != null)
            {
                EdgePointerForTarget.Dispose();
                EdgePointerForTarget = null;
            }
            if (EventOptions != null)
                EventOptions.Clean();
        }
        #endregion

#if WPF 
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

        #region Vertex position tracing
        internal void ActivateSourceListener()
        {
            if (Source != null && !_posTracersActivatedS)
            {
                _sourceTrace = Source.EventOptions.PositionChangeNotification;
                Source.EventOptions.PositionChangeNotification = true;
                Source.PositionChanged += source_PositionChanged;
                Source.SizeChanged += Source_SizeChanged;
                _sourceWatcher = new PropertyChangeNotifier(this, SourceProperty);
                _sourceWatcher.ValueChanged += SourceChanged;
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
                Target.SizeChanged += Source_SizeChanged;
                _targetWatcher = new PropertyChangeNotifier(this, TargetProperty);
                _targetWatcher.ValueChanged += TargetChanged;
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

        private void SourceChanged(object sender, EventArgs e)
        {
            if (_oldSource != null)
            {
                _oldSource.PositionChanged -= source_PositionChanged;
                _oldSource.SizeChanged -= Source_SizeChanged;
                _oldSource.EventOptions.PositionChangeNotification = _sourceTrace;
            }
            _oldSource = Source;
            if (Source != null)
            {
                _sourceTrace = Source.EventOptions.PositionChangeNotification;
                Source.EventOptions.PositionChangeNotification = true;
                Source.PositionChanged += source_PositionChanged;
                Source.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }
        private void TargetChanged(object sender, EventArgs e)
        {
            if (_oldTarget != null)
            {
                _oldTarget.PositionChanged -= source_PositionChanged;
                _oldTarget.SizeChanged -= Source_SizeChanged;
                _oldTarget.EventOptions.PositionChangeNotification = _targetTrace;
            }
            _oldTarget = Target;
            if (Target != null)
            {
                _targetTrace = Target.EventOptions.PositionChangeNotification;
                Target.EventOptions.PositionChangeNotification = true;
                Target.PositionChanged += source_PositionChanged;
                Target.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }

        private void source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge(true);
        }

        void Source_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateEdge();
        }


        private bool _sourceTrace;
        private bool _targetTrace;
        private VertexControl _oldSource;
        private VertexControl _oldTarget;
        #endregion

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
#elif METRO
        #region Position tracing

        private void TargetChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_oldTarget != null)
                _oldTarget.PositionChanged -= source_PositionChanged;
            _oldTarget = Target;
            if (Target != null)
                Target.PositionChanged += source_PositionChanged;
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }

        private void SourceChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_oldSource != null)
                _oldSource.PositionChanged -= source_PositionChanged;
            _oldSource = Source;

            if (Source != null)
                Source.PositionChanged += source_PositionChanged;
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }


        private void source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge();
        }

        private readonly IDisposable _sourceWatcher;
        private readonly IDisposable _targetWatcher;
        private VertexControl _oldSource;
        private VertexControl _oldTarget;
        #endregion

        protected internal virtual void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled) PointerPressed += GraphEdge_MouseDown;
                    else PointerPressed -= GraphEdge_MouseDown;
                    break;
                case EventType.MouseDoubleClick:
                    //if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += EdgeControl_MouseDoubleClick;
                    //else MouseDoubleClick -= EdgeControl_MouseDoubleClick;
                    break;
                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled) PointerEntered += EdgeControl_MouseEnter;
                    else PointerEntered -= EdgeControl_MouseEnter;
                    break;
                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled) PointerExited += EdgeControl_MouseLeave;
                    else PointerExited -= EdgeControl_MouseLeave;
                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled) PointerMoved += EdgeControl_MouseMove;
                    else PointerMoved -= EdgeControl_MouseMove;
                    break;
            }
        }
#endif

        public EdgeControl()
            : this(null, null, null)
        {
        }

        public EdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
        {
            DataContext = edge;
            Source = source; Target = target;
            Edge = edge; DataContext = edge;
            this.SetCurrentValue(ShowArrowsProperty, showArrows);
            this.SetCurrentValue(ShowLabelProperty, showLabels);
            IsHiddenEdgesUpdated = true;

#if METRO
            DefaultStyleKey = typeof(EdgeControl);
#elif WPF
#endif

            if (!this.IsInDesignMode())
            {
                EventOptions = new EdgeEventOptions(this);
                foreach (var item in Enum.GetValues(typeof (EventType)).Cast<EventType>())
                    UpdateEventhandling(item);

#if WPF
                ActivateSourceListener();
                ActivateTargetListener();
#elif METRO
                SourceChanged(null, null);
                _sourceWatcher = this.WatchProperty("Source", SourceChanged);
                _targetWatcher = this.WatchProperty("Target", TargetChanged);
#endif
            }

            IsSelfLooped = IsSelfLoopedInternal;
        }

        #region Event handlers

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

        /// <summary>
        /// Gets Edge data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataEdge<T>() where T : IGraphXCommonEdge
        {
            return (T)Edge;
        }   
    }
}