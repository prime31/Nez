using System;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledImageCollectionTileset : TiledTileset
	{
		public TiledImageCollectionTileset( int firstId ) : base( firstId )
		{}


		public override Subtexture getTileTextureRegion( int id )
		{
			// TODO: this is entirely wrong and just a placeholder to allow stuff to compile
			return Graphics.instance.pixelTexture;
		}
	}
}

