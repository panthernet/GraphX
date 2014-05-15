using System.Collections.Generic;
using System.Linq;

namespace GraphX.GraphSharp.Algorithms.OverlapRemoval
{
	public class StandardOverlapRemovalAlgorithmFactory<TVertex> : IOverlapRemovalAlgorithmFactory<TVertex>
		where TVertex : class
	{
		protected static readonly string[] algorithmTypes = new string[] { "FSA", "OneWayFSA" };
		public IEnumerable<string> AlgorithmTypes
		{
			get { return algorithmTypes; }
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
					return oldParameters as OverlapRemovalParameters == null
					       	? new OverlapRemovalParameters()
					       	: (IOverlapRemovalParameters)(oldParameters as OverlapRemovalParameters).Clone();
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
			else if ( algorithm is OneWayFSAAlgorithm<TVertex> )
				return "OneWayFSA";
			else
				return string.Empty;
		}

		public bool IsValidAlgorithm( string algorithmType )
		{
			return AlgorithmTypes.Contains( algorithmType );
		}
	}
}