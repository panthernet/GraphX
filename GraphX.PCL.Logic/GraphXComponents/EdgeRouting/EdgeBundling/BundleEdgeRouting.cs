
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using QuickGraph;

/* Code here is partially used from NodeXL (https://nodexl.codeplex.com/)
 * 
 * 
 * 
 * */

namespace GraphX.GraphSharp.Algorithms.EdgeRouting
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
            _parameters = parameters;
            this.AreaRectangle = graphArea;
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

        public override void Compute()
        {
            BundleAllEdges(_graph);
        }
        public override Point[] ComputeSingle(TEdge edge)
        {
            BundleEdges(_graph, new List<TEdge>() { edge });
            return EdgeRoutes.ContainsKey(edge) ? EdgeRoutes[edge] : null;
        }
    
        /// <summary>
        /// Bundles edges of the graph.
        /// </summary>
        /// 
        /// <param name="graph">
        /// Graph whose edges should be bundled
        /// </param>
        /// 
        /// <param name="rectangle">
        /// Rectangle in which the graph is laid out.
        /// Control points of bundled edges should not fall outside of this rectangle.
        /// </param>
        public void BundleAllEdges(TGraph graph)
        {
            EdgeRoutes.Clear();

            //this.rectangle = rectangle;
            directed = true; // as we use bidirectional by default

            AddDataForAllEdges(graph.Edges);

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            FindCompatibleEdges(edgeGroupData);

            //sw.Stop();


            DivideAllEdges(subdivisionPoints);

            //sw = new Stopwatch();
            //sw.Start();

            for (var i = 0; i < iterations; i++)
                MoveControlPoints(edgeGroupData);

            //prevents oscillating movements
            for (var i = 0; i < 5; i++)
            {
                cooldown *= 0.5f;
                MoveControlPoints(edgeGroupData);
            }

            //sw.Stop();

            cooldown = 1f;

            if (straightening > 0)
                StraightenEdgesInternally(edgeGroupData, straightening);

            foreach (var e in graph.Edges)
            {
                if (!e.IsSelfLoop)
                {
                    var key = new KeyPair(e.Source.ID, e.Target.ID);
                    var list2 = edgeGroupData[key].controlPoints.ToList(); 

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
        /// 
        /// <param name="rectangle">
        /// Rectangle in which the graph is laid out.
        /// Control points of bundled edges should not fall outside of this rectangle.
        /// </param>
        public void BundleEdges(TGraph graph, IEnumerable<TEdge> edges)
        {

            directed = true;

            AddAllExistingData(graph.Edges);
            AddEdgeDataForMovedEdges(edges);
            FindCompatibleEdges(movedEdgeGroupData);
            ResetMovedEdges();

            for (var i = 0; i < iterations; i++)
                MoveControlPoints(movedEdgeGroupData);

            for (var i = 0; i < 5; i++)
            {
                cooldown *= 0.5f;
                MoveControlPoints(movedEdgeGroupData);
            }

            cooldown = 1f;

            if (straightening > 0)
                StraightenEdgesInternally(movedEdgeGroupData, straightening);

            foreach (var e in edges)
            {
                EdgeGroupData ed;
                var key = new KeyPair(e.Source.ID, e.Target.ID);
                movedEdgeGroupData.TryGetValue(key, out ed);
                if (ed != null)
                {
                    var list2 = ed.controlPoints.ToList();

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
                    if (!movedEdgeGroupData.ContainsKey(key))
                        movedEdgeGroupData.Add(key, edgeGroupData[key]);
                }
        }

        /// <summary>
        /// Collects edge data from all edges in the specified collection.
        /// Used by the <see cref="BundleAllEdges"/> method.
        /// </summary>
        /// 
        /// <param name="edges">
        /// Collection of edges whose data should be collected
        /// </param>
        private void AddDataForAllEdges(IEnumerable<TEdge> edges)
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

            edgeGroupData.TryGetValue(key, out ed);

            if (ed == null)
            {
                var p1 = VertexPositions[e.Source];// e.Vertices[0].Location;
                var p2 = VertexPositions[e.Target];//e.Vertices[1].Location;
                ed = new EdgeGroupData();
                ed.v1 = p1;
                ed.v2 = p2;
                ed.id = key;
                var mid = VectorTools.MidPoint(p1, p2);
                ed.middle = mid;
                ed.length = VectorTools.Distance(p1, p2);
                ed.compatibleGroups = new Dictionary<KeyPair, GroupPairData>();
                //ed.edges = new HashSet<int>();
                ed.edgeCount = 0;
                edgeGroupData.Add(key, ed);
            }
            //ed.edges.Add(e.ID);
            ed.edgeCount++;
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
            subdivisionPoints = 0;
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

            edgeGroupData.TryGetValue(key, out ed);

            if (ed == null)
            {
                var p1 = VertexPositions[e.Source];// e.Vertices[0].Location;
                var p2 = VertexPositions[e.Target];//e.Vertices[1].Location;
                ed = new EdgeGroupData();
                ed.v1 = p1;
                ed.v2 = p2;
                ed.id = key;
                var mid = VectorTools.MidPoint(p1, p2);
                ed.middle = mid;
                ed.length = VectorTools.Distance(p1, p2);

                ed.controlPoints = e.RoutingPoints; //e.GetValue(ReservedMetadataKeys.PerEdgeIntermediateCurvePoints);

                if (subdivisionPoints == 0) subdivisionPoints = ed.controlPoints.Length;
                ed.newControlPoints = new Point[subdivisionPoints];
                ed.k = springConstant * (subdivisionPoints + 1) / ed.length;
                if (ed.k > 0.5f) ed.k = 0.5f;
                //ed.edges = new HashSet<int>();
                ed.edgeCount = 0;
                ed.compatibleGroups = new Dictionary<KeyPair, GroupPairData>();
                edgeGroupData.Add(key, ed);
            }
            //ed.edges.Add(e.ID);
            ed.edgeCount++;
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
            var a = VectorTools.Angle(ed1.v1, ed1.v2, ed2.v1, ed2.v2);
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
            var avg = (ed1.length + ed2.length) / 2;
            var dis = VectorTools.Distance(ed1.middle, ed2.middle);
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
            var l1 = ed1.length;
            var l2 = ed2.length;
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
            if (c > threshold) c *= ScaleCompatibility(ed1, ed2);
            else return 0;
            if (c > threshold) c *= AngleCompatibility(ed1, ed2);
            else return 0;
            if (c > threshold) c *= VisibilityCompatibility(ed1, ed2);
            else return 0;
            if (c > threshold)
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
            var p1 = ed1.v1;
            var p2 = ed1.v2;
            var q1 = ed2.v1;
            var q2 = ed2.v2;

            var pn = new Point();
            pn.X = p1.Y - p2.Y;
            pn.Y = p2.X - p1.X;

            var pn1 = VectorTools.Plus(pn, p1);
            var pn2 = VectorTools.Plus(pn, p2);

            var i1 = new Point();
            var i2 = new Point();

            float r1 = 0, r2 = 0;

            if (!Intersects(q1, q2, p1, pn1, ref i1, ref r1)) return 0;
            Intersects(q1, q2, p2, pn2, ref i2, ref r2);

            if ((r1 < 0 && r2 < 0) || (r1 > 1 && r2 > 1)) return 0;

            var im = VectorTools.MidPoint(i1, i2);

            var qm = ed2.middle;

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
            if ((VectorTools.Distance(ed1.v1, ed2.v1) + VectorTools.Distance(ed1.v2, ed2.v2)) <
                (VectorTools.Distance(ed1.v1, ed2.v2) + VectorTools.Distance(ed1.v2, ed2.v1)))
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
        /// 
        /// <param name="rp">
        /// Parameter used for determining on which segment the intersection point lies
        /// </param>
        /// 
        /// <returns>
        /// True if lines are not parallel, false otherwise
        /// </returns>
        private bool Intersects(Point p1, Point p2, Point q1, Point q2, ref Point intersection, ref float rp)
        {
            var q = (p1.Y - q1.Y) * (q2.X - q1.X) - (p1.X - q1.X) * (q2.Y - q1.Y);
            var d = (p2.X - p1.X) * (q2.Y - q1.Y) - (p2.Y - p1.Y) * (q2.X - q1.X);

            if (d == 0) // parallel lines
            {
                return false;
            }

            var r = q / d;

            q = (p1.Y - q1.Y) * (p2.X - p1.X) - (p1.X - q1.X) * (p2.Y - p1.Y);
            var s = q / d;

            intersection = VectorTools.Plus(p1, VectorTools.Multiply(VectorTools.Minus(p2, p1), r));

            return true;
        }

        /// <summary>
        /// Finds compatible edges for the specified set of edges
        /// </summary>
        /// 
        /// <param name="edgeSet">
        /// Edges for which we should find compatible edges
        /// </param>
        private void FindCompatibleEdges(Dictionary<KeyPair, EdgeGroupData> edgeSet)
        {
            foreach (var p1 in edgeSet)
            {
                if (p1.Value.length < 50) continue;//??????
                foreach (var p2 in edgeGroupData)
                {
                    if (p2.Value.length < 50) continue;//??????
                    if (((p1.Key.k1 == p2.Key.k1) && (p1.Key.k2 == p2.Key.k2))
                        || (p1.Value.compatibleGroups.ContainsKey(p2.Key)))
                        continue;
                    //if ((((p1.Value.v1 == p2.Value.v1) && (p1.Value.v1.Y == p2.Value.v1.Y)) && (p1.Value.v2.X == p2.Value.v2.X) && (p1.Value.v2.Y == p2.Value.v2.Y)) ||
                    //(((p1.Value.v1.X == p2.Value.v2.X) && (p1.Value.v1.Y == p2.Value.v2.Y)) && (p1.Value.v2.X == p2.Value.v1.X) && (p1.Value.v2.Y == p2.Value.v1.Y)))
                    //    continue;
                    var c = CalculateCompatibility(p1.Value, p2.Value);
                    if (c == 0) continue;
                    var d = CalculateDirectedness(p1.Value, p2.Value);
                    var epd = new GroupPairData(c, p1.Value, p2.Value, d);
                    p1.Value.compatibleGroups.Add(p2.Key, epd);
                    p2.Value.compatibleGroups.Add(p1.Key, epd);
                }
            }
        }

        /// <summary>
        /// Divides edges into segments by adding subdivision points to them
        /// </summary>
        /// 
        /// <param name="subdivisionPointsNum">
        /// Number of subdivision points that should be created
        /// </param>
        private void DivideAllEdges(int subdivisionPointsNum)
        {
            if (subdivisionPointsNum < 1) return;

            foreach (var ed in edgeGroupData.Values)
                DivideEdge(ed, subdivisionPointsNum);

            subdivisionPoints = subdivisionPointsNum;
        }


        /// <summary>
        /// Straightens moved edges.
        /// </summary>
        private void ResetMovedEdges()
        {
            foreach (var ed in movedEdgeGroupData.Values)
                DivideEdge(ed, subdivisionPoints);
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
            var r = ed.length / (subdivisionPointsNum + 1);
            var sPoints = new Point[subdivisionPointsNum];
            ed.newControlPoints = new Point[subdivisionPointsNum];
            Point move;
            if (ed.length == 0)
                move = new Point(0, 0);
            else
                move = VectorTools.Multiply(VectorTools.Minus(ed.v2, ed.v1), 1f / ed.length);
            for (var i = 0; i < subdivisionPointsNum; i++)
                sPoints[i] = VectorTools.Plus(ed.v1, VectorTools.Multiply(move, r * (i + 1)));
            ed.controlPoints = sPoints;
            ed.k = springConstant * (subdivisionPointsNum + 1) / ed.length;
            if (ed.k > 0.5f) ed.k = 0.5f;
        }

        /// <summary>
        /// Doubles subdivision points for an edge by adding one new subdivision point between each two
        /// </summary>
        /// 
        /// <param name="ed">
        /// Edge data that contains subdivision points to be doubled
        /// </param>
        private void DoubleSubdivisionPoints(EdgeGroupData ed)
        {
            if (subdivisionPoints == 0) //make one subdivision point
            {
                ed.k = springConstant * 2 / ed.length;
                if (ed.k > 0.5f) ed.k = 0.5f;
                ed.controlPoints = new Point[1];
                ed.newControlPoints = new Point[1];
                ed.controlPoints[0] = ed.middle;

                return;
            }

            var sPoints = ed.controlPoints;
            var sPointsDoubled = new Point[subdivisionPoints * 2 + 1];
            ed.newControlPoints = new Point[subdivisionPoints * 2 + 1];
            for (var i = 0; i < subdivisionPoints; i++)
                sPointsDoubled[i * 2 + 1] = sPoints[i];


            for (var i = 0; i < subdivisionPoints - 1; i++)
                sPointsDoubled[i * 2 + 2] = VectorTools.MidPoint(sPoints[i], sPoints[i + 1]);


            sPointsDoubled[0] = VectorTools.MidPoint(ed.v1, sPoints[0]);
            sPointsDoubled[subdivisionPoints * 2] = VectorTools.MidPoint(sPoints[subdivisionPoints - 1], ed.v2);
            //ed.K = springConstant * (subdivisionPoints * 2 + 2) / ed.Length;
            ed.k *= 2f;
            if (ed.k > 0.5f) ed.k = 0.5f;
            ed.controlPoints = sPointsDoubled;
        }

        /// <summary>
        /// Doubles subdivision points for all edges
        /// </summary>
        private void DoubleSubdivisionPointsForAllEdges()
        {
            foreach (var ed in edgeGroupData.Values) DoubleSubdivisionPoints(ed);
            subdivisionPoints = subdivisionPoints * 2 + 1;
        }

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
            for (var i = 0; i < subdivisionPoints; i++)
            {
                var p = ed.controlPoints[i];
                Point p1, p2;
                if (i == 0) p1 = ed.v1; else p1 = ed.controlPoints[i - 1];
                if (i == (subdivisionPoints - 1)) p2 = ed.v2; else p2 = ed.controlPoints[i + 1];
                //SizeF sp = new SizeF(p);
                var f = VectorTools.Multiply(VectorTools.Plus(VectorTools.Minus(p1, p), VectorTools.Minus(p2, p)), ed.k);
                var r = new Point(0, 0);
                foreach (var epd in ed.compatibleGroups.Values)
                {
                    Point q;
                    var j = 1f;
                    EdgeGroupData ed2;
                    if ((epd.ed1.id.k1 == ed.id.k1) && (epd.ed1.id.k2 == ed.id.k2))
                        ed2 = epd.ed2;
                    else
                        ed2 = epd.ed1;

                    if (epd.d)
                        q = ed2.controlPoints[i];
                    else
                    {
                        q = ed2.controlPoints[subdivisionPoints - i - 1];
                        if (directed && repulseOpposite) j = repulsionCoefficient;
                    }
                    var fs = VectorTools.Minus(q, p);
                    //PointF fs = new PointF(q.X - p.X, q.Y - p.Y);

                    var l = VectorTools.Length(fs);
                    if (l > 0)//???
                    {
                        fs = VectorTools.Multiply(fs, epd.c / (l));

                        //fs = VectorTools.Multiply(fs, VectorTools.Length(fs) * ed2.edges.Count);
                        fs = VectorTools.Multiply(fs, VectorTools.Length(fs) * ed2.edgeCount);

                        r.X += (j * fs.X);
                        r.Y += (j * fs.Y);
                    }
                }

                var rl = VectorTools.Length(r);
                if (rl>0)
                    r = VectorTools.Multiply(r, (float)(1.0/Math.Sqrt(rl)));

                var move = new Point(f.X + r.X, f.Y + r.Y);
                var moveL = VectorTools.Length(move);

                //float len = ed.Length / (subdivisionPoints + 1);
                //if (moveL > (len)) move = VectorTools.Multiply(move, len*cooldown / moveL);
                //if (moveL != 0) move = VectorTools.Multiply(move, cooldown / moveL);
                move = VectorTools.Multiply(move, cooldown*0.5f);
                ed.newControlPoints[i] = VectorTools.Plus(move, p);

                if (ed.newControlPoints[i].X < AreaRectangle.Left)
                    ed.newControlPoints[i].X = AreaRectangle.Left;
                else
                    if (ed.newControlPoints[i].X > AreaRectangle.Right)
                        ed.newControlPoints[i].X = AreaRectangle.Right;

                if (ed.newControlPoints[i].Y < AreaRectangle.Top)
                    ed.newControlPoints[i].Y = AreaRectangle.Top;
                else
                    if (ed.newControlPoints[i].Y > AreaRectangle.Bottom)
                        ed.newControlPoints[i].Y = AreaRectangle.Bottom;
            }
            if (useThreading) sem.Release();
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
            if (useThreading)
            {
                foreach (var ed in groupsToMove.Values)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.CalculateNewControlPoints), ed);
                for (var i = 0; i < groupsToMove.Values.Count; i++)
                    sem.WaitOne();
            }
            else
            {
                foreach (var ed in groupsToMove.Values)
                    CalculateNewControlPoints(ed);
            }


            foreach (var ed in groupsToMove.Values)
            {
                ed.controlPoints = ed.newControlPoints;
                ed.newControlPoints = new Point[subdivisionPoints];
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
                for (var i = 0; i < subdivisionPoints; i++)
                {
                    //STRONG CHANGE
                    var p = ed.controlPoints[i];
                    p = VectorTools.Plus(VectorTools.Multiply(p, 1 - s),
                        VectorTools.Multiply(VectorTools.Plus(ed.v1,
                            VectorTools.Multiply(VectorTools.Minus(ed.v2, ed.v1), 
                                1.0f * (i + 1) / (subdivisionPoints + 1))), s));
                    ed.controlPoints[i].X = p.X;
                    ed.controlPoints[i].Y = p.Y;
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

        private Dictionary<KeyPair, EdgeGroupData> edgeGroupData = new Dictionary<KeyPair, EdgeGroupData>();

        private Dictionary<KeyPair, EdgeGroupData> movedEdgeGroupData = new Dictionary<KeyPair, EdgeGroupData>();



        private int subdivisionPoints = 15;

        private int iterations = 50;

        private bool repulseOpposite = false;

        private bool directed = true;

        private bool useThreading = true;

        private float springConstant = 10f;

        private float threshold = 0.2f;

        private float cooldown = 1f;

        private float repulsionCoefficient = -0.1f;

        private float straightening = 0.15f;

        private Semaphore sem = new Semaphore(0, Int32.MaxValue);
        
        /// <summary>
        /// Gets or sets the number of subdivision points each edge should have.
        /// Default value is 15.
        /// </summary>
        public int SubdivisionPoints
        {
            get { return subdivisionPoints; }
            set { subdivisionPoints = value; }
        }

        /// <summary>
        /// Gets or sets the number of iterations for moving the control points.
        /// Default value is 50.
        /// </summary>
        public int Iterations
        {
            get { return iterations; }
            set { iterations = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether opposite edges should attracts or repulse each other.
        /// Default value is false.
        /// </summary>
        public bool RepulseOpposite
        {
            get { return repulseOpposite; }
            set { repulseOpposite = value; }
        }

        /// <summary>
        /// Gets or sets the the value that determines if multiple threads should be used for the calculations.
        /// Default value is true.
        /// </summary>
        public bool UseThreading
        {
            get { return useThreading; }
            set { useThreading = value; }
        }

        /// <summary>
        /// Gets or sets the value for the spring constant.
        /// Edges are more easely bent if the value is lower.
        /// Default value is 10.
        /// </summary>
        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
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
            get { return threshold; }
            set { threshold = value; }
        }

        /// <summary>
        /// If repulseOpposite is true, this determines how much will opposite edges repulse eachother.
        /// From -1 to 0.
        /// Default is -0.1
        /// </summary>
        public float RepulsionCoefficient
        {
            get { return repulsionCoefficient; }
            set { repulsionCoefficient = value; }
        }

        /// <summary>
        /// Gets or sets the amount of straightening that will be applied after every bundling.
        /// This can produce better-looking graphs.
        /// Default value is 0.15, range is from 0 to 1.
        /// </summary>
        public float Straightening
        {
            get { return straightening; }
            set { straightening = value; }
        }

        struct KeyPair
        {
            public KeyPair(int n1, int n2)
            {
                k1 = n1;
                k2 = n2;
            }

            public int k1;
            
            public int k2;
        }

        /// <summary>
        /// Class used for storing the needed edge metadata
        /// </summary>
        class EdgeGroupData
        {
            public Point v1;

            public Point v2;

            public Point middle;

            public Point[] controlPoints;

            public Point[] newControlPoints;

            public Dictionary<KeyPair, GroupPairData> compatibleGroups;

            //public HashSet<Int32> edges;

            public int edgeCount;

            public float length;

            public float k;

            public KeyPair id;
        }

        /// <summary>
        /// Class used for storing data for a pair of groups of edges (direction and compatibility coefficient)
        /// </summary>
        class GroupPairData
        {
            public GroupPairData(float cc, EdgeGroupData e1, EdgeGroupData e2, bool dd)
            {
                c = cc;
                ed1 = e1;
                ed2 = e2;
                d = dd;
            }

            public float c;

            public EdgeGroupData ed1;

            public EdgeGroupData ed2;

            public bool d;
        }

    }
}
