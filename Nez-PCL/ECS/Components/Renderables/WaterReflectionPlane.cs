using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// adds a water reflection effect designed to be placed on the bottom of the screen. Note that transform.position is the top-left point
	/// in the water plane. Usage is as follows:
	/// - create a Renderer that renders after all of your other renderers and renders only renderLayer WATER_LAYER (you choose the int value)
	/// - put the WaterReflectionPlane on renderLayer WATER_LAYER and ensure no other Renderers are rendering WATER_LAYER
	/// - configure the material (you can fetch it via component.getMaterial<WaterReflectionMaterial>()). Be sure to set the normalMap property.
	/// </summary>
	public class WaterReflectionPlane : RenderableComponent
	{
		public override float width { get { return _width; } }
		public override float height { get { return _height; } }
		public override Material material
		{
			get { return _waterReflectionMaterial; }
			set { _waterReflectionMaterial = value as WaterReflectionMaterial; }
		}

		float _width;
		float _height;
		Texture2D _texture;
		WaterReflectionMaterial _waterReflectionMaterial;


		public WaterReflectionPlane( float width, float height )
		{
			// we need a separate texture (not part of an atlas) so that we get uvs in the 0 - 1 range that the Effect requires
			_texture = Graphics.createSingleColorTexture( 1, 1, Color.Bisque );
			_width = width;
			_height = height;

			_waterReflectionMaterial = new WaterReflectionMaterial();
		}


		~WaterReflectionPlane()
		{
			_texture.Dispose();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// we need to send the top of of the plane to the Effect
			var screenSpaceTop = entity.scene.camera.worldToScreenPoint( entity.transform.position );
			( material as WaterReflectionMaterial ).typedEffect.screenSpaceVerticalOffset = screenSpaceTop.Y / entity.scene.sceneRenderTargetSize.Y;

			graphics.batcher.draw( _texture, bounds, new Rectangle( 0, 0, 1, 1 ), color );
		}
	}
}

