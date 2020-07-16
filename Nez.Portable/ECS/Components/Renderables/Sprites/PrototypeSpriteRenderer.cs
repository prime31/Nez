using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;


namespace Nez
{
	/// <summary>
	/// skewable rectangle sprite for prototyping
	/// </summary>
	public class PrototypeSpriteRenderer : SpriteRenderer
	{
		public override float Width => _width;
		public override float Height => _height;

		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin, Entity.Transform.Scale,
						Entity.Transform.Rotation, _width, _height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public float SkewTopX;
		public float SkewBottomX;
		public float SkewLeftY;
		public float SkewRightY;

		float _width, _height;


		public PrototypeSpriteRenderer() : this(50, 50)
		{ }

		public PrototypeSpriteRenderer(float width, float height) : base(Graphics.Instance.PixelTexture)
		{
			_width = width;
			_height = height;
		}

		/// <summary>
		/// sets the width of the sprite
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="width">Width.</param>
		public PrototypeSpriteRenderer SetWidth(float width)
		{
			_width = width;
			return this;
		}

		/// <summary>
		/// sets the height of the sprite
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="height">Height.</param>
		public PrototypeSpriteRenderer SetHeight(float height)
		{
			_height = height;
			return this;
		}

		/// <summary>
		/// sets the skew values for the sprite
		/// </summary>
		/// <returns>The skew.</returns>
		/// <param name="skewTopX">Skew top x.</param>
		/// <param name="skewBottomX">Skew bottom x.</param>
		/// <param name="skewLeftY">Skew left y.</param>
		/// <param name="skewRightY">Skew right y.</param>
		public PrototypeSpriteRenderer SetSkew(float skewTopX, float skewBottomX, float skewLeftY, float skewRightY)
		{
			SkewTopX = skewTopX;
			SkewBottomX = skewBottomX;
			SkewLeftY = skewLeftY;
			SkewRightY = skewRightY;
			return this;
		}

		public override void OnAddedToEntity()
		{
			OriginNormalized = Vector2Ext.HalfVector();
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			var pos = (Entity.Transform.Position - (Origin * Entity.Transform.Scale) + LocalOffset);
			var size = new Point((int) (_width * Entity.Transform.Scale.X), (int) (_height * Entity.Transform.Scale.Y));
			var destRect = new Rectangle((int) pos.X, (int) pos.Y, size.X, size.Y);
			batcher.Draw(_sprite, destRect, _sprite.SourceRect, Color, Entity.Transform.Rotation,
				SpriteEffects.None, LayerDepth, SkewTopX, SkewBottomX, SkewLeftY, SkewRightY);
		}
	}
}