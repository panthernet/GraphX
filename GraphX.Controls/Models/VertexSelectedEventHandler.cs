namespace GraphX.Controls.Models
{
    public delegate void VertexSelectedEventHandler(object sender, VertexSelectedEventArgs args);
    public delegate void VertexMovedEventHandler(object sender, VertexMovedEventArgs e);
#if WPF
    public delegate void VertexClickedEventHandler(object sender, VertexClickedEventArgs args);
#endif
}
