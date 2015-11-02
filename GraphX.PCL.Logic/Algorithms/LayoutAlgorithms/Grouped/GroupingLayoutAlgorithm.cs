using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Exceptions;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.PCL.Logic.Helpers;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms.Grouped
{
    public class GroupingLayoutAlgorithm<TVertex, TEdge, TGraph> : LayoutAlgorithmBase<TVertex, TEdge, TGraph>
        where TVertex: class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {        

        public override bool NeedVertexSizes { get { return true; } }

        public override void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            //
        }

        readonly GroupingLayoutAlgorithmParameters<TVertex, TEdge> _params;

        public GroupingLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions, GroupingLayoutAlgorithmParameters<TVertex, TEdge> groupParams)
            :base(graph, positions)
        {
            _params = groupParams;
            if (_params.GroupParametersList == null || _params.GroupParametersList.Count == 0)
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with at least one group parameters data!");
            if (_params.GroupParametersList.GroupBy(a => a.GroupId).Count() != _params.GroupParametersList.Count)
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has unique GroupId in each record!");
            if (_params.GroupParametersList.Any(a => a.ZoneRectangle == Rect.Empty))
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has non-empty or null ZoneRectangle defined in each record!");
            if (_params.GroupParametersList.Any(a => a.LayoutAlgorithm == null))
                throw new GX_InvalidDataException("GroupingLayoutAlgorithm should be provided with group parameters data that has not null LayoutAlgorithm defined in each record!");
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            var groups = _params.GroupParametersList.Select(a => a.GroupId).OrderByDescending(a=> a).ToList();
            var listRect = new Dictionary<object, Rect>();
            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var groupId = group;
                var gp = _params.GroupParametersList.First(a=> a.GroupId == groupId);
                //get vertices of the same group
                //var vertices = new List<TVertex>();
                var vertices = VisitedGraph.Vertices.Where(a => a.GroupId == groupId).ToList();
                //skip processing if there are no vertices in this group
                if(vertices.Count == 0) continue;
                //get edges between vertices in the same group
                var edges = VisitedGraph.Edges.Where(a => a.Source.GroupId == a.Target.GroupId && a.Target.GroupId == groupId).ToList();
                //create and compute graph for a group
                var graph = GenerateGroupGraph(vertices, edges);
                //inject custom vertex and edge set into existing algorithm
                gp.LayoutAlgorithm.ResetGraph(graph.Vertices, graph.Edges);

                //assign vertex sizes to internal algorithm if needed
                if (gp.LayoutAlgorithm.NeedVertexSizes)
                    gp.LayoutAlgorithm.VertexSizes = VertexSizes.Where(a => a.Key.GroupId == groupId)
                        .ToDictionary(a => a.Key, b => b.Value);
                gp.LayoutAlgorithm.Compute(cancellationToken);

                //Move vertices to bound box if layout algorithm don't use bounds
                if (gp.ZoneRectangle.HasValue && !gp.IsAlgorithmBounded)
                {
                    var offsetX = gp.ZoneRectangle.Value.X;
                    var offsetY = gp.ZoneRectangle.Value.Y;
                    gp.LayoutAlgorithm.VertexPositions.ForEach(a =>
                    {
                        a.Value.Offset(offsetX, offsetY);
                    });
                }

                //write results to global positions storage
                double?[] left = {null};
                double?[] top = {null};
                double?[] right = {null};
                double?[] bottom = {null};
                gp.LayoutAlgorithm.VertexPositions.ForEach(a =>
                {
                    left[0] = left[0].HasValue ? (a.Value.X < left[0] ? a.Value.X : left[0]) : a.Value.X;
                    var w = a.Value.X + VertexSizes[a.Key].Width;
                    var h = a.Value.Y + VertexSizes[a.Key].Height;
                    right[0] = right[0].HasValue ? (w > right[0] ? w : right[0]) : w;
                    top[0] = top[0].HasValue ? (a.Value.Y < top[0] ? a.Value.Y : top[0]) : a.Value.Y;
                    bottom[0] = bottom[0].HasValue ? (h > bottom[0] ? h : bottom[0]) : h;

                    if (VertexPositions.ContainsKey(a.Key)) VertexPositions[a.Key] = a.Value;
                    else VertexPositions.Add(a.Key, a.Value);
                });
                if (_params.ArrangeGroups)
                {
                    if (left[0] == null) left[0] = 0;
                    if (right[0] == null) right[0] = 0;
                    if (top[0] == null) top[0] = 0;
                    if (bottom[0] == null) bottom[0] = 0;
                    listRect.Add(gp.GroupId, gp.ZoneRectangle ?? new Rect(new Point(left[0].Value, top[0].Value), new Point(right[0].Value, bottom[0].Value)));
                }
            }

            if (_params.ArrangeGroups)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var origList = listRect.ToDictionary(a => a.Key, a => a.Value);
                var ora = _params != null && _params.OverlapRemovalAlgorithm != null ? _params.OverlapRemovalAlgorithm : new FSAAlgorithm<object>(listRect, new OverlapRemovalParameters {HorizontalGap = 10, VerticalGap = 10});
                ora.Initialize(listRect);
                ora.Compute(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                ora.Rectangles.ForEach(a =>
                {
                    int group = (int) a.Key;
                    //_params.GroupParametersList.FirstOrDefault(b => b.GroupId == (int)a.Key).ZoneRectangle = origList[a.Key];
                    ArrangeRectangle(a.Value, group, origList[a.Key]);
                });
            }
        }

        private void ArrangeRectangle(Rect rectangle, int groupId, Rect originalRect)
        {
            var offsetX = rectangle.X - originalRect.X;
            var offsetY = rectangle.Y - originalRect.Y;
            VertexPositions.Where(a => a.Key.GroupId == groupId).Select(a=> a.Key).ToList().ForEach(a =>
            {
                VertexPositions[a] = new Point( VertexPositions[a].X + offsetX, VertexPositions[a].Y + offsetY);
            });
            var gp = _params.GroupParametersList.FirstOrDefault(a => a.GroupId == groupId);
            if(gp == null)
                throw new GX_ObjectNotFoundException("Grouped graph -> Can't find group data after calc!");
            gp.ZoneRectangle = rectangle;
        }

        private static TGraph GenerateGroupGraph(ICollection<TVertex> vertices, ICollection<TEdge> edges)
        {
            var graph = new BidirectionalGraph<TVertex, TEdge>(true, vertices.Count, edges.Count);
            graph.AddVertexRange(vertices);
            graph.AddEdgeRange(edges);
            return (TGraph)(object)graph;
        }
    }

    public class AlgorithmGroupParameters<TVertex, TEdge>
        where TVertex: class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
    {
        /// <summary>
        /// Gets or sets group Id for parameters to apply
        /// </summary>
        public int GroupId { get; set; }
        /// <summary>
        /// Gets or sets rectangular area in which vertices will be placed. If not set then vertices will not be bound.
        /// </summary>
        public Rect? ZoneRectangle { get; set; }
        /// <summary>
        /// Gets or sets layout algorithm that will be used to calculate vertices positions inside the group
        /// </summary>
        public IExternalLayout<TVertex, TEdge> LayoutAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets if specified algorithm directly places vertices in a specified bounds or overwise should grouping algorithm offset its vertices to a ZoneRectangle
        /// For ex. RandomLayoutAlgorithm or BoundedFR can layout vertices in a custom bounds with no need to additionaly offset vertices. And LinLog algorithm do not have bounds and all its vertices layouted 
        /// in a 0 - 1000+ positions by default so if you set ZoneRectangle for the group you will need to offset generated vertices into the custom bounds.
        /// </summary>
        public bool IsAlgorithmBounded { get; set; }
    }
}
