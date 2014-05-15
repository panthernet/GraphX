using System;
using QuickGraph.Algorithms;

namespace GraphX.GraphSharp.Algorithms
{
	public abstract class AlgorithmBase : IAlgorithm
	{
		private volatile object syncRoot = new object();
		private int cancelling;
		private volatile ComputationState state = ComputationState.NotRunning;

		public Object SyncRoot
		{
			get { return syncRoot; }
		}

		public ComputationState State
		{
			get
			{
				lock ( syncRoot )
				{
					return state;
				}
			}
		}

		protected bool IsAborting
		{
			get { return cancelling > 0; }
		}

		public void Compute()
		{
			BeginComputation();
			InternalCompute();
			EndComputation();
		}

		protected abstract void InternalCompute();

		public virtual void Abort()
		{
			bool raise = false;
			lock ( syncRoot )
			{
				if ( state == ComputationState.Running )
				{
					state = ComputationState.PendingAbortion;
					System.Threading.Interlocked.Increment( ref cancelling );
					raise = true;
				}
			}
			if ( raise )
				OnStateChanged( EventArgs.Empty );
		}

		public event EventHandler StateChanged;
		protected virtual void OnStateChanged( EventArgs e )
		{
			EventHandler eh = StateChanged;
			if ( eh != null )
				eh( this, e );
		}

		public event EventHandler Started;
		protected virtual void OnStarted( EventArgs e )
		{
			EventHandler eh = Started;
			if ( eh != null )
				eh( this, e );
		}

		public event EventHandler Finished;
		protected virtual void OnFinished( EventArgs e )
		{
			EventHandler eh = Finished;
			if ( eh != null )
				eh( this, e );
		}

		public event EventHandler Aborted;
		protected virtual void OnAborted( EventArgs e )
		{
			EventHandler eh = Aborted;
			if ( eh != null )
				eh( this, e );
		}

		protected void BeginComputation()
		{
			lock ( syncRoot )
			{
				//if ( state != ComputationState.NotRunning )
				//	throw new InvalidOperationException();

				state = ComputationState.Running;
				cancelling = 0;
				OnStarted( EventArgs.Empty );
				OnStateChanged( EventArgs.Empty );
			}
		}

		protected void EndComputation()
		{
			lock ( syncRoot )
			{
				switch ( state )
				{
					case ComputationState.Running:
						state = ComputationState.Finished;
						OnFinished( EventArgs.Empty );
						break;
					case ComputationState.PendingAbortion:
						state = ComputationState.Aborted;
						OnAborted( EventArgs.Empty );
						break;
					default:
						throw new InvalidOperationException();
				}
				cancelling = 0;
				OnStateChanged( EventArgs.Empty );
			}
		}
	}
}