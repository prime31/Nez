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
			var scene = new O2DScene();
			scene.sceneName = reader.ReadString();
			scene.ambientColor = reader.ReadColor();
			scene.composite = readComposite( reader );

			return scene;
		}


		O2DComposite readComposite( ContentReader reader )
		{
			var composite = new O2DComposite();

			var imageCount = reader.ReadInt32();
			for( var i = 0; i < imageCount; i++ )
			{
				var image = new O2DImage();
				readMainItem( reader, image );
				image.imageName = reader.ReadString();

				composite.images.Add( image );
			}


			var compositeItemCount = reader.ReadInt32();
			for( var i = 0; i < compositeItemCount; i++ )
			{
				var compositeItem = new O2DCompositeItem();
				readMainItem( reader, compositeItem );
				compositeItem.composite = readComposite( reader );

				composite.compositeItems.Add( compositeItem );
			}

			return composite;
		}


		void readMainItem( ContentReader reader, O2DMainItem item )
		{
			item.uniqueId = reader.ReadInt32();
			item.itemIdentifier = reader.ReadString();
			item.itemName = reader.ReadString();
			item.customVars = reader.ReadString();
			item.x = reader.ReadSingle();
			item.y = reader.ReadSingle();
			item.scaleX = reader.ReadSingle();
			item.scaleY = reader.ReadSingle();
			item.originX = reader.ReadSingle();
			item.originY = reader.ReadSingle();
			item.rotation = reader.ReadSingle();
			item.zIndex = reader.ReadInt32();
			item.layerName = reader.ReadString();
			item.tint = reader.ReadColor();
		}

	}
}

