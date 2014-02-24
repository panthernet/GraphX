using GraphX.GraphSharp.Algorithms.EdgeRouting;
using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphX.Logic
{
    public partial class GXLogicCore<TVertex, TEdge, TGraph>
        where TVertex : VertexBase
        where TEdge : EdgeBase<TVertex>
        where TGraph : BidirectionalGraph<TVertex, TEdge>
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
            else
            {
                var overlap = ExternalOverlapRemovalAlgorithm;
                overlap.Rectangles = rectangles;
                return overlap;
            }
        }

        public IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size>  vertexSizes)
        {
            if (ExternalLayoutAlgorithm != null)
            {
                var alg = ExternalLayoutAlgorithm;
                if (alg.NeedVertexSizes) alg.VertexSizes = vertexSizes;
                return alg;
            }
            else return AlgorithmFactory.CreateLayoutAlgorithm(DefaultLayoutAlgorithm, Graph, null, vertexSizes, DefaultLayoutAlgorithmParams);
        }

        public IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size DesiredSize)
        {
            if (ExternalEdgeRoutingAlgorithm == null && DefaultEdgeRoutingAlgorithm != EdgeRoutingAlgorithmTypeEnum.None)
            {
                return AlgorithmFactory.CreateEdgeRoutingAlgorithm(DefaultEdgeRoutingAlgorithm, new Rect(DesiredSize), Graph, null, null, DefaultEdgeRoutingAlgorithmParams);
            }
            else if (ExternalEdgeRoutingAlgorithm != null)
            {
                return ExternalEdgeRoutingAlgorithm;
            }
            else return null;
        }
    }
}
