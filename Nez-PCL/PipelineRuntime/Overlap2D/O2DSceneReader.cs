using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace Nez.Overlap2D
{
	public class O2DSceneReader : ContentTypeReader<O2DScene>
	{
		protected override O2DScene Read( ContentReader reader, O2DScene existingInstance )
		{
			O2DScene scene = new O2DScene();
			scene.sceneName = reader.ReadString();

			var sImagesCount = reader.ReadInt32();
			for( int i = 0; i < sImagesCount; i++ )
			{
				O2DSimpleImage si = new O2DSimpleImage();
				si.uniqueId = reader.ReadInt32();
				si.itemIdentifier = reader.ReadString();
				si.itemName = reader.ReadString();
				si.x = (float)reader.ReadSingle();
				si.y = (float)reader.ReadSingle();
				si.scaleX = (float)reader.ReadSingle();
				si.scaleY = (float)reader.ReadSingle();
				si.originX = (float)reader.ReadSingle();
				si.originY = (float)reader.ReadSingle();
				si.rotation = (float)reader.ReadSingle();
				si.zIndex = reader.ReadInt32();
				si.layerName = reader.ReadString();
				si.imageName = reader.ReadString();
				scene.sImages.Add( si );
			}
			return scene;
		}

	}
}

