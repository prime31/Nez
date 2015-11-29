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
	public class ShapeVO
	{
		public Vector2[][] polygons;
		public Circle[] circles;

		public ShapeVO clone() {
			ShapeVO newVo = new ShapeVO();
			Vector2 [][] target = new Vector2[polygons.Length][];

			for (int i = 0; i < polygons.Length; i++) {
				target[i] = new Vector2[polygons[i].Length];
				for(int j=0;j<polygons[i].Length;j++){
					target[i][j] = polygons[i][j];
				}
			}
			newVo.polygons = target;

			return newVo;
		}

		public static ShapeVO createRect(float width, float height) {
			ShapeVO vo = new ShapeVO();
			vo.polygons = new Vector2[1][];

			vo.polygons[0] = new Vector2[4];
			vo.polygons[0][0] = new Vector2(0, 0);
			vo.polygons[0][1] = new Vector2(0, height);
			vo.polygons[0][2] = new Vector2(width, height);
			vo.polygons[0][3] = new Vector2(width, 0);

			return vo;
		}
	}
}

