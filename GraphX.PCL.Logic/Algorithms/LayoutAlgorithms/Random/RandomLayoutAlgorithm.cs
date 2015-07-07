using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class RandomLayoutAlgorithm<TVertex, TEdge, TGraph> : LayoutAlgorithmBase<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        private readonly Random _rnd = new Random((int)DateTime.Now.Ticks);
        private readonly RandomLayoutAlgorithmParams _parameters;

        public RandomLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions, RandomLayoutAlgorithmParams prms)
            : base(graph, positions)
        {
            _parameters = prms;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            VertexPositions.Clear();
            var bounds = _parameters == null ? new RandomLayoutAlgorithmParams().Bounds : _parameters.Bounds;
            int boundsWidth = (int)bounds.Width;
            int boundsHeight = (int)bounds.Height;
            foreach (var item in VisitedGraph.Vertices)
            {
                if (item.SkipProcessing != ProcessingOptionEnum.Freeze || VertexPositions.Count == 0)
                {
                    var x = (int) bounds.X;
                    var y = (int) bounds.Y;
                    var size = VertexSizes.FirstOrDefault(a => a.Key == item).Value;
                    VertexPositions.Add(item,
                        new Point(_rnd.Next(x, x + boundsWidth - (int) size.Width),
                            _rnd.Next(y, y + boundsHeight - (int) size.Height)));
                }
                /*else if (VertexPositions != null)
                {
                    var res = VertexPositions.FirstOrDefault(a => a.Key == item);
                    if (res.Key != null)
                        VertexPositions.Add(res.Key, res.Value);
                }*/
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
    }
}
