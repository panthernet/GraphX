using System.Threading;
using GraphX.GraphSharp.Algorithms.EdgeRouting;
using System.Collections.Generic;
using GraphX.Measure;

namespace ShowcaseApp.WPF
{
    public class ExampleExternalEdgeRoutingAlgorithm<TVertex, TEdge> : IExternalEdgeRouting<TVertex, TEdge>
    {
        public void Compute(CancellationToken cancellationToken)
        {
        }

        public IDictionary<TVertex, Rect> VertexSizes { get; set; }

        public IDictionary<TVertex, Point> VertexPositions { get; set; }

        readonly Dictionary<TEdge, Point[]> _edgeRoutes = new Dictionary<TEdge,Point[]>();
        public IDictionary<TEdge, Point[]> EdgeRoutes { get { return _edgeRoutes; }}

        public Point[] ComputeSingle(TEdge edge) { return null; }

        public void UpdateVertexData(TVertex vertex, Point position, Rect size) { }


        public Rect AreaRectangle { get; set; }

    }
}
