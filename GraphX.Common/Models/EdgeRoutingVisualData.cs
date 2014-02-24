using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphX
{
    public class EdgeRoutingVisualData
    {
        public bool HaveTemplate { get; set; }
        public bool IsEdgeVisible { get; set; }
        public bool IsEdgeSelfLooped { get; set; }

        public Point SourcePosition { get; set; }
        public Size SourceSize { get; set; }
        public Point TargetPosition { get; set; }
        public Size TargetSize { get; set; }

        IRoutingInfo Edge { get; set; }
    }
}
