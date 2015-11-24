using System;


namespace Nez
{
	/// <summary>
	/// utility class to assist with dealing with bitmasks
	/// </summary>
	public static class Flags
	{
		public static bool isFlagSet( this int self, int flag )
		{
			return ( self & flag ) != 0;
		}


		public static void setFlag( ref int self, int flag )
		{
			self = ( self | flag );
		}


		public static void unsetFlag( ref int self, int flag )
		{
			self = ( self & ( ~flag ) );
		}


		public static void invertFlags( ref int self )
		{
			self = ~self;
		}

	}
}

