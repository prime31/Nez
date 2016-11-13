using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Sprites
{
	/// <summary>
	/// Scrolling sprite. Note that ScrollingSprite overrides the Material so that it can wrap the UVs. This class requires the texture
	/// to not be part of an atlas so that wrapping can work.
	/// </summary>
	public class ScrollingSprite : TiledSprite, IUpdatable
	{
		/// <summary>
		/// x speed of automatic scrolling
		/// </summary>
		public float scrollSpeedX = 0;

		/// <summary>
		/// y speed of automatic scrolling
		/// </summary>
		public float scrollSpeedY = 0;

		// accumulate scroll in a separate float so that we can round it without losing precision for small scroll speeds
		float _scrollX, _scrollY;


		public ScrollingSprite( Subtexture subtexture ) : base( subtexture )
		{}


		public ScrollingSprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		void IUpdatable.update()
		{
			_scrollX += scrollSpeedX * Time.deltaTime;
			_scrollY += scrollSpeedY * Time.deltaTime;
			_sourceRect.X = (int)_scrollX;
			_sourceRect.Y = (int)_scrollY;
		}

	}
}

