using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using GraphX.Common.Enums;

namespace GraphX.Controls
{
    public class StaticVertexConnectionPoint : ContentControl, IVertexConnectionPoint
    {
        #region Common part

        /// <summary>
        /// Connector identifier
        /// </summary>
        public int Id { get; set; }

        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register(nameof(Shape),
                          typeof(VertexShape),
                          typeof(StaticVertexConnectionPoint),
                          new PropertyMetadata(VertexShape.Circle));

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        public VertexShape Shape
        {
            get { return (VertexShape)GetValue(ShapeProperty); }
            set { SetValue(ShapeProperty, value); }
        }

        private Rect _rectangularSize;

        public Rect RectangularSize
        {
            get
            {
                if (_rectangularSize == Rect.Empty)
                    UpdateLayout();
                return _rectangularSize;
            }
            private set { _rectangularSize = value; }
        }

        public void Show()
        {
            SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);
        }

        public void Hide()
        {
            SetCurrentValue(UIElement.VisibilityProperty, Visibility.Collapsed);
        }

        private static VertexControl? GetVertexControl(DependencyObject? parent)
        {
            while (parent != null)
            {
                if (parent is VertexControl control) return control;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        #endregion Common part

        private VertexControl? _vertexControl;
        protected VertexControl? VertexControl => _vertexControl ??= GetVertexControl(GetParent());

        public StaticVertexConnectionPoint()
        {
            RenderTransformOrigin = new Point(.5, .5);
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            LayoutUpdated += OnLayoutUpdated;
        }

        public void Update()
        {
            UpdateLayout();
        }

        public void Dispose()
        {
            _vertexControl = null;
        }

        public DependencyObject GetParent()
        {
            return VisualParent;
        }

        protected virtual void OnLayoutUpdated(object? sender, EventArgs e)
        {
            var position = TranslatePoint(new Point(), VertexControl);
            var vPos = VertexControl!.GetPosition();
            position = new Point(position.X + vPos.X, position.Y + vPos.Y);
            RectangularSize = new Rect(position, new Size(ActualWidth, ActualHeight));
        }
    }
}