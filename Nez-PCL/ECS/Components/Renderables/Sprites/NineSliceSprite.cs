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
					_bounds.location = entity.transform.position + _localPosition;
					_bounds.width = width;
					_bounds.height = height;
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		int marginLeft = 19;
		int marginRight = 19;
		int marginTop = 19;
		int marginBottom = 19;

		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;
		Rectangle[] _sourceRects = new Rectangle[9];
		Rectangle[] _destRects = new Rectangle[9];
		bool _destRectsDirty = true;


		public NineSliceSprite( Subtexture subtexture, int top, int bottom, int left, int right ) : base( subtexture )
		{
			marginLeft = left;
			marginRight = right;
			marginTop = top;
			marginBottom = bottom;
			subtexture.generateNinePatchRects( subtexture.sourceRect, _sourceRects, marginTop, marginBottom, marginLeft, marginRight );

			width = subtexture.sourceRect.Width + 150;
			height = subtexture.sourceRect.Height + 50;
		}


		public NineSliceSprite( Texture2D texture, int top, int bottom, int left, int right ) : this( new Subtexture( texture ), top, bottom, left, right )
		{}


		public override void render( Graphics graphics, Camera camera )
		{
			if( isVisibleFromCamera( camera ) )
			{
				if( _destRectsDirty )
				{
					subtexture.generateNinePatchRects( _finalRenderRect, _destRects, marginTop, marginBottom, marginRight, marginLeft );
					_destRectsDirty = false;
				}

				var pos = ( entity.transform.position + _localPosition ).ToPoint();

				for( var i = 0; i < 9; i++ )
				{
					// shift our destination rect over to our position
					var dest = _destRects[i];
					dest.X += pos.X;
					dest.Y += pos.Y;
					graphics.spriteBatch.Draw( subtexture, dest, _sourceRects[i], color );
				}
			}
		}
	}
}

