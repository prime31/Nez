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


		public Image(IDrawable drawable, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center)
		{
			SetDrawable(drawable);
			_scaling = scaling;
			_align = align;
			SetSize(PreferredWidth, PreferredHeight);
			touchable = Touchable.Disabled;
		}


		public Image() : this((IDrawable) null)
		{
		}


		public Image(Sprite sprite, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center) : this(
			new SpriteDrawable(sprite), scaling, align)
		{
		}


		public Image(Texture2D texture, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center) : this(
			new Sprite(texture), scaling, align)
		{
		}


		#region Configuration

		public Image SetDrawable(IDrawable drawable)
		{
			if (_drawable != drawable)
			{
				if (drawable != null)
				{
					if (PreferredWidth != drawable.MinWidth || PreferredHeight != drawable.MinHeight)
						InvalidateHierarchy();
				}
				else
				{
					InvalidateHierarchy();
				}

				_drawable = drawable;
			}

			return this;
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public Image SetAlignment(Align alignment)
		{
			_align = (int) alignment;
			return this;
		}


		public Image SetScaling(Scaling scaling)
		{
			_scaling = scaling;
			return this;
		}

		#endregion


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			Validate();

			var col = color * (color.A / 255f);

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

			_drawable?.Draw(batcher, x + imageX, y + imageY, imageWidth, imageHeight, col);
		}


		public override void Layout()
		{
			if (_drawable == null)
				return;

			var regionWidth = _drawable.MinWidth;
			var regionHeight = _drawable.MinHeight;

			var size = _scaling.Apply(regionWidth, regionHeight, width, height);
			imageWidth = size.X;
			imageHeight = size.Y;

			if ((_align & AlignInternal.Left) != 0)
				imageX = 0;
			else if ((_align & AlignInternal.Right) != 0)
				imageX = (int) (width - imageWidth);
			else
				imageX = (int) (width / 2 - imageWidth / 2);

			if ((_align & AlignInternal.Top) != 0)
				imageY = (int) (height - imageHeight);
			else if ((_align & AlignInternal.Bottom) != 0)
				imageY = 0;
			else
				imageY = (int) (height / 2 - imageHeight / 2);
		}


		#region ILayout

		public override float PreferredWidth => _drawable != null ? scaleX * _drawable.MinWidth : 0;

		public override float PreferredHeight => _drawable != null ? scaleY * _drawable.MinHeight : 0;

		#endregion
	}
}
