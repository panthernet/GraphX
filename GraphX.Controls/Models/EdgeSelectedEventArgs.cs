namespace GraphX.Models
{
    public sealed class EdgeSelectedEventArgs : System.EventArgs
    {
        public EdgeControl EdgeControl { get; set; }

        public EdgeSelectedEventArgs(EdgeControl ec)
            : base()
        {
            EdgeControl = ec; 
        }
    }
}
