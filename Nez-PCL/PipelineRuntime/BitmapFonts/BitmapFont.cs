using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System;


namespace Nez.BitmapFonts
{
	public class BitmapFont
	{
		/// <summary>
		/// Gets or sets the line spacing (the distance from baseline to baseline) of the font.
		/// </summary>
		/// <value>The height of the line.</value>
		public int lineHeight { get; private set; }

		/// <summary>
		/// Gets or sets the spacing (tracking) between characters in the font.
		/// </summary>
		public float spacing;

		/// <summary>
		/// Gets or sets the character that will be substituted when a given character is not included in the font.
		/// </summary>
		public char defaultCharacter
		{
			set
			{
				if( !_characterMap.TryGetValue( value, out _defaultCharacterRegion ) )
					Debug.error( "BitmapFont does not contain a region for the default character being set: {0}", value );
			}
		}

		BitmapFontRegion _defaultCharacterRegion;


		private readonly Dictionary<char,BitmapFontRegion> _characterMap;


		class CharComparer : IEqualityComparer<char>
		{
			public bool Equals( char x, char y )
			{
				return x == y;
			}

			public int GetHashCode( char b )
			{
				return ( b | ( b << 16 ) );
			}

			static public readonly CharComparer defaultCharComparer = new CharComparer();
		}


		internal BitmapFont( BitmapFontRegion[] regions, int lineHeight )
		{
			_characterMap = new Dictionary<char,BitmapFontRegion>( regions.Length, CharComparer.defaultCharComparer );
			for( var i = 0; i < regions.Length; i++ )
				_characterMap[regions[i].character] = regions[i];

			this.lineHeight = lineHeight;
			defaultCharacter = ' ';
		}


		/// <summary>
		/// Returns the size of the contents of a string when rendered in this font.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		public Vector2 measureString( string text )
		{
			var source = new BitmapFont.CharacterSource( text );
			Vector2 size;
			measureString( ref source, out size );
			return size;
		}


		/// <summary>
		/// Returns the size of the contents of a StringBuilder when rendered in this font.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		public Vector2 measureString( StringBuilder text )
		{
			var source = new BitmapFont.CharacterSource( text );
			Vector2 size;
			measureString( ref source, out size );
			return size;
		}


		void measureString( ref CharacterSource text, out Vector2 size )
		{
			if( text.Length == 0 )
			{
				size = Vector2.Zero;
				return;
			}

			var width = 0.0f;
			var finalLineHeight = (float)lineHeight;
			var fullLineCount = 0;
			BitmapFontRegion currentFontRegion = null;
			var offset = Vector2.Zero;

			for( var i = 0; i < text.Length; i++ )
			{
				var c = text[i];

				if( c == '\r' )
					continue;

				if( c == '\n' )
				{
					fullLineCount++;
					finalLineHeight = lineHeight;

					offset.X = 0;
					offset.Y = lineHeight * fullLineCount;
					continue;
				}


				if( !_characterMap.TryGetValue( c, out currentFontRegion ) )
					currentFontRegion = _defaultCharacterRegion;

				width += spacing + currentFontRegion.xAdvance;

				if( currentFontRegion.height + currentFontRegion.yOffset > finalLineHeight )
					finalLineHeight = currentFontRegion.height + currentFontRegion.yOffset;
			}

			size.X = width;
			size.Y = fullLineCount * lineHeight + finalLineHeight;
		}


		internal void drawInto( SpriteBatch spriteBatch, ref CharacterSource text, Vector2 position, Color color,
		                        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			var flipAdjustment = Vector2.Zero;

			var flippedVert = ( effect & SpriteEffects.FlipVertically ) == SpriteEffects.FlipVertically;
			var flippedHorz = ( effect & SpriteEffects.FlipHorizontally ) == SpriteEffects.FlipHorizontally;

			if( flippedVert || flippedHorz )
			{
				Vector2 size;
				measureString( ref text, out size );

				if( flippedHorz )
				{
					origin.X *= -1;
					flipAdjustment.X = -size.X;
				}

				if( flippedVert )
				{
					origin.Y *= -1;
					flipAdjustment.Y = lineHeight - size.Y;
				}
			}


			// TODO: This looks excessive... i suspect we could do most
			// of this with simple vector math and avoid this much matrix work.

			Matrix transformation, temp;
			Matrix.CreateTranslation( -origin.X, -origin.Y, 0f, out transformation );
			Matrix.CreateScale( ( flippedHorz ? -scale.X : scale.X ), ( flippedVert ? -scale.Y : scale.Y ), 1f, out temp );
			Matrix.Multiply( ref transformation, ref temp, out transformation );
			Matrix.CreateTranslation( flipAdjustment.X, flipAdjustment.Y, 0, out temp );
			Matrix.Multiply( ref temp, ref transformation, out transformation );
			Matrix.CreateRotationZ( rotation, out temp );
			Matrix.Multiply( ref transformation, ref temp, out transformation );
			Matrix.CreateTranslation( position.X, position.Y, 0f, out temp );
			Matrix.Multiply( ref transformation, ref temp, out transformation );

			BitmapFontRegion currentFontRegion = null;
			var offset = Vector2.Zero;

			for( var i = 0; i < text.Length; ++i )
			{
				var c = text[i];
				if( c == '\r' )
					continue;

				if( c == '\n' )
				{
					offset.X = 0;
					offset.Y += lineHeight;
					continue;
				}

				if( currentFontRegion != null )
				{
					//offset.X += spacing + currentFontRegion.Width + currentFontRegion.RightSideBearing;
					offset.X += spacing + currentFontRegion.xAdvance;
				}

				if( !_characterMap.TryGetValue( c, out currentFontRegion ) )
					currentFontRegion = _defaultCharacterRegion;


				var p = offset;

				if( flippedHorz )
					p.X += currentFontRegion.textureRegion.sourceRect.Width;
				p.X += currentFontRegion.xOffset;

				if( flippedVert )
					p.Y += currentFontRegion.textureRegion.sourceRect.Height - lineHeight;
				p.Y += currentFontRegion.yOffset;

				Vector2.Transform( ref p, ref transformation, out p );

				var destRect = RectangleExtension.fromFloats
				(
					p.X, p.Y, 
					currentFontRegion.textureRegion.sourceRect.Width * scale.X,
					currentFontRegion.textureRegion.sourceRect.Height * scale.Y
				);

				spriteBatch.Draw( currentFontRegion.textureRegion, destRect, currentFontRegion.textureRegion.sourceRect, color, rotation, Vector2.Zero, effect, depth );
			}
		}


		/// <summary>
		/// helper that wraps either a string or StringBuilder and provides a common API to read them for measuring/drawing
		/// </summary>
		internal struct CharacterSource
		{
			private readonly string _string;
			private readonly StringBuilder _builder;
			public readonly int Length;


			public CharacterSource( string s )
			{
				_string = s;
				_builder = null;
				Length = s.Length;
			}


			public CharacterSource( StringBuilder builder )
			{
				_builder = builder;
				_string = null;
				Length = _builder.Length;
			}


			public char this[int index]
			{
				get
				{
					if( _string != null )
						return _string[index];
					return _builder[index];
				}
			}
		}

	}
}
