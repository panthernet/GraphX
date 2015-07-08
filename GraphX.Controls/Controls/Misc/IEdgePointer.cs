using System;
#if WPF
using System.Windows;
#elif METRO
using Windows.Foundation;
using Vector = GraphX.Measure.Vector;
using Windows.UI.Xaml;
#endif


namespace GraphX.Controls
{  
    public interface IEdgePointer: IDisposable
    {                                                                                             
        /// <summary>
        /// Gets or sets if image has to be rotated according to edge directions
        /// </summary>
        bool NeedRotation { get; }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        Point Update(Point? position, Vector direction, double angle = 0d);

        void SetManualPosition(Point position);

        void Hide();
        void Show();

        /// <summary>
        /// Gets is control visible
        /// </summary>
        Visibility Visibility { get; }
    }
}
