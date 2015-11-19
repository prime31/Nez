using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// A virtual input that is represented as a int that is either -1, 0, or 1
	/// </summary>
	public class VirtualIntegerAxis : VirtualInput
	{
		public List<VirtualAxis.Node> nodes = new List<VirtualAxis.Node>();

		public int value
		{
			get
			{
				for( var i = 0; i < nodes.Count; i++ )
				{
					var value = nodes[i].value;
					if( value != 0 )
						return Math.Sign( value );
				}

				return 0;
			}
		}


		public VirtualIntegerAxis() : base()
		{}


		public VirtualIntegerAxis( params VirtualAxis.Node[] nodes )
		{
			this.nodes.AddRange( nodes );
		}


		public override void update()
		{
			for( var i = 0; i < nodes.Count; i++ )
				nodes[i].update();
		}


		static public implicit operator int( VirtualIntegerAxis axis )
		{
			return axis.value;
		}

	}
}

