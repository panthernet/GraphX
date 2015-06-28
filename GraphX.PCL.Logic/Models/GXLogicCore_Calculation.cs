using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using QuickGraph;

namespace GraphX.PCL.Logic.Models
{
    public partial class GXLogicCore<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : class, IGraphXEdge<TVertex>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
    {
        /// <summary>
        /// Gets if current algorithms set needs vertices sizes
        /// </summary>
        public bool AreVertexSizesNeeded()
        {
            return (ExternalLayoutAlgorithm == null && AlgorithmFactory.NeedSizes(DefaultLayoutAlgorithm))
                ||
                    (ExternalLayoutAlgorithm != null && ExternalLayoutAlgorithm.NeedVertexSizes)
                || 
                    AreOverlapNeeded();
        }

        /// <summary>
        /// Gets if current algorithms set actualy needs overlap removal algorithm
        /// </summary>
        public bool AreOverlapNeeded()
        {
            return (ExternalOverlapRemovalAlgorithm == null && AlgorithmFactory.NeedOverlapRemoval(DefaultLayoutAlgorithm) && DefaultOverlapRemovalAlgorithm != OverlapRemovalAlgorithmTypeEnum.None) ||
                    (ExternalOverlapRemovalAlgorithm != null);
        }

        /// <summary>
        /// Generate overlap removal algorithm according to LogicCore overlap removal algorithm default/external properties set
        /// </summary>
        /// <param name="rectangles">Vertices rectangular sizes</param>
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

        /// <summary>
        /// Generate layout algorithm according to LogicCore layout algorithm default/external properties set
        /// </summary>
        /// <param name="vertexSizes">Vertices sizes</param>
        /// <param name="vertexPositions">Vertices positions</param>
        public IExternalLayout<TVertex> GenerateLayoutAlgorithm(Dictionary<TVertex, Size>  vertexSizes, IDictionary<TVertex, Point> vertexPositions)
        {
            if (ExternalLayoutAlgorithm == null) return AlgorithmFactory.CreateLayoutAlgorithm(DefaultLayoutAlgorithm, Graph, vertexPositions, vertexSizes, DefaultLayoutAlgorithmParams);
            var alg = ExternalLayoutAlgorithm;
            if (alg.NeedVertexSizes) alg.VertexSizes = vertexSizes;
            return alg;
        }

        /// <summary>
        /// Generate layout algorithm according to LogicCore layout algorithm default/external properties set
        /// </summary>
        /// <param name="desiredSize">Desired rectangular area size that will be taken into account</param>
        /// <param name="vertexPositions">Vertices positions</param>
        /// <param name="rectangles">Vertices rectangular sizes</param>
        public IExternalEdgeRouting<TVertex, TEdge> GenerateEdgeRoutingAlgorithm(Size desiredSize, IDictionary<TVertex, Point> vertexPositions = null, IDictionary<TVertex, Rect> rectangles = null)
        {
            if (ExternalEdgeRoutingAlgorithm == null && DefaultEdgeRoutingAlgorithm != EdgeRoutingAlgorithmTypeEnum.None)
            {
                return AlgorithmFactory.CreateEdgeRoutingAlgorithm(DefaultEdgeRoutingAlgorithm, new Rect(desiredSize), Graph, vertexPositions, rectangles, DefaultEdgeRoutingAlgorithmParams);
            }
            return ExternalEdgeRoutingAlgorithm;
        }


        /// <summary>
        /// Computes all edge routes related to specified vertex
        /// </summary>
        /// <param name="dataVertex">Vertex data</param>
        /// <param name="vertexPosition">Vertex position</param>
        /// <param name="vertexSize">Vertex size</param>
        public void ComputeEdgeRoutesByVertex(TVertex dataVertex, Point? vertexPosition = null, Size? vertexSize = null)
        {
            if (AlgorithmStorage == null || AlgorithmStorage.EdgeRouting == null)
                throw new GX_InvalidDataException("GXC: Algorithm storage is not initialized!");
            if (dataVertex == null) return;
            var list = new List<TEdge>();
            IEnumerable<TEdge> edges;
            Graph.TryGetInEdges(dataVertex, out edges);
            if(edges != null)
                list.AddRange(edges);
            Graph.TryGetOutEdges(dataVertex, out edges);
            if(edges != null)
                list.AddRange(edges);

            if (vertexPosition.HasValue && vertexSize.HasValue)
                UpdateVertexDataForEr(dataVertex, vertexPosition.Value, vertexSize.Value);

            foreach (var item in list)
                item.RoutingPoints = AlgorithmStorage.EdgeRouting.ComputeSingle(item);
        }

        public IDictionary<TVertex, Point> Compute(CancellationToken cancellationToken)
        {
            if (Graph == null)
                throw new GX_InvalidDataException("LogicCore -> Graph property not set!");

            IDictionary<TVertex, Point> resultCoords;
            Dictionary<TVertex, Rect> rectangles = null; //rectangled size data
            
            if (AlgorithmStorage.Layout != null)
            {
                AlgorithmStorage.Layout.Compute(cancellationToken);
                resultCoords = AlgorithmStorage.Layout.VertexPositions;
            }//get default coordinates if using Custom layout
            else resultCoords = _vertexPosSource;

            //overlap removal
            if (AlgorithmStorage.OverlapRemoval != null)
            {
                //generate rectangle data from sizes
                rectangles = GetVertexSizeRectangles(resultCoords, _vertexSizes, true);

                AlgorithmStorage.OverlapRemoval.Rectangles = rectangles;
                AlgorithmStorage.OverlapRemoval.Compute(cancellationToken);
                resultCoords = new Dictionary<TVertex, Point>();
                foreach (var res in AlgorithmStorage.OverlapRemoval.Rectangles)
                    resultCoords.Add(res.Key, new Point(res.Value.Left, res.Value.Top));
            }

            //Edge Routing
            var algEr = AlgorithmStorage.Layout as ILayoutEdgeRouting<TEdge>;
            if (AlgorithmStorage.EdgeRouting != null && (algEr == null || algEr.EdgeRoutes == null || !algEr.EdgeRoutes.Any()))
            {
                    //var size = Parent is ZoomControl ? (Parent as ZoomControl).Presenter.ContentSize : DesiredSize;
                AlgorithmStorage.EdgeRouting.AreaRectangle = CalculateContentRectangle(resultCoords);// new Rect(TopLeft.X, TopLeft.Y, size.Width, size.Height);
                rectangles = GetVertexSizeRectangles(resultCoords, _vertexSizes);

                AlgorithmStorage.EdgeRouting.VertexPositions = resultCoords;
                AlgorithmStorage.EdgeRouting.VertexSizes = rectangles;
                AlgorithmStorage.EdgeRouting.Compute(cancellationToken);
                if (AlgorithmStorage.EdgeRouting.EdgeRoutes != null)
                    foreach (var item in AlgorithmStorage.EdgeRouting.EdgeRoutes)
                        item.Key.RoutingPoints = item.Value;
            }

            if (algEr != null && algEr.EdgeRoutes != null)
            {
                foreach (var item in algEr.EdgeRoutes)
                    item.Key.RoutingPoints = item.Value;
            }

            return resultCoords;
        }

        private Rect CalculateContentRectangle(IDictionary<TVertex, Point> actualPositions = null)
        {
            double minX=0;
            double minY=0;
            double maxX=0;
            double maxY=0;

            actualPositions = actualPositions ?? _vertexPosSource;

            foreach (var pos in actualPositions.Values)
            {
                if (pos.X < minX) minX = pos.X;
                if (pos.Y < minY) minY = pos.Y;
                if (pos.X > maxX) maxX = pos.X;
                if (pos.Y > maxY) maxY = pos.Y;
            }

            return new Rect(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }
    }
}
