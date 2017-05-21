﻿using System;
using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
using USize = System.Windows.Size;
using Point = System.Windows.Point;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
#endif
using GraphX.Controls.Models;
using GraphX.PCL.Common;
using GraphX.PCL.Common.Enums;
using Rect = GraphX.Measure.Rect;

namespace GraphX.Controls
{
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    [TemplatePart(Name = "PART_vcproot", Type = typeof(Panel))]
    public abstract class VertexControlBase : Control, IGraphControl
    {
        protected internal IVertexLabelControl VertexLabelControl;
        /// <summary>
        /// Fires when new label is attached to VertexControl
        /// </summary>
        public event EventHandler<EventArgs> LabelAttached;

        protected void OnLabelAttached()
        {
            LabelAttached?.Invoke(this, null);
        }

        /// <summary>
        /// Fires when new label is detached from VertexControl
        /// </summary>
        public event EventHandler<EventArgs> LabelDetached;

        protected void OnLabelDetached()
        {
            LabelDetached?.Invoke(this, null);
        }

        /// <summary>
        /// Fires when IsPositionTraceEnabled property set and object changes its coordinates.
        /// </summary>
        public event VertexPositionChangedEH PositionChanged;

        protected void OnPositionChanged(Point offset, Point pos)
        {
            PositionChanged?.Invoke(this, new VertexPositionEventArgs(offset, pos, this));
        }

        protected VertexControlBase()
        {
            VertexConnectionPointsList = new List<IVertexConnectionPoint>();
        }

        /// <summary>
        /// Hides this control with all related edges
        /// </summary>
        public void HideWithEdges()
        {
            this.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            SetConnectionPointsVisibility(false);
            RootArea.GetRelatedControls(this, GraphControlType.Edge, EdgesType.All).ForEach(a =>
            {
                //if (a is EdgeControlBase)
               //     ((EdgeControlBase)a).SetVisibility(Visibility.Collapsed);
               // else
                a.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Shows this control with all related edges
        /// </summary>
        public void ShowWithEdges()
        {
            this.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            SetConnectionPointsVisibility(true);
            RootArea.GetRelatedControls(this, GraphControlType.Edge, EdgesType.All).ForEach(a =>
            {
                if(a is EdgeControlBase)
                    ((EdgeControlBase)a).SetVisibility(Visibility.Visible);
                else a.Visibility = Visibility.Visible;
            });
        }

#region Properties

        /// <summary>
        /// List of found vertex connection points
        /// </summary>
        public List<IVertexConnectionPoint> VertexConnectionPointsList { get; protected set; }

        /// <summary>
        /// Provides settings for event calls within single vertex control
        /// </summary>
        public VertexEventOptions EventOptions { get; protected set; }

        private double _labelAngle;
        /// <summary>
        /// Gets or sets vertex label angle
        /// </summary>
        public double LabelAngle
        {
            get
            {
                return VertexLabelControl?.Angle ?? _labelAngle;
            }
            set
            {
                _labelAngle = value;
                if (VertexLabelControl != null) 
                    VertexLabelControl.Angle = _labelAngle;
            }
        }

        public static readonly DependencyProperty VertexShapeProperty =
            DependencyProperty.Register(nameof(VertexShape), typeof(VertexShape), typeof(VertexControlBase), new PropertyMetadata(VertexShape.Rectangle));

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
            set { SetValue(VertexProperty, value); }
        }

        public static readonly DependencyProperty VertexProperty =
            DependencyProperty.Register(nameof(Vertex), typeof(object), typeof(VertexControlBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets vertex control parent GraphArea object (don't need to be set manualy)
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register(nameof(RootArea), typeof(GraphAreaBase), typeof(VertexControlBase), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register(nameof(ShowLabel), typeof(bool), typeof(VertexControlBase), new PropertyMetadata(false, ShowLabelChanged));

        private static void ShowLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as VertexControlBase;
            if (obj.VertexLabelControl == null) return;
            if ((bool)e.NewValue) obj.VertexLabelControl.Show(); else obj.VertexLabelControl.Hide();
        }

        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
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

        public abstract void Clean();

        /// <summary>
        /// Get control position on the GraphArea panel in attached coords X and Y
        /// </summary>
        /// <param name="final"></param>
        /// <param name="round"></param>
        public Point GetPosition(bool final = false, bool round = false)
        {
            return round ?
                new Point(final ? (int)GraphAreaBase.GetFinalX(this) : (int)GraphAreaBase.GetX(this), final ? (int)GraphAreaBase.GetFinalY(this) : (int)GraphAreaBase.GetY(this)) :
                new Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
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
        /// Get vertex center position
        /// </summary>
        public Point GetCenterPosition(bool final = false)
        {
            var pos = GetPosition();
            return new Point(pos.X + ActualWidth * .5, pos.Y + ActualHeight * .5);
        }

        /// <summary>
        /// Returns first connection point found with specified Id
        /// </summary>
        /// <param name="id">Connection point identifier</param>
        /// <param name="runUpdate">Update connection point if found</param>
        public IVertexConnectionPoint GetConnectionPointById(int id, bool runUpdate = false)
        {
            var result = VertexConnectionPointsList.FirstOrDefault(a => a.Id == id);
            result?.Update();
            return result;
        }

        public IVertexConnectionPoint GetConnectionPointAt(Point position)
        {
            Measure(new USize(double.PositiveInfinity, double.PositiveInfinity));

            return VertexConnectionPointsList.FirstOrDefault(a =>
            {
                var rect = new Rect(a.RectangularSize.X, a.RectangularSize.Y, a.RectangularSize.Width, a.RectangularSize.Height);
                return rect.Contains(position.ToGraphX());
            });
        }

        /// <summary>
        /// Internal method. Attaches label to control
        /// </summary>
        /// <param name="ctrl">Control</param>
        public void AttachLabel(IVertexLabelControl ctrl)
        {
            VertexLabelControl = ctrl;
            OnLabelAttached();
        }

        /// <summary>
        /// Internal method. Detaches label from control.
        /// </summary>
        public void DetachLabel()
        {
            if(VertexLabelControl is IAttachableControl<VertexControl>)
                ((IAttachableControl<VertexControl>)VertexLabelControl).Detach();
            VertexLabelControl = null;
            OnLabelDetached();
        }

        /// <summary>
        /// Sets visibility of all connection points
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetConnectionPointsVisibility(bool isVisible)
        {
            foreach (var item in VertexConnectionPointsList)
            {
                if (isVisible) item.Show(); else item.Hide();
            }
        }

#if METRO
        void IPositionChangeNotify.OnPositionChanged()
        {
            if (ShowLabel && VertexLabelControl != null)
                VertexLabelControl.UpdatePosition();
            OnPositionChanged(new Point(), GetPosition());
        }
#endif
    }
}
