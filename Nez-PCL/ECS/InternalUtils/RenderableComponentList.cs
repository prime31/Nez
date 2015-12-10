using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class RenderableComponentList : IEnumerable<RenderableComponent>, IEnumerable
	{
		// global layerDepth sort for RenderableComponent lists
		internal static Comparison<RenderableComponent> compareComponentLayerDepth = ( a, b ) => { return Math.Sign( b.layerDepth - a.layerDepth ); };

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		List<RenderableComponent> _components = new List<RenderableComponent>();

		/// <summary>
		/// tracks components by renderLayer for easy retrieval
		/// </summary>
		Dictionary<int,List<RenderableComponent>> _componentsByRenderLayer;
		List<int> _unsortedRenderLayers;


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


		void addToRenderLayerList( RenderableComponent component, int renderLayer )
		{
			var list = componentsWithRenderLayer( renderLayer );
			Debug.assertIsFalse( list.Contains( component ), "Component renderLayer list already contains this component" );

			list.Add( component );
			if( !_unsortedRenderLayers.Contains( renderLayer ) )
				_unsortedRenderLayers.Add( renderLayer );
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
			if( _unsortedRenderLayers.Count > 0 )
			{
				foreach( var renderLayer in _unsortedRenderLayers )
					_componentsByRenderLayer[renderLayer].Sort( compareComponentLayerDepth );

				_unsortedRenderLayers.Clear();
			}
		}


		public int Count
		{
			get
			{
				return _components.Count;
			}
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

