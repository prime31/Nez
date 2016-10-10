using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Tiled sprite. Note that TiledSprite overrides the Material so that it can wrap the UVs. This will class requires the texture
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

		public new int width
		{
			get { return _sourceRect.Width; }
			set { _sourceRect.Width = value; }
		}

		public new int height
		{
			get { return _sourceRect.Height; }
			set { _sourceRect.Height = value; }
		}

		/// <summary>
		/// we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
		/// </summary>
		Rectangle _sourceRect;


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
			graphics.batcher.draw( subtexture, entity.transform.position + _localOffset, _sourceRect, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, _layerDepth );
		}

	}
}

