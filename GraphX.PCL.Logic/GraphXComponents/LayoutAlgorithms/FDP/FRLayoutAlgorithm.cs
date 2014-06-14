using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
{
    public class FRLayoutAlgorithm<Vertex, Edge, Graph> : ParameterizedLayoutAlgorithmBase<Vertex, Edge, Graph, FRLayoutParametersBase>
        where Vertex : class
        where Edge : IEdge<Vertex>
        where Graph : IVertexAndEdgeListGraph<Vertex, Edge>
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
        public FRLayoutAlgorithm(Graph visitedGraph)
            : base(visitedGraph) { }

        public FRLayoutAlgorithm(Graph visitedGraph, IDictionary<Vertex, Point> vertexPositions, FRLayoutParametersBase parameters)
            : base(visitedGraph, vertexPositions, parameters) { }
        #endregion

        /// <summary>
        /// It computes the layout of the vertices.
        /// </summary>
        protected override void InternalCompute()
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
                  && _temperature > minimalTemperature
                  && State != QuickGraph.Algorithms.ComputationState.PendingAbortion;
                  i++)
            {
                IterateOne();

                //make some cooling
                switch (Parameters._coolingFunction)
                {
                    case FRCoolingFunction.Linear:
                        _temperature *= (1.0 - (double)i / (double)Parameters._iterationLimit);
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


        protected void IterateOne()
        {
            //create the forces (zero forces)
            var forces = new Dictionary<Vertex, Vector>();

            #region Repulsive forces
            var force = new Vector(0, 0);
            foreach (Vertex v in VisitedGraph.Vertices)
            {
                force.X = 0; force.Y = 0;
                Point posV = VertexPositions[v];
                foreach (Vertex u in VisitedGraph.Vertices)
                {
                    //doesn't repulse itself
                    if (u.Equals(v))
                        continue;

                    //calculating repulsive force
                    Vector delta = posV - VertexPositions[u];
                    double length = Math.Max(delta.Length, double.Epsilon);
                    delta = delta / length * Parameters.ConstantOfRepulsion / length;

                    force += delta;
                }
                forces[v] = force;
            }
            #endregion

            #region Attractive forces
            foreach (Edge e in VisitedGraph.Edges)
            {
                Vertex source = e.Source;
                Vertex target = e.Target;

                //vonzóerõ számítása a két pont közt
                Vector delta = VertexPositions[source] - VertexPositions[target];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Pow(length, 2) / Parameters.ConstantOfAttraction;

                forces[source] -= delta;
                forces[target] += delta;
            }
            #endregion

            #region Limit displacement
            foreach (Vertex v in VisitedGraph.Vertices)
            {
                Point pos = VertexPositions[v];

                //erõ limitálása a temperature-el
                Vector delta = forces[v];
                double length = Math.Max(delta.Length, double.Epsilon);
                delta = delta / length * Math.Min(delta.Length, _temperature);

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