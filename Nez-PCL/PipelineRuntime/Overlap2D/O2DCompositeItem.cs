using System;


namespace Nez.Overlap2D
{
	public class O2DCompositeItem : O2DMainItem
	{
		public O2DComposite composite;


		public float layerDepthForChild( float zIndexMaxComp, O2DMainItem child, float zIndexMaxChild = 100 )
		{
			return layerDepth( zIndexMaxComp ) - ( 1 - child.layerDepth( zIndexMaxChild ) );
		}
	}
}

