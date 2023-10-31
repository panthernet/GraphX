﻿using System.Windows;

namespace GraphX.Controls
{
    /// <summary>
    /// Common imterface for all possible zoomcontrol objects
    /// </summary>
    public interface IZoomControl
    {
        UIElement PresenterVisual { get; }
        double Zoom { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double ActualWidth { get; }
        double ActualHeight { get; }
    }
}
