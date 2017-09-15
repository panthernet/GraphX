using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Models
{
    public sealed class AlgorithmStorage<TVertex, TEdge> : IAlgorithmStorage<TVertex, TEdge>
    {
        public IExternalLayout<TVertex, TEdge> Layout { get; private set; }
        public IExternalOverlapRemoval<TVertex> OverlapRemoval { get; private set; }
        public IExternalEdgeRouting<TVertex, TEdge> EdgeRouting { get; private set; }

        public AlgorithmStorage(IExternalLayout<TVertex, TEdge> layout, IExternalOverlapRemoval<TVertex> or, IExternalEdgeRouting<TVertex, TEdge> er)
        {
            Layout = layout; OverlapRemoval = or; EdgeRouting = er;
        }

        public void RemoveSingleEdge(TEdge edge)
        {
            if (EdgeRouting != null && EdgeRouting.EdgeRoutes != null && EdgeRouting.EdgeRoutes.ContainsKey(edge))
                EdgeRouting.EdgeRoutes.Remove(edge);
        }

        public void AddSingleEdge(TEdge edge, Point[] routingPoints = null)
        {
            if (EdgeRouting != null && EdgeRouting.EdgeRoutes != null && !EdgeRouting.EdgeRoutes.ContainsKey(edge))
                EdgeRouting.EdgeRoutes.Add(edge,routingPoints);
        }

        public void RemoveSingleVertex(TVertex vertex)
        {
            if (Layout != null)
            {
                if (Layout.VertexPositions != null && Layout.VertexPositions.ContainsKey(vertex))
                    Layout.VertexPositions.Remove(vertex);
                if (Layout.VertexSizes != null && Layout.VertexSizes.ContainsKey(vertex))
                    Layout.VertexSizes.Remove(vertex);
            }
            if (OverlapRemoval != null && OverlapRemoval.Rectangles != null && OverlapRemoval.Rectangles.ContainsKey(vertex))
                OverlapRemoval.Rectangles.Remove(vertex);
            if (EdgeRouting != null)
            {
                if (EdgeRouting.VertexSizes != null && EdgeRouting.VertexSizes.ContainsKey(vertex))
                    EdgeRouting.VertexSizes.Remove(vertex);
                if (EdgeRouting.VertexPositions != null && EdgeRouting.VertexPositions.ContainsKey(vertex))
                    EdgeRouting.VertexPositions.Remove(vertex);
            }
        }

        public void AddSingleVertex(TVertex vertex, Point position, Rect size)
        {
            if (Layout != null)
            {
                if (Layout.VertexPositions != null && !Layout.VertexPositions.ContainsKey(vertex))
                    Layout.VertexPositions.Add(vertex, position);
                if (Layout.VertexSizes != null && !Layout.VertexSizes.ContainsKey(vertex))
                    Layout.VertexSizes.Add(vertex, size.Size);
            }
            if (OverlapRemoval != null && OverlapRemoval.Rectangles != null && !OverlapRemoval.Rectangles.ContainsKey(vertex))
                OverlapRemoval.Rectangles.Add(vertex, size);
            if (EdgeRouting != null)
            {
                if (EdgeRouting.VertexSizes != null && !EdgeRouting.VertexSizes.ContainsKey(vertex))
                    EdgeRouting.VertexSizes.Add(vertex, size);
                if (EdgeRouting.VertexPositions != null && !EdgeRouting.VertexPositions.ContainsKey(vertex))
                    EdgeRouting.VertexPositions.Add(vertex, position);
            }
        }
    }
}
