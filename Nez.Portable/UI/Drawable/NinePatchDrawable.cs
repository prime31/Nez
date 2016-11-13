using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	/// <summary>
	/// The drawable sizes are set when the ninepatch is set, but they are separate values. Eg, {@link Drawable#getLeftWidth()} could
	/// be set to more than {@link NinePatch#getLeftWidth()} in order to provide more space on the left than actually exists in the
	/// ninepatch.
	/// 
	/// The min size is set to the ninepatch total size by default. It could be set to the left+right and top+bottom, excluding the
	/// middle size, to allow the drawable to be sized down as small as possible.
	/// </summary>
	public class NinePatchDrawable : IDrawable
	{
		#region IDrawable implementation

		public float leftWidth { get; set; }
		public float rightWidth { get; set; }
		public float topHeight { get; set; }
		public float bottomHeight { get; set; }
		public float minWidth { get; set; }
		public float minHeight { get; set; }


		public void setPadding( float top, float bottom, float left, float right )
		{
			topHeight = top;
			bottomHeight = bottom;
			leftWidth = left;
			rightWidth = right;
		}

		#endregion

		public Color? tintColor;

		const int TOP_LEFT = 0;
		const int TOP_CENTER = 1;
		const int TOP_RIGHT = 2;
		const int MIDDLE_LEFT = 3;
		const int MIDDLE_CENTER = 4;
		const int MIDDLE_RIGHT = 5;
		const int BOTTOM_LEFT = 6;
		const int BOTTOM_CENTER = 7;
		const int BOTTOM_RIGHT = 8;

		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;
		Rectangle[] _destRects = new Rectangle[9];

		NinePatchSubtexture _subtexture;


		public NinePatchDrawable( NinePatchSubtexture subtexture )
		{
			_subtexture = subtexture;
			minWidth = _subtexture.ninePatchRects[MIDDLE_LEFT].Width + _subtexture.ninePatchRects[MIDDLE_CENTER].Width + _subtexture.ninePatchRects[MIDDLE_RIGHT].Width;
			minHeight = _subtexture.ninePatchRects[TOP_CENTER].Height + _subtexture.ninePatchRects[MIDDLE_CENTER].Height + _subtexture.ninePatchRects[BOTTOM_CENTER].Height;

			// by default, we will pad the content by the nine patch margins
			leftWidth = _subtexture.left;
			rightWidth = _subtexture.right;
			topHeight = _subtexture.top;
			bottomHeight = _subtexture.bottom;
		}


		/// <summary>
		/// creates a NinePatchDrawable using the full texture
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		/// <param name="top">Top.</param>
		/// <param name="bottom">Bottom.</param>
		public NinePatchDrawable( Texture2D texture, int left, int right, int top, int bottom ) : this( new NinePatchSubtexture( texture, left, right, top, bottom ) )
		{}


		public NinePatchDrawable( Subtexture subtexture, int left, int right, int top, int bottom ) : this( new NinePatchSubtexture( subtexture.texture2D, subtexture.sourceRect, left, right, top, bottom ) )
		{}


		/// <summary>
		/// sets the padding on the NinePatchSubtexture
		/// </summary>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		/// <param name="top">Top.</param>
		/// <param name="bottom">Bottom.</param>
		public void setPadding( int left, int right, int top, int bottom )
		{
			_subtexture.left = left;
			_subtexture.right = right;
			_subtexture.top = top;
			_subtexture.bottom = bottom;
		}


		public void draw( Graphics graphics, float x, float y, float width, float height, Color color )
		{
			if( tintColor.HasValue )
				color = color.multiply( tintColor.Value );
			
			if( _finalRenderRect.Height != height || _finalRenderRect.Width != width )
			{
				_finalRenderRect.Height = (int)height;
				_finalRenderRect.Width = (int)width;
				_subtexture.generateNinePatchRects( _finalRenderRect, _destRects, _subtexture.left, _subtexture.right, _subtexture.top, _subtexture.bottom );
			}

			for( var i = 0; i < 9; i++ )
			{
				// only draw if we have width/height to draw.
				if( _destRects[i].Width == 0 || _destRects[i].Height == 0 )
					continue;
				
				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += (int)x;
				dest.Y += (int)y;
				graphics.batcher.draw( _subtexture, dest, _subtexture.ninePatchRects[i], color );
			}
		}


		/// <summary>
		/// returns a new drawable with the tint color specified
		/// </summary>
		/// <returns>The tinted drawable.</returns>
		/// <param name="tint">Tint.</param>
		public NinePatchDrawable newTintedDrawable( Color tint )
		{
			return new NinePatchDrawable( _subtexture ) {
				leftWidth = leftWidth,
				rightWidth = rightWidth,
				topHeight = topHeight,
				bottomHeight = bottomHeight,
				minWidth = minWidth,
				minHeight = minHeight,
				tintColor = tint
			};
		}

	}
}

