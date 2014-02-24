using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;

namespace GraphX.GraphSharp.Algorithms
{
	public class LayeredTopologicalSortAlgorithm<TVertex, TEdge> : AlgorithmBase<IVertexAndEdgeListGraph<TVertex, TEdge>>
		where TEdge : IEdge<TVertex>
	{
		#region Private values
		private readonly IDictionary<TVertex, int> layerIndices = new Dictionary<TVertex, int>(); //the index of the layer where the vertex belongs to
		private readonly List<IList<TVertex>> layers = new List<IList<TVertex>>(); //the list of the vertices in the layers
		private int layer; //gives the index of the actual layer
		private readonly IMutableBidirectionalGraph<TVertex, TEdge> tmpGraph;
		#endregion

		#region Properties
		/// <summary>
		/// This dictionary contains the layer-index for every vertices.
		/// </summary>
		public IDictionary<TVertex, int> LayerIndices
		{
			get { return layerIndices; }
		}

		/// <summary>
		/// The count of the layers in the graph.
		/// </summary>
		public int LayerCount
		{
			get { return layer; }
		}

		/// <summary>
		/// The vertices grouped by their LayerIndex.
		/// </summary>
		public IList<IList<TVertex>> Layers
		{
			get { return layers; }
		}
		#endregion

		public delegate void LayerFinishedDelegate( object sender, LayeredTopologicalSortEventArgs e );
		public event LayerFinishedDelegate LayerFinished;

		public LayeredTopologicalSortAlgorithm( IVertexAndEdgeListGraph<TVertex, TEdge> g )
			: base( g )
		{
			tmpGraph = new BidirectionalGraph<TVertex, TEdge>();

			//create a copy from the graph
			tmpGraph.AddVertexRange( g.Vertices );
			foreach ( var e in g.Edges )
				tmpGraph.AddEdge( e );
		}

		protected override void InternalCompute()
		{
			//initializing the sources
			var sources = GetSources( tmpGraph.Vertices );

			//initializing the candidates (candidate for 'source' of the next layer)
			var newSources = new HashSet<TVertex>();

			for ( layer = 0; sources.Count != 0; layer++ )
			{
				foreach ( var s in sources )
				{
					layerIndices[s] = layer;

					//get the neighbours of this source
					var outNeighbours = tmpGraph.GetOutNeighbours( s );

					//remove this source
					tmpGraph.RemoveVertex( s );

					//check if any of the neighbours became a source
					foreach ( var n in outNeighbours )
						if ( tmpGraph.IsInEdgesEmpty( n ) )
							newSources.Add( n );
				}

				//the actual layer have been finished
				layers.Add( sources );
				OnLayerFinished( new LayeredTopologicalSortEventArgs
				                 	{
				                 		LayerIndex = layer,
				                 		Vertices = sources
				                 	} );

				//prepare for the next layer
				sources = newSources.ToList();
				newSources = new HashSet<TVertex>();
			}


			//if the graph is not empty, it's a problem
			if ( !tmpGraph.IsVerticesEmpty )
				throw new NonAcyclicGraphException();
		}

		protected IList<TVertex> GetSources( IEnumerable<TVertex> vertices )
		{
			return ( from v in vertices
			         where tmpGraph.IsInEdgesEmpty( v )
			         select v ).ToList();
		}

		protected void OnLayerFinished( LayeredTopologicalSortEventArgs args )
		{
			if ( LayerFinished != null )
				LayerFinished( this, args );
		}

		public class LayeredTopologicalSortEventArgs : EventArgs
		{
			public int LayerIndex { get; internal set; }
			public IEnumerable<TVertex> Vertices { get; internal set; }
		}
	}
}