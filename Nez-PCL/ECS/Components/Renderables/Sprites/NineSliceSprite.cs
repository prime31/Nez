using System;
using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NineSliceSprite : Sprite
	{
		public new float width
		{
			get { return _finalRenderRect.Width; }
			set
			{
				_finalRenderRect.Width = (int)value;
				_destRectsDirty = true;
			}
		}

		public new float height
		{
			get { return _finalRenderRect.Height; }
			set
			{
				_finalRenderRect.Height = (int)value;
				_destRectsDirty = true;
			}
		}

		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.location = entity.transform.position + _localOffset;
					_bounds.width = width;
					_bounds.height = height;
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public new NinePatchSubtexture subtexture;


		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;
		Rectangle[] _destRects = new Rectangle[9];
		bool _destRectsDirty = true;


		public NineSliceSprite( NinePatchSubtexture subtexture ) : base( subtexture )
		{
			this.subtexture = subtexture;
		}


		public NineSliceSprite( Subtexture subtexture, int top, int bottom, int left, int right ) : this( new NinePatchSubtexture( subtexture, top, bottom, left, right ) )
		{}


		public NineSliceSprite( Texture2D texture, int top, int bottom, int left, int right ) : this( new NinePatchSubtexture( texture, top, bottom, left, right ) )
		{}


		public override void render( Graphics graphics, Camera camera )
		{
			if( _destRectsDirty )
			{
				subtexture.generateNinePatchRects( _finalRenderRect, _destRects, subtexture.top, subtexture.bottom, subtexture.right, subtexture.left );
				_destRectsDirty = false;
			}

			var pos = ( entity.transform.position + _localOffset ).ToPoint();

			for( var i = 0; i < 9; i++ )
			{
				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += pos.X;
				dest.Y += pos.Y;
				graphics.batcher.draw( subtexture, dest, subtexture.ninePatchRects[i], color );
			}
		}
	}
}

