using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GraphX.Measure;
using GraphX.PCL.Common.Exceptions;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    public class FRLayoutAlgorithm<TVertex, TEdge, TGraph> : ParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, FRLayoutParametersBase>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        /// <summary>
        /// Actual temperature of the 'mass'.
        /// </summary>
        private double _temperature;

        private double _maxWidth = double.PositiveInfinity;
        private double _maxHeight = double.PositiveInfinity;

        protected override FRLayoutParametersBase DefaultParameters
        {
            get { return new FreeFRLayoutParameters(); }
        }

        #region Constructors
        public FRLayoutAlgorithm(TGraph visitedGraph)
            : base(visitedGraph) { }

        public FRLayoutAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, FRLayoutParametersBase parameters)
            : base(visitedGraph, vertexPositions, parameters) { }
        #endregion

        /// <summary>
        /// It computes the layout of the vertices.
        /// </summary>
        public override void Compute(CancellationToken cancellationToken)
        {
            if (VisitedGraph.VertexCount == 1)
            {
                VertexPositions.Add(VisitedGraph.Vertices.First(), new Point(0, 0));
                return;
            }

            //initializing the positions
            if (Parameters is BoundedFRLayoutParameters)
            {
                var param = Parameters as BoundedFRLayoutParameters;
                InitializeWithRandomPositions(param.Width, param.Height);
                _maxWidth = param.Width;
                _maxHeight = param.Height;
            }
            else
            {
                InitializeWithRandomPositions(10.0, 10.0);
            }
            Parameters.VertexCount = VisitedGraph.VertexCount;

            // Actual temperature of the 'mass'. Used for cooling.
            var minimalTemperature = Parameters.InitialTemperature*0.01;
            _temperature = Parameters.InitialTemperature;
            for (int i = 0;
                  i < Parameters._iterationLimit
                  && _temperature > minimalTemperature;
                  i++)
            {
                IterateOne(cancellationToken);

                //make some cooling
                switch (Parameters._coolingFunction)
                {
                    case FRCoolingFunction.Linear:
                        _temperature *= (1.0 - i / (double)Parameters._iterationLimit);
                        break;
                    case FRCoolingFunction.Exponential:
                        _temperature *= Parameters._lambda;
                        break;
                }

                //iteration ended, do some report
                /*if (ReportOnIterationEndNeeded)
                {
                    double statusInPercent = (double)i / (double)Parameters._iterationLimit;
                    OnIterationEnded(i, statusInPercent, string.Empty, true);
                }*/
            }
        }

        public override void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            if (VisitedGraph == null && !TryCreateNewGraph())
                throw new GX_GeneralException("Can't create new graph through reflection. Make sure it support default constructor.");
            VisitedGraph.Clear();
            VisitedGraph.AddVertexRange(vertices);
            VisitedGraph.AddEdgeRange(edges);
        }


        protected void IterateOne(CancellationToken cancellationToken)
        {
            //create the forces (zero forces)
            var forces = new Dictionary<TVertex, Vector>();

            #region Repulsive forces
            var force = new Vector(0, 0);
            foreach (TVertex v in VisitedGraph.Vertices)
            {
                force.X = 0; force.Y = 0;
                Point posV = VertexPositions[v];
                foreach (TVertex u in VisitedGraph.Vertices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    //doesn't repulse itself
                    if (u.Equals(v))
                        continue;

                    //calculating repulsive force
                    Vector delta = posV - VertexPositions[u];
                    double length = Math.Max(delta.Length, double.Epsilon);
                    delta = delta / length * Parameters.ConstantOfRepulsion / length;

					// Check for NaN
					if (double.IsNaN(delta.X) || double.IsNaN(delta.Y))
						delta = new Vector(0, 0);

                    force += delta;
                }
                forces[v] = force;
            }
            #endregion

            #region Attractive forces
            foreach (TEdge e in VisitedGraph.Edges)
            {
                TVertex source = e.Source;
                TVertex target = e.Target;

                //vonzóerõ számítása a két pont közt
                Vector delta = VertexPositions[source] - VertexPositions[target];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Pow(length, 2) / Parameters.ConstantOfAttraction;

				// Check for NaN
				if (double.IsNaN(delta.X) || double.IsNaN(delta.Y))
					delta = new Vector(0, 0);

                forces[source] -= delta;
                forces[target] += delta;
            }
            #endregion

            #region Limit displacement
            foreach (TVertex v in VisitedGraph.Vertices)
            {
                Point pos = VertexPositions[v];

                //erõ limitálása a temperature-el
                Vector delta = forces[v];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Min(delta.Length, _temperature);

				// Check for NaN
				if (double.IsNaN(delta.X) || double.IsNaN(delta.Y))
					delta = new Vector(0, 0);

                //erõhatás a pontra
                pos += delta;

                //falon ne menjünk ki
                pos.X = Math.Min(_maxWidth, Math.Max(0, pos.X));
                pos.Y = Math.Min(_maxHeight, Math.Max(0, pos.Y));
                VertexPositions[v] = pos;
            }
            #endregion
        }
    }
}
