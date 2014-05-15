using GraphX.Measure;
using QuickGraph;
using System;
using System.Collections.Generic;

namespace GraphX.GraphSharp.Algorithms.Layout
{
    public class RandomLayoutAlgorithm<TVertex, TEdge, TGraph> : ILayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        private TGraph Graph;
        private Random Rnd = new Random((int)DateTime.Now.Ticks);

        public RandomLayoutAlgorithm(TGraph graph)
        {
            Graph = graph;
        }        
    
        public void Compute()
        {
            foreach (var item in Graph.Vertices)
                vertexPositions.Add(item, new Point(Rnd.Next(0, 2000), Rnd.Next(0, 2000)));
            //var vlist = Graph.Vertices.ToList();
            /*VertexPositions.Add(vlist[0], new Point(100, 100));
            VertexPositions.Add(vlist[1], new Point(300, 100));
            VertexPositions.Add(vlist[2], new Point(200, 300));

            VertexPositions.Add(vlist[3], new Point(10000, 1000));
            VertexPositions.Add(vlist[4], new Point(10300, 1000));
            VertexPositions.Add(vlist[5], new Point(10200, 1300));*/

        }

        IDictionary<TVertex, Point> vertexPositions = new Dictionary<TVertex, Point>();

        public IDictionary<TVertex, Point> VertexPositions { get { return vertexPositions; } }

        public IDictionary<TVertex, Size> VertexSizes { get; set; }

        public bool NeedVertexSizes
        {
            get { return false; }
        }

        public TGraph VisitedGraph
        {
            get { return Graph; }
        }
    }
}
