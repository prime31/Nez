using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Colors
	{
		private const string HEX = "0123456789ABCDEF";


		static public byte hexToByte( char c )
		{
			return (byte)HEX.IndexOf( char.ToUpper( c ) );
		}


		static public Color invert( this Color color )
		{
			return new Color( 255 - color.R, 255 - color.G, 255 - color.B, color.A );
		}


		static public Color hexToColor( string hex )
		{
			float r = ( hexToByte( hex[0] ) * 16 + hexToByte( hex[1] ) ) / 255.0f;
			float g = ( hexToByte( hex[2] ) * 16 + hexToByte( hex[3] ) ) / 255.0f;
			float b = ( hexToByte( hex[4] ) * 16 + hexToByte( hex[5] ) ) / 255.0f;

			return new Color( r, g, b );
		}
	}
}

