using GraphX.GraphSharp.Algorithms.EdgeRouting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphX.Measure;

namespace ShowcaseExample.ExampleModels
{
    public class ExampleExternalEdgeRoutingAlgorithm<TVertex, TEdge> : IExternalEdgeRouting<TVertex, TEdge>
    {
        public void Compute()
        {
        }

        public IDictionary<TVertex, Rect> VertexSizes { get; set; }

        public IDictionary<TVertex, Point> VertexPositions { get; set; }

        Dictionary<TEdge, Point[]> _edgeRoutes = new Dictionary<TEdge,Point[]>();
        public IDictionary<TEdge, Point[]> EdgeRoutes { get { return _edgeRoutes; }}

        public Point[] ComputeSingle(TEdge edge) { return null; }

        public void UpdateVertexData(TVertex vertex, Point position, Rect size) { }


        public Rect AreaRectangle { get; set; }

    }
}
