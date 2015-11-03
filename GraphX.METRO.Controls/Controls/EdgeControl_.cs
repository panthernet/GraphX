using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using SysRect = Windows.Foundation.Rect;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
 /*   public class EdgeControlZ : EdgeControlBase
    {
        #region Dependency Properties

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(Thickness),
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
        protected void OnLabelMouseDown(PointerRoutedEventArgs mArgs)
        {
            if (LabelMouseDown != null)
                LabelMouseDown(this, new EdgeLabelSelectedEventArgs(EdgeLabelControl, this, mArgs));
        }

        protected override void OnEdgeLabelUpdated()
        {
            if (EdgeLabelControl is Control)
                ((Control)EdgeLabelControl).PointerPressed += (sender, args) => OnLabelMouseDown(args);
        }  

        #region public Clean()
        public override void Clean()
        {
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

        public EdgeControlZ()
            : this(null, null, null)
        {
        }

        public EdgeControlZ(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
        {
            DefaultStyleKey = typeof(EdgeControl);
            DataContext = edge;
            Source = source; Target = target;
            Edge = edge; DataContext = edge;
            ShowArrows = showArrows;
            ShowLabel = showLabels;
            IsHiddenEdgesUpdated = true;

            EventOptions = new EdgeEventOptions(this);
            foreach (var item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                UpdateEventhandling(item);

            TargetChanged(null, null);
            SourceChanged(null, null);
            _sourceWatcher = this.WatchProperty("Source", SourceChanged);
            _targetWatcher = this.WatchProperty("Target", TargetChanged);
        }

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

        #region Event handlers

        internal void UpdateEventhandling(EventType typ)
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

        void EdgeControl_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseLeave(this, e, null);
            // e.Handled = true;
        }

        void EdgeControl_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseEnter(this, e, null);
            // e.Handled = true;
        }

        void EdgeControl_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeMouseMove(this, e, null);
            e.Handled = true;
        }

        void GraphEdge_MouseDown(object sender, PointerRoutedEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
                RootArea.OnEdgeSelected(this, e, null);
            e.Handled = true;
        }

        #endregion

        public override void Dispose()
        {
            Clean();
        }
    }*/
}