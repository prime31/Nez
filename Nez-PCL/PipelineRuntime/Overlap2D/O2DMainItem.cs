using System;


namespace Nez
{
	public class O2DMainItem
	{
		public int uniqueId = -1;
		public String itemIdentifier = "";
		public String itemName = "";
		public String[] tags = null;
		public String customVars = "";
		public float x; 
		public float y;
		public float scaleX	=	1f; 
		public float scaleY	=	1f;
		public float originX	= 0;
		public float originY	= 0;
		public float rotation;
		public int zIndex = 0;
		public String layerName = "";
		public float[] tint = {1, 1, 1, 1};

		public String shaderName = "";

		public O2DMainItem()
		{
		}
	}
}

