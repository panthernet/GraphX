using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.FDP
{
    public class FreeFRLayoutParameters : FRLayoutParametersBase
    {
        private double _idealEdgeLength = 10;

        public override double K
        {
            get { return _idealEdgeLength; }
        }

        public override double InitialTemperature
        {
            get { return Math.Sqrt(Math.Pow(_idealEdgeLength, 2) * VertexCount); }
        }

        /// <summary>
        /// Constant. Represents the ideal length of the edges.
        /// </summary>
        public double IdealEdgeLength
        {
            get { return _idealEdgeLength; }
            set
            {
                _idealEdgeLength = value;
                UpdateParameters();
            }
        }
    }
}
