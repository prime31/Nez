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
using System.Collections;
using System.Collections.Generic;

namespace Nez.Overlap2D.Runtime
{
	public class CompositeItemVO : MainItemVO
	{
		public CompositeVO composite;

		public float scissorX; 
		public float scissorY;
		public float scissorWidth; 
		public float scissorHeight;

		public float width;
		public float height;

		public CompositeItemVO() : base() {
			composite = new CompositeVO();
		}

		public CompositeItemVO(CompositeVO vo) : base() {
			composite = new CompositeVO(vo);
		}

		public CompositeItemVO(CompositeItemVO vo) : base(vo) {
			composite = new CompositeVO(vo.composite);
		}

		public void update(CompositeItemVO vo) {
			composite = new CompositeVO(vo.composite);
		}

		public CompositeItemVO clone() {
			CompositeItemVO tmp = new CompositeItemVO();
			tmp.composite = composite;
			tmp.itemName = itemName;
			tmp.layerName = layerName;
			tmp.rotation = rotation;
			tmp.tint = tint;
			tmp.x = x;
			tmp.y = y;
			tmp.zIndex = zIndex;

			tmp.scissorX = scissorX;
			tmp.scissorY = scissorY;
			tmp.scissorWidth = scissorWidth;
			tmp.scissorHeight = scissorHeight;

			tmp.width = width;
			tmp.height = height;

			return tmp;
		}

		/*
		@Override
		public void loadFromEntity(Entity entity) {
			super.loadFromEntity(entity);
			//scissorsX
			//scissorsY
			composite = new CompositeVO();
			composite.loadFromEntity(entity);

			DimensionsComponent dimensionsComponent = ComponentRetriever.get(entity, DimensionsComponent.class);

			width = dimensionsComponent.width;
			height = dimensionsComponent.height;
		}*/

		public void cleanIds() {
			uniqueId = -1;
			List<MainItemVO> items = composite.getAllItems();
			foreach(MainItemVO subItem in items) {
				subItem.uniqueId = -1;
			}
		}	
	}
}

