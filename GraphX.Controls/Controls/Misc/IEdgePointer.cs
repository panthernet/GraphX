using System;
using System.Windows;

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

        void Hide();
        void Show();
    }
}
