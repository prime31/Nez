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
		public readonly Rectangle sourceRect;

		/// <summary>
		/// center of the sourceRect if it had a 0,0 origin. This is basically the center in sourceRect-space.
		/// </summary>
		/// <value>The center.</value>
		public readonly Vector2 center;
		

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
		/// generates nine patch Rectangles. destArray should have 9 elements. renderRect is the final area in which the nine patch will be rendered.
		/// To just get the source rects for rendering pass in the Subtexture.sourceRect. Pass in a larger Rectangle to get final destination
		/// rendering Rectangles.
		/// </summary>
		/// <param name="renderRect">Render rect.</param>
		/// <param name="destArray">Destination array.</param>
		/// <param name="marginTop">Margin top.</param>
		/// <param name="marginBottom">Margin bottom.</param>
		/// <param name="marginLeft">Margin left.</param>
		/// <param name="marginRight">Margin right.</param>
		public void generateNinePatchRects( Rectangle renderRect, Rectangle[] destArray, int marginLeft, int marginRight, int marginTop, int marginBottom )
		{
			Assert.isTrue( destArray.Length == 9, "destArray does not have a length of 9" );

			var stretchedCenterWidth = renderRect.Width - marginLeft - marginRight;
			var stretchedCenterHeight = renderRect.Height - marginTop - marginBottom;
			var bottomY = renderRect.Y + renderRect.Height - marginBottom;
			var rightX = renderRect.X + renderRect.Width - marginRight;
			var leftX = renderRect.X + marginLeft;
			var topY = renderRect.Y + marginTop;

			destArray[0] = new Rectangle( renderRect.X, renderRect.Y, marginLeft, marginTop ); // top-left
			destArray[1] = new Rectangle( leftX, renderRect.Y, stretchedCenterWidth, marginTop ); // top-center
			destArray[2] = new Rectangle( rightX, renderRect.Y, marginRight, marginTop ); // top-right

			destArray[3] = new Rectangle( renderRect.X, topY, marginLeft, stretchedCenterHeight ); // middle-left
			destArray[4] = new Rectangle( leftX, topY, stretchedCenterWidth, stretchedCenterHeight ); // middle-center
			destArray[5] = new Rectangle( rightX, topY, marginRight, stretchedCenterHeight); // middle-right

			destArray[6] = new Rectangle( renderRect.X, bottomY, marginLeft, marginBottom ); // bottom-left
			destArray[7] = new Rectangle( leftX, bottomY, stretchedCenterWidth, marginBottom ); // bottom-center
			destArray[8] = new Rectangle( rightX, bottomY, marginRight, marginBottom ); // bottom-right
		}


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
