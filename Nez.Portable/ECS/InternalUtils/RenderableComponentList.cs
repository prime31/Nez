using System.Collections.Generic;


namespace Nez
{
	public class RenderableComponentList
	{
		// global updateOrder sort for the IUpdatable list
		static IRenderableComparer compareUpdatableOrder = new IRenderableComparer();

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		FastList<IRenderable> _components = new FastList<IRenderable>();

		/// <summary>
		/// tracks components by renderLayer for easy retrieval
		/// </summary>
		Dictionary<int, FastList<IRenderable>> _componentsByRenderLayer = new Dictionary<int, FastList<IRenderable>>();
		List<int> _unsortedRenderLayers = new List<int>();
		bool _componentsNeedSort = true;


		#region array access

		public int count { get { return _components.length; } }

		public IRenderable this[int index] { get { return _components.buffer[index]; } }

		#endregion


		public void add( IRenderable component )
		{
			_components.add( component );
			addToRenderLayerList( component, component.renderLayer );
		}


		public void remove( IRenderable component )
		{
			_components.remove( component );
			_componentsByRenderLayer[component.renderLayer].remove( component );
		}


		public void updateRenderableRenderLayer( IRenderable component, int oldRenderLayer, int newRenderLayer )
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


		void addToRenderLayerList( IRenderable component, int renderLayer )
		{
			var list = componentsWithRenderLayer( renderLayer );
			Assert.isFalse( list.contains( component ), "Component renderLayer list already contains this component" );

			list.add( component );
			if( !_unsortedRenderLayers.Contains( renderLayer ) )
				_unsortedRenderLayers.Add( renderLayer );
			_componentsNeedSort = true;
		}


		public FastList<IRenderable> componentsWithRenderLayer( int renderLayer )
		{
			FastList<IRenderable> list = null;
			if( !_componentsByRenderLayer.TryGetValue( renderLayer, out list ) )
			{
				list = new FastList<IRenderable>();
				_componentsByRenderLayer[renderLayer] = list;
			}

			return _componentsByRenderLayer[renderLayer];
		}


		public void updateLists()
		{
			if( _componentsNeedSort )
			{
				_components.sort( compareUpdatableOrder );
				_componentsNeedSort = false;
			}

			if( _unsortedRenderLayers.Count > 0 )
			{
				for( int i = 0, count = _unsortedRenderLayers.Count; i < count; i++ )
				{
					FastList<IRenderable> renderLayerComponents;
					if( _componentsByRenderLayer.TryGetValue( _unsortedRenderLayers[i], out renderLayerComponents ) )
						renderLayerComponents.sort( compareUpdatableOrder );
				}

				_unsortedRenderLayers.Clear();
			}
		}

	}
}

