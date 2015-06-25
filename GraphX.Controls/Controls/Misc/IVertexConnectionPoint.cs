using System;
#if WPF
using System.Windows;
#elif METRO
using Windows.Foundation;
using Windows.UI.Xaml;
#endif
using GraphX.PCL.Common.Enums;

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
