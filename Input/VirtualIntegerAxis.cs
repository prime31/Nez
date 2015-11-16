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
				foreach( var node in nodes )
				{
					var value = node.value;
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
			foreach( var node in nodes )
				node.update();
		}


		static public implicit operator int( VirtualIntegerAxis axis )
		{
			return axis.value;
		}

	}
}

