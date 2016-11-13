using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class RenderableComponentList
	{
		/// <summary>
		/// list of components added to the entity
		/// </summary>
		FastList<RenderableComponent> _components = new FastList<RenderableComponent>();

		/// <summary>
		/// tracks components by renderLayer for easy retrieval
		/// </summary>
		Dictionary<int,FastList<RenderableComponent>> _componentsByRenderLayer = new Dictionary<int, FastList<RenderableComponent>>();
		List<int> _unsortedRenderLayers = new List<int>();
		bool _componentsNeedSort = true;


		#region array access

		public int count { get { return _components.length; } }

		public RenderableComponent this[int index] { get { return _components.buffer[index]; } }

		#endregion


		public void add( RenderableComponent component )
		{
			_components.add( component );
			addToRenderLayerList( component, component.renderLayer );
		}


		public void remove( RenderableComponent component )
		{
			_components.remove( component );
			_componentsByRenderLayer[component.renderLayer].remove( component );
		}


		public void updateRenderableRenderLayer( RenderableComponent component, int oldRenderLayer, int newRenderLayer )
		{
			// a bit of care needs to be taken in case a renderLayer is changed before the component is "live". this can happen when a component
			// changes its renderLayer immediately after being created
			if( _componentsByRenderLayer.ContainsKey( oldRenderLayer ) && _componentsByRenderLayer[oldRenderLayer].contains( component ) )
			{
				_componentsByRenderLayer[oldRenderLayer].remove( component );
				addToRenderLayerList( component, newRenderLayer );
			}
		}


    	public void setRenderLayerNeedsComponentSort( int renderLayer )
    	{
			if( !_unsortedRenderLayers.Contains( renderLayer ) )
				_unsortedRenderLayers.Add( renderLayer );
			_componentsNeedSort = true;
    	}


		internal void setNeedsComponentSort()
		{
			_componentsNeedSort = true;
		}


		void addToRenderLayerList( RenderableComponent component, int renderLayer )
		{
			var list = componentsWithRenderLayer( renderLayer );
			Assert.isFalse( list.contains( component ), "Component renderLayer list already contains this component" );

			list.add( component );
			if( !_unsortedRenderLayers.Contains( renderLayer ) )
				_unsortedRenderLayers.Add( renderLayer );
			_componentsNeedSort = true;
		}


		public FastList<RenderableComponent> componentsWithRenderLayer( int renderLayer )
		{
			FastList<RenderableComponent> list = null;
			if( !_componentsByRenderLayer.TryGetValue( renderLayer, out list ) )
			{
				list = new FastList<RenderableComponent>();
				_componentsByRenderLayer[renderLayer] = list;
			}

			return _componentsByRenderLayer[renderLayer];
		}


		public void updateLists()
		{
			if( _componentsNeedSort )
			{
				_components.sort();
				_componentsNeedSort = false;
			}
			
			if( _unsortedRenderLayers.Count > 0 )
			{
				for( int i = 0, count = _unsortedRenderLayers.Count; i < count; i++ )
					_componentsByRenderLayer[_unsortedRenderLayers[i]].sort();
				_unsortedRenderLayers.Clear();
			}
		}

	}
}

