using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;


namespace Nez
{
	public class NezSpriteFont : IFont
	{
		SpriteFont _font;
		private readonly Dictionary<char,SpriteFont.Glyph> _glyphs;

		/// <summary>
		/// this sucker gets used a lot so we cache it to avoid having to create it every frame
		/// </summary>
		Matrix _transformationMatrix = Matrix.Identity;


		public NezSpriteFont( SpriteFont font )
		{
			_font = font;
			_glyphs = font.GetGlyphs();
		}


		/// <summary>
		/// Returns the size of a string when rendered in this font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size, in pixels, of 'text' when rendered in
		/// this font.</returns>
		public Vector2 measureString( string text )
		{
			var source = new FontCharacterSource( text );
			Vector2 size;
			measureString( ref source, out size );
			return size;
		}


		/// <summary>
		/// Returns the size of the contents of a StringBuilder when
		/// rendered in this font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size, in pixels, of 'text' when rendered in
		/// this font.</returns>
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

			// Get the default glyph here once.
			SpriteFont.Glyph? defaultGlyph = null;
			if( _font.DefaultCharacter.HasValue )
				defaultGlyph = _glyphs[_font.DefaultCharacter.Value];

			var width = 0.0f;
			var finalLineHeight = (float)_font.LineSpacing;

			var currentGlyph = SpriteFont.Glyph.Empty;
			var offset = Vector2.Zero;
			var firstGlyphOfLine = true;

			for( var i = 0; i < text.Length; ++i )
			{
				var c = text[i];

				if( c == '\r' )
					continue;

				if( c == '\n' )
				{
					finalLineHeight = _font.LineSpacing;

					offset.X = 0;
					offset.Y += _font.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				}

				if( !_glyphs.TryGetValue( c, out currentGlyph ) )
				{
					if( !defaultGlyph.HasValue )
						throw new ArgumentException( "Errors.TextContainsUnresolvableCharacters", "text" );

					currentGlyph = defaultGlyph.Value;
				}

				// The first character on a line might have a negative left side bearing.
				// In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
				//  so that text does not hang off the left side of its rectangle.
				if( firstGlyphOfLine )
				{
					offset.X = Math.Max( currentGlyph.LeftSideBearing, 0 );
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += _font.Spacing + currentGlyph.LeftSideBearing;
				}

				offset.X += currentGlyph.Width;

				var proposedWidth = offset.X + Math.Max( currentGlyph.RightSideBearing, 0 );
				if( proposedWidth > width )
					width = proposedWidth;

				offset.X += currentGlyph.RightSideBearing;

				if( currentGlyph.Cropping.Height > finalLineHeight )
					finalLineHeight = currentGlyph.Cropping.Height;
			}

			size.X = width;
			size.Y = offset.Y + finalLineHeight;
		}


		/// <summary>
		/// gets the BitmapFontRegion for the given char optionally substituting the default region if it isnt present.
		/// </summary>
		/// <returns><c>true</c>, if get font region for char was tryed, <c>false</c> otherwise.</returns>
		/// <param name="c">C.</param>
		/// <param name="fontRegion">Font region.</param>
		/// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
		public bool tryGetFontRegionForChar( char c, out SpriteFont.Glyph fontGlyph, bool useDefaultRegionIfNotPresent = false )
		{
			if( !_glyphs.TryGetValue( c, out fontGlyph ) )
			{
				if( useDefaultRegionIfNotPresent )
				{
					fontGlyph = _glyphs[_font.DefaultCharacter.Value];
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
			SpriteFont.Glyph fontGlyph;
			return tryGetFontRegionForChar( c, out fontGlyph );
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
					flipAdjustment.Y = _font.LineSpacing - size.Y;
				}
			}

			// TODO: This looks excessive... i suspect we could do most of this with simple vector math and avoid this much matrix work.
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

			// Get the default glyph here once.
			SpriteFont.Glyph? defaultGlyph = null;
			if( _font.DefaultCharacter.HasValue )
				defaultGlyph = _glyphs[_font.DefaultCharacter.Value];

			var currentGlyph = SpriteFont.Glyph.Empty;
			var offset = requiresTransformation ? Vector2.Zero : position - origin;
			var firstGlyphOfLine = true;

			for( var i = 0; i < text.Length; ++i )
			{
				var c = text[i];

				if( c == '\r' )
					continue;

				if( c == '\n' )
				{
					offset.X = requiresTransformation ? 0f : position.X - origin.X;
					offset.Y += _font.LineSpacing;
					firstGlyphOfLine = true;
					continue;
				}

				if( !_glyphs.TryGetValue( c, out currentGlyph ) )
				{
					if( !defaultGlyph.HasValue )
						throw new ArgumentException( "Errors.TextContainsUnresolvableCharacters", "text" );

					currentGlyph = defaultGlyph.Value;
				}

				// The first character on a line might have a negative left side bearing.
				// In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
				// so that text does not hang off the left side of its rectangle.
				if( firstGlyphOfLine )
				{
					offset.X += Math.Max( currentGlyph.LeftSideBearing, 0 );
					firstGlyphOfLine = false;
				}
				else
				{
					offset.X += _font.Spacing + currentGlyph.LeftSideBearing;
				}

				var p = offset;

				if( flippedHorz )
					p.X += currentGlyph.BoundsInTexture.Width;
				p.X += currentGlyph.Cropping.X;

				if( flippedVert )
					p.Y += currentGlyph.BoundsInTexture.Height - _font.LineSpacing;
				p.Y += currentGlyph.Cropping.Y;

				// transform our point if we need to
				if( requiresTransformation )
					Vector2.Transform( ref p, ref _transformationMatrix, out p );

				var destRect = RectangleExt.fromFloats( p.X, p.Y, 
					               currentGlyph.BoundsInTexture.Width * scale.X,
					               currentGlyph.BoundsInTexture.Height * scale.Y );

				batcher.draw( _font.Texture, destRect, currentGlyph.BoundsInTexture, color, rotation, Vector2.Zero, effect, depth );

				offset.X += currentGlyph.Width + currentGlyph.RightSideBearing;
			}
		}

		#endregion

	}
}

