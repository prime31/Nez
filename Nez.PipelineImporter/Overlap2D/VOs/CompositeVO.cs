using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nez.Overlap2D.Runtime
{
	public class CompositeVO
	{
		public List<SimpleImageVO> sImages = new List<SimpleImageVO>();
		public List<CompositeItemVO> sComposites = new List<CompositeItemVO>();
		public List<ColorPrimitiveVO> sColorPrimitives = new List<ColorPrimitiveVO>();

		public List<Image9patchVO> sImage9patchs;
		public List<TextBoxVO> sTextBox;
		public List<LabelVO> sLabels;
		public List<SelectBoxVO> sSelectBoxes;
		public List<ParticleEffectVO> sParticleEffects;
		public List<LightVO> sLights;
		public List<SpriteAnimationVO> sSpriteAnimations;
		public List<SpineVO> sSpineAnimations;
		public List<SpriterVO> sSpriterAnimations;
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


		public Tuple<Dictionary<int,int>,Dictionary<int,int>> findMinMaxZindexForRenderLayers()
		{
			var maxIndicies = new Dictionary<int,int>();
			var minIndicies = new Dictionary<int,int>();

			for( var i = 0; i < sImages.Count; i++ )
			{
				var renderLayer = sImages[i].renderLayer;
				if( !maxIndicies.ContainsKey( renderLayer ) )
					maxIndicies[renderLayer] = int.MinValue;
				if( !minIndicies.ContainsKey( renderLayer ) )
					minIndicies[renderLayer] = int.MaxValue;
				
				maxIndicies[renderLayer] = Math.Max( maxIndicies[renderLayer], sImages[i].zIndex );
				minIndicies[renderLayer] = Math.Min( minIndicies[renderLayer], sImages[i].zIndex );
			}

			for( var i = 0; i < sComposites.Count; i++ )
			{
				var renderLayer = sComposites[i].renderLayer;
				if( !maxIndicies.ContainsKey( renderLayer ) )
					maxIndicies[renderLayer] = int.MinValue;
				if( !minIndicies.ContainsKey( renderLayer ) )
					minIndicies[renderLayer] = int.MaxValue;
				
				maxIndicies[renderLayer] = Math.Max( maxIndicies[renderLayer], sComposites[i].zIndex );
				minIndicies[renderLayer] = Math.Min( minIndicies[renderLayer], sComposites[i].zIndex );

				if( sComposites[i].composite != null )
				{
					var compositeIndices = sComposites[i].composite.findMinMaxZindexForRenderLayers();
					var compositeMinIndices = compositeIndices.Item1;
					var compositeMaxIndices = compositeIndices.Item2;
					if( compositeMaxIndices.ContainsKey( renderLayer ) )
					{
						maxIndicies[renderLayer] = Math.Max( maxIndicies[renderLayer], compositeMaxIndices[renderLayer] );
						minIndicies[renderLayer] = Math.Min( minIndicies[renderLayer], compositeMinIndices[renderLayer] );
					}
				}
			}

			return new Tuple<Dictionary<int,int>,Dictionary<int,int>>( minIndicies, maxIndicies );
		}


		public void setLayerDepthRecursively( float zIndexMin, float zIndexMax, CompositeItemVO parentCompositeVO )
		{
			for( var i = 0; i < sImages.Count; i++ )
			{
				sImages[i].layerDepth = sImages[i].calculateLayerDepth( zIndexMin, zIndexMax, parentCompositeVO );
				Overlap2DImporter.logger.LogMessage( "[image] zIndex: {0} -> {1}, parent: {2}, renderLayer: {3}", sImages[i].zIndex, sImages[i].layerDepth, parentCompositeVO, sImages[i].renderLayer );
			}

			for( var i = 0; i < sComposites.Count; i++ )
			{
				// compositeItems are considered global so they have their own zIndex that isnt affected by parents up the chain
				sComposites[i].layerDepth = sComposites[i].calculateLayerDepth( zIndexMin, zIndexMax, null );

				if( sComposites[i].composite != null )
					sComposites[i].composite.setLayerDepthRecursively( zIndexMin, zIndexMax, sComposites[i] );
			}
		}


		public void setLayerDepthRecursively( Dictionary<int,int> minIndicies, Dictionary<int,int> maxIndicies, CompositeItemVO parentCompositeVO )
		{
			for( var i = 0; i < sImages.Count; i++ )
			{
				var renderLayer = sImages[i].renderLayer;
				var minIndex = minIndicies[renderLayer];
				var maxIndex = maxIndicies[renderLayer];

				sImages[i].layerDepth = sImages[i].calculateLayerDepth( minIndex, maxIndex, parentCompositeVO );
				Overlap2DImporter.logger.LogMessage( "[image] zIndex: {0} -> {1}, parent: {2}, renderLayer: {3}", sImages[i].zIndex, sImages[i].layerDepth, parentCompositeVO, sImages[i].renderLayer );
			}

			for( var i = 0; i < sComposites.Count; i++ )
			{
				var renderLayer = sComposites[i].renderLayer;
				var minIndex = minIndicies[renderLayer];
				var maxIndex = maxIndicies[renderLayer];

				// compositeItems are considered global so they have their own zIndex that isnt affected by parents up the chain
				sComposites[i].layerDepth = sComposites[i].calculateLayerDepth( minIndex, maxIndex, null );

				if( sComposites[i].composite != null )
					sComposites[i].composite.setLayerDepthRecursively( minIndex, maxIndex, sComposites[i] );
			}
		}
	}
}

