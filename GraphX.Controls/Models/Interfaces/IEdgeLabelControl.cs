using System;
using System.Windows;

namespace GraphX.Controls.Models.Interfaces
{
    public interface IEdgeLabelControl: IDisposable
    {
        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        double Angle { get; set; }

        /// <summary>
        /// Automaticaly update edge label position
        /// </summary>
        void UpdatePosition();

        void UpdateLayout();

        void Show();
        void Hide();

        /// <summary>
        /// Get label rectangular size
        /// </summary>
        Rect GetSize();
        /// <summary>
        /// Set label rectangular size
        /// </summary>
        void SetSize(Rect size);
    }
}
