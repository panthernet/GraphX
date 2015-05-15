using System.Collections.Generic;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Models
{
    public partial class GXLogicCore<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        public bool AreVertexSizesNeeded()
        {
            return (ExternalLayoutAlgorithm == null && AlgorithmFactory.NeedSizes(DefaultLayoutAlgorithm)) ||
                    (ExternalLayoutAlgorithm != null && ExternalLayoutAlgorithm.NeedVertexSizes);
        }

        public bool AreOverlapNeeded()
        {
            return (ExternalOverlapRemovalAlgorithm == null && AlgorithmFactory.NeedOverlapRemoval(DefaultLayoutAlgorithm) && DefaultOverlapRemovalAlgorithm != OverlapRemovalAlgorithmTypeEnum.None) ||
                    (ExternalOverlapRemovalAlgorithm != null);
        }

        public IExternalOverlapRemoval<TVertex> GenerateOverlapRemovalAlgorithm(Dictionary<TVertex, Rect> rectangles = null)
        {
            if (ExternalOverlapRemovalAlgorithm == null)
            {
                //create default OR
                return AlgorithmFactory.CreateOverlapRemovalAlgorithm(DefaultOverlapRemovalAlgorithm, null, DefaultOverlapRemovalAlgorithmParams);
            }
            var overlap = ExternalOverlapRemovalAlgorithm;
            overlap.Rectangles = rectangles;
            return overlap;
        }

        public IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size>  vertexSizes, IDictionary<TVertex, Point> vertexPositions)
        {
            if (ExternalLayoutAlgorithm == null) return AlgorithmFactory.CreateLayoutAlgorithm(DefaultLayoutAlgorithm, Graph, vertexPositions, vertexSizes, DefaultLayoutAlgorithmParams);
            var alg = ExternalLayoutAlgorithm;
            if (alg.NeedVertexSizes) alg.VertexSizes = vertexSizes;
            return alg;
        }

        public IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size desiredSize, IDictionary<TVertex, Point> positions = null, IDictionary<TVertex, Rect> rectangles = null)
        {
            if (ExternalEdgeRoutingAlgorithm == null && DefaultEdgeRoutingAlgorithm != EdgeRoutingAlgorithmTypeEnum.None)
            {
                return AlgorithmFactory.CreateEdgeRoutingAlgorithm(DefaultEdgeRoutingAlgorithm, new Rect(desiredSize), Graph, positions, rectangles, DefaultEdgeRoutingAlgorithmParams);
            }
            return ExternalEdgeRoutingAlgorithm;
        }
    }
}
