namespace GraphX.Controls.Models
{
    public sealed class ControlEventArgs : System.EventArgs
    {
        public IGraphControl Control { get; private set; }

        public bool RemoveDataObject { get; private set; }

        public ControlEventArgs(IGraphControl vc, bool removeDataObject)
        {
            RemoveDataObject = removeDataObject;
            Control = vc;
        }
    }
}
