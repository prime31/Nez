using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Nez.BitmapFonts
{
	public class BitmapFont
	{
		public int lineHeight { get; private set; }

		private readonly Dictionary<char, BitmapFontRegion> _characterMap;


		internal BitmapFont( IEnumerable<BitmapFontRegion> regions, int lineHeight )
		{
			_characterMap = regions.ToDictionary( r => r.character );// BuildCharacterMap(textures, _fontFile);
			this.lineHeight = lineHeight;
		}


		public BitmapFontRegion getCharacterRegion( char character )
		{
			BitmapFontRegion region;
			return _characterMap.TryGetValue( character, out region ) ? region : null;
		}


		public Rectangle getStringRectangle( string text, Vector2 position )
		{
			var width = 0;
			var height = 0;

			for( var i = 0; i < text.Length; i++ )
			{
				var c = text[i];

				BitmapFontRegion fontRegion;
				if( _characterMap.TryGetValue( c, out fontRegion ) )
				{
					width += fontRegion.xAdvance;

					if( fontRegion.height + fontRegion.yOffset > height )
						height = fontRegion.height + fontRegion.yOffset;
				}
			}

			var p = position.ToPoint();
			return new Rectangle( p.X, p.Y, width, height );
		}
	}
}
