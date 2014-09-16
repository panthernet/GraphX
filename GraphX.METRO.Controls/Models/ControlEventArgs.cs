namespace GraphX.Models
{
    public sealed class ControlEventArgs : System.EventArgs
    {
        public IGraphControl Control { get; private set; }

        public ControlEventArgs(IGraphControl vc)
            : base()
        {
            Control = vc;
        }
    }
}
