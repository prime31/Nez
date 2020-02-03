using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	/// <summary>
	/// Drawable for a {@link Sprite}
	/// </summary>
	public class SpriteDrawable : IDrawable
	{
		public Color? TintColor;

		public SpriteEffects SpriteEffects = SpriteEffects.None;

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped horizontally
		/// </summary>
		/// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
		public bool FlipX
		{
			get => (SpriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			set =>
				SpriteEffects = value
					? (SpriteEffects | SpriteEffects.FlipHorizontally)
					: (SpriteEffects & ~SpriteEffects.FlipHorizontally);
		}

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped vertically
		/// </summary>
		/// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
		public bool FlipY
		{
			get => (SpriteEffects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			set =>
				SpriteEffects = value
					? (SpriteEffects | SpriteEffects.FlipVertically)
					: (SpriteEffects & ~SpriteEffects.FlipVertically);
		}

		public Sprite Sprite
		{
			get => _sprite;
			set
			{
				_sprite = value;
				MinWidth = Sprite.SourceRect.Width;
				MinHeight = Sprite.SourceRect.Height;
			}
		}

		protected Sprite _sprite;


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


		public SpriteDrawable(Sprite sprite)
		{
			Sprite = sprite;
		}

		public SpriteDrawable(Texture2D texture) : this(new Sprite(texture))
		{ }

		public virtual void Draw(Batcher batcher, float x, float y, float width, float height, Color color)
		{
			if (TintColor.HasValue)
				color = color.Multiply(TintColor.Value);

			batcher.Draw(Sprite, new Rectangle((int) x, (int) y, (int) width, (int) height),
				Sprite.SourceRect, color, SpriteEffects);
		}

		/// <summary>
		/// returns a new drawable with the tint color specified
		/// </summary>
		/// <returns>The tinted drawable.</returns>
		/// <param name="tint">Tint.</param>
		public SpriteDrawable NewTintedDrawable(Color tint)
		{
			return new SpriteDrawable(Sprite)
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