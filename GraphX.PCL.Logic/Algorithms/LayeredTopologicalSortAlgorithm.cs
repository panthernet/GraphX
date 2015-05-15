using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;

namespace GraphX.PCL.Logic.Algorithms
{
	public class LayeredTopologicalSortAlgorithm<TVertex, TEdge> : AlgorithmBase<IVertexAndEdgeListGraph<TVertex, TEdge>>
		where TEdge : IEdge<TVertex>
	{
		#region Private values
		private readonly IDictionary<TVertex, int> _layerIndices = new Dictionary<TVertex, int>(); //the index of the layer where the vertex belongs to
		private readonly List<IList<TVertex>> _layers = new List<IList<TVertex>>(); //the list of the vertices in the layers
		private int _layer; //gives the index of the actual layer
		private readonly IMutableBidirectionalGraph<TVertex, TEdge> _tmpGraph;
		#endregion

		#region Properties
		/// <summary>
		/// This dictionary contains the layer-index for every vertices.
		/// </summary>
		public IDictionary<TVertex, int> LayerIndices
		{
			get { return _layerIndices; }
		}

		/// <summary>
		/// The count of the layers in the graph.
		/// </summary>
		public int LayerCount
		{
			get { return _layer; }
		}

		/// <summary>
		/// The vertices grouped by their LayerIndex.
		/// </summary>
		public IList<IList<TVertex>> Layers
		{
			get { return _layers; }
		}
		#endregion

		public delegate void LayerFinishedDelegate( object sender, LayeredTopologicalSortEventArgs e );
		public event LayerFinishedDelegate LayerFinished;

		public LayeredTopologicalSortAlgorithm( IVertexAndEdgeListGraph<TVertex, TEdge> g )
			: base( g )
		{
			_tmpGraph = new BidirectionalGraph<TVertex, TEdge>();

			//create a copy from the graph
			_tmpGraph.AddVertexRange( g.Vertices );
			foreach ( var e in g.Edges )
				_tmpGraph.AddEdge( e );
		}

		protected override void InternalCompute()
		{
			//initializing the sources
			var sources = GetSources( _tmpGraph.Vertices );

			//initializing the candidates (candidate for 'source' of the next layer)
			var newSources = new HashSet<TVertex>();

			for ( _layer = 0; sources.Count != 0; _layer++ )
			{
				foreach ( var s in sources )
				{
					_layerIndices[s] = _layer;

					//get the neighbours of this source
					var outNeighbours = _tmpGraph.GetOutNeighbours( s );

					//remove this source
					_tmpGraph.RemoveVertex( s );

					//check if any of the neighbours became a source
					foreach ( var n in outNeighbours )
						if ( _tmpGraph.IsInEdgesEmpty( n ) )
							newSources.Add( n );
				}

				//the actual layer have been finished
				_layers.Add( sources );
				OnLayerFinished( new LayeredTopologicalSortEventArgs
				                 	{
				                 		LayerIndex = _layer,
				                 		Vertices = sources
				                 	} );

				//prepare for the next layer
				sources = newSources.ToList();
				newSources = new HashSet<TVertex>();
			}


			//if the graph is not empty, it's a problem
			if ( !_tmpGraph.IsVerticesEmpty )
				throw new NonAcyclicGraphException();
		}

		protected IList<TVertex> GetSources( IEnumerable<TVertex> vertices )
		{
			return ( from v in vertices
			         where _tmpGraph.IsInEdgesEmpty( v )
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