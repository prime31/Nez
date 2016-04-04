using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Nez.TextureAtlasGenerator
{
	/// <summary>
	/// Helper for arranging many small sprites into a single larger sheet.
	/// </summary>
	public static class TextureAtlasPacker
	{
		/// <summary>
		/// Packs a list of sprites into a single big texture,
		/// recording where each one was stored.
		/// </summary>
		public static PixelBitmapContent<Color> packSprites( IList<BitmapContent> sourceSprites, ICollection<Rectangle> outputSprites, bool isCompressed, ContentProcessorContext context )
		{
			if( sourceSprites.Count == 0 )
				throw new InvalidContentException( "There are no sprites to pack" );

			// Build up a list of all the sprites needing to be arranged.
			var sprites = new List<ArrangedSprite>();
			for( var i = 0; i < sourceSprites.Count; i++ )
			{
				var sprite = new ArrangedSprite();

				// Include a single pixel padding around each sprite, to avoid filtering problems if the sprite is scaled or rotated.
				sprite.width = sourceSprites[i].Width + 2;
				sprite.height = sourceSprites[i].Height + 2;
				sprite.index = i;

				sprites.Add( sprite );
			}

			// Sort so the largest sprites get arranged first.
			sprites.Sort( compareSpriteSizes );

			// Work out how big the output bitmap should be.
			var outputWidth = guessOutputWidth( sprites );
			var outputHeight = 0;
			var totalSpriteSize = 0;

			// Choose positions for each sprite, one at a time.
			for( var i = 0; i < sprites.Count; i++ )
			{
				positionSprite( sprites, i, outputWidth );

				outputHeight = Math.Max( outputHeight, sprites[i].y + sprites[i].height );
				totalSpriteSize += sprites[i].width * sprites[i].height;
			}

			// DXT compression requires texture sizes to be a multiple of 4
			if( isCompressed )
				outputHeight = (outputHeight + 3) & ~3;

			// sort the sprites back into index order.
			sprites.Sort( compareSpriteIndices );

			context.Logger.LogImportantMessage(
				"\nPacked {0} sprites into a {1}x{2} sheet, {3}% efficiency",
				sprites.Count, outputWidth, outputHeight,
				totalSpriteSize * 100 / outputWidth / outputHeight );

			return copySpritesToOutput( sprites, sourceSprites, outputSprites, outputWidth, outputHeight );
		}


		/// <summary>
		/// Once the arranging is complete, copies the bitmap data for each
		/// sprite to its chosen position in the single larger output bitmap.
		/// </summary>
		static PixelBitmapContent<Color> copySpritesToOutput( List<ArrangedSprite> sprites, IList<BitmapContent> sourceSprites,
		                                               ICollection<Rectangle> outputSprites, int width, int height )
		{
			var output = new PixelBitmapContent<Color>( width, height );

			foreach( var sprite in sprites )
			{
				var source = sourceSprites[sprite.index];

				var x = sprite.x;
				var y = sprite.y;

				var w = source.Width;
				var h = source.Height;

				// Copy the main sprite data to the output sheet.
				BitmapContent.Copy( source, new Rectangle( 0, 0, w, h ), output, new Rectangle( x + 1, y + 1, w, h ) );

				// Copy a border strip from each edge of the sprite, creating
				// a one pixel padding area to avoid filtering problems if the
				// sprite is scaled or rotated.
				BitmapContent.Copy( source, new Rectangle( 0, 0, 1, h ), output, new Rectangle( x, y + 1, 1, h ) );
				BitmapContent.Copy( source, new Rectangle( w - 1, 0, 1, h ), output, new Rectangle( x + w + 1, y + 1, 1, h ) );
				BitmapContent.Copy( source, new Rectangle( 0, 0, w, 1 ), output, new Rectangle( x + 1, y, w, 1 ) );
				BitmapContent.Copy( source, new Rectangle( 0, h - 1, w, 1 ), output, new Rectangle( x + 1, y + h + 1, w, 1 ) );

				// Copy a single pixel from each corner of the sprite, filling in the corners of the one pixel padding area.
				BitmapContent.Copy( source, new Rectangle( 0, 0, 1, 1 ), output, new Rectangle( x, y, 1, 1 ) );
				BitmapContent.Copy( source, new Rectangle( w - 1, 0, 1, 1 ), output, new Rectangle( x + w + 1, y, 1, 1 ) );
				BitmapContent.Copy( source, new Rectangle( 0, h - 1, 1, 1 ), output, new Rectangle( x, y + h + 1, 1, 1 ) );
				BitmapContent.Copy( source, new Rectangle( w - 1, h - 1, 1, 1 ), output, new Rectangle( x + w + 1, y + h + 1, 1, 1 ) );

				// Remember where we placed this sprite.
				outputSprites.Add( new Rectangle( x + 1, y + 1, w, h ) );
			}

			return output;
		}


		/// <summary>
		/// Internal helper class keeps track of a sprite while it is being arranged.
		/// </summary>
		class ArrangedSprite
		{
			public int index;

			public int x;
			public int y;

			public int width;
			public int height;
		}


		/// <summary>
		/// Works out where to position a single sprite.
		/// </summary>
		static void positionSprite( List<ArrangedSprite> sprites, int index, int outputWidth )
		{
			int x = 0;
			int y = 0;

			while( true )
			{
				// Is this position free for us to use?
				var intersects = findIntersectingSprite( sprites, index, x, y );
				if( intersects < 0 )
				{
					sprites[index].x = x;
					sprites[index].y = y;

					return;
				}

				// Skip past the existing sprite that we collided with.
				x = sprites[intersects].x + sprites[intersects].width;

				// If we ran out of room to move to the right,
				// try the next line down instead.
				if( x + sprites[index].width > outputWidth )
				{
					x = 0;
					y++;
				}
			}
		}


		/// <summary>
		/// Checks if a proposed sprite position collides with anything
		/// that we already arranged.
		/// </summary>
		static int findIntersectingSprite( List<ArrangedSprite> sprites, int index, int x, int y )
		{
			var w = sprites[index].width;
			var h = sprites[index].height;

			for( var i = 0; i < index; i++ )
			{
				if( sprites[i].x >= x + w )
					continue;

				if( sprites[i].x + sprites[i].width <= x )
					continue;

				if( sprites[i].y >= y + h )
					continue;

				if( sprites[i].y + sprites[i].height <= y )
					continue;

				return i;
			}

			return -1;
		}


		/// <summary>
		/// Comparison function for sorting sprites by size.
		/// </summary>
		static int compareSpriteSizes( ArrangedSprite a, ArrangedSprite b )
		{
			var aSize = a.height * 1024 + a.width;
			var bSize = b.height * 1024 + b.width;

			return bSize.CompareTo( aSize );
		}


		/// <summary>
		/// Comparison function for sorting sprites by their original indices.
		/// </summary>
		static int compareSpriteIndices( ArrangedSprite a, ArrangedSprite b )
		{
			return a.index.CompareTo( b.index );
		}


		/// <summary>
		/// Heuristic guesses what might be a good output width for a list of sprites.
		/// </summary>
		static int guessOutputWidth( List<ArrangedSprite> sprites )
		{
			// Gather the widths of all our sprites into a temporary list.
			var widths = new List<int>();

			foreach( ArrangedSprite sprite in sprites )
			{
				widths.Add( sprite.width );
			}

			// Sort the widths into ascending order.
			widths.Sort();

			// Extract the maximum and median widths.
			var maxWidth = widths[widths.Count - 1];
			var medianWidth = widths[widths.Count / 2];

			// Heuristic assumes an NxN grid of median sized sprites.
			var width = medianWidth * (int)Math.Round( Math.Sqrt( sprites.Count ) );

			// Make sure we never choose anything smaller than our largest sprite.
			return Math.Max( width, maxWidth );
		}
	
	}
}
