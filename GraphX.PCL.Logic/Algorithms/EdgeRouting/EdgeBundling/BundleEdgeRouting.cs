using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Common.Models.Semaphore;
using QuickGraph;

/* Code here is partially used from NodeXL (https://nodexl.codeplex.com/)
 * 
 * 
 * 
 * */

namespace GraphX.PCL.Logic.Algorithms.EdgeRouting
{
    /// <summary>
    /// EdgeBundler class is intended to be used for bundling and straightening of the edges of the graph.
    /// The goal is to get the layout that is less clutered and more suitable for analiyzing.
    /// 
    /// This class is based on the paper "Force-Directed Edge Bundling for Graph Visualization"
    /// by Danny Holten and Jarke J. van Wijk.
    /// http://www.win.tue.nl/~dholten/papers/forcebundles_eurovis.pdf
    /// 
    /// It was implemented and modified by Luka Potkonjak.
    /// Adapted for GraphX by PantheR.
    /// </summary>
    public class BundleEdgeRouting<TVertex, TEdge, TGraph> : EdgeRoutingAlgorithmBase<TVertex, TEdge, TGraph>
        where TGraph : class, IMutableBidirectionalGraph<TVertex, TEdge>
        where TEdge : class, IGraphXEdge<TVertex>
        where TVertex : class, IGraphXVertex
    {
        public BundleEdgeRouting(Rect graphArea, TGraph graph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Rect> vertexSizes, IEdgeRoutingParameters parameters = null)
            : base(graph, vertexPositions, vertexSizes, parameters)
        {
            Parameters = parameters;
            AreaRectangle = graphArea;
            var prm = parameters as BundleEdgeRoutingParameters;
            if (prm != null)
            {
                SubdivisionPoints = prm.SubdivisionPoints;
                Straightening = prm.Straightening;
                RepulsionCoefficient = prm.RepulsionCoefficient;
                Threshold = prm.Threshold;
                SpringConstant = prm.SpringConstant;
                UseThreading = prm.UseThreading;
                RepulseOpposite = prm.RepulseOpposite;
                Iterations = prm.Iterations;
            }
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            BundleAllEdges(Graph, cancellationToken);
        }
        public override Point[] ComputeSingle(TEdge edge)
        {
            BundleEdges(Graph, new List<TEdge>() { edge });
            return EdgeRoutes.ContainsKey(edge) ? EdgeRoutes[edge] : null;
        }

        /// <summary>
        /// Bundles edges of the graph.
        /// </summary>
        /// <param name="graph">
        ///     Graph whose edges should be bundled
        /// </param>
        /// <param name="cancellationToken"></param>
        public void BundleAllEdges(TGraph graph, CancellationToken cancellationToken)
        {
            EdgeRoutes.Clear();

            //this.rectangle = rectangle;
            _directed = true; // as we use bidirectional by default

            AddDataForAllEdges(graph.Edges, cancellationToken);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            FindCompatibleEdges(_edgeGroupData, cancellationToken);

            //sw.Stop();


            DivideAllEdges(_subdivisionPoints, cancellationToken);

            //sw = new Stopwatch();
            //sw.Start();

            for (var i = 0; i < _iterations; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                MoveControlPoints(_edgeGroupData);
            }

            //prevents oscillating movements
            for (var i = 0; i < 5; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _cooldown *= 0.5f;
                MoveControlPoints(_edgeGroupData);
            }

            //sw.Stop();

            _cooldown = 1f;

            if (_straightening > 0)
                StraightenEdgesInternally(_edgeGroupData, _straightening);

            foreach (var e in graph.Edges)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!e.IsSelfLoop)
                {
                    var key = new KeyPair(e.Source.ID, e.Target.ID);
                    var list2 = _edgeGroupData[key].ControlPoints.ToList(); 

                    //Point p1 = GeometryHelper.GetEdgeEndpointOnRectangle(VertexPositions[e.Source], VertexSizes[e.Source], list2.First());
                    //Point p2 = GeometryHelper.GetEdgeEndpointOnRectangle(VertexPositions[e.Target], VertexSizes[e.Target], list2.Last());
                    //list2.Insert(0, p1); list2.Add(p2);
                    list2.Insert(0, list2.First()); list2.Add(list2.Last());

                    EdgeRoutes.Add(e, list2.ToArray());
                }
            }
        }

        /// <summary>
        /// Bundles specified edges. Shapes of all the other edges remain the same,
        /// so this method is faster than the one for bundling all edges, but also produces less optimal layout.
        /// </summary>
        /// 
        /// <param name="graph">
        /// Parent graph of the edge set
        /// </param>
        /// 
        /// <param name="edges">
        /// Edges that should be bundled
        /// </param>
        public void BundleEdges(TGraph graph, IEnumerable<TEdge> edges)
        {

            _directed = true;

            AddAllExistingData(graph.Edges);
            AddEdgeDataForMovedEdges(edges);
            FindCompatibleEdges(_movedEdgeGroupData, CancellationToken.None);
            ResetMovedEdges();

            for (var i = 0; i < _iterations; i++)
                MoveControlPoints(_movedEdgeGroupData);

            for (var i = 0; i < 5; i++)
            {
                _cooldown *= 0.5f;
                MoveControlPoints(_movedEdgeGroupData);
            }

            _cooldown = 1f;

            if (_straightening > 0)
                StraightenEdgesInternally(_movedEdgeGroupData, _straightening);

            foreach (var e in edges)
            {
                EdgeGroupData ed;
                var key = new KeyPair(e.Source.ID, e.Target.ID);
                _movedEdgeGroupData.TryGetValue(key, out ed);
                if (ed != null)
                {
                    var list2 = ed.ControlPoints.ToList();

                    //Point p1 = GeometryHelper.GetEdgeEndpointOnRectangle(VertexPositions[e.Source], VertexSizes[e.Source], list2.First());
                    //Point p2 = GeometryHelper.GetEdgeEndpointOnRectangle(VertexPositions[e.Target], VertexSizes[e.Target], list2.Last());
                    //list2.Insert(0, p1); list2.Add(p2);
                    if (list2.Count > 0)
                    {
                        list2.Insert(0, list2.First());
                        list2.Add(list2.Last());
                    }

                    if (EdgeRoutes.ContainsKey(e))
                        EdgeRoutes[e] = list2.ToArray();
                    else EdgeRoutes.Add(e, list2.ToArray());
                }
                    //e.SetValue(ReservedMetadataKeys.PerEdgeIntermediateCurvePoints, ed.controlPoints);
            }
        }

        /// <summary>
        /// Collects edge data from the specified edges
        /// </summary>
        /// 
        /// <param name="edges">
        /// Edges whose data should be added to the collection
        /// </param>
        private void AddEdgeDataForMovedEdges(IEnumerable<TEdge> edges)
        {
            foreach (var e in edges)
                if (!e.IsSelfLoop)
                {
                    var key = new KeyPair(e.Source.ID, e.Target.ID);
                    if (!_movedEdgeGroupData.ContainsKey(key))
                        _movedEdgeGroupData.Add(key, _edgeGroupData[key]);
                }
        }

        /// <summary>
        /// Collects edge data from all edges in the specified collection.
        /// Used by the <see cref="BundleAllEdges"/> method.
        /// </summary>
        /// <param name="edges">
        ///     Collection of edges whose data should be collected
        /// </param>
        /// <param name="cancellationToken"></param>
        private void AddDataForAllEdges(IEnumerable<TEdge> edges, CancellationToken cancellationToken)
        {
            foreach (var e in edges)
                if (!e.IsSelfLoop)
                    AddEdgeData(e);
        }

        /// <summary>
        /// Collects data from the specified edge
        /// </summary>
        /// 
        /// <param name="e">
        /// Edge to collect data from
        /// </param>
        private void AddEdgeData(TEdge e)
        {
            EdgeGroupData ed;
            var key = new KeyPair(e.Source.ID, e.Target.ID);

            _edgeGroupData.TryGetValue(key, out ed);

            if (ed == null)
            {
                var p1 = VertexPositions[e.Source];// e.Vertices[0].Location;
                var p2 = VertexPositions[e.Target];//e.Vertices[1].Location;
                ed = new EdgeGroupData {V1 = p1, V2 = p2, ID = key};
                var mid = VectorTools.MidPoint(p1, p2);
                ed.Middle = mid;
                ed.Length = VectorTools.Distance(p1, p2);
                ed.CompatibleGroups = new Dictionary<KeyPair, GroupPairData>();
                //ed.edges = new HashSet<int>();
                ed.EdgeCount = 0;
                _edgeGroupData.Add(key, ed);
            }
            //ed.edges.Add(e.ID);
            ed.EdgeCount++;
        }

        /// <summary>
        /// Collects edge data from all edges in the specified collection.
        /// Used for edges that already have control points metadata.
        /// </summary>
        /// 
        /// <param name="edges">
        /// Collection of edges whose data should be collected
        /// </param>
        private void AddAllExistingData(IEnumerable<TEdge> edges)
        {
            _subdivisionPoints = 0;
            foreach (var e in edges)
                if (!e.IsSelfLoop)
                    AddExistingData(e);
        }

        /// <summary>
        /// Collects data from the specified edge.
        /// Used for edges that already have control points metadata.
        /// </summary>
        /// 
        /// <param name="e">
        /// Edge to collect data from
        /// </param>
        private void AddExistingData(TEdge e)
        {
            EdgeGroupData ed;
            var key = new KeyPair(e.Source.ID, e.Target.ID);

            _edgeGroupData.TryGetValue(key, out ed);

            if (ed == null)
            {
                var p1 = VertexPositions[e.Source];// e.Vertices[0].Location;
                var p2 = VertexPositions[e.Target];//e.Vertices[1].Location;
                ed = new EdgeGroupData {V1 = p1, V2 = p2, ID = key};
                var mid = VectorTools.MidPoint(p1, p2);
                ed.Middle = mid;
                ed.Length = VectorTools.Distance(p1, p2);

                ed.ControlPoints = e.RoutingPoints; //e.GetValue(ReservedMetadataKeys.PerEdgeIntermediateCurvePoints);

                if (_subdivisionPoints == 0) _subdivisionPoints = ed.ControlPoints.Length;
                ed.NewControlPoints = new Point[_subdivisionPoints];
                ed.K = _springConstant * (_subdivisionPoints + 1) / ed.Length;
                if (ed.K > 0.5f) ed.K = 0.5f;
                //ed.edges = new HashSet<int>();
                ed.EdgeCount = 0;
                ed.CompatibleGroups = new Dictionary<KeyPair, GroupPairData>();
                _edgeGroupData.Add(key, ed);
            }
            //ed.edges.Add(e.ID);
            ed.EdgeCount++;
        }

        /// <summary>
        /// Calculates angle compatibility of the two edges
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Angle compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float AngleCompatibility(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            var a = VectorTools.Angle(ed1.V1, ed1.V2, ed2.V1, ed2.V2);
            return (float)Math.Abs(Math.Cos(a));
        }

        /// <summary>
        /// Calculates position compatibility of the two edges
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Position compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float PositionCompatibility(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            var avg = (ed1.Length + ed2.Length) / 2;
            var dis = VectorTools.Distance(ed1.Middle, ed2.Middle);
            if ((avg + dis) == 0) return 0;
            return (avg / (avg + dis));
        }

        /// <summary>
        /// Calculates scale compatibility of the two edges
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Scale compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float ScaleCompatibility(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            var l1 = ed1.Length;
            var l2 = ed2.Length;
            var l = l1 + l2;
            if (l == 0)
                return 0;
            else
            {
                var ret = 4 * l1 * l2 / (l * l);
                return ret * ret;
            }
        }

        /// <summary>
        /// Calculates compatibility of the two edges.
        /// Combines angle, position, scale, and visibility compatibility coefficient.
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float CalculateCompatibility(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            var c = PositionCompatibility(ed1, ed2);
            if (c > _threshold) c *= ScaleCompatibility(ed1, ed2);
            else return 0;
            if (c > _threshold) c *= AngleCompatibility(ed1, ed2);
            else return 0;
            if (c > _threshold) c *= VisibilityCompatibility(ed1, ed2);
            else return 0;
            if (c > _threshold)
                return c;
            else return 0;
        }

        /// <summary>
        /// Calculates visibility compatibility of the two edges.
        /// Uses lower of the two calculated visibility coefficients.
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Visibility compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float VisibilityCompatibility(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            float c1, c2;

            c1 = VisibilityCoefficient(ed1, ed2);
            if (c1 == 0)
                return 0;
            c2 = VisibilityCoefficient(ed2, ed1);

            return Math.Min(c1, c2);
        }

        /// <summary>
        /// Calculates visibility coefficient of the two edges.
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// Compatibility coefficient ranging from 0 to 1
        /// </returns>
        private float VisibilityCoefficient(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            float c;
            var p1 = ed1.V1;
            var p2 = ed1.V2;
            var q1 = ed2.V1;
            var q2 = ed2.V2;

            var pn = new Point();
            pn.X = p1.Y - p2.Y;
            pn.Y = p2.X - p1.X;

            var pn1 = VectorTools.Plus(pn, p1);
            var pn2 = VectorTools.Plus(pn, p2);

            var i1 = new Point();
            var i2 = new Point();

            float r1 = 0, r2 = 0;

            if (!Intersects(q1, q2, p1, pn1, ref i1)) return 0;
            Intersects(q1, q2, p2, pn2, ref i2);

            if ((r1 < 0 && r2 < 0) || (r1 > 1 && r2 > 1)) return 0;

            var im = VectorTools.MidPoint(i1, i2);

            var qm = ed2.Middle;

            var i = VectorTools.Distance(i1, i2);
            var m = VectorTools.Distance(qm, im);

            if (i == 0) return 0;

            c = 1f - 2f * m / i;

            if (c < 0)
                return 0;
            else
                return c;
        }

        /// <summary>
        /// Calculates directedness of the two edges.
        /// </summary>
        /// 
        /// <param name="ed1">
        /// First edge to be used in calculation
        /// </param>
        /// 
        /// <param name="ed2">
        /// Second edge to be used in calculation
        /// </param>
        /// 
        /// <returns>
        /// True if edges have roughly the same direction, false otherwise
        /// </returns>
        private bool CalculateDirectedness(EdgeGroupData ed1, EdgeGroupData ed2)
        {
            if ((VectorTools.Distance(ed1.V1, ed2.V1) + VectorTools.Distance(ed1.V2, ed2.V2)) <
                (VectorTools.Distance(ed1.V1, ed2.V2) + VectorTools.Distance(ed1.V2, ed2.V1)))
                return true;
            else return false;
        }

        /// <summary>
        /// Finds an intersection point of the two lines
        /// </summary>
        /// 
        /// <param name="p1">
        /// First point of the first line
        /// </param>
        /// 
        /// <param name="p2">
        /// Second point of the first line
        /// </param>
        /// 
        /// <param name="q1">
        /// First point of the second line
        /// </param>
        /// 
        /// <param name="q2">
        /// Second point of the second line
        /// </param>
        /// 
        /// <param name="intersection">
        /// Point of intersection
        /// </param>
        /// <returns>
        /// True if lines are not parallel, false otherwise
        /// </returns>
        private bool Intersects(Point p1, Point p2, Point q1, Point q2, ref Point intersection)
        {
            var q = (p1.Y - q1.Y) * (q2.X - q1.X) - (p1.X - q1.X) * (q2.Y - q1.Y);
            var d = (p2.X - p1.X) * (q2.Y - q1.Y) - (p2.Y - p1.Y) * (q2.X - q1.X);

            if (d == 0) // parallel lines
            {
                return false;
            }

            var r = q / d;

            /*q = (p1.Y - q1.Y) * (p2.X - p1.X) - (p1.X - q1.X) * (p2.Y - p1.Y);
            var s = q / d;*/

            intersection = VectorTools.Plus(p1, VectorTools.Multiply(VectorTools.Minus(p2, p1), r));

            return true;
        }

        /// <summary>
        /// Finds compatible edges for the specified set of edges
        /// </summary>
        /// <param name="edgeSet">
        ///     Edges for which we should find compatible edges
        /// </param>
        /// <param name="cancellationToken"></param>
        private void FindCompatibleEdges(Dictionary<KeyPair, EdgeGroupData> edgeSet, CancellationToken cancellationToken)
        {
            foreach (var p1 in edgeSet)
            {
                if (p1.Value.Length < 50) continue;//??????
                foreach (var p2 in _edgeGroupData)
                {
                    if (p2.Value.Length < 50) continue;//??????
                    if (((p1.Key.K1 == p2.Key.K1) && (p1.Key.K2 == p2.Key.K2))
                        || (p1.Value.CompatibleGroups.ContainsKey(p2.Key)))
                        continue;
                    //if ((((p1.Value.v1 == p2.Value.v1) && (p1.Value.v1.Y == p2.Value.v1.Y)) && (p1.Value.v2.X == p2.Value.v2.X) && (p1.Value.v2.Y == p2.Value.v2.Y)) ||
                    //(((p1.Value.v1.X == p2.Value.v2.X) && (p1.Value.v1.Y == p2.Value.v2.Y)) && (p1.Value.v2.X == p2.Value.v1.X) && (p1.Value.v2.Y == p2.Value.v1.Y)))
                    //    continue;
                    var c = CalculateCompatibility(p1.Value, p2.Value);
                    if (c == 0) continue;
                    var d = CalculateDirectedness(p1.Value, p2.Value);
                    var epd = new GroupPairData(c, p1.Value, p2.Value, d);
                    p1.Value.CompatibleGroups.Add(p2.Key, epd);
                    p2.Value.CompatibleGroups.Add(p1.Key, epd);
                }
            }
        }

        /// <summary>
        /// Divides edges into segments by adding subdivision points to them
        /// </summary>
        /// <param name="subdivisionPointsNum">
        ///     Number of subdivision points that should be created
        /// </param>
        /// <param name="cancellationToken"></param>
        private void DivideAllEdges(int subdivisionPointsNum, CancellationToken cancellationToken)
        {
            if (subdivisionPointsNum < 1) return;

            foreach (var ed in _edgeGroupData.Values)
                DivideEdge(ed, subdivisionPointsNum);

            _subdivisionPoints = subdivisionPointsNum;
        }


        /// <summary>
        /// Straightens moved edges.
        /// </summary>
        private void ResetMovedEdges()
        {
            foreach (var ed in _movedEdgeGroupData.Values)
                DivideEdge(ed, _subdivisionPoints);
        }

        /// <summary>
        /// Divides an edge into segments by adding subdivision points to it
        /// </summary>
        /// 
        /// <param name="ed">
        /// Edge data that is used for creating new subdivision points
        /// </param>
        /// 
        /// <param name="subdivisionPointsNum">
        /// Number of subdivision points that should be created
        /// </param>
        private void DivideEdge(EdgeGroupData ed, int subdivisionPointsNum)
        {
            var r = ed.Length / (subdivisionPointsNum + 1);
            var sPoints = new Point[subdivisionPointsNum];
            ed.NewControlPoints = new Point[subdivisionPointsNum];
            Point move;
            move = ed.Length == 0 ? new Point(0, 0) : VectorTools.Multiply(VectorTools.Minus(ed.V2, ed.V1), 1f / ed.Length);
            for (var i = 0; i < subdivisionPointsNum; i++)
                sPoints[i] = VectorTools.Plus(ed.V1, VectorTools.Multiply(move, r * (i + 1)));
            ed.ControlPoints = sPoints;
            ed.K = _springConstant * (subdivisionPointsNum + 1) / ed.Length;
            if (ed.K > 0.5f) ed.K = 0.5f;
        }

/*
        /// <summary>
        /// Doubles subdivision points for an edge by adding one new subdivision point between each two
        /// </summary>
        /// 
        /// <param name="ed">
        /// Edge data that contains subdivision points to be doubled
        /// </param>
        private void DoubleSubdivisionPoints(EdgeGroupData ed)
        {
            if (_subdivisionPoints == 0) //make one subdivision point
            {
                ed.K = _springConstant * 2 / ed.Length;
                if (ed.K > 0.5f) ed.K = 0.5f;
                ed.ControlPoints = new Point[1];
                ed.NewControlPoints = new Point[1];
                ed.ControlPoints[0] = ed.Middle;

                return;
            }

            var sPoints = ed.ControlPoints;
            var sPointsDoubled = new Point[_subdivisionPoints * 2 + 1];
            ed.NewControlPoints = new Point[_subdivisionPoints * 2 + 1];
            for (var i = 0; i < _subdivisionPoints; i++)
                sPointsDoubled[i * 2 + 1] = sPoints[i];


            for (var i = 0; i < _subdivisionPoints - 1; i++)
                sPointsDoubled[i * 2 + 2] = VectorTools.MidPoint(sPoints[i], sPoints[i + 1]);


            sPointsDoubled[0] = VectorTools.MidPoint(ed.V1, sPoints[0]);
            sPointsDoubled[_subdivisionPoints * 2] = VectorTools.MidPoint(sPoints[_subdivisionPoints - 1], ed.V2);
            //ed.K = springConstant * (subdivisionPoints * 2 + 2) / ed.Length;
            ed.K *= 2f;
            if (ed.K > 0.5f) ed.K = 0.5f;
            ed.ControlPoints = sPointsDoubled;
        }
*/

/*
        /// <summary>
        /// Doubles subdivision points for all edges
        /// </summary>
        private void DoubleSubdivisionPointsForAllEdges()
        {
            foreach (var ed in _edgeGroupData.Values) DoubleSubdivisionPoints(ed);
            _subdivisionPoints = _subdivisionPoints * 2 + 1;
        }
*/

        /// <summary>
        /// Calculates new positions for the control points of an edge by applying elastic and electrostatic forces to them
        /// </summary>
        /// 
        /// <param name="o">
        /// Edge data that contains subdivision points to be moved
        /// </param>
        private void CalculateNewControlPoints(Object o)
        {
            var ed = (EdgeGroupData)o;
            for (var i = 0; i < _subdivisionPoints; i++)
            {
                var p = ed.ControlPoints[i];
                Point p1, p2;
                p1 = i == 0 ? ed.V1 : ed.ControlPoints[i - 1];
                p2 = i == (_subdivisionPoints - 1) ? ed.V2 : ed.ControlPoints[i + 1];
                //SizeF sp = new SizeF(p);
                var f = VectorTools.Multiply(VectorTools.Plus(VectorTools.Minus(p1, p), VectorTools.Minus(p2, p)), ed.K);
                var r = new Point(0, 0);
                foreach (var epd in ed.CompatibleGroups.Values)
                {
                    Point q;
                    var j = 1f;
                    EdgeGroupData ed2;
                    if ((epd.Ed1.ID.K1 == ed.ID.K1) && (epd.Ed1.ID.K2 == ed.ID.K2))
                        ed2 = epd.Ed2;
                    else
                        ed2 = epd.Ed1;

                    if (epd.D)
                        q = ed2.ControlPoints[i];
                    else
                    {
                        q = ed2.ControlPoints[_subdivisionPoints - i - 1];
                        if (_directed && _repulseOpposite) j = _repulsionCoefficient;
                    }
                    var fs = VectorTools.Minus(q, p);
                    //PointF fs = new PointF(q.X - p.X, q.Y - p.Y);

                    var l = VectorTools.Length(fs);
                    if (l > 0)//???
                    {
                        fs = VectorTools.Multiply(fs, epd.C / (l));

                        //fs = VectorTools.Multiply(fs, VectorTools.Length(fs) * ed2.edges.Count);
                        fs = VectorTools.Multiply(fs, VectorTools.Length(fs) * ed2.EdgeCount);

                        r.X += (j * fs.X);
                        r.Y += (j * fs.Y);
                    }
                }

                var rl = VectorTools.Length(r);
                if (rl>0)
                    r = VectorTools.Multiply(r, (float)(1.0/Math.Sqrt(rl)));

                var move = new Point(f.X + r.X, f.Y + r.Y);
                VectorTools.Length(move);

                //float len = ed.Length / (subdivisionPoints + 1);
                //if (moveL > (len)) move = VectorTools.Multiply(move, len*cooldown / moveL);
                //if (moveL != 0) move = VectorTools.Multiply(move, cooldown / moveL);
                move = VectorTools.Multiply(move, _cooldown*0.5f);
                ed.NewControlPoints[i] = VectorTools.Plus(move, p);

                if (ed.NewControlPoints[i].X < AreaRectangle.Left)
                    ed.NewControlPoints[i].X = AreaRectangle.Left;
                else
                    if (ed.NewControlPoints[i].X > AreaRectangle.Right)
                        ed.NewControlPoints[i].X = AreaRectangle.Right;

                if (ed.NewControlPoints[i].Y < AreaRectangle.Top)
                    ed.NewControlPoints[i].Y = AreaRectangle.Top;
                else
                    if (ed.NewControlPoints[i].Y > AreaRectangle.Bottom)
                        ed.NewControlPoints[i].Y = AreaRectangle.Bottom;
            }
            if (_useThreading) _sem.Release();
        }

        /// <summary>
        /// Moves control points for the specified edges
        /// </summary>
        /// 
        /// <param name="groupsToMove">
        /// Edges that should be moved
        /// </param>
        private void MoveControlPoints(Dictionary<KeyPair, EdgeGroupData> groupsToMove)
        {
            if (_useThreading)
            {
                foreach (var ed in groupsToMove.Values)
                    ThreadPool.QueueUserWorkItem(CalculateNewControlPoints, ed);
                for (var i = 0; i < groupsToMove.Values.Count; i++)
                    _sem.WaitOne();
            }
            else
            {
                foreach (var ed in groupsToMove.Values)
                    CalculateNewControlPoints(ed);
            }


            foreach (var ed in groupsToMove.Values)
            {
                ed.ControlPoints = ed.NewControlPoints;
                ed.NewControlPoints = new Point[_subdivisionPoints];
            }

            //if (cooldown > 0.05) cooldown *= 0.95f; else cooldown = 0;
            //cooldown *= 0.99f;
        }

        /// <summary>
        /// Straightens the edges using internal data sturctures
        /// </summary>
        /// 
        /// <param name="groupsToStraighten">
        /// Groups of edges that should be straightened
        /// </param>
        /// 
        /// <param name="s">
        /// Specifies the amount of straightening, from 0 to 1
        /// </param>
        private void StraightenEdgesInternally(Dictionary<KeyPair, EdgeGroupData> groupsToStraighten, float s)
        {
            foreach (var ed in groupsToStraighten.Values)
            {
                for (var i = 0; i < _subdivisionPoints; i++)
                {
                    //STRONG CHANGE
                    var p = ed.ControlPoints[i];
                    p = VectorTools.Plus(VectorTools.Multiply(p, 1 - s),
                        VectorTools.Multiply(VectorTools.Plus(ed.V1,
                            VectorTools.Multiply(VectorTools.Minus(ed.V2, ed.V1), 
                                1.0f * (i + 1) / (_subdivisionPoints + 1))), s));
                    ed.ControlPoints[i].X = p.X;
                    ed.ControlPoints[i].Y = p.Y;
                }
            }
        }

        /// <summary>
        /// Moves the control points of all the edges of the graph closer to their original position on the straight edge
        /// </summary>
        /// 
        /// <param name="graph">
        /// Graph whose edges should be straightened
        /// </param>
        /// 
        /// <param name="s">
        /// Specifies the amount of straightening, from 0 to 1
        /// </param>
        public void StraightenEdges(TGraph graph, float s)
        {
            foreach (var e in graph.Edges)
            {
                if (e.IsSelfLoop) continue;
                var controlPoints = e.RoutingPoints;//(PointF[])e.GetValue(ReservedMetadataKeys.PerEdgeIntermediateCurvePoints);
                var newControlPoints = new Point[controlPoints.Length];
                for (var i = 0; i < controlPoints.Length; i++)
                {
                    //STRONG CHANGE
                    var p = controlPoints[i];
                    p = VectorTools.Plus(VectorTools.Multiply(p, 1 - s),
                        VectorTools.Multiply(VectorTools.Plus(VertexPositions[e.Source], 
                            VectorTools.Multiply(VectorTools.Minus(VertexPositions[e.Target], VertexPositions[e.Source]),
                                1.0f * (i + 1) / (controlPoints.Length + 1))), s));
                    newControlPoints[i].X = p.X;
                    newControlPoints[i].Y = p.Y;
                }
                //e.SetValue(ReservedMetadataKeys.PerEdgeIntermediateCurvePoints, newControlPoints);
                e.RoutingPoints = newControlPoints;
            }
        }

        private readonly Dictionary<KeyPair, EdgeGroupData> _edgeGroupData = new Dictionary<KeyPair, EdgeGroupData>();

        private readonly Dictionary<KeyPair, EdgeGroupData> _movedEdgeGroupData = new Dictionary<KeyPair, EdgeGroupData>();



        private int _subdivisionPoints = 15;

        private int _iterations = 50;

        private bool _repulseOpposite;

        private bool _directed = true;

        private bool _useThreading = true;

        private float _springConstant = 10f;

        private float _threshold = 0.2f;

        private float _cooldown = 1f;

        private float _repulsionCoefficient = -0.1f;

        private float _straightening = 0.15f;

        private readonly Semaphore _sem = new Semaphore(0, Int32.MaxValue);
        
        /// <summary>
        /// Gets or sets the number of subdivision points each edge should have.
        /// Default value is 15.
        /// </summary>
        public int SubdivisionPoints
        {
            get { return _subdivisionPoints; }
            set { _subdivisionPoints = value; }
        }

        /// <summary>
        /// Gets or sets the number of iterations for moving the control points.
        /// Default value is 50.
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether opposite edges should attracts or repulse each other.
        /// Default value is false.
        /// </summary>
        public bool RepulseOpposite
        {
            get { return _repulseOpposite; }
            set { _repulseOpposite = value; }
        }

        /// <summary>
        /// Gets or sets the the value that determines if multiple threads should be used for the calculations.
        /// Default value is true.
        /// </summary>
        public bool UseThreading
        {
            get { return _useThreading; }
            set { _useThreading = value; }
        }

        /// <summary>
        /// Gets or sets the value for the spring constant.
        /// Edges are more easely bent if the value is lower.
        /// Default value is 10.
        /// </summary>
        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; }
        }

        /// <summary>
        /// Gets or sets the treshold for the edge compatibility.
        /// Every pair of edges has the compatibility coefficient assigned to it.
        /// Range of the coefficient is from 0 to 1.
        /// Edges that have coefficient lower than the treshold between them are not considered for interaction.
        /// Default value is 0.2.
        /// </summary>
        public float Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>
        /// If repulseOpposite is true, this determines how much will opposite edges repulse eachother.
        /// From -1 to 0.
        /// Default is -0.1
        /// </summary>
        public float RepulsionCoefficient
        {
            get { return _repulsionCoefficient; }
            set { _repulsionCoefficient = value; }
        }

        /// <summary>
        /// Gets or sets the amount of straightening that will be applied after every bundling.
        /// This can produce better-looking graphs.
        /// Default value is 0.15, range is from 0 to 1.
        /// </summary>
        public float Straightening
        {
            get { return _straightening; }
            set { _straightening = value; }
        }

        struct KeyPair
        {
            public KeyPair(int n1, int n2)
            {
                K1 = n1;
                K2 = n2;
            }

            public readonly int K1;
            
            public readonly int K2;
        }

        /// <summary>
        /// Class used for storing the needed edge metadata
        /// </summary>
        class EdgeGroupData
        {
            public Point V1;

            public Point V2;

            public Point Middle;

            public Point[] ControlPoints;

            public Point[] NewControlPoints;

            public Dictionary<KeyPair, GroupPairData> CompatibleGroups;

            //public HashSet<Int32> edges;

            public int EdgeCount;

            public float Length;

            public float K;

            public KeyPair ID;
        }

        /// <summary>
        /// Class used for storing data for a pair of groups of edges (direction and compatibility coefficient)
        /// </summary>
        class GroupPairData
        {
            public GroupPairData(float cc, EdgeGroupData e1, EdgeGroupData e2, bool dd)
            {
                C = cc;
                Ed1 = e1;
                Ed2 = e2;
                D = dd;
            }

            public readonly float C;

            public readonly EdgeGroupData Ed1;

            public readonly EdgeGroupData Ed2;

            public readonly bool D;
        }

    }
}
