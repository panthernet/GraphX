using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphX.GraphSharp.Algorithms.Layout.Simple.Tree
{
	public class BalloonTreeLayoutParameters : LayoutParametersBase
	{
		internal int minRadius = 2;
		internal float border = 20.0f;

		public int MinRadius
		{
			get { return minRadius; }
			set
			{
				if ( value != minRadius )
				{
					minRadius = value;
					NotifyPropertyChanged( "MinRadius" );
				}
			}
		}


		public float Border
		{
			get { return border; }
			set
			{
				if ( value != border )
				{
					border = value;
					NotifyPropertyChanged( "Border" );
				}
			}
		}
	}
}
