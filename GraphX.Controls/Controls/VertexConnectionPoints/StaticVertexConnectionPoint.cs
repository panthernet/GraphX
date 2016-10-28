﻿#if WPF

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#elif METRO
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

using GraphX.PCL.Common.Enums;

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
#if WPF
            SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);
#else
            SetValue(UIElement.VisibilityProperty, Visibility.Visible);
#endif
        }

        public void Hide()
        {
#if WPF
            SetCurrentValue(UIElement.VisibilityProperty, Visibility.Collapsed);
#else
            SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
#endif
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

        #endregion Common part

        private VertexControl _vertexControl;
        protected VertexControl VertexControl => _vertexControl ?? (_vertexControl = GetVertexControl(GetParent()));

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

#if WPF

        public DependencyObject GetParent()
        {
            return VisualParent;
        }

        protected virtual void OnLayoutUpdated(object sender, EventArgs e)
        {
            var position = TranslatePoint(new Point(), VertexControl);
            var vPos = VertexControl.GetPosition();
            position = new Point(position.X + vPos.X, position.Y + vPos.Y);
            RectangularSize = new Rect(position, DesiredSize);
        }

#elif METRO
        public DependencyObject GetParent()
        {
            return Parent;
        }

        protected virtual void OnLayoutUpdated(object sender, object o)
        {
            var position = TransformToVisual(VertexControl).TransformPoint(new Point());
            var vPos = VertexControl.GetPosition();
            position = new Point(position.X + vPos.X, position.Y + vPos.Y);
            RectangularSize = new Rect(position, DesiredSize);
        }
#endif
    }
}