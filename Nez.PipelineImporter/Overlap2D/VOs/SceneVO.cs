using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nez.Overlap2D.Runtime
{
	public class SceneVO
	{
		// this is a project property, not part of the SceneVO overlap spec
		public int pixelToWorld = 1;

		public String sceneName = string.Empty;
		public CompositeVO composite;
		public bool lightSystemEnabled = false;
		public float[] ambientColor = { 1f, 1f, 1f, 1f };
		public PhysicsPropertiesVO physicsPropertiesVO;
		public List<float> verticalGuides;
		public List<float> horizontalGuides;


		public override string ToString()
		{
			return JsonConvert.SerializeObject( this, Formatting.Indented );
		}
	}
}

