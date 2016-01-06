using System;
using System.Collections.Generic;
using QuickGraph;

namespace GraphX.PCL.Logic.Helpers
{
    public static class TypeExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> func)
        {
            foreach (var item in list)
                func(item);
        }

        /// <summary>
        /// Get all edges associated with the vertex
        /// </summary>
        /// <typeparam name="TVertex">Vertex data type</typeparam>
        /// <typeparam name="TEdge">Edge data type</typeparam>
        /// <param name="graph">Graph</param>
        /// <param name="vertex">Vertex</param>
        public static IEnumerable<TEdge> GetAllEdges<TVertex, TEdge>(this IBidirectionalGraph<TVertex, TEdge> graph, TVertex vertex)
            where TEdge: IEdge<TVertex>
        {
            var result = new List<TEdge>();
            IEnumerable<TEdge> edges;
            graph.TryGetOutEdges(vertex, out edges);
            if(edges != null)
                result.AddRange(edges);
            graph.TryGetInEdges(vertex, out edges);
            if(edges != null)
                result.AddRange(edges);
            return result;
        }
    }
}
