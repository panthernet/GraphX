using System.Collections.Generic;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
#elif METRO
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
#endif
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;

namespace GraphX.Controls
{
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    public abstract class VertexControlBase : Control, IGraphControl
    {
        protected IVertexLabelControl VertexLabelControl;

        /// <summary>
        /// Fires when IsPositionTraceEnabled property set and object changes its coordinates.
        /// </summary>
        public event VertexPositionChangedEH PositionChanged;

        protected void OnPositionChanged(Point offset, Point pos)
        {
            if (PositionChanged != null)
                PositionChanged.Invoke(this, new VertexPositionEventArgs(offset, pos, this));
        }

        protected VertexControlBase()
        {
            VertexConnectionPointsList = new List<IVertexConnectionPoint>();
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
                return VertexLabelControl != null ? VertexLabelControl.Angle : _labelAngle;
            }
            set
            {
                _labelAngle = value;
                if (VertexLabelControl != null) 
                    VertexLabelControl.Angle = _labelAngle;
            }
        }

        public static readonly DependencyProperty VertexShapeProperty =
            DependencyProperty.Register("VertexShape", typeof(VertexShape), typeof(VertexControlBase), new PropertyMetadata(VertexShape.Rectangle));

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
            DependencyProperty.Register("Vertex", typeof(object), typeof(VertexControlBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets vertex control parent GraphArea object (don't need to be set manualy)
        /// </summary>
        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootCanvasProperty); }
            set { SetValue(RootCanvasProperty, value); }
        }

        public static readonly DependencyProperty RootCanvasProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(VertexControlBase), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(VertexControlBase), new PropertyMetadata(false, ShowLabelChanged));

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
            if (result != null) result.Update();
            return result;
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
