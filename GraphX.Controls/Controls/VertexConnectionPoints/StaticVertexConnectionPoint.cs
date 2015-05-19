using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GraphX.PCL.Common.Enums;

namespace GraphX.WPF.Controls
{
    public class StaticVertexConnectionPoint: Image, IVertexConnectionPoint
    {
        #region Common part

        /// <summary>
        /// Connector identifier
        /// </summary>
        public int Id { get; set; }


        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register("Shape", typeof(VertexShape), typeof(VertexControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        public VertexShape Shape
        {
            get { return (VertexShape)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }


        private Rect _rectangularSize;
        public Rect RectangularSize { 
            get 
            { 
                if(_rectangularSize == Rect.Empty)
                    UpdateLayout();
                return _rectangularSize;
            }
            private set { _rectangularSize = value; }
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        private static VertexControl GetVertexControl(DependencyObject parent)
        {
            while (parent != null)
            {
                var control = parent as VertexControl;
                if (control != null) return control;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        #endregion


        private VertexControl _vertexControl;
        protected VertexControl VertexControl { get { return _vertexControl ?? (_vertexControl = GetVertexControl(VisualParent)); } }

        public StaticVertexConnectionPoint()
        {
            RenderTransformOrigin = new Point(.5, .5);
            LayoutUpdated += StaticVertexConnector_LayoutUpdated;
        }

        void StaticVertexConnector_LayoutUpdated(object sender, EventArgs e)
        {
            var position = this.TranslatePoint(new Point(), VertexControl);
            var vPos = VertexControl.GetPosition();
            position = new Point(position.X + vPos.X, position.Y + vPos.Y);
            RectangularSize = new Rect(position, DesiredSize);
        }

        public void Update()
        {
            UpdateLayout();
        }

        public void Dispose()
        {
            _vertexControl = null;
        }
    }
}
