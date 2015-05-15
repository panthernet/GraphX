using System;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    /// <summary>
    /// Parameters of the Fruchterman-Reingold Algorithm (FDP), bounded version.
    /// </summary>
    public class BoundedFRLayoutParameters : FRLayoutParametersBase
    {
        #region Properties, Parameters
        //some of the parameters declared with 'internal' modifier to 'speed up'

        private double _width = 1000;
        private double _height = 1000;
        private double _k;

        /// <summary>
        /// Width of the bounding box.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                UpdateParameters();
                NotifyPropertyChanged("Width");
            }
        }

        /// <summary>
        /// Height of the bounding box.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdateParameters();
                NotifyPropertyChanged("Height");
            }
        }

        /// <summary>
        /// Constant. <code>IdealEdgeLength = sqrt(height * width / vertexCount)</code>
        /// </summary>
        public override double K
        {
            get { return _k; }
        }

        /// <summary>
        /// Gets the initial temperature of the mass.
        /// </summary>
        public override double InitialTemperature
        {
            get { return Math.Min(Width, Height) / 10; }
        }

        protected override void UpdateParameters()
        {
            _k = Math.Sqrt(_width * Height / VertexCount);
            NotifyPropertyChanged("K");
            base.UpdateParameters();
        }

        #endregion
    }
}