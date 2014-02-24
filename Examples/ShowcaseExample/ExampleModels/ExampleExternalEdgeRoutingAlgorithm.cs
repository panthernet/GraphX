using GraphX.GraphSharp.Algorithms.EdgeRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShowcaseExample.ExampleModels
{
    public class ExampleExternalEdgeRoutingAlgorithm<TVertex, TEdge> : IExternalEdgeRouting<TVertex, TEdge>
    {
        public void Compute()
        {
        }

        public IDictionary<TVertex, System.Windows.Rect> VertexSizes { get; set; }

        public IDictionary<TVertex, System.Windows.Point> VertexPositions { get; set; }

        Dictionary<TEdge, System.Windows.Point[]> _edgeRoutes = new Dictionary<TEdge,System.Windows.Point[]>();
        public IDictionary<TEdge, System.Windows.Point[]> EdgeRoutes { get { return _edgeRoutes; }}

        public System.Windows.Point[] ComputeSingle(TEdge edge) { return null; }

        public void UpdateVertexData(TVertex vertex, System.Windows.Point position, System.Windows.Rect size) { }


        public System.Windows.Rect AreaRectangle { get; set; }

    }
}
