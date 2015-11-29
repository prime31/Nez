using System;
using System.Collections;
using System.Collections.Generic;

namespace Nez.Overlap2D
{
	public class O2DScene
	{
		public List<O2DLayer> layers = new List<O2DLayer>();
		public List<O2DSimpleImage> sImages = new List<O2DSimpleImage>();
		public string sceneName;

		public O2DScene()
		{
		}

		/*
		public void draw( SpriteBatch spriteBatch, Vector2 position, float layerDepth, Rectangle cameraClipBounds )
		{
			// render any visible image or tile layer
			foreach( var layer in layers )
			{
				if( !layer.visible )
					continue;

				layer.draw( spriteBatch, position, layerDepth, cameraClipBounds );
			}
		}*/

	}
}

