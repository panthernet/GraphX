using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class RandomLayoutAlgorithm<TVertex, TEdge, TGraph> : LayoutAlgorithmBase<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private readonly RandomLayoutAlgorithmParams _parameters;

        public RandomLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions, RandomLayoutAlgorithmParams prms)
            : base(graph, positions)
        {
            _parameters = prms;
        }

        public RandomLayoutAlgorithm(RandomLayoutAlgorithmParams prms)
            : base(default(TGraph), null)
        {
            _parameters = prms;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            VertexPositions.Clear();
            var bounds = _parameters == null ? new RandomLayoutAlgorithmParams().Bounds : _parameters.Bounds;
            var boundsWidth = (int)bounds.Width;
            var boundsHeight = (int)bounds.Height;
            var seed = _parameters == null ? Guid.NewGuid().GetHashCode() : _parameters.Seed;
            var rnd = new Random(seed);
            foreach (var item in VisitedGraph.Vertices)
            {
                if (item.SkipProcessing != ProcessingOptionEnum.Freeze || VertexPositions.Count == 0)
                {
                    var x = (int) bounds.X;
                    var y = (int) bounds.Y;
                    var size = VertexSizes.FirstOrDefault(a => a.Key == item).Value;
                    VertexPositions.Add(item,
                        new Point(rnd.Next(x, x + boundsWidth - (int) size.Width),
                            rnd.Next(y, y + boundsHeight - (int) size.Height)));
                }
            }
           
        }

        public override bool NeedVertexSizes
        {
            get { return true; }
        }

        public override bool SupportsObjectFreeze
        {
            get { return true; }
        }

        public override void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            if (VisitedGraph == null && !TryCreateNewGraph())
                throw new GX_GeneralException("Can't create new graph through reflection. Make sure it support default constructor.");
            VisitedGraph.Clear();
            VisitedGraph.AddVertexRange(vertices);
            VisitedGraph.AddEdgeRange(edges);
        }
    }
}
