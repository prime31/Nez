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
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					if( subtexture != null )
						_bounds.calculateBounds( entity.transform.position, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// x value of the texture scroll
		/// </summary>
		/// <value>The scroll x.</value>
		public int scrollX
		{
			get => _sourceRect.X;
			set => _sourceRect.X = value;
		}

		/// <summary>
		/// y value of the texture scroll
		/// </summary>
		/// <value>The scroll y.</value>
		public int scrollY
		{
			get => _sourceRect.Y;
			set => _sourceRect.Y = value;
		}

		/// <summary>
		/// scale of the texture
		/// </summary>
		/// <value>The texture scale.</value>
		public virtual Vector2 textureScale
		{
			get => _textureScale;
			set
			{
				_textureScale = value;

				// recalulcate our inverseTextureScale and the source rect size
				_inverseTexScale = new Vector2( 1f / _textureScale.X, 1f / _textureScale.Y );
				_sourceRect.Width = (int)( subtexture.sourceRect.Width * _inverseTexScale.X );
				_sourceRect.Height = (int)( subtexture.sourceRect.Height * _inverseTexScale.Y );
			}
		}

		/// <summary>
		/// overridden width value so that the TiledSprite can have an independent width than its texture
		/// </summary>
		/// <value>The width.</value>
		public new int width
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
		public new int height
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
		{}

		public TiledSprite( Subtexture subtexture ) : base( subtexture )
		{
			_sourceRect = subtexture.sourceRect;
			material = new Material
			{
				samplerState = Core.defaultWrappedSamplerState
			};
		}

		public TiledSprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}

		public override void render( Graphics graphics, Camera camera )
		{
			if( subtexture == null )
				return;

			var topLeft = entity.transform.position + _localOffset;
			var destinationRect = RectangleExt.fromFloats( topLeft.X, topLeft.Y, _sourceRect.Width * entity.transform.scale.X * textureScale.X, _sourceRect.Height * entity.transform.scale.Y * textureScale.Y );

			graphics.batcher.draw( subtexture, destinationRect, _sourceRect, color, entity.transform.rotation, origin * _inverseTexScale, spriteEffects, _layerDepth );
		}

	}
}

