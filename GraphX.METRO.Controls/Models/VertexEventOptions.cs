using System;
using GraphX.Controls;

namespace GraphX
{
    public sealed class VertexEventOptions
    {
        /// <summary>
        /// Gets or sets if MouseMove event should be enabled
        /// </summary>
        public bool MouseMoveEnabled { get { return mousemove; } set { if (mousemove != value) { mousemove = value; _vc.UpdateEventhandling(EventType.MouseMove); } } }
        private bool mousemove = true;
        /// <summary>
        /// Gets or sets if MouseEnter event should be enabled
        /// </summary>
        public bool MouseEnterEnabled { get { return mouseenter; } set { if (mouseenter != value) { mouseenter = value; _vc.UpdateEventhandling(EventType.MouseEnter); } } }
        private bool mouseenter = true;
        /// <summary>
        /// Gets or sets if MouseLeave event should be enabled
        /// </summary>
        public bool MouseLeaveEnabled { get { return mouseleave; } set { if (mouseleave != value) { mouseleave = value; _vc.UpdateEventhandling(EventType.MouseLeave); } } }
        private bool mouseleave = true;
        /// <summary>
        /// Gets or sets if MouseDown event should be enabled
        /// </summary>
        public bool MouseClickEnabled { get { return mouseclick; } set { if (mouseclick != value) { mouseclick = value; _vc.UpdateEventhandling(EventType.MouseClick); } } }
        private bool mouseclick = true;
        /// <summary>
        /// Gets or sets if MouseDoubleClick event should be enabled
        /// </summary>
        public bool MouseDoubleClickEnabled { get { return mousedblclick; } set { if (mousedblclick != value) { mousedblclick = value; _vc.UpdateEventhandling(EventType.MouseDoubleClick); } } }
        private bool mousedblclick = true;

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
