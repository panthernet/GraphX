using GraphX.PCL.Common.Enums;

namespace GraphX.METRO.Controls.Models
{
    public sealed class VertexEventOptions
    {
        /// <summary>
        /// Gets or sets if MouseMove event should be enabled
        /// </summary>
        public bool MouseMoveEnabled { get { return _mousemove; } set { if (_mousemove != value) { _mousemove = value; _vc.UpdateEventhandling(EventType.MouseMove); } } }
        private bool _mousemove = true;
        /// <summary>
        /// Gets or sets if MouseEnter event should be enabled
        /// </summary>
        public bool MouseEnterEnabled { get { return _mouseenter; } set { if (_mouseenter != value) { _mouseenter = value; _vc.UpdateEventhandling(EventType.MouseEnter); } } }
        private bool _mouseenter = true;
        /// <summary>
        /// Gets or sets if MouseLeave event should be enabled
        /// </summary>
        public bool MouseLeaveEnabled { get { return _mouseleave; } set { if (_mouseleave != value) { _mouseleave = value; _vc.UpdateEventhandling(EventType.MouseLeave); } } }
        private bool _mouseleave = true;
        /// <summary>
        /// Gets or sets if MouseDown event should be enabled
        /// </summary>
        public bool MouseClickEnabled { get { return _mouseclick; } set { if (_mouseclick != value) { _mouseclick = value; _vc.UpdateEventhandling(EventType.MouseClick); } } }
        private bool _mouseclick = true;
        /// <summary>
        /// Gets or sets if MouseDoubleClick event should be enabled
        /// </summary>
        public bool MouseDoubleClickEnabled { get { return _mousedblclick; } set { if (_mousedblclick != value) { _mousedblclick = value; _vc.UpdateEventhandling(EventType.MouseDoubleClick); } } }
        private bool _mousedblclick = true;

        private VertexControl _vc;

        public VertexEventOptions(VertexControl vc)
        {
            _vc = vc;
        }

        public void Clean()
        {
            _vc = null;
        }

    }
}
