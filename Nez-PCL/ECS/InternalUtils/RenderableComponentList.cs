using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class RenderableComponentList : IEnumerable<RenderableComponent>, IEnumerable
	{
		// global sorts for RenderableComponent lists. The main list is sorted by renderLayer first then layerDepth. Each renderLayerList is sorted by layerDepth.

		/// <summary>
		/// sorts the renderLayer lists. The sort is first by layerDepth and then by renderState so that all common renderStates are together
		/// to avoid state switches.
		/// </summary>
		internal static Comparison<RenderableComponent> compareComponentsRenderLayer = ( a, b ) =>
		{
			var res = Math.Sign( b.layerDepth - a.layerDepth );
			if( res == 0 && b.renderState != null )
				return b.renderState.CompareTo( a.renderState );
			return res;
		};

		/// <summary>
		/// sorts the main components list. The sort is first by renderLayer, then layerDepth and finally by renderState
		/// </summary>
		internal static Comparison<RenderableComponent> compareComponents = ( a, b ) =>
		{
			var res = b.renderLayer.CompareTo( a.renderLayer );
			if( res == 0 )
			{
				var layerDepthRes = b.layerDepth.CompareTo( a.layerDepth );
				if( layerDepthRes == 0 && b.renderState != null )
					return b.renderState.CompareTo( a.renderState );
			}

			return res;
		};

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		List<RenderableComponent> _components = new List<RenderableComponent>();

		/// <summary>
		/// tracks components by renderLayer for easy retrieval
		/// </summary>
		Dictionary<int,List<RenderableComponent>> _componentsByRenderLayer;
		List<int> _unsortedRenderLayers;
		bool _componentsNeedSort = true;


		public RenderableComponentList()
		{
			_componentsByRenderLayer = new Dictionary<int,List<RenderableComponent>>();
			_unsortedRenderLayers = new List<int>();
		}


		public void add( RenderableComponent component )
		{
			_components.Add( component );
			addToRenderLayerList( component, component.renderLayer );
		}


		public void remove( RenderableComponent component )
		{
			_components.Remove( component );
			_componentsByRenderLayer[component.renderLayer].Remove( component );
		}


		public void updateRenderableRenderLayer( RenderableComponent component, int oldRenderLayer, int newRenderLayer )
		{
			_componentsByRenderLayer[oldRenderLayer].Remove( component );
			addToRenderLayerList( component, newRenderLayer );
		}


		internal void setNeedsComponentSort()
		{
			_componentsNeedSort = true;
		}


		void addToRenderLayerList( RenderableComponent component, int renderLayer )
		{
			var list = componentsWithRenderLayer( renderLayer );
			Assert.isFalse( list.Contains( component ), "Component renderLayer list already contains this component" );

			list.Add( component );
			if( !_unsortedRenderLayers.Contains( renderLayer ) )
				_unsortedRenderLayers.Add( renderLayer );
			_componentsNeedSort = true;
		}


		public List<RenderableComponent> componentsWithRenderLayer( int renderLayer )
		{
			List<RenderableComponent> list = null;
			if( !_componentsByRenderLayer.TryGetValue( renderLayer, out list ) )
			{
				list = new List<RenderableComponent>();
				_componentsByRenderLayer[renderLayer] = list;
			}

			return _componentsByRenderLayer[renderLayer];
		}


		public void updateLists()
		{
			if( _componentsNeedSort )
			{
				_components.Sort( compareComponents );
				_componentsNeedSort = false;
			}
			
			if( _unsortedRenderLayers.Count > 0 )
			{
				foreach( var renderLayer in _unsortedRenderLayers )
					_componentsByRenderLayer[renderLayer].Sort( compareComponentsRenderLayer );

				_unsortedRenderLayers.Clear();
			}
		}


		public int Count
		{
			get { return _components.Count; }
		}


		#region IEnumerable and array access

		public RenderableComponent this[int index]
		{
			get
			{
				return _components[index];
			}
		}


		public IEnumerator<RenderableComponent> GetEnumerator()
		{
			return _components.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		#endregion

	}
}

