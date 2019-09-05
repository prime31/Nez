using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	/// <summary>
	/// Drawable for a {@link Subtexture}
	/// </summary>
	public class SubtextureDrawable : IDrawable
	{
		public Color? TintColor;

		public SpriteEffects SpriteEffects = SpriteEffects.None;

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped horizontally
		/// </summary>
		/// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
		public bool FlipX
		{
			get { return (SpriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally; }
			set
			{
				SpriteEffects = value
					? (SpriteEffects | SpriteEffects.FlipHorizontally)
					: (SpriteEffects & ~SpriteEffects.FlipHorizontally);
			}
		}

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped vertically
		/// </summary>
		/// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
		public bool FlipY
		{
			get { return (SpriteEffects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically; }
			set
			{
				SpriteEffects = value
					? (SpriteEffects | SpriteEffects.FlipVertically)
					: (SpriteEffects & ~SpriteEffects.FlipVertically);
			}
		}

		public Subtexture Subtexture
		{
			get { return _subtexture; }
			set
			{
				_subtexture = value;
				MinWidth = _subtexture.SourceRect.Width;
				MinHeight = _subtexture.SourceRect.Height;
			}
		}

		protected Subtexture _subtexture;


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


		public SubtextureDrawable(Subtexture subtexture)
		{
			this.Subtexture = subtexture;
		}


		public SubtextureDrawable(Texture2D texture) : this(new Subtexture(texture))
		{
		}


		public virtual void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
		{
			if (TintColor.HasValue)
				color = color.Multiply(TintColor.Value);

			graphics.Batcher.Draw(_subtexture, new Rectangle((int) x, (int) y, (int) width, (int) height),
				_subtexture.SourceRect, color, SpriteEffects);
		}


		/// <summary>
		/// returns a new drawable with the tint color specified
		/// </summary>
		/// <returns>The tinted drawable.</returns>
		/// <param name="tint">Tint.</param>
		public SubtextureDrawable NewTintedDrawable(Color tint)
		{
			return new SubtextureDrawable(_subtexture)
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