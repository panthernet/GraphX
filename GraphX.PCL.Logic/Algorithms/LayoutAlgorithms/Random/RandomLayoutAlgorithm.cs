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
    public class RandomLayoutAlgorithm<TVertex, TEdge, TGraph> : ILayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        //public Dictionary<TVertex, Point> FreezedVertices { get; set; } 

        private readonly TGraph _graph;
        private readonly System.Random _rnd = new System.Random((int)DateTime.Now.Ticks);
        private readonly IDictionary<TVertex, Point> _positions;

        public RandomLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions)
        {
            _graph = graph;
            _positions = positions;
        }

        public void Compute(CancellationToken cancellationToken)
        {
            _vertexPositions.Clear();
            foreach (var item in _graph.Vertices)
            {
                if (item.SkipProcessing != ProcessingOptionEnum.Freeze || _positions == null)
                    _vertexPositions.Add(item, new Point(_rnd.Next(0, 2000), _rnd.Next(0, 2000)));
                else if (_positions != null)
                {
                    var res = _positions.FirstOrDefault(a => a.Key == item);
                    if(res.Key != null)
                        _vertexPositions.Add(res.Key, res.Value);
                }            
            }
           
        }

        readonly IDictionary<TVertex, Point> _vertexPositions = new Dictionary<TVertex, Point>();

        public IDictionary<TVertex, Point> VertexPositions { get { return _vertexPositions; } }

        public IDictionary<TVertex, Size> VertexSizes { get; set; }

        public bool NeedVertexSizes
        {
            get { return false; }
        }

        public TGraph VisitedGraph
        {
            get { return _graph; }
        }

        public bool SupportsObjectFreeze
        {
            get { return true; }
        }
    }
}
