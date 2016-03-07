using System;


namespace Nez.Overlap2D.Runtime
{
	public class MainItemVO
	{
		public int uniqueId = -1;
		public String itemIdentifier = string.Empty;
		public String itemName = string.Empty;
		public String[] tags;
		public String customVars = string.Empty;
		public float x;
		public float y;
		public float scaleX	= 1f;
		public float scaleY	= 1f;
		public float originX = 0;
		public float originY = 0;
		public float rotation;
		public int zIndex = 0;
		public String layerName = string.Empty;
		public float[] tint = { 1, 1, 1, 1 };

		public String shaderName = string.Empty;

		public ShapeVO shape;
		public PhysicsBodyDataVO physics;
	}
}

