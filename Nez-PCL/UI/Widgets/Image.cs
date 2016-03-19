using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.UI
{
	public class Image : Element
	{
		Scaling _scaling;
		int _align;

		IDrawable _drawable;
		float imageX, imageY, imageWidth, imageHeight;


		public Image( IDrawable drawable, Scaling scaling = Scaling.Stretch, int align = AlignInternal.center )
		{
			setDrawable( drawable );
			_scaling = scaling;
			_align = align;
			setSize( preferredWidth, preferredHeight );
			touchable = Touchable.Disabled;
		}


		public Image() : this( (IDrawable)null )
		{}


		public Image( Subtexture subtexture, Scaling scaling = Scaling.Stretch, int align = AlignInternal.center ) : this( new SubtextureDrawable( subtexture ), scaling, align )
		{
		}


		public Image( Texture2D texture, Scaling scaling = Scaling.Stretch, int align = AlignInternal.center ) : this( new Subtexture( texture ), scaling, align )
		{
		}


		#region Configuration

		public Image setDrawable( IDrawable drawable )
		{
			if( _drawable != drawable )
			{
				if( _drawable != null )
				{
					if( preferredWidth != drawable.minWidth || preferredHeight != drawable.minHeight )
						invalidateHierarchy();
				}
				else
				{
					invalidateHierarchy();
				}
				_drawable = drawable;
			}

			return this;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public Image setAlignment( Align alignment )
		{
			_align = (int)alignment;
			return this;
		}


		public Image setScaling( Scaling scaling )
		{
			_scaling = scaling;
			return this;
		}

		#endregion


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();

			var col = new Color( color.R, color.G, color.B, color.A * parentAlpha );

//			if( drawable instanceof TransformDrawable )
//			{
//				float rotation = getRotation();
//				if (scaleX != 1 || scaleY != 1 || rotation != 0)
//				{
//					((TransformDrawable)drawable).draw(batch, x + imageX, y + imageY, getOriginX() - imageX, getOriginY() - imageY,
//						imageWidth, imageHeight, scaleX, scaleY, rotation);
//					return;
//				}
//			}

			if( _drawable != null )
				_drawable.draw( graphics, x + imageX, y + imageY, imageWidth * scaleX, imageHeight * scaleY, col );
		}


		public override void layout()
		{
			if( _drawable == null )
				return;
			
			var regionWidth = _drawable.minWidth;
			var regionHeight = _drawable.minHeight;

			var size = _scaling.apply( regionWidth, regionHeight, width, height );
			imageWidth = size.X;
			imageHeight = size.Y;

			if( ( _align & AlignInternal.left ) != 0 )
				imageX = 0;
			else if( ( _align & AlignInternal.right ) != 0 )
				imageX = (int)( width - imageWidth );
			else
				imageX = (int)( width / 2 - imageWidth / 2 );

			if( ( _align & AlignInternal.top ) != 0 )
				imageY = (int)( height - imageHeight );
			else if( ( _align & AlignInternal.bottom ) != 0 )
				imageY = 0;
			else
				imageY = (int)( height / 2 - imageHeight / 2 );
		}


		#region ILayout

		public override float preferredWidth
		{
			get { return _drawable != null ? _drawable.minWidth : 0; }
		}

		public override float preferredHeight
		{
			get { return _drawable != null ? _drawable.minHeight : 0; }
		}

		#endregion

	}
}

