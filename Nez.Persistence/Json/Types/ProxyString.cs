using System;


namespace Nez.Persistence
{
	public sealed class ProxyString : Variant
	{
		readonly string value;


		public ProxyString( string value ) => this.value = value;

		public override string ToString( IFormatProvider provider ) => value;
	}
}
