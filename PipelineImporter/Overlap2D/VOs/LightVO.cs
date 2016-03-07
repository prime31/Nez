using System;


namespace Nez.Overlap2D.Runtime
{
	public class LightVO : MainItemVO
	{
		//public int itemId = -1;
		public enum LightType
		{
			POINT,
			CONE
		};

		public LightType type;
		public int rays = 12;
		public float distance = 300;
		public float directionDegree = 0;
		public float coneDegree = 30;
		public float softnessLength = -1f;
		public bool isStatic = true;
		public bool isXRay = true;
	}
}

