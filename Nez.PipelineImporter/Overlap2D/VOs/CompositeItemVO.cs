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


		/// <summary>
		/// gets the layerDepth for a child composite. It calculates it by first getting the composite (parent) layerDepth the standard way
		/// (via zIndexMin/Max) and then subtracting the inverse of the child (O2DMainItem) layerDepth which uses zIndexMaxChild. For this to
		/// work intelligently zIndexMaxComp should be the standard scene.zIndexMax value and zIndexMaxChild should be a much higher number. The
		/// reason for this is so that the parent component has a small offset to use for each child and so they dont overflow to the next zIndex.
		/// 
		/// Example: parent is 14. Any children must end up being between 14 and 15 so they dont overflow on top of another composite.
		/// </summary>
		/// <returns>The depth for child.</returns>
		/// <param name="zIndexMaxComp">Z index max comp.</param>
		/// <param name="child">Child.</param>
		/// <param name="zIndexMaxChild">Z index max child.</param>
		public float calculateLayerDepthForChild( float zIndexMin, float zIndexMax, MainItemVO child, float zIndexMaxChild = 100 )
		{
			if( zIndexMaxChild < zIndexMax )
				zIndexMaxChild = zIndexMax * 10;
			if( child.zIndex < zIndex )
				child.zIndex = zIndex + 1;

			var ourLayerDepth = calculateLayerDepth( zIndexMin, zIndexMax, null );
			var childLayerDepth = child.calculateLayerDepth( zIndexMin, zIndexMax, null );

			return Mathf.clamp01( ourLayerDepth + childLayerDepth );
			//return calculateLayerDepth( zIndexMin, zIndexMax, null ) - ( 1 - child.calculateLayerDepth( zIndexMin, zIndexMax, null ) );
		}


		public override string ToString()
		{
			return string.Format( "[CompositeItemVO] zIndex: {0}", zIndex );
		}

	}
}

