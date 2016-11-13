using System;


namespace Nez.Overlap2D
{
	public class O2DCompositeItem : O2DMainItem
	{
		public O2DComposite composite;


		/// <summary>
		/// gets the layerDepth for a child composite. It calculates it by first getting the composite (parent) layerDepth the standard way
		/// (via zIndexMaxComp) and then subtracting the inverse of the child (O2DMainItem) layerDepth which uses zIndexMaxChild. For this to
		/// work intelligently zIndexMaxComp should be the standard scene.zIndexMax value and zIndexMaxChild should be a much higher number. The
		/// reason for this is so that the parent component has a small offset to use for each child and so they dont overflow to the next zIndex.
		/// 
		/// Example: parent is 14. Any children must end up being between 14 and 15 so they dont overflow on top of another composite.
		/// </summary>
		/// <returns>The depth for child.</returns>
		/// <param name="zIndexMaxComp">Z index max comp.</param>
		/// <param name="child">Child.</param>
		/// <param name="zIndexMaxChild">Z index max child.</param>
		public float layerDepthForChild( float zIndexMaxComp, O2DMainItem child, float zIndexMaxChild = 100 )
		{
			return calculateLayerDepth( zIndexMaxComp ) - ( 1 - child.calculateLayerDepth( zIndexMaxChild ) );
		}
	}
}

