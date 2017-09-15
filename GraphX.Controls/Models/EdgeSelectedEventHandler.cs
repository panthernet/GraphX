namespace GraphX.Controls.Models
{
    public delegate void EdgeSelectedEventHandler(object sender, EdgeSelectedEventArgs args);
#if WPF
    public delegate void EdgeClickedEventHandler(object sender, EdgeClickedEventArgs args);
#endif
    public delegate void EdgeLabelEventHandler(object sender, EdgeLabelSelectedEventArgs args);
}
