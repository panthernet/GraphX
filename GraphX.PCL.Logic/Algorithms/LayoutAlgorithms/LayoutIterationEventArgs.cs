using System;
using System.Collections.Generic;
using GraphX.Measure;
using QuickGraph;

namespace GraphX.PCL.Logic.Algorithms.LayoutAlgorithms
{
	public class LayoutIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo> 
        : LayoutIterationEventArgs<TVertex, TEdge>,
            ILayoutInfoIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
		where TVertex : class
		where TEdge : IEdge<TVertex>
	{
		public LayoutIterationEventArgs()
			: this( 0, 0, string.Empty, null, null, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent )
			: this( iteration, statusInPercent, string.Empty, null, null, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent, string message )
			: this( iteration, statusInPercent, message, null, null, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent,
		                                 IDictionary<TVertex, Point> vertexPositions )
			: this( iteration, statusInPercent, string.Empty, vertexPositions, null, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent, string message,
		                                 IDictionary<TVertex, Point> vertexPositions,
		                                 IDictionary<TVertex, TVertexInfo> vertexInfos,
		                                 IDictionary<TEdge, TEdgeInfo> edgeInfos)
			: base( iteration, statusInPercent, message, vertexPositions )
		{
			this.VertexInfos = vertexInfos;
			this.EdgeInfos = edgeInfos;
		}

		public IDictionary<TVertex, TVertexInfo> VertexInfos { get; private set; }
		public IDictionary<TEdge, TEdgeInfo> EdgeInfos { get; private set; }

		public sealed override object GetVertexInfo( TVertex vertex )
		{
			if ( VertexInfos == null )
				return null;
			TVertexInfo info;
			return VertexInfos.TryGetValue( vertex, out info ) ? info : default( TVertexInfo );
		}

		public sealed override object GetEdgeInfo( TEdge edge )
		{
			if ( EdgeInfos == null )
				return null;
			TEdgeInfo info;
			return EdgeInfos.TryGetValue( edge, out info ) ? info : default( TEdgeInfo );
		}
	}

	public class LayoutIterationEventArgs<TVertex, TEdge> 
        : EventArgs, 
            ILayoutIterationEventArgs<TVertex>, 
            ILayoutInfoIterationEventArgs<TVertex, TEdge>
		where TVertex : class
		where TEdge : IEdge<TVertex>
	{
		public LayoutIterationEventArgs()
			: this( 0, 0, string.Empty, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent )
			: this( iteration, statusInPercent, string.Empty, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent, string message )
			: this( iteration, statusInPercent, message, null )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent,
		                                 IDictionary<TVertex, Point> vertexPositions )
			: this( iteration, statusInPercent, string.Empty, vertexPositions )
		{ }

		public LayoutIterationEventArgs( int iteration, double statusInPercent, string message,
		                                 IDictionary<TVertex, Point> vertexPositions )
		{
			this.StatusInPercent = statusInPercent;
			this.Iteration = iteration;
			this.Abort = false;
			this.Message = message;
			this.VertexPositions = vertexPositions;
		}

		/// <summary>
		/// Represent the status of the layout algorithm in percent.
		/// </summary>
		public double StatusInPercent { get; private set; }

		/// <summary>
		/// If the user sets this value to <code>true</code>, the algorithm aborts ASAP.
		/// </summary>
		public bool Abort { get; set; }

		/// <summary>
		/// Number of the actual iteration.
		/// </summary>
		public int Iteration { get; private set; }

		/// <summary>
		/// Message, textual representation of the status of the algorithm.
		/// </summary>
		public string Message { get; private set; }

		public IDictionary<TVertex, Point> VertexPositions { get; private set; }

		public virtual object GetVertexInfo( TVertex vertex )
		{
			return null;
		}

		public virtual object GetEdgeInfo( TEdge edge )
		{
			return null;
		}
	}
}