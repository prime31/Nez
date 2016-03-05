using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace Nez.Textures
{
	/// <summary>
	/// represents a single element in a texture atlas consisting of a texture and the source rectangle for the frame
	/// </summary>
	public class Subtexture
	{
		/// <summary>
		/// the actual Texture2D
		/// </summary>
		public Texture2D texture2D;

		/// <summary>
		/// rectangle in the Texture2D for this element
		/// </summary>
		public Rectangle sourceRect;

		/// <summary>
		/// center of the sourceRect if it had a 0,0 origin. This is basically the center in sourceRect-space.
		/// </summary>
		/// <value>The center.</value>
		public Vector2 center;
		

		public Subtexture( Texture2D texture, Rectangle sourceRect )
		{
			this.texture2D = texture;
			this.sourceRect = sourceRect;
			center = new Vector2( sourceRect.Width * 0.5f, sourceRect.Height * 0.5f );
		}


		public Subtexture( Texture2D texture ) : this( texture, new Rectangle( 0, 0, texture.Width, texture.Height ) )
		{}


		public Subtexture( Texture2D texture, int x, int y, int width, int height ) : this( texture, new Rectangle( x, y, width, height ) )
		{}


		/// <summary>
		/// convenience constructor that casts floats to ints for the sourceRect
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Subtexture( Texture2D texture, float x, float y, float width, float height ) : this( texture, (int)x, (int)y, (int)width, (int)height )
		{}


		/// <summary>
		/// provides a List of subtextures given an atlas with equally spaced rows/columns of sprites
		/// </summary>
		/// <returns>The from atlas.</returns>
		/// <param name="texture">Texture.</param>
		/// <param name="cellWidth">Cell width.</param>
		/// <param name="cellHeight">Cell height.</param>
		/// <param name="cellOffset">the first cell to include while processing. 0 based indexing.</param>
		/// <param name="maxCellsToInclude">Max cells to included.</param>
		public static List<Subtexture> subtexturesFromAtlas( Texture2D texture, int cellWidth, int cellHeight, int cellOffset = 0, int maxCellsToInclude = int.MaxValue )
		{
			var subtextures = new List<Subtexture>();

			var cols = texture.Width / cellWidth;
			var rows = texture.Height / cellHeight;
			var i = 0;

			for( var y = 0; y < rows; y++ )
			{
				for( var x = 0; x < cols; x++ )
				{
					// skip everything before the first cellOffset
					if( i++ < cellOffset )
						continue;
					
					subtextures.Add( new Subtexture( texture, new Rectangle( x * cellWidth, y * cellHeight, cellWidth, cellHeight ) ) );

					// once we hit the max number of cells to include bail out. were done.
					if( subtextures.Count == maxCellsToInclude )
						break;
				}
			}

			return subtextures;
		}


		public static implicit operator Texture2D( Subtexture tex )
		{
			return tex.texture2D;
		}


		public override string ToString()
		{
			return string.Format( "{0}", sourceRect );
		}
	
	}
}
