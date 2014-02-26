using System;
using System.Windows;
using System.Windows.Input;

namespace GraphX.Models
{
    public delegate void VertexSelectedEventHandler(object sender, VertexSelectedEventArgs args);
    public delegate void VertexMovedEventHandler(object sender, VertexMovedEventArgs e);


    public delegate void ContentSizeChangedEventHandler(object sender, ContentSizeChangedEventArgs e);
}
