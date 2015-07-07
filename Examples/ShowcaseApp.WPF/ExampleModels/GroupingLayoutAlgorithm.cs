using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Controls;
using GraphX.Measure;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using QuickGraph;

namespace ShowcaseApp.WPF
{
    public class GroupingLayoutAlgorithm<TVertex, TEdge, TGraph> : LayoutAlgorithmBase<TVertex, TEdge, TGraph>
        where TVertex: class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : class, IVertexAndEdgeListGraph<TVertex, TEdge>
    {        

        public override bool NeedVertexSizes { get { return true; } }
        readonly List<AlgorithmGroupParameters<TVertex>> _groupParams;

        public GroupingLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions, List<AlgorithmGroupParameters<TVertex>> groupParams)
            :base(graph, positions)
        {
            _groupParams = groupParams;
            if (_groupParams == null || _groupParams.Count == 0)
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with at least one group parameters data!");
            if (groupParams.GroupBy(a => a.GroupId).Count() != groupParams.Count)
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has unique GroupId in each record!");
            if (groupParams.Any(a => a.ZoneRectangle == Rect.Empty))
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has non-empty ZoneRectangle defined in each record!");
            if (groupParams.Any(a => a.LayoutAlgorithm == null))
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has not null LayoutAlgorithm defined in each record!");
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            var groups = _groupParams.Select(a => a.GroupId).OrderByDescending(a=> a).ToList();
            foreach (var group in groups)
            {
                var groupId = group;
                var gp = _groupParams.First(a=> a.GroupId == groupId);
                //get vertices of the same group
                //var vertices = new List<TVertex>();
                var vertices = VisitedGraph.Vertices.Where(a => a.GroupId == groupId).ToList();
                //skip processing if there are no vertices in this group
                if(vertices.Count == 0) continue;
                //get edges between vertices in the same group
                var edges = VisitedGraph.Edges.Where(a => a.Source.GroupId == a.Target.GroupId && a.Target.GroupId == groupId).ToList();
                //create and compute graph for a group
                var graph = GenerateGroupGraph(vertices, edges);
                //HACK - had to use LayoutAlgorithmBase to be able to set VisibleGraph property
                var alg = gp.LayoutAlgorithm as LayoutAlgorithmBase<TVertex, TEdge, TGraph>;
                if (alg != null)
                    alg.VisitedGraph = graph;
                //assign vertex sizes to internal algorithm if needed
                if (gp.LayoutAlgorithm.NeedVertexSizes)
                    gp.LayoutAlgorithm.VertexSizes = VertexSizes.Where(a => a.Key.GroupId == groupId)
                        .ToDictionary(a => a.Key, b => b.Value);
                gp.LayoutAlgorithm.Compute(cancellationToken);

                //DEBUG as we use bounded in this test lets shift coords
                var offsetX = gp.ZoneRectangle.X;
                var offsetY = gp.ZoneRectangle.Y;
                gp.LayoutAlgorithm.VertexPositions.ForEach(a =>
                {
                    a.Value.Offset(offsetX, offsetY);
                });

                //write results to global positions storage
                gp.LayoutAlgorithm.VertexPositions.ForEach(a =>
                {
                    if (VertexPositions.ContainsKey(a.Key)) VertexPositions[a.Key] = a.Value;
                    else VertexPositions.Add(a.Key, a.Value);
                });
            }
        }

        private static TGraph GenerateGroupGraph(ICollection<TVertex> vertices, ICollection<TEdge> edges)
        {
            var graph = new BidirectionalGraph<TVertex, TEdge>(true, vertices.Count, edges.Count);
            graph.AddVertexRange(vertices);
            graph.AddEdgeRange(edges);
            return graph as TGraph;
        }
    }

    public class AlgorithmGroupParameters<TVertex>
        where TVertex: class, IGraphXVertex
    {
        /// <summary>
        /// Gets or sets group Id for parameters to apply
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// Gets or sets rectangular area in which vertices will be placed
        /// </summary>
        public Rect ZoneRectangle { get; set; } 
        /// <summary>
        /// Gets or sets layout algorithm that will be used to calculate vertices positions inside the group
        /// </summary>
        public IExternalLayout<TVertex> LayoutAlgorithm { get; set; }
    }
}
