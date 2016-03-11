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
		public int lineHeight;

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

		/// <summary>
		/// populated with ' ' by default and reset whenever defaultCharacter is set
		/// </summary>
		BitmapFontRegion _defaultCharacterRegion;

		/// <summary>
		/// this sucker gets used a lot so we cache it to avoid having to create it every frame
		/// </summary>
		Matrix _transformationMatrix = Matrix.Identity;


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


		public string wrapText( string text, float maxLineWidth )
		{
			var words = text.Split( ' ' );
			var sb = new StringBuilder();
			var lineWidth = 0f;
			var spaceWidth = measureString( " " ).X;

			if( maxLineWidth < spaceWidth )
				return string.Empty;

			foreach( var word in words )
			{
				var size = measureString( word );

				if( lineWidth + size.X < maxLineWidth )
				{
					sb.Append( word + " " );
					lineWidth += size.X + spaceWidth;
				}
				else
				{
					if( size.X > maxLineWidth )
					{
						if( sb.ToString() == "" )
							sb.Append( wrapText( word.Insert( word.Length / 2, " " ) + " ", maxLineWidth ) );
						else
							sb.Append( "\n" + wrapText( word.Insert( word.Length / 2, " " ) + " ", maxLineWidth ) );
					}
					else
					{
						sb.Append( "\n" + word + " " );
						lineWidth = size.X + spaceWidth;
					}
				}
			}

			return sb.ToString();
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
					currentFontRegion = null;
					continue;
				}

				if( currentFontRegion != null )
					offset.X += spacing + currentFontRegion.xAdvance;

				if( !_characterMap.TryGetValue( c, out currentFontRegion ) )
					currentFontRegion = _defaultCharacterRegion;

				var proposedWidth = offset.X + currentFontRegion.xAdvance + spacing;
				if( proposedWidth > width )
					width = proposedWidth;

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


			var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != Vector2.One;
			if( requiresTransformation )
			{
				Matrix temp;
				Matrix.CreateTranslation( -origin.X, -origin.Y, 0f, out _transformationMatrix );
				Matrix.CreateScale( ( flippedHorz ? -scale.X : scale.X ), ( flippedVert ? -scale.Y : scale.Y ), 1f, out temp );
				Matrix.Multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix.CreateTranslation( flipAdjustment.X, flipAdjustment.Y, 0, out temp );
				Matrix.Multiply( ref temp, ref _transformationMatrix, out _transformationMatrix );
				Matrix.CreateRotationZ( rotation, out temp );
				Matrix.Multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix.CreateTranslation( position.X, position.Y, 0f, out temp );
				Matrix.Multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
			}

			BitmapFontRegion currentFontRegion = null;
			var offset = requiresTransformation ? Vector2.Zero : position - origin;

			for( var i = 0; i < text.Length; ++i )
			{
				var c = text[i];
				if( c == '\r' )
					continue;

				if( c == '\n' )
				{
					offset.X = requiresTransformation ? 0f : position.X - origin.X;
					offset.Y += lineHeight;
					currentFontRegion = null;
					continue;
				}

				if( currentFontRegion != null )
					offset.X += spacing + currentFontRegion.xAdvance;

				if( !_characterMap.TryGetValue( c, out currentFontRegion ) )
					currentFontRegion = _defaultCharacterRegion;


				var p = offset;

				if( flippedHorz )
					p.X += currentFontRegion.width;
				p.X += currentFontRegion.xOffset;

				if( flippedVert )
					p.Y += currentFontRegion.height - lineHeight;
				p.Y += currentFontRegion.yOffset;

				// transform our point if we need to
				if( requiresTransformation )
					Vector2.Transform( ref p, ref _transformationMatrix, out p );

				var destRect = RectangleExt.fromFloats
				(
					               p.X, p.Y, 
					               currentFontRegion.width * scale.X,
					               currentFontRegion.height * scale.Y
				               );

				spriteBatch.Draw( currentFontRegion.subtexture, destRect, currentFontRegion.subtexture.sourceRect, color, rotation, Vector2.Zero, effect, depth );
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
