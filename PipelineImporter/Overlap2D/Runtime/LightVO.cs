/*
The MIT License (MIT)

Copyright (c) 2015 Valerio Santinelli

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;

namespace Nez.Overlap2D.Runtime
{
	public class LightVO : MainItemVO
	{
		//public int itemId = -1;
		public enum LightType {POINT, CONE};
		public LightType type;
		public int rays = 12;
		public float distance = 300;
		public float directionDegree = 0;
		public float coneDegree = 30;
		public float softnessLength = -1f;
		public bool isStatic = true;
		public bool isXRay = true;

		public LightVO() {
			tint = new float[4];
			tint[0] = 1f;
			tint[1] = 1f;
			tint[2] = 1f;
			tint[3] = 1f;
		}

		public LightVO(LightVO vo) : base(vo) {
			type = vo.type;
			rays = vo.rays;
			distance = vo.distance;
			directionDegree = vo.directionDegree;
			coneDegree = vo.coneDegree;
			isStatic = vo.isStatic;
			isXRay = vo.isXRay;
			softnessLength = vo.softnessLength;
		}

		/*
		@Override
		public void loadFromEntity(Entity entity) {
			super.loadFromEntity(entity);

			LightObjectComponent lightObjectComponent = entity.getComponent(LightObjectComponent.class);
			type = lightObjectComponent.getType();
			rays = lightObjectComponent.rays;
			distance = lightObjectComponent.distance;
			directionDegree = lightObjectComponent.directionDegree;
			coneDegree = lightObjectComponent.coneDegree;
			isStatic = lightObjectComponent.isStatic;
			isXRay = lightObjectComponent.isXRay;
			softnessLength = lightObjectComponent.softnessLength;
		}*/
	}
}

