using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.Overlap2D.Runtime;
using Microsoft.Xna.Framework;


namespace Nez.Overlap2D
{
	[ContentTypeWriter]
	public class Overlap2DWriter : ContentTypeWriter<SceneVO>
	{
		protected override void Write( ContentWriter writer, SceneVO scene )
		{
			Overlap2DImporter.logger.LogMessage( "" );
			Overlap2DImporter.logger.LogMessage( "--- writing {0}", scene.sceneName );

			writer.Write( scene.sceneName );
			writer.Write( new Color( scene.ambientColor[0], scene.ambientColor[1], scene.ambientColor[2], scene.ambientColor[3] ) );
			writeComposite( writer, scene.composite, scene );

			// not implemented for scene
//			public bool lightSystemEnabled = false;
//			public PhysicsPropertiesVO physicsPropertiesVO;
//			public List<float> verticalGuides;
//			public List<float> horizontalGuides;
		}


		void writeComposite( ContentWriter writer, CompositeVO composite, SceneVO scene )
		{
			Overlap2DImporter.logger.LogMessage( "-- processing composite --" );

			Overlap2DImporter.logger.LogMessage( "processing {0} simple images", composite.sImages.Count );
			writer.Write( composite.sImages.Count );
			foreach( var image in composite.sImages )
			{
				Overlap2DImporter.logger.LogMessage( "\tprocessing image: {0}", image.imageName );
				writeMainItem( writer, image, scene );
				writer.Write( image.imageName );
			}


			Overlap2DImporter.logger.LogMessage( "processing {0} color primitives", composite.sColorPrimitives.Count );
			writer.Write( composite.sColorPrimitives.Count );
			foreach( var colorPrim in composite.sColorPrimitives )
			{
				Overlap2DImporter.logger.LogMessage( "\tprocessing color primitive item: {0}", colorPrim.itemIdentifier );
				writeMainItem( writer, colorPrim, scene );
				writeColorPrimitive( writer, colorPrim, scene );
			}


			Overlap2DImporter.logger.LogMessage( "processing {0} composite items", composite.sComposites.Count );
			writer.Write( composite.sComposites.Count );
			foreach( var compositeItem in composite.sComposites )
			{
				Overlap2DImporter.logger.LogMessage( "\tprocessing composite item: {0}", compositeItem.itemName );
				writeMainItem( writer, compositeItem, scene );
				writeComposite( writer, compositeItem.composite, scene );

				// not implemented for composite item
//				public float scissorX;
//				public float scissorY;
//				public float scissorWidth;
//				public float scissorHeight;
//				public float width;
//				public float height;
			}
		}


		void writeMainItem( ContentWriter writer, MainItemVO item, SceneVO scene )
		{
			writer.Write( item.uniqueId );
			writer.Write( item.itemIdentifier );
			writer.Write( item.itemName );
			writer.Write( item.customVars );
			writer.Write( item.x * scene.pixelToWorld );
			writer.Write( -item.y * scene.pixelToWorld );
			writer.Write( item.scaleX );
			writer.Write( item.scaleY );
			writer.Write( item.originX * scene.pixelToWorld );
			writer.Write( -item.originY * scene.pixelToWorld );
			writer.Write( item.rotation );
			writer.Write( item.zIndex );
			writer.Write( item.layerDepth );
			writer.Write( item.layerName );
			writer.Write( item.renderLayer );
			writer.Write( new Color( item.tint[0], item.tint[1], item.tint[2], item.tint[3] ) );

			// not implemented for main item
//			public string[] tags;
//			public string shaderName;
//			public ShapeVO shape; // implemented only for ColorPrimitiveVO
//			public PhysicsBodyDataVO physics;
		}


		void writeColorPrimitive( ContentWriter writer, ColorPrimitiveVO colorPrim, SceneVO scene )
		{
			// we really only have the ShapeVO to write. nothing else is different from MainItemVO. Actually, MainItemVO really contains the
			// ShapeVO class but it doesnt serve much purpose there so we dont write it out

			// technically, we should write this out like this since the data format can contain multiple polygons but in reality the editor
			// only lets us add one so no reason to write out extra crap for nothing
			//writer.Write( colorPrim.shape.polygons.Length );
			//foreach( var polygon in colorPrim.shape.polygons )
			//write polygon

			// we have to multiply by our scenes pixelToWorld and invert y
			var vectorFix = new Vector2( scene.pixelToWorld, -scene.pixelToWorld );
			var polygon = colorPrim.shape.polygons[0];
			writer.Write( polygon.Length );
			foreach( var vec in polygon )
				writer.Write( vec * vectorFix );
		}



		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Overlap2D.O2DScene ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.Overlap2D.O2DSceneReader ).AssemblyQualifiedName;
		}
	}
}

