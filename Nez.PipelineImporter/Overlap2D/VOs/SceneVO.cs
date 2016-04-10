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

