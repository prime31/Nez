using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.PipelineImporter
{
	public static class PixelBitmapContentExt
	{
		public static Color[] getData( this PixelBitmapContent<Color> self )
		{
			var data = new Color[self.Width * self.Height];
			var i = 0;
			for( var y = 0; y < self.Height; y++ )
			{
				for( var x = 0; x < self.Width; x++ )
					data[i++] = self.GetPixel( x, y );
			}

			return data;
		}


		public static void setData( this PixelBitmapContent<Color> self, Color[] data )
		{
			var i = 0;
			for( var y = 0; y < self.Height; y++ )
			{
				for( var x = 0; x < self.Width; x++ )
					self.SetPixel( x, y, data[i++] );
			}
		}

	}
}

