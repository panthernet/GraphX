﻿using System;
using System.Windows;
using GraphX.Common.Enums;

namespace GraphX.Controls
{
    public interface IVertexConnectionPoint : IDisposable
    {
        /// <summary>
        /// Connector identifier
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        VertexShape Shape { get; set; }

        void Hide();
        void Show();

        Rect RectangularSize { get; }

        void Update();
        DependencyObject GetParent();
    }
}
