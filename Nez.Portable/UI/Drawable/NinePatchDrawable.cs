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

		public float LeftWidth { get; set; }
		public float RightWidth { get; set; }
		public float TopHeight { get; set; }
		public float BottomHeight { get; set; }
		public float MinWidth { get; set; }
		public float MinHeight { get; set; }


		public void SetPadding(float top, float bottom, float left, float right)
		{
			TopHeight = top;
			BottomHeight = bottom;
			LeftWidth = left;
			RightWidth = right;
		}

		#endregion

		public Color? TintColor;

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

		NinePatchSprite _sprite;


		public NinePatchDrawable(NinePatchSprite sprite)
		{
			_sprite = sprite;
			MinWidth = _sprite.NinePatchRects[MIDDLE_LEFT].Width + _sprite.NinePatchRects[MIDDLE_CENTER].Width +
			           _sprite.NinePatchRects[MIDDLE_RIGHT].Width;
			MinHeight = _sprite.NinePatchRects[TOP_CENTER].Height +
			            _sprite.NinePatchRects[MIDDLE_CENTER].Height +
			            _sprite.NinePatchRects[BOTTOM_CENTER].Height;

			// by default, if padding isn't given, we will pad the content by the nine patch margins
			if (_sprite.HasPadding)
			{
				LeftWidth = _sprite.PadLeft;
				RightWidth = _sprite.PadRight;
				TopHeight = _sprite.PadTop;
				BottomHeight = _sprite.PadBottom;
			}
			else
			{
				LeftWidth = _sprite.Left;
				RightWidth = _sprite.Right;
				TopHeight = _sprite.Top;
				BottomHeight = _sprite.Bottom;
			}
		}


		/// <summary>
		/// creates a NinePatchDrawable using the full texture
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		/// <param name="top">Top.</param>
		/// <param name="bottom">Bottom.</param>
		public NinePatchDrawable(Texture2D texture, int left, int right, int top, int bottom) : this(
			new NinePatchSprite(texture, left, right, top, bottom))
		{ }


		public NinePatchDrawable(Sprite sprite, int left, int right, int top, int bottom) : this(
			new NinePatchSprite(sprite.Texture2D, sprite.SourceRect, left, right, top, bottom))
		{ }


		/// <summary>
		/// sets the padding on the NinePatchSprite
		/// </summary>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		/// <param name="top">Top.</param>
		/// <param name="bottom">Bottom.</param>
		public void SetPadding(int left, int right, int top, int bottom)
		{
			_sprite.Left = left;
			_sprite.Right = right;
			_sprite.Top = top;
			_sprite.Bottom = bottom;
		}


		public void Draw(Batcher batcher, float x, float y, float width, float height, Color color)
		{
			if (TintColor.HasValue)
				color = color.Multiply(TintColor.Value);

			if (_finalRenderRect.Height != height || _finalRenderRect.Width != width)
			{
				_finalRenderRect.Height = (int) height;
				_finalRenderRect.Width = (int) width;
				_sprite.GenerateNinePatchRects(_finalRenderRect, _destRects, _sprite.Left, _sprite.Right,
					_sprite.Top, _sprite.Bottom);
			}

			for (var i = 0; i < 9; i++)
			{
				// only draw if we have width/height to draw.
				if (_destRects[i].Width == 0 || _destRects[i].Height == 0)
					continue;

				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += (int) x;
				dest.Y += (int) y;
				batcher.Draw(_sprite, dest, _sprite.NinePatchRects[i], color);
			}
		}


		/// <summary>
		/// returns a new drawable with the tint color specified
		/// </summary>
		/// <returns>The tinted drawable.</returns>
		/// <param name="tint">Tint.</param>
		public NinePatchDrawable NewTintedDrawable(Color tint)
		{
			return new NinePatchDrawable(_sprite)
			{
				LeftWidth = LeftWidth,
				RightWidth = RightWidth,
				TopHeight = TopHeight,
				BottomHeight = BottomHeight,
				MinWidth = MinWidth,
				MinHeight = MinHeight,
				TintColor = tint
			};
		}
	}
}