using GraphX.PCL.Common.Enums;

namespace GraphX.Controls.Models
{
    public sealed class EdgeEventOptions
    {
        /// <summary>
        /// Gets or sets if MouseMove event should be enabled
        /// </summary>
        public bool MouseMoveEnabled { get { return mousemove; } set { if (mousemove != value) { mousemove = value; _ec.UpdateEventhandling(EventType.MouseMove); } } }
        private bool mousemove = true;
        /// <summary>
        /// Gets or sets if MouseEnter event should be enabled
        /// </summary>
        public bool MouseEnterEnabled { get { return mouseenter; } set { if (mouseenter != value) { mouseenter = value; _ec.UpdateEventhandling(EventType.MouseEnter); } } }
        private bool mouseenter = true;
        /// <summary>
        /// Gets or sets if MouseLeave event should be enabled
        /// </summary>
        public bool MouseLeaveEnabled { get { return mouseleave; } set { if (mouseleave != value) { mouseleave = value; _ec.UpdateEventhandling(EventType.MouseLeave); } } }
        private bool mouseleave = true;

        /// <summary>
        /// Gets or sets if MouseDown event should be enabled
        /// </summary>
        public bool MouseClickEnabled { get { return mouseclick; } set { if (mouseclick != value) { mouseclick = value; _ec.UpdateEventhandling(EventType.MouseClick); } } }
        private bool mouseclick = true;
        /// <summary>
        /// Gets or sets if MouseDoubleClick event should be enabled
        /// </summary>
        public bool MouseDoubleClickEnabled { get { return mousedblclick; } set { if (mousedblclick != value) { mousedblclick = value; _ec.UpdateEventhandling(EventType.MouseDoubleClick); } } }
        private bool mousedblclick = true;

        private EdgeControl _ec;

        public EdgeEventOptions(EdgeControl ec)
        {
            _ec = ec;
        }

        public void Clean()
        {
            _ec = null;
        }
    }
}
