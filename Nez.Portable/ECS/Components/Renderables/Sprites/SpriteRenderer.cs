using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	/// <summary>
	/// the most basic and common Renderable. Renders a Sprite/Texture.
	/// </summary>
	public class SpriteRenderer : RenderableComponent
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					if (_sprite != null)
						_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin,
							Entity.Transform.Scale, Entity.Transform.Rotation, _sprite.SourceRect.Width,
							_sprite.SourceRect.Height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// the origin of the Sprite. This is set automatically when setting a Sprite.
		/// </summary>
		/// <value>The origin.</value>
		public Vector2 Origin
		{
			get => _origin;
			set => SetOrigin(value);
		}

		/// <summary>
		/// helper property for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 OriginNormalized
		{
			get => new Vector2(_origin.X / Width * Entity.Transform.Scale.X,
				_origin.Y / Height * Entity.Transform.Scale.Y);
			set => SetOrigin(new Vector2(value.X * Width / Entity.Transform.Scale.X,
				value.Y * Height / Entity.Transform.Scale.Y));
		}

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped horizontally
		/// </summary>
		/// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
		public bool FlipX
		{
			get => (SpriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			set => SpriteEffects = value
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
			set => SpriteEffects = value
				? (SpriteEffects | SpriteEffects.FlipVertically)
				: (SpriteEffects & ~SpriteEffects.FlipVertically);
		}

		/// <summary>
		/// Set the FlipX but also adjust the LocalOffset to account for the flip.  
		/// This multiplies the x value of your LocalOffset by -1 so the sprite will appear in the expected place relative to your entity 
		/// </summary>
		/// <param name="isFlippedX"></param>
		public void SetFlipXAndAdjustLocalOffset(bool isFlippedX)
		{
			if (FlipX == isFlippedX)
			{
				return;
			}

			FlipX = isFlippedX;
			LocalOffset *= new Vector2(-1, 1);
		}

		/// <summary>
		///Set the FlipY but also adjust the LocalOffset to account for the flip.  
		/// This multiplies the y value of your LocalOffset by -1 so the sprite will appear in the expected place relative to your entity 
		/// </summary>
		/// <param name="isFlippedY"></param>
		public void SetFlipYAndAdjustLocalOffset(bool isFlippedY)
		{
			if (FlipY == isFlippedY)
			{
				return;
			}

			FlipX = isFlippedY;
			LocalOffset *= new Vector2(1, -1);
		}

		/// <summary>
		/// Batchers passed along to the Batcher when rendering. flipX/flipY are helpers for setting this.
		/// </summary>
		public SpriteEffects SpriteEffects = SpriteEffects.None;

		/// <summary>
		/// the Sprite that should be displayed by this Sprite. When set, the origin of the Sprite is also set to match Sprite.origin.
		/// </summary>
		/// <value>The sprite.</value>
		public Sprite Sprite
		{
			get => _sprite;
			set => SetSprite(value);
		}

		protected Vector2 _origin;
		protected Sprite _sprite;


		public SpriteRenderer()
		{ }

		public SpriteRenderer(Texture2D texture) : this(new Sprite(texture))
		{ }

		public SpriteRenderer(Sprite sprite) => SetSprite(sprite);

		#region fluent setters

		/// <summary>
		/// sets the Sprite and updates the origin of the Sprite to match Sprite.origin. If for whatever reason you need
		/// an origin different from the Sprite either clone it or set the origin AFTER setting the Sprite here.
		/// </summary>
		public SpriteRenderer SetSprite(Sprite sprite)
		{
			_sprite = sprite;
			if (_sprite != null)
				SetOrigin(_sprite.Origin); // set origin with setting _areBoundsDirty
			return this;
		}

		/// <summary>
		/// sets the Texture by creating a new sprite. See SetSprite() for details.
		/// </summary>
		public SpriteRenderer SetTexture(Texture2D texture)
		{
			SetSprite(new Sprite(texture));
			return this;
		}

		/// <summary>
		/// sets the origin for the Renderable
		/// </summary>
		public SpriteRenderer SetOrigin(Vector2 origin)
		{
			if (_origin != origin)
			{
				_origin = origin;
				_areBoundsDirty = true;
			}

			return this;
		}

		/// <summary>
		/// helper for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		public SpriteRenderer SetOriginNormalized(Vector2 value)
		{
			SetOrigin(new Vector2(value.X * Width / Entity.Transform.Scale.X,
				value.Y * Height / Entity.Transform.Scale.Y));
			return this;
		}

		#endregion


		/// <summary>
		/// Draws the Renderable with an outline. Note that this should be called on disabled Renderables since they shouldnt take part in default
		/// rendering if they need an ouline.
		/// </summary>
		public void DrawOutline(Batcher batcher, Camera camera, int offset = 1) => DrawOutline(batcher, camera, Color.Black, offset);

		public void DrawOutline(Batcher batcher, Camera camera, Color outlineColor, int offset = 1)
		{
			// save the stuff we are going to modify so we can restore it later
			var originalPosition = _localOffset;
			var originalColor = Color;
			var originalLayerDepth = _layerDepth;

			// set our new values
			Color = outlineColor;
			_layerDepth += 0.01f;

			for (var i = -1; i < 2; i++)
			{
				for (var j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						_localOffset = originalPosition + new Vector2(i * offset, j * offset);
						Render(batcher, camera);
					}
				}
			}

			// restore changed state
			_localOffset = originalPosition;
			Color = originalColor;
			_layerDepth = originalLayerDepth;
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			batcher.Draw(Sprite, Entity.Transform.Position + LocalOffset, Color,
				Entity.Transform.Rotation, Origin, Entity.Transform.Scale, SpriteEffects, _layerDepth);
		}
	}
}