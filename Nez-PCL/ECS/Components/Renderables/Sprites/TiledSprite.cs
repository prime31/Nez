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
		/// <summary>
		/// x value of the texture scroll
		/// </summary>
		/// <value>The scroll x.</value>
		public int scrollX
		{
			get { return _sourceRect.X; }
			set { _sourceRect.X = value; }
		}

		/// <summary>
		/// y value of the texture scroll
		/// </summary>
		/// <value>The scroll y.</value>
		public int scrollY
		{
			get { return _sourceRect.Y; }
			set { _sourceRect.Y = value; }
		}

		/// <summary>
		/// scale of the texture
		/// </summary>
		/// <value>The texture scale.</value>
		public Vector2 textureScale
		{
			get { return _textureScale; }
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
			get { return _sourceRect.Width; }
			set { _sourceRect.Width = value; }
		}

		/// <summary>
		/// overridden height value so that the TiledSprite can have an independent height than its texture
		/// </summary>
		/// <value>The height.</value>
		public new int height
		{
			get { return _sourceRect.Height; }
			set { _sourceRect.Height = value; }
		}

		/// <summary>
		/// we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
		/// </summary>
		protected Rectangle _sourceRect;
		Vector2 _textureScale = Vector2.One;
		Vector2 _inverseTexScale = Vector2.One;


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
			var topLeft = entity.transform.position + _localOffset;
			var destinationRect = RectangleExt.fromFloats( topLeft.X, topLeft.Y, _sourceRect.Width * entity.transform.scale.X * textureScale.X, _sourceRect.Height * entity.transform.scale.Y * textureScale.Y );

			graphics.batcher.draw( subtexture, destinationRect, _sourceRect, color, entity.transform.rotation, origin * _inverseTexScale, spriteEffects, _layerDepth );
		}

	}
}

