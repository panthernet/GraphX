using System.Collections.Generic;
using System.Linq;
using GraphX.PCL.Common.Interfaces;

namespace GraphX.PCL.Logic.Algorithms.OverlapRemoval
{
	public class StandardOverlapRemovalAlgorithmFactory<TVertex> : IOverlapRemovalAlgorithmFactory<TVertex>
		where TVertex : class
	{
		protected static readonly string[] _algorithmTypes = { "FSA", "OneWayFSA" };
		public IEnumerable<string> AlgorithmTypes
		{
			get { return _algorithmTypes; }
		}

		public IOverlapRemovalAlgorithm<TVertex> CreateAlgorithm( string newAlgorithmType, IOverlapRemovalContext<TVertex> context, IOverlapRemovalParameters parameters )
		{
			if ( context == null || context.Rectangles == null )
				return null;

			switch ( newAlgorithmType )
			{
				case "FSA":
					return new FSAAlgorithm<TVertex>( context.Rectangles, parameters );
				case "OneWayFSA":
					return new OneWayFSAAlgorithm<TVertex>( context.Rectangles, parameters as OneWayFSAParameters );
				default:
					return null;
			}
		}

		public IOverlapRemovalParameters CreateParameters( string algorithmType, IOverlapRemovalParameters oldParameters )
		{
			switch ( algorithmType )
			{
				case "FSA":
					return !(oldParameters is OverlapRemovalParameters)
					       	? new OverlapRemovalParameters()
					       	: (IOverlapRemovalParameters)((OverlapRemovalParameters) oldParameters).Clone();
				case "OneWayFSA":
					return ( oldParameters as OneWayFSAParameters ) == null
					       	? new OneWayFSAParameters()
					       	: (IOverlapRemovalParameters)oldParameters.Clone();
				default:
					return null;
			}
		}

		public string GetAlgorithmType( IOverlapRemovalAlgorithm<TVertex> algorithm )
		{
		    if ( algorithm is FSAAlgorithm<TVertex> )
				return "FSA";
		    if ( algorithm is OneWayFSAAlgorithm<TVertex> )
		        return "OneWayFSA";
		    return string.Empty;
		}

	    public bool IsValidAlgorithm( string algorithmType )
		{
			return AlgorithmTypes.Contains( algorithmType );
		}
	}
}