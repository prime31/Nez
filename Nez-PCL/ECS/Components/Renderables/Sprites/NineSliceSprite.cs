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
				_finalRenderRectDirty = true;
			}
		}

		public new float height
		{
			get { return _finalRenderRect.Height; }
			set
			{
				_finalRenderRect.Height = (int)value;
				_finalRenderRectDirty = true;
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

		public int marginLeft = 19;
		public int marginRight = 19;
		public int marginTop = 19;
		public int marginBottom = 19;

		Rectangle[] _sourceRects = new Rectangle[9];
		Rectangle[] _destRects = new Rectangle[9];
		Rectangle _finalRenderRect;
		bool _finalRenderRectDirty = true;


		public NineSliceSprite( Subtexture subtexture ) : base( subtexture )
		{
			generateRects( subtexture.sourceRect, _sourceRects );

			width = subtexture.sourceRect.Width + 150;
			height = subtexture.sourceRect.Height + 50;
		}


		public NineSliceSprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		void generateRects( Rectangle rect, Rectangle[] array )
		{
			var stretchedCenterWidth = rect.Width - marginLeft - marginRight;
			var stretchedCenterHeight = rect.Height - marginTop - marginBottom;
			var bottomY = rect.Y + rect.Height - marginBottom;
			var rightX = rect.X + rect.Width - marginRight;
			var leftX = rect.X + marginLeft;
			var topY = rect.Y + marginTop;

			array[0] = new Rectangle( rect.X, rect.Y, marginLeft, marginTop ); // top-left
			array[1] = new Rectangle( leftX, rect.Y, stretchedCenterWidth, marginTop ); // top-center
			array[2] = new Rectangle( rightX, rect.Y, marginRight, marginTop ); // top-right

			array[3] = new Rectangle( rect.X, topY, marginLeft, stretchedCenterHeight ); // middle-left
			array[4] = new Rectangle( leftX, topY, stretchedCenterWidth, stretchedCenterHeight ); // middle-center
			array[5] = new Rectangle( rightX, topY, marginRight, stretchedCenterHeight); // middle-right

			array[6] = new Rectangle( rect.X, bottomY, marginLeft, marginBottom ); // bottom-left
			array[7] = new Rectangle( leftX, bottomY, stretchedCenterWidth, marginBottom ); // bottom-center
			array[8] = new Rectangle( rightX, bottomY, marginRight, marginBottom ); // bottom-right
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( isVisibleFromCamera( camera ) )
			{
				if( _finalRenderRectDirty )
				{
					generateRects( _finalRenderRect, _destRects );
					_finalRenderRectDirty = false;
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

