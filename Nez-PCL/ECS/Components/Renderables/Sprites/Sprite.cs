using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Sprites
{
	public class Sprite : RenderableComponent
	{
		public override float width
		{
			get { return subtexture.sourceRect.Width; }
		}

		public override float height
		{
			get { return subtexture.sourceRect.Height; }
		}

		/// <summary>
		/// the Subtexture that should be displayed by this Sprite. When set, the origin of the Sprite is also set to match Subtexture.origin.
		/// </summary>
		/// <value>The subtexture.</value>
		public Subtexture subtexture
		{
			get { return _subtexture; }
			set { setSubtexture( value ); }
		}

		protected Subtexture _subtexture;


		public Sprite( Subtexture subtexture )
		{
			_subtexture = subtexture;
			_origin = subtexture.center;
		}


		public Sprite( Texture2D texture ) : this( new Subtexture( texture ) )
		{}


		/// <summary>
		/// sets the Subtexture and updates the origin of the Sprite to match Subtexture.origin. If for whatever reason you need
		/// an origin different from the Subtexture either clone it or set the origin AFTER setting the Subtexture here.
		/// </summary>
		/// <returns>The subtexture.</returns>
		/// <param name="subtexture">Subtexture.</param>
		public Sprite setSubtexture( Subtexture subtexture )
		{
			_subtexture = subtexture;

			if( _subtexture != null )
				_origin = subtexture.origin;
			return this;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( _subtexture, entity.transform.position + localOffset, _subtexture.sourceRect, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, _layerDepth );
		}

	}
}

