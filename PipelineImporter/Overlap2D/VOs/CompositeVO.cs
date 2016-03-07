using System;
using System.Collections;
using System.Collections.Generic;


namespace Nez.Overlap2D.Runtime
{
	public class CompositeVO
	{
		public List<SimpleImageVO> sImages = new List<SimpleImageVO>();
		public List<CompositeItemVO> sComposites = new List<CompositeItemVO>();

		public List<Image9patchVO> sImage9patchs;
		public List<TextBoxVO> sTextBox;
		public List<LabelVO> sLabels;
		public List<SelectBoxVO> sSelectBoxes;
		public List<ParticleEffectVO> sParticleEffects;
		public List<LightVO> sLights;
		public List<SpriteAnimationVO> sSpriteAnimations;
		public List<SpineVO> sSpineAnimations;
		public List<SpriterVO> sSpriterAnimations;
		public List<ColorPrimitiveVO> sColorPrimitives;
		public List<LayerItemVO> layers;


		/// <summary>
		/// finds the max zIndex for all children
		/// </summary>
		/// <returns>The max zindex.</returns>
		public int findMaxZindex()
		{
			var maxIndex = int.MinValue;

			for( var i = 0; i < sImages.Count; i++ )
				maxIndex = Math.Max( maxIndex, sImages[i].zIndex );

			for( var i = 0; i < sComposites.Count; i++ )
			{
				maxIndex = Math.Max( maxIndex, sComposites[i].zIndex );

				if( sComposites[i].composite != null )
					maxIndex = Math.Max( maxIndex, sComposites[i].composite.findMaxZindex() );
			}

			return maxIndex;
		}


		public void setLayerDepthRecursively( float zIndexMax, CompositeItemVO parentCompositeVO )
		{
			for( var i = 0; i < sImages.Count; i++ )
			{
				sImages[i].layerDepth = sImages[i].calculateLayerDepth( zIndexMax, parentCompositeVO );
				//Overlap2DImporter.logger.LogMessage( "[image] zIndex: {0} -> {1}, parent: {2}", sImages[i].zIndex, sImages[i].layerDepth, parentCompositeVO );
			}

			for( var i = 0; i < sComposites.Count; i++ )
			{
				// compositeItems are considered global so they have their own zIndex that isnt affected by parents up the chain
				sComposites[i].layerDepth = sComposites[i].calculateLayerDepth( zIndexMax, null );

				if( sComposites[i].composite != null )
					sComposites[i].composite.setLayerDepthRecursively( zIndexMax, sComposites[i] );
			}
		}
	}
}

