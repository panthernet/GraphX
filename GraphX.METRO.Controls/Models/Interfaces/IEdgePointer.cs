using System;
using GraphX.Measure;

namespace GraphX.METRO.Controls.Models.Interfaces
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
        void Update(Windows.Foundation.Point? position, Vector direction, double angle = 0d);

        void Hide();
        void Show();
    }
}
