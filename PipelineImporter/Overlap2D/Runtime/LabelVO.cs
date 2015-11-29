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
	public class LabelVO : MainItemVO
	{
		public String 	text 	= "Label";
		public String	style	=  "";
		public int		size;
		public int		align;

		public float width = 0;
		public float height = 0;

		public bool multiline = false;

		public LabelVO() : base() {
		}

		public LabelVO(LabelVO vo) : base(vo) {
			text 	= vo.text;
			style 	= vo.style;
			size 	= vo.size;
			align 	= vo.align;
			width 	= vo.width;
			height 	= vo.height;
			multiline 	= vo.multiline;
		}

		/*
		@Override
		public void loadFromEntity(Entity entity) {
			super.loadFromEntity(entity);
			LabelComponent labelComponent = entity.getComponent(LabelComponent.class);
			DimensionsComponent dimensionsComponent = entity.getComponent(DimensionsComponent.class);
			text = labelComponent.getText().toString();
			style = labelComponent.fontName;
			size = labelComponent.fontSize;
			align = labelComponent.labelAlign;
			multiline = labelComponent.wrap;

			width = dimensionsComponent.width;
			height = dimensionsComponent.height;
		}*/
	}
		
}

