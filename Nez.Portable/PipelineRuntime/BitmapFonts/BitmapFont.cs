using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System;


namespace Nez.BitmapFonts
{
	public class BitmapFont : IFont
	{
		float IFont.lineSpacing { get { return lineHeight; } }

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
		/// The distance from the bottom of the glyph that extends the lowest to the baseline. This number is negative.
		/// </summary>
		public float descent;

		/// <summary>
		/// these are currently read in from the .fnt file but not used
		/// </summary>
		public float padTop, padBottom, padLeft, padRight;

		/// <summary>
		/// Gets or sets the character that will be substituted when a given character is not included in the font.
		/// </summary>
		public char defaultCharacter
		{
			set
			{
				if( !_characterMap.TryGetValue( value, out defaultCharacterRegion ) )
					Debug.error( "BitmapFont does not contain a region for the default character being set: {0}", value );
			}
		}

		/// <summary>
		/// populated with ' ' by default and reset whenever defaultCharacter is set
		/// </summary>
		public BitmapFontRegion defaultCharacterRegion;

		/// <summary>
		/// this sucker gets used a lot so we cache it to avoid having to create it every frame
		/// </summary>
		Matrix2D _transformationMatrix = Matrix2D.identity;

		/// <summary>
		/// width of a space
		/// </summary>
		public readonly int spaceWidth;


		readonly Dictionary<char,BitmapFontRegion> _characterMap;


		class CharComparer : IEqualityComparer<char>
		{
			static public readonly CharComparer defaultCharComparer = new CharComparer();

			public bool Equals( char x, char y )
			{
				return x == y;
			}

			public int GetHashCode( char b )
			{
				return ( b | ( b << 16 ) );
			}
		}


		internal BitmapFont( BitmapFontRegion[] regions, int lineHeight )
		{
			_characterMap = new Dictionary<char,BitmapFontRegion>( regions.Length, CharComparer.defaultCharComparer );
			for( var i = 0; i < regions.Length; i++ )
				_characterMap[regions[i].character] = regions[i];

			this.lineHeight = lineHeight;
			defaultCharacter = ' ';
			spaceWidth = defaultCharacterRegion.width + defaultCharacterRegion.xAdvance;
		}


		public string wrapText( string text, float maxLineWidth )
		{
			var words = text.Split( ' ' );
			var sb = new StringBuilder();
			var lineWidth = 0f;

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
		/// truncates text and returns a new string with ellipsis appended if necessary. This method ignores all
		/// line breaks.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="text">Text.</param>
		/// <param name="ellipsis">Ellipsis.</param>
		/// <param name="maxLineWidth">Max line width.</param>
		public string truncateText( string text, string ellipsis, float maxLineWidth )
		{
			if( maxLineWidth < spaceWidth )
				return string.Empty;

			var size = measureString( text );

			// do we even need to truncate?
			var ellipsisWidth = measureString( ellipsis ).X;
			if( size.X > maxLineWidth )
			{
				var sb = new StringBuilder();

				var width = 0.0f;
				BitmapFontRegion currentFontRegion = null;
				var offsetX = 0.0f;

				// determine how many chars we can fit in maxLineWidth - ellipsisWidth
				for( var i = 0; i < text.Length; i++ )
				{
					var c = text[i];

					// we dont deal with line breaks or tabs
					if( c == '\r' || c == '\n' )
						continue;

					if( currentFontRegion != null )
						offsetX += spacing + currentFontRegion.xAdvance;

					if( !_characterMap.TryGetValue( c, out currentFontRegion ) )
						currentFontRegion = defaultCharacterRegion;

					var proposedWidth = offsetX + currentFontRegion.xAdvance + spacing;
					if( proposedWidth > width )
						width = proposedWidth;

					if( width < maxLineWidth - ellipsisWidth )
					{
						sb.Append( c );
					}
					else
					{
						// no more room. append our ellipsis and get out of here
						sb.Append( ellipsis );
						break;
					}
				}

				return sb.ToString();
			}

			return text;
		}


		/// <summary>
		/// Returns the size of the contents of a string when rendered in this font.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		public Vector2 measureString( string text )
		{
			var source = new FontCharacterSource( text );
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
			var source = new FontCharacterSource( text );
			Vector2 size;
			measureString( ref source, out size );
			return size;
		}


		void measureString( ref FontCharacterSource text, out Vector2 size )
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
					currentFontRegion = defaultCharacterRegion;

				var proposedWidth = offset.X + currentFontRegion.xAdvance + spacing;
				if( proposedWidth > width )
					width = proposedWidth;

				if( currentFontRegion.height + currentFontRegion.yOffset > finalLineHeight )
					finalLineHeight = currentFontRegion.height + currentFontRegion.yOffset;
			}

			size.X = width;
			size.Y = fullLineCount * lineHeight + finalLineHeight;
		}


		/// <summary>
		/// gets the BitmapFontRegion for the given char optionally substituting the default region if it isnt present.
		/// </summary>
		/// <returns><c>true</c>, if get font region for char was tryed, <c>false</c> otherwise.</returns>
		/// <param name="c">C.</param>
		/// <param name="fontRegion">Font region.</param>
		/// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
		public bool tryGetFontRegionForChar( char c, out BitmapFontRegion fontRegion, bool useDefaultRegionIfNotPresent = false )
		{
			if( !_characterMap.TryGetValue( c, out fontRegion ) )
			{
				if( useDefaultRegionIfNotPresent )
				{
					fontRegion = defaultCharacterRegion;
					return true;
				}
				return false;
			}

			return true;
		}


		/// <summary>
		/// checks to see if a BitmapFontRegion exists for the char
		/// </summary>
		/// <returns><c>true</c>, if region exists for char was fonted, <c>false</c> otherwise.</returns>
		/// <param name="c">C.</param>
		public bool hasCharacter( char c )
		{
			BitmapFontRegion fontRegion;
			return tryGetFontRegionForChar( c, out fontRegion );
		}


		/// <summary>
		/// gets the BitmapFontRegion for char. Returns null if it doesnt exist and useDefaultRegionIfNotPresent is false.
		/// </summary>
		/// <returns>The region for char.</returns>
		/// <param name="c">C.</param>
		/// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
		public BitmapFontRegion fontRegionForChar( char c, bool useDefaultRegionIfNotPresent = false )
		{
			BitmapFontRegion fontRegion;
			tryGetFontRegionForChar( c, out fontRegion, useDefaultRegionIfNotPresent );
			return fontRegion;
		}


		#region drawing

		void IFont.drawInto( Batcher batcher, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			var source = new FontCharacterSource( text );
			drawInto( batcher, ref source, position, color, rotation, origin, scale, effect, depth );
		}


		void IFont.drawInto( Batcher batcher, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth )
		{
			var source = new FontCharacterSource( text );
			drawInto( batcher, ref source, position, color, rotation, origin, scale, effect, depth );
		}


		internal void drawInto( Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
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
				Matrix2D temp;
				Matrix2D.createTranslation( -origin.X, -origin.Y, out _transformationMatrix );
				Matrix2D.createScale( ( flippedHorz ? -scale.X : scale.X ), ( flippedVert ? -scale.Y : scale.Y ), out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createTranslation( flipAdjustment.X, flipAdjustment.Y, out temp );
				Matrix2D.multiply( ref temp, ref _transformationMatrix, out _transformationMatrix );
				Matrix2D.createRotation( rotation, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createTranslation( position.X, position.Y, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
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
					currentFontRegion = defaultCharacterRegion;


				var p = offset;

				if( flippedHorz )
					p.X += currentFontRegion.width;
				p.X += currentFontRegion.xOffset;

				if( flippedVert )
					p.Y += currentFontRegion.height - lineHeight;
				p.Y += currentFontRegion.yOffset;

				// transform our point if we need to
				if( requiresTransformation )
					Vector2Ext.transform( ref p, ref _transformationMatrix, out p );

				var destRect = RectangleExt.fromFloats
				(
	               p.X, p.Y, 
	               currentFontRegion.width * scale.X,
	               currentFontRegion.height * scale.Y
               );

				batcher.draw( currentFontRegion.subtexture, destRect, currentFontRegion.subtexture.sourceRect, color, rotation, Vector2.Zero, effect, depth );
			}
		}


		/// <summary>
		/// old SpriteBatch drawing method. This should probably be removed since SpriteBatch was replaced by Batcher
		/// </summary>
		/// <param name="spriteBatch">Sprite batch.</param>
		/// <param name="text">Text.</param>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="effect">Effect.</param>
		/// <param name="depth">Depth.</param>
		internal void drawInto( SpriteBatch spriteBatch, ref FontCharacterSource text, Vector2 position, Color color,
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
				Matrix2D temp;
				Matrix2D.createTranslation( -origin.X, -origin.Y, out _transformationMatrix );
				Matrix2D.createScale( ( flippedHorz ? -scale.X : scale.X ), ( flippedVert ? -scale.Y : scale.Y ), out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createTranslation( flipAdjustment.X, flipAdjustment.Y, out temp );
				Matrix2D.multiply( ref temp, ref _transformationMatrix, out _transformationMatrix );
				Matrix2D.createRotation( rotation, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createTranslation( position.X, position.Y, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
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
					currentFontRegion = defaultCharacterRegion;


				var p = offset;

				if( flippedHorz )
					p.X += currentFontRegion.width;
				p.X += currentFontRegion.xOffset;

				if( flippedVert )
					p.Y += currentFontRegion.height - lineHeight;
				p.Y += currentFontRegion.yOffset;

				// transform our point if we need to
				if( requiresTransformation )
					Vector2Ext.transform( ref p, ref _transformationMatrix, out p );

				var destRect = RectangleExt.fromFloats
				(
					p.X, p.Y, 
					currentFontRegion.width * scale.X,
					currentFontRegion.height * scale.Y
				);

				spriteBatch.Draw( currentFontRegion.subtexture, destRect, currentFontRegion.subtexture.sourceRect, color, rotation, Vector2.Zero, effect, depth );
			}
		}

		#endregion

	}
}
