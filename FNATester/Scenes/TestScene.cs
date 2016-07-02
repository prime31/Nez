using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Tweens;


namespace FNATester
{
	public class TestScene : Scene
	{
		public override void initialize()
		{
			addRenderer( new DefaultRenderer() );

			// load up some textures
			var moonTex = contentManager.Load<Texture2D>( Content.Images.moon );
			var textureAtlas = contentManager.Load<TextureAtlas>( Content.TextureAtlasTest.atlasImages );
			var tex = textureAtlas.getSubtexture( "tree" );

			// create Entities with Sprites
			createEntity( "tree-sprite" )
				.addComponent( new Sprite( tex ) )
				.transform.setPosition( Screen.center );

			var moon = createEntity( "moon-sprite" )
				.addComponent( new Sprite( moonTex ) )
				.transform.setPosition( Screen.center );

			// add a tween
			moon.tweenLocalPositionTo( new Vector2(), 0.5f )
			    .setLoops( LoopType.PingPong, 5000 )
			    .start();
		}
	}
}

