using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Overlap2D
{
	public class O2DScene
	{
		public string sceneName;
		public Color ambientColor;
		public O2DComposite composite;

		/// <summary>
		/// the maximum zIndex + 1 on any of the MainItems in the scene
		/// </summary>
		public int zIndexMax;


		public O2DScene()
		{}

	}
}

