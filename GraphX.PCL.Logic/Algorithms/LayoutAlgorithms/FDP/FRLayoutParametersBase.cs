using System;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
    /// <summary>
    /// Parameters base for the Fruchterman-Reingold Algorithm (FDP).
    /// </summary>
    public abstract class FRLayoutParametersBase : LayoutParametersBase
    {
        #region Properties, Parameters
        private int _vertexCount;
        internal double _attractionMultiplier = 1.2;
        internal double _repulsiveMultiplier = 0.6;
        internal int _iterationLimit = 200;
        internal double _lambda = 0.95;
        internal FRCoolingFunction _coolingFunction = FRCoolingFunction.Exponential;

        /// <summary>
		/// Count of the vertices (used to calculate the constants)
		/// </summary>
		internal int VertexCount
		{
			get { return _vertexCount; }
			set
			{
				_vertexCount = value;
				UpdateParameters();
				NotifyPropertyChanged( "VertexCount" );
			}
		}

		protected virtual void UpdateParameters()
		{
			CalculateConstantOfRepulsion();
			CalculateConstantOfAttraction();
		}

		private void CalculateConstantOfRepulsion()
		{
			ConstantOfRepulsion = Math.Pow( K * _repulsiveMultiplier, 2 );
			NotifyPropertyChanged( "ConstantOfRepulsion" );
		}

		private void CalculateConstantOfAttraction()
		{
			ConstantOfAttraction = K * _attractionMultiplier;
			NotifyPropertyChanged( "ConstantOfAttraction" );
		}

		/// <summary>
		/// Gets the computed ideal edge length.
		/// </summary>
		public abstract double K { get; }

        /// <summary>
        /// Gets the initial temperature of the mass.
        /// </summary>
        public abstract double InitialTemperature { get; }

		/// <summary>
		/// Constant. <code>Equals with K * attractionMultiplier</code>
		/// </summary>
		public double ConstantOfAttraction { get; private set; }

		/// <summary>
		/// Multiplier of the attraction. Default value is 2.
		/// </summary>
		public double AttractionMultiplier
		{
			get { return _attractionMultiplier; }
			set
			{
				_attractionMultiplier = value;
				CalculateConstantOfAttraction();
				NotifyPropertyChanged( "AttractionMultiplier" );
			}
		}

		/// <summary>
		/// Constant. Equals with <code>Pow(K * repulsiveMultiplier, 2)</code>
		/// </summary>
		public double ConstantOfRepulsion { get; private set; }

		/// <summary>
		/// Multiplier of the repulsion. Default value is 1.
		/// </summary>
		public double RepulsiveMultiplier
		{
			get { return _repulsiveMultiplier; }
			set
			{
				_repulsiveMultiplier = value;
				CalculateConstantOfRepulsion();
				NotifyPropertyChanged( "RepulsiveMultiplier" );
			}
		}

		/// <summary>
		/// Limit of the iterations. Default value is 200.
		/// </summary>
		public int IterationLimit
		{
			get { return _iterationLimit; }
			set
			{
				_iterationLimit = value;
				NotifyPropertyChanged( "IterationLimit" );
			}
		}

		/// <summary>
		/// Lambda for the cooling. Default value is 0.95.
		/// </summary>
		public double Lambda
		{
			get { return _lambda; }
			set
			{
				_lambda = value;
				NotifyPropertyChanged( "Lamdba" );
			}
		}

		/// <summary>
		/// Gets or sets the cooling function which could be Linear or Exponential.
		/// </summary>
		public FRCoolingFunction CoolingFunction
		{
			get { return _coolingFunction; }
			set
			{
				_coolingFunction = value;
				NotifyPropertyChanged( "CoolingFunction" );
			}
		}

		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		protected FRLayoutParametersBase()
		{
			//update the parameters
			UpdateParameters();
		}
    }
}
