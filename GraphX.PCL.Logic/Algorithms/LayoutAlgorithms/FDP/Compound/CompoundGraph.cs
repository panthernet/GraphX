using System.Collections.Generic;
using System.Linq;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class CompoundGraph<TVertex, TEdge> : BidirectionalGraph<TVertex, TEdge>, IMutableCompoundGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        public CompoundGraph()
        {

        }

        public CompoundGraph(bool allowParallelEdges)
            : base(allowParallelEdges)
        {

        }

        public CompoundGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity)
        {

        }

        public CompoundGraph(IBidirectionalGraph<TVertex, TEdge> graph)
            : base(graph.AllowParallelEdges, graph.VertexCount)
        {
            //copy the vertices
            AddVertexRange(graph.Vertices);

            //copy the edges
            AddEdgeRange(graph.Edges);
        }

        public CompoundGraph(ICompoundGraph<TVertex, TEdge> graph)
            : base(graph.AllowParallelEdges, graph.VertexCount)
        {
            //copy the vertices
            AddVertexRange(graph.Vertices);

            //copy the containment information
            foreach (var vertex in graph.Vertices)
            {
                if (!graph.IsChildVertex(vertex))
                    continue;

                var parent = graph.GetParent(vertex);
                AddChildVertex(parent, vertex);
            }

            //copy the edges
            AddEdgeRange(graph.Edges);
        }

        private readonly IDictionary<TVertex, TVertex> _parentRegistry =
            new Dictionary<TVertex, TVertex>();

        private readonly IDictionary<TVertex, IList<TVertex>> _childrenRegistry =
            new Dictionary<TVertex, IList<TVertex>>();

        public IEnumerable<TVertex> CompoundVertices
        {
            get
            {
                return _childrenRegistry.Keys;
            }
        }

        public IEnumerable<TVertex> SimpleVertices
        {
            get { return Vertices.Where(v => !_childrenRegistry.ContainsKey(v)); }
        }

        private IList<TVertex> GetChildrenList(TVertex vertex, bool createIfNotExists)
        {
            IList<TVertex> childrenList;
            if (_childrenRegistry.TryGetValue(vertex, out childrenList) || !createIfNotExists)
                return childrenList;

            childrenList = new List<TVertex>();
            _childrenRegistry[vertex] = childrenList;
            return childrenList;
        }

        #region ICompoundGraph<TVertex,TEdge> Members

        public bool AddChildVertex(TVertex parent, TVertex child)
        {
            if (!ContainsVertex(child))
                AddVertex(child);
            _parentRegistry[child] = parent;
            GetChildrenList(parent, true).Add(child);
            return true;
        }

        public int AddChildVertexRange(TVertex parent, IEnumerable<TVertex> children)
        {
            int ret = AddVertexRange(children);
            IList<TVertex> childrenList = GetChildrenList(parent, true);
            foreach (var v in children)
            {
                _parentRegistry[v] = parent;
                childrenList.Add(v);
            }
            return ret;
        }

        public TVertex GetParent(TVertex vertex)
        {
            TVertex parent;
            if (_parentRegistry.TryGetValue(vertex, out parent))
                return parent;

            return default(TVertex);
        }

        public bool IsChildVertex(TVertex vertex)
        {
            return _parentRegistry.ContainsKey(vertex);
        }

        public IEnumerable<TVertex> GetChildrenVertices(TVertex vertex)
        {
            return GetChildrenList(vertex, false);
        }

        public int GetChildrenCount(TVertex vertex)
        {
            IList<TVertex> childrenList = GetChildrenList(vertex, false);
            if (childrenList == null)
                return 0;

            return childrenList.Count;
        }

        public bool IsCompoundVertex(TVertex vertex)
        {
            return GetChildrenList(vertex, false) != null;
        }

        #endregion

        public override bool RemoveVertex(TVertex v)
        {
            bool removed = base.RemoveVertex(v);
            if (removed)
            {
                _parentRegistry.Remove(v);
                _childrenRegistry.Remove(v);
            }
            return removed;
        }
    }
}
