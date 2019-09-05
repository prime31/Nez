using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Tiled sprite. Note that TiledSprite overrides the Material so that it can wrap the UVs. This class requires the texture
	/// to not be part of an atlas so that wrapping can work.
	/// </summary>
	public class TiledSprite : Sprite
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					if (Subtexture != null)
						_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin,
							Entity.Transform.Scale, Entity.Transform.Rotation, Width, Height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// x value of the texture scroll
		/// </summary>
		/// <value>The scroll x.</value>
		public int ScrollX
		{
			get => _sourceRect.X;
			set => _sourceRect.X = value;
		}

		/// <summary>
		/// y value of the texture scroll
		/// </summary>
		/// <value>The scroll y.</value>
		public int ScrollY
		{
			get => _sourceRect.Y;
			set => _sourceRect.Y = value;
		}

		/// <summary>
		/// scale of the texture
		/// </summary>
		/// <value>The texture scale.</value>
		public virtual Vector2 TextureScale
		{
			get => _textureScale;
			set
			{
				_textureScale = value;

				// recalulcate our inverseTextureScale and the source rect size
				_inverseTexScale = new Vector2(1f / _textureScale.X, 1f / _textureScale.Y);
				_sourceRect.Width = (int) (Subtexture.SourceRect.Width * _inverseTexScale.X);
				_sourceRect.Height = (int) (Subtexture.SourceRect.Height * _inverseTexScale.Y);
			}
		}

		/// <summary>
		/// overridden width value so that the TiledSprite can have an independent width than its texture
		/// </summary>
		/// <value>The width.</value>
		public new int Width
		{
			get => _sourceRect.Width;
			set
			{
				_areBoundsDirty = true;
				_sourceRect.Width = value;
			}
		}

		/// <summary>
		/// overridden height value so that the TiledSprite can have an independent height than its texture
		/// </summary>
		/// <value>The height.</value>
		public new int Height
		{
			get => _sourceRect.Height;
			set
			{
				_areBoundsDirty = true;
				_sourceRect.Height = value;
			}
		}

		/// <summary>
		/// we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
		/// </summary>
		protected Rectangle _sourceRect;

		protected Vector2 _textureScale = Vector2.One;
		protected Vector2 _inverseTexScale = Vector2.One;


		public TiledSprite()
		{
		}

		public TiledSprite(Subtexture subtexture) : base(subtexture)
		{
			_sourceRect = subtexture.SourceRect;
			Material = new Material
			{
				SamplerState = Core.DefaultWrappedSamplerState
			};
		}

		public TiledSprite(Texture2D texture) : this(new Subtexture(texture))
		{
		}

		public override void Render(Graphics graphics, Camera camera)
		{
			if (Subtexture == null)
				return;

			var topLeft = Entity.Transform.Position + _localOffset;
			var destinationRect = RectangleExt.FromFloats(topLeft.X, topLeft.Y,
				_sourceRect.Width * Entity.Transform.Scale.X * TextureScale.X,
				_sourceRect.Height * Entity.Transform.Scale.Y * TextureScale.Y);

			graphics.Batcher.Draw(Subtexture, destinationRect, _sourceRect, Color, Entity.Transform.Rotation,
				Origin * _inverseTexScale, SpriteEffects, _layerDepth);
		}
	}
}