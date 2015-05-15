using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public partial class CompoundFDPLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        private static void RemoveAll<T>(ICollection<T> set, IEnumerable<T> elements)
        {
            foreach (var e in elements)
            {
                set.Remove(e);
            }
        }

        /// <summary>
        /// <list type="ul">
        /// <listheader>
        /// Initializes the algorithm, and the following things:
        /// </listheader>
        /// <item>the nodes sizes (of the compound vertices)</item>
        /// <item>the thresholds for the convergence</item>
        /// <item>random initial positions (if the position is not null)</item>
        /// <item>remove the 'tree-nodes' from the root graph (level 0)</item>
        /// </list>
        /// </summary>
        /// <param name="vertexSizes">The dictionary of the inner canvas sizes 
        /// of the compound vertices.</param>
        /// <param name="vertexBorders">The dictionary of the border thickness of
        /// the compound vertices.</param>
        /// <param name="layoutTypes">The dictionary of the layout types of 
        /// the compound vertices.</param>
        private void Init(IDictionary<TVertex, Size> vertexSizes, IDictionary<TVertex, Thickness> vertexBorders, IDictionary<TVertex, CompoundVertexInnerLayoutType> layoutTypes)
        {
            InitializeWithRandomPositions(100, 100);

            var movableParentUpdateQueue = new Queue<TVertex>();
            _rootCompoundVertex.Children = new HashSet<VertexData>();

            InitialLevels();

            InitSimpleVertices(vertexSizes);
            InitCompoundVertices(vertexBorders, vertexSizes, layoutTypes, /*out*/ movableParentUpdateQueue);

            SetLevelIndices();

            //TODO do we need this?
            InitMovableParentOfFixedVertices(movableParentUpdateQueue);

            RemoveTreeNodesFromRootGraph();

            InitGravitationMagnitude();
        }

        private void InitGravitationMagnitude()
        {
            //get the average width and height
            double sumWidth = 0, sumHeight = 0;
            foreach (var vertex in _levels[0])
            {
                var v = _vertexDatas[vertex];
                sumWidth += v.Size.Width;
                sumHeight += v.Size.Height;
            }
            if (_levels[0].Count > 0)
                _gravityForceMagnitude = Math.Min(sumWidth, sumHeight) / _levels[0].Count;
        }

        private void RemoveTreeNodesFromRootGraph()
        {
            bool removed = true;
            for (int i = 0; removed; i++)
            {
                removed = false;
                foreach (var vertex in _levels[0])
                {
                    if (_compoundGraph.Degree(vertex) != 1 || _compoundGraph.IsCompoundVertex(vertex))
                        continue;

                    TEdge edge = default(TEdge);
                    if (_compoundGraph.InDegree(vertex) > 0)
                        edge = _compoundGraph.InEdge(vertex, 0);
                    else
                        edge = _compoundGraph.OutEdge(vertex, 0);
                    if (_removedRootTreeEdges.Contains(edge))
                        continue;

                    //the vertex is a leaf tree node
                    removed = true;
                    while (_removedRootTreeNodeLevels.Count <= i)
                        _removedRootTreeNodeLevels.Push(new List<RemovedTreeNodeData>());

                    //add to the removed vertices
                    _removedRootTreeNodeLevels.Peek().Add(new RemovedTreeNodeData(vertex, edge));
                    _removedRootTreeNodes.Add(vertex);
                    _removedRootTreeEdges.Add(edge);
                }

                if (removed && _removedRootTreeNodeLevels.Count > 0)
                    //remove from the level
                    foreach (var tnd in _removedRootTreeNodeLevels.Peek())
                    {
                        _levels[0].Remove(tnd.Vertex);

                        //remove the vertex from the graph
                        _compoundGraph.RemoveEdge(tnd.Edge);
                        _compoundGraph.RemoveVertex(tnd.Vertex);
                    }
            }
        }

        private void InitMovableParentOfFixedVertices(Queue<TVertex> movableParentUpdateQueue)
        {
            while (movableParentUpdateQueue.Count > 0)
            {
                //geth the compound vertex with the fixed layout
                var fixedLayoutedCompoundVertex = movableParentUpdateQueue.Dequeue();

                //find the not fixed predecessor
                TVertex movableVertex = fixedLayoutedCompoundVertex;
                for (; ; )
                {
                    //if the vertex hasn't parent
                    if (movableVertex == null)
                        break;

                    TVertex parent = _compoundGraph.GetParent(movableVertex);
                    if (parent == null)
                        break;

                    //if the parent's layout type is fixed
                    if (_compoundVertexDatas[parent].InnerVertexLayoutType != CompoundVertexInnerLayoutType.Fixed)
                        break;

                    movableVertex = parent;
                }
                //the movable vertex is the ancestor of the children of the vertex
                //that could be moved

                //fix the child vertices and set the movable parent
                foreach (var childVertex in _compoundGraph.GetChildrenVertices(fixedLayoutedCompoundVertex))
                {
                    var data = _vertexDatas[childVertex];
                    data.IsFixedToParent = true;
                    data.MovableParent = _vertexDatas[movableVertex];
                }
            }
        }

        /// <summary>
        /// Initializes the data of the simple vertices.
        /// </summary>
        /// <param name="vertexSizes">Dictionary of the vertex sizes.</param>
        private void InitSimpleVertices(IDictionary<TVertex, Size> vertexSizes)
        {
            foreach (var vertex in _compoundGraph.SimpleVertices)
            {
                Size vertexSize;
                vertexSizes.TryGetValue(vertex, out vertexSize);

                var position = new Point();
                VertexPositions.TryGetValue(vertex, out position);

                //create the information container for this simple vertex
                var dataContainer = new SimpleVertexData(vertex, _rootCompoundVertex, false, position, vertexSize);
                dataContainer.Parent = _rootCompoundVertex;
                _simpleVertexDatas[vertex] = dataContainer;
                _vertexDatas[vertex] = dataContainer;
                _rootCompoundVertex.Children.Add(dataContainer);
            }
        }

        /// <summary>
        /// Initializes the data of the compound vertices.
        /// </summary>
        /// <param name="vertexBorders">Dictionary of the border thicknesses.</param>
        /// <param name="vertexSizes">Dictionary of the vertex sizes.</param>
        /// <param name="layoutTypes">Dictionary of the layout types.</param>
        /// <param name="movableParentUpdateQueue">The compound vertices with fixed layout
        /// should be added to this queue.</param>
        private void InitCompoundVertices(
            IDictionary<TVertex, Thickness> vertexBorders,
            IDictionary<TVertex, Size> vertexSizes,
            IDictionary<TVertex, CompoundVertexInnerLayoutType> layoutTypes,
            Queue<TVertex> movableParentUpdateQueue)
        {
            for (int i = _levels.Count - 1; i >= 0; i--)
            {
                foreach (var vertex in _levels[i])
                {
                    if (!_compoundGraph.IsCompoundVertex(vertex))
                        continue;

                    //get the data of the vertex
                    Thickness border;
                    vertexBorders.TryGetValue(vertex, out border);

                    Size vertexSize;
                    vertexSizes.TryGetValue(vertex, out vertexSize);
                    var layoutType = CompoundVertexInnerLayoutType.Automatic;
                    layoutTypes.TryGetValue(vertex, out layoutType);

                    if (layoutType == CompoundVertexInnerLayoutType.Fixed)
                    {
                        movableParentUpdateQueue.Enqueue(vertex);
                    }

                    var position = new Point();
                    VertexPositions.TryGetValue(vertex, out position);

                    //create the information container for this compound vertex
                    var dataContainer = new CompoundVertexData(vertex, _rootCompoundVertex, false, position, vertexSize, border, layoutType);
                    if (i == 0)
                    {
                        dataContainer.Parent = _rootCompoundVertex;
                        _rootCompoundVertex.Children.Add(dataContainer);
                    }
                    _compoundVertexDatas[vertex] = dataContainer;
                    _vertexDatas[vertex] = dataContainer;

                    //add the datas of the childrens
                    var children = _compoundGraph.GetChildrenVertices(vertex);
                    var childrenData = children.Select(v => _vertexDatas[v]).ToList();
                    dataContainer.Children = childrenData;
                    foreach (var child in dataContainer.Children)
                    {
                        _rootCompoundVertex.Children.Remove(child);
                        child.Parent = dataContainer;
                    }
                }
            }
        }

        private void InitialLevels()
        {
            var verticesLeft = new HashSet<TVertex>(VisitedGraph.Vertices);

            //initial 0th level
            _levels.Add(new HashSet<TVertex>(VisitedGraph.Vertices.Where(v => _compoundGraph.GetParent(v) == default(TVertex))));
            RemoveAll(verticesLeft, _levels[0]);

            //other layers
            for (int i = 1; verticesLeft.Count > 0; i++)
            {
                var nextLevel = new HashSet<TVertex>();
                foreach (var parent in _levels[i - 1])
                {
                    if (_compoundGraph.GetChildrenCount(parent) <= 0)
                        continue;

                    foreach (var children in _compoundGraph.GetChildrenVertices(parent))
                        nextLevel.Add(children);
                }
                _levels.Add(nextLevel);
                RemoveAll(verticesLeft, nextLevel);
            }
        }

        private void SetLevelIndices()
        {
            //set the level indexes in the vertex datas
            for (int i = 0; i < _levels.Count; i++)
            {
                foreach (var v in _levels[i])
                    _vertexDatas[v].Level = i;
            }
        }
    }
}