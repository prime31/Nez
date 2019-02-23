using System;
using System.Globalization;


namespace Nez.Persistance
{
	public sealed class ProxyNumber : Variant
	{
		static readonly char[] floatingPointCharacters = { '.', 'e' };
		readonly IConvertible value;


		public ProxyNumber( IConvertible value )
		{
			var stringValue = value as string;
			this.value = stringValue != null ? Parse( stringValue ) : value;
		}

		public static IConvertible Parse( string value )
		{
			if( value.IndexOfAny( floatingPointCharacters ) == -1 )
			{
				if( value[0] == '-' )
				{
					Int64 parsedValue;
					if( Int64.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ) )
					{
						return parsedValue;
					}
				}
				else
				{
					UInt64 parsedValue;
					if( UInt64.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ) )
					{
						return parsedValue;
					}
				}
			}

			Decimal decimalValue;
			if( Decimal.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimalValue ) )
			{
				// Check for decimal underflow.
				if( decimalValue == Decimal.Zero )
				{
					Double parsedValue;
					if( Double.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ) )
					{
						if( Math.Abs( parsedValue ) > Double.Epsilon )
						{
							return parsedValue;
						}
					}
				}

				return decimalValue;
			}

			Double doubleValue;
			if( Double.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleValue ) )
			{
				return doubleValue;
			}

			return 0;
		}

		public override bool ToBoolean( IFormatProvider provider ) => value.ToBoolean( provider );

		public override byte ToByte( IFormatProvider provider ) => value.ToByte( provider );

		public override char ToChar( IFormatProvider provider ) => value.ToChar( provider );

		public override decimal ToDecimal( IFormatProvider provider ) => value.ToDecimal( provider );

		public override double ToDouble( IFormatProvider provider ) => value.ToDouble( provider );

		public override short ToInt16( IFormatProvider provider ) => value.ToInt16( provider );

		public override int ToInt32( IFormatProvider provider ) => value.ToInt32( provider );

		public override long ToInt64( IFormatProvider provider ) => value.ToInt64( provider );

		public override sbyte ToSByte( IFormatProvider provider ) => value.ToSByte( provider );

		public override float ToSingle( IFormatProvider provider ) => value.ToSingle( provider );

		public override string ToString( IFormatProvider provider ) => value.ToString( provider );

		public override ushort ToUInt16( IFormatProvider provider ) => value.ToUInt16( provider );

		public override uint ToUInt32( IFormatProvider provider ) => value.ToUInt32( provider );

		public override ulong ToUInt64( IFormatProvider provider ) => value.ToUInt64( provider );
	}
}
