using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class TestingCompoundLayoutIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
        : CompoundLayoutIterationEventArgs<TVertex, TEdge>, ILayoutInfoIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
        where TVertex : class 
        where TEdge : IEdge<TVertex>
    {
        private readonly IDictionary<TVertex, TVertexInfo> _vertexInfos;

        public Point GravitationCenter { get; private set; }

        public TestingCompoundLayoutIterationEventArgs(
            int iteration, 
            double statusInPercent, 
            string message, 
            IDictionary<TVertex, Point> vertexPositions, 
            IDictionary<TVertex, Size> innerCanvasSizes,
            IDictionary<TVertex, TVertexInfo> vertexInfos,
            Point gravitationCenter) 
            : base(iteration, statusInPercent, message, vertexPositions, innerCanvasSizes)
        {
            _vertexInfos = vertexInfos;
            GravitationCenter = gravitationCenter;
        }

        public override object GetVertexInfo(TVertex vertex)
        {
            TVertexInfo info;
            if (_vertexInfos.TryGetValue(vertex, out info))
                return info;

            return null;
        }

        public IDictionary<TVertex, TVertexInfo> VertexInfos
        {
            get { return _vertexInfos; }
        }

        public IDictionary<TEdge, TEdgeInfo> EdgeInfos
        {
            get { return null; }
        }
    }
}
