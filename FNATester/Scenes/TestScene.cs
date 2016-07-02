using Nez;
using Nez.Sprites;
using Nez.TextureAtlases;


namespace FNATester
{
	public class TestScene : Scene
	{
		public override void initialize()
		{
			addRenderer( new DefaultRenderer() );

			var textureAtlas = contentManager.Load<TextureAtlas>( Content.AtlasImages );
			var tex = textureAtlas.getSubtexture( "tree" );

			createEntity( "sprite" )
				.addComponent( new Sprite( tex ) )
				.transform.setPosition( Screen.center );
		}
	}
}

