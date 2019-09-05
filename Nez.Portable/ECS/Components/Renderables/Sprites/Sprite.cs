using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	/// <summary>
	/// the most basic and common Renderable. Renders a Subtexture/Texture.
	/// </summary>
	public class Sprite : RenderableComponent
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					if (Subtexture != null)
						_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin,
							Entity.Transform.Scale, Entity.Transform.Rotation, Subtexture.SourceRect.Width,
							Subtexture.SourceRect.Height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// the origin of the Sprite. This is set automatically when setting a Subtexture.
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
		/// Batchers passed along to the Batcher when rendering. flipX/flipY are helpers for setting this.
		/// </summary>
		public SpriteEffects SpriteEffects = SpriteEffects.None;

		/// <summary>
		/// the Subtexture that should be displayed by this Sprite. When set, the origin of the Sprite is also set to match Subtexture.origin.
		/// </summary>
		/// <value>The subtexture.</value>
		public Subtexture Subtexture
		{
			get => _subtexture;
			set => SetSubtexture(value);
		}

		protected Vector2 _origin;
		protected Subtexture _subtexture;


		public Sprite()
		{
		}

		public Sprite(Subtexture subtexture)
		{
			_subtexture = subtexture;
			_origin = subtexture.Center;
		}

		public Sprite(Texture2D texture) : this(new Subtexture(texture))
		{
		}


		#region fluent setters

		/// <summary>
		/// sets the Subtexture and updates the origin of the Sprite to match Subtexture.origin. If for whatever reason you need
		/// an origin different from the Subtexture either clone it or set the origin AFTER setting the Subtexture here.
		/// </summary>
		/// <returns>The subtexture.</returns>
		/// <param name="subtexture">Subtexture.</param>
		public Sprite SetSubtexture(Subtexture subtexture)
		{
			_subtexture = subtexture;

			if (_subtexture != null)
				_origin = subtexture.Origin;
			return this;
		}

		/// <summary>
		/// sets the origin for the Renderable
		/// </summary>
		/// <returns>The origin.</returns>
		/// <param name="origin">Origin.</param>
		public Sprite SetOrigin(Vector2 origin)
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
		/// <returns>The origin normalized.</returns>
		/// <param name="origin">Origin.</param>
		public Sprite SetOriginNormalized(Vector2 value)
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
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		/// <param name="offset">Offset.</param>
		public void DrawOutline(Graphics graphics, Camera camera, int offset = 1)
		{
			DrawOutline(graphics, camera, Color.Black, offset);
		}

		public void DrawOutline(Graphics graphics, Camera camera, Color outlineColor, int offset = 1)
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
						Render(graphics, camera);
					}
				}
			}

			// restore changed state
			_localOffset = originalPosition;
			Color = originalColor;
			_layerDepth = originalLayerDepth;
		}

		public override void Render(Graphics graphics, Camera camera)
		{
			graphics.Batcher.Draw(_subtexture, Entity.Transform.Position + LocalOffset, Color,
				Entity.Transform.Rotation, Origin, Entity.Transform.Scale, SpriteEffects, _layerDepth);
		}
	}
}