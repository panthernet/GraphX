using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;

namespace GraphX.PCL.Logic.Algorithms
{
    public static class GraphXExtensions
    {
        /// <summary>
        /// Returns with the adjacent vertices of the <code>vertex</code>.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="vertex">The vertex which neighbours' we want to get.</param>
        /// <returns>List of the adjacent vertices of the <code>vertex</code>.</returns>
        public static IEnumerable<TVertex> GetNeighbours<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge> g, TVertex vertex)
            where TEdge : IEdge<TVertex>
        {
            return ((from e in g.InEdges(vertex) select e.Source)
                .Concat(
                (from e in g.OutEdges(vertex) select e.Target))).Distinct();
        }


        public static IEnumerable<TVertex> GetOutNeighbours<TVertex, TEdge>(this IVertexAndEdgeListGraph<TVertex, TEdge> g, TVertex vertex)
            where TEdge : IEdge<TVertex>
        {
            return (from e in g.OutEdges(vertex)
                    select e.Target).Distinct();
        }


        /// <summary>
        /// If the graph g is directed, then returns every edges which source is one of the vertices in the <code>set1</code>
        /// and the target is one of the vertices in <code>set2</code>.
        /// </summary>
        /// <typeparam name="TVertex">Type of the vertex.</typeparam>
        /// <typeparam name="TEdge">Type of the edge.</typeparam>
        /// <param name="g">The graph.</param>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns>Return the list of the selected edges.</returns>
        public static IEnumerable<TEdge> GetEdgesBetween<TVertex, TEdge>(this IVertexAndEdgeListGraph<TVertex, TEdge> g, List<TVertex> set1, List<TVertex> set2)
            where TEdge : IEdge<TVertex>
        {
            var edgesBetween = new List<TEdge>();

            //vegig kell menni az osszes vertex minden elen, es megnezni, hogy a target hol van
            foreach (var v in set1)
            {
                edgesBetween.AddRange(g.OutEdges(v).Where(edge => set2.Contains(edge.Target)));
            }

            return edgesBetween;
        }


        /// <summary>
        /// Returns with the sources in the graph.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g">The graph.</param>
        /// <returns>Returns with the sources in the graph.</returns>
        public static IEnumerable<TVertex> GetSources<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge> g)
            where TEdge : IEdge<TVertex>
        {
            return from v in g.Vertices
                   where g.InDegree(v) == 0
                   select v;
        }

        /// <summary>
        /// Gets the diameter of a graph.
        /// The diameter is the greatest distance between two vertices.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <returns>The diameter of the Graph <code>g</code>.</returns>
        public static double GetDiameter<TVertex, TEdge, TGraph>(this TGraph g)
            where TEdge : IEdge<TVertex>
            where TGraph : IBidirectionalGraph<TVertex, TEdge>
        {
            double[,] distances;
            return g.GetDiameter<TVertex, TEdge, TGraph>(out distances);
        }

        /// <summary>
        /// Gets the diameter of a graph.
        /// The diameter is the greatest distance between two vertices.
        /// </summary>
        /// <param name="g">The graph.</param>
        /// <param name="distances">This is an out parameter. It gives the distances between every vertex-pair.</param>
        /// <returns>The diameter of the Graph <code>g</code>.</returns>
        public static double GetDiameter<TVertex, TEdge, TGraph>(this TGraph g, out double[,] distances)
            where TEdge : IEdge<TVertex>
            where TGraph : IBidirectionalGraph<TVertex, TEdge>
        {
            distances = GetDistances<TVertex, TEdge, TGraph>(g);

            var n = g.VertexCount;
            var distance = double.NegativeInfinity;
            for (var i = 0; i < n - 1; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    if (double.MaxValue == distances[i, j])
                        continue;

                    distance = Math.Max(distance, distances[i, j]);
                }
            }
            return distance;
        }

        /// <param name="g">The graph.</param>
        /// <returns>Returns with the distance between the vertices (distance: number of the edges).</returns>
        private static double[,] GetDistances<TVertex, TEdge, TGraph>(TGraph g)
            where TEdge : IEdge<TVertex>
            where TGraph : IBidirectionalGraph<TVertex, TEdge>
        {
            var distances = new double[g.VertexCount, g.VertexCount];
            for (var k = 0; k < g.VertexCount; k++)
            {
                for (var j = 0; j < g.VertexCount; j++)
                {
                    distances[k, j] = double.PositiveInfinity;
                }
            }

            var undirected = new UndirectedBidirectionalGraph<TVertex, TEdge>(g);
            //minden йlet egy hosszal veszьnk figyelembe - unweighted
            var weights = new Dictionary<TEdge, double>();
            foreach (var edge in undirected.Edges)
            {
                weights[edge] = 1;
            }

            //compute the distances from every vertex: O(n(n^2 + e)) complexity
            var i = 0;
            foreach (var source in g.Vertices)
            {
                //compute the distances from the 'source'
                var spaDijkstra =
                    new UndirectedDijkstraShortestPathAlgorithm<TVertex, TEdge>(undirected, edge => weights[edge], QuickGraph.Algorithms.DistanceRelaxers.ShortestDistance);
                spaDijkstra.Compute(source);

                var j = 0;
                foreach (var v in undirected.Vertices)
                {
                    var d = spaDijkstra.Distances[v];
                    distances[i, j] = Math.Min(distances[i, j], d);
                    distances[i, j] = Math.Min(distances[i, j], distances[j, i]);
                    distances[j, i] = Math.Min(distances[i, j], distances[j, i]);
                    j++;
                }
                i++;
            }

            return distances;
        }

        public static TVertex OtherVertex<TVertex>(this IEdge<TVertex> edge, TVertex thisVertex)
        {
            return edge.Source.Equals(thisVertex) ? edge.Target : edge.Source;
        }



        public static void AddEdgeRange<TVertex, TEdge>(this IMutableEdgeListGraph<TVertex, TEdge> graph, IEnumerable<TEdge> edges)
            where TEdge : IEdge<TVertex>
        {
            foreach (var edge in edges)
                graph.AddEdge(edge);
        }



        public static BidirectionalGraph<TNewVertex, TNewEdge> Convert<TOldVertex, TOldEdge, TNewVertex, TNewEdge>(
            this IVertexAndEdgeListGraph<TOldVertex, TOldEdge> oldGraph,
            Func<TOldVertex, TNewVertex> vertexMapperFunc,
            Func<TOldEdge, TNewEdge> edgeMapperFunc)
            where TOldEdge : IEdge<TOldVertex>
            where TNewEdge : IEdge<TNewVertex>
        {
            return oldGraph.Convert(
                new BidirectionalGraph<TNewVertex, TNewEdge>(oldGraph.AllowParallelEdges, oldGraph.VertexCount),
                vertexMapperFunc,
                edgeMapperFunc);
        }



        public static BidirectionalGraph<TOldVertex, TNewEdge> Convert<TOldVertex, TOldEdge, TNewEdge>(
            this IVertexAndEdgeListGraph<TOldVertex, TOldEdge> oldGraph,
            Func<TOldEdge, TNewEdge> edgeMapperFunc)
            where TOldEdge : IEdge<TOldVertex>
            where TNewEdge : IEdge<TOldVertex>
        {
            return oldGraph.Convert<TOldVertex, TOldEdge, TOldVertex, TNewEdge>(null, edgeMapperFunc);
        }



        public static TNewGraph Convert<TOldVertex, TOldEdge, TNewVertex, TNewEdge, TNewGraph>(
            this IVertexAndEdgeListGraph<TOldVertex, TOldEdge> oldGraph,
            TNewGraph newGraph,
            Func<TOldVertex, TNewVertex> vertexMapperFunc,
            Func<TOldEdge, TNewEdge> edgeMapperFunc)
            where TOldEdge : IEdge<TOldVertex>
            where TNewEdge : IEdge<TNewVertex>
            where TNewGraph : IMutableVertexAndEdgeListGraph<TNewVertex, TNewEdge>
        {
            //VERTICES
            newGraph.AddVertexRange(vertexMapperFunc != null ? oldGraph.Vertices.Select(vertexMapperFunc) : oldGraph.Vertices.Cast<TNewVertex>());

            //EDGES
            newGraph.AddEdgeRange(edgeMapperFunc != null ? oldGraph.Edges.Select(edgeMapperFunc) : oldGraph.Edges.Cast<TNewEdge>());

            return newGraph;
        }



        public static TNewGraph Convert<TOldVertex, TOldEdge, TNewEdge, TNewGraph>(
            this IVertexAndEdgeListGraph<TOldVertex, TOldEdge> oldGraph,
            TNewGraph newGraph,
            Func<TOldEdge, TNewEdge> edgeMapperFunc)
            where TOldEdge : IEdge<TOldVertex>
            where TNewEdge : IEdge<TOldVertex>
            where TNewGraph : IMutableVertexAndEdgeListGraph<TOldVertex, TNewEdge>
        {
            return oldGraph.Convert<TOldVertex, TOldEdge, TOldVertex, TNewEdge, TNewGraph>(newGraph, null, edgeMapperFunc);
        }



        public static TNewGraph Convert<TOldVertex, TOldEdge, TNewGraph>(
            this IVertexAndEdgeListGraph<TOldVertex, TOldEdge> oldGraph,
            TNewGraph newGraph)
            where TOldEdge : IEdge<TOldVertex>
            where TNewGraph : IMutableVertexAndEdgeListGraph<TOldVertex, TOldEdge>
        {
            return oldGraph.Convert<TOldVertex, TOldEdge, TOldVertex, TOldEdge, TNewGraph>(newGraph, null, null);
        }


        public static BidirectionalGraph<TVertex, TEdge> CopyToBidirectionalGraph<TVertex, TEdge>(
            this IVertexAndEdgeListGraph<TVertex, TEdge> graph, bool includeEmpty = true)
            where TEdge : IEdge<TVertex>
        {
            var newGraph = new BidirectionalGraph<TVertex, TEdge>();

            //copy the vertices
            if(!includeEmpty)
                newGraph.AddVerticesAndEdgeRange(graph.Edges);
            else
            {
                newGraph.AddVertexRange(graph.Vertices);
                newGraph.AddEdgeRange(graph.Edges);
            }

            return newGraph;
        }
    }
}
