using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using GraphX;

namespace ShowcaseApp.WPF.Models
{
    public class EditorObjectManager: IDisposable
    {
        private GraphAreaExample _graphArea;

        public EditorObjectManager(GraphAreaExample graphArea)
        {
            _graphArea = graphArea;
        }



        public void Dispose()
        {
            _graphArea = null;
        }
    }

    public class EdgeBlueprint
    {
        public VertexControl Source { get; set; }
        public VertexControl Target { get; set; }
        public Path EdgePath { get; set; }
    }
}
