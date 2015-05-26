using System;
using Windows.Foundation;
using GraphX.PCL.Common.Enums;

namespace GraphX.Controls
{
    public interface IVertexConnectionPoint : IDisposable
    {
        /// <summary>
        /// Connector identifier
        /// </summary>
        int Id { get; }

        void Hide();
        void Show();

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        VertexShape Shape { get; }

        Rect RectangularSize { get; }

        void Update();
    }
}
