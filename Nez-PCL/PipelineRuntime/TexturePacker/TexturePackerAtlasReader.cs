using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;


namespace Nez.TextureAtlases
{
	public class TexturePackerAtlasReader : ContentTypeReader<TexturePackerAtlas>
	{
		protected override TexturePackerAtlas Read( ContentReader reader, TexturePackerAtlas existingInstance )
		{
			var assetName = reader.getRelativeAssetPath( reader.ReadString() );
			var texture = reader.ContentManager.Load<Texture2D>( assetName );
			var atlas = new TexturePackerAtlas( texture );

			var regionCount = reader.ReadInt32();
			for( var i = 0; i < regionCount; i++ )
			{
				atlas.createRegion
				(
					name: reader.ReadString(),
					x: reader.ReadInt32(),
					y: reader.ReadInt32(),
					width: reader.ReadInt32(),
					height: reader.ReadInt32()
				);
			}

			return atlas;
		}
	}
}