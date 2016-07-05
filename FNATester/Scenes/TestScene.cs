using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
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
			var tree = createEntity( "tree-sprite" );
			tree.addComponent( new Sprite( tex ) )
			    .setLayerDepth( 1 )
				.transform.setPosition( Screen.center );

			var moon = createEntity( "moon-sprite" )
				.addComponent( new Sprite( moonTex ) )
				.transform.setPosition( Screen.center );

			// add a tween
			moon.tweenLocalPositionTo( new Vector2(), 0.5f )
			    .setLoops( LoopType.PingPong, 5000 )
			    .start();

			// test an effect
			var effect = contentManager.Load<Effect>( "effects/Grayscale" );
			tree.getComponent<Sprite>().setMaterial( new Material( effect ) );

			// test a Song
			var song = contentManager.Load<Song>( "audio/CromaticMinor.ogg" );
			MediaPlayer.Play( song );

			// test a SoundEffect
			var sound = contentManager.Load<Microsoft.Xna.Framework.Audio.SoundEffect>( "audio/airlock.wav" );
			sound.Play();
		}
	}
}

