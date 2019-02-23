using System;


namespace Nez.Persistance
{
	public sealed class ProxyBoolean : Variant
	{
		public readonly bool value;

		public ProxyBoolean( bool value ) => this.value = value;

		public override bool ToBoolean( IFormatProvider provider ) => value;

		public override string ToString( IFormatProvider provider ) => value ? "true" : "false";
	}
}
