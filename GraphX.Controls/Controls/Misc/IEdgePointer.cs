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
        /// Returns edge pointer center position coordinates
        /// </summary>
        Point GetPosition();

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
		/// Gets a value indicating whether the pointer is suppressed. A suppressed pointer won't be displayed, but
		/// suppressing does not alter the underlying Visibility property value.
		/// </summary>
		bool IsSuppressed { get; }
		/// <summary>
		/// Suppresses the pointer display without altering the underlying Visibility property value.
		/// </summary>
		void Suppress();
		/// <summary>
		/// Removes pointer display suppression, restoring the pointer to its underlying Visibility property value. If Visibility
		/// was set to Hidden or Collapsed, the pointer will remain invisible to the user, but if the Visibility base value
		/// is Visible, it should appear again.
		/// </summary>
		void UnSuppress();

        /// <summary>
        /// Gets is control visible
        /// </summary>
        Visibility Visibility { get; }
    }
}
