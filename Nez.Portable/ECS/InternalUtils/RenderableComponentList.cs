using System.Collections.Generic;

namespace Nez
{
	public class RenderableComponentList
	{
		// global updateOrder sort for the IRenderable lists
		public static IComparer<IRenderable> CompareUpdatableOrder = new RenderableComparer();

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

		public int Count => _components.Length;

		public IRenderable this[int index] => _components.Buffer[index];

		#endregion


		public void Add(IRenderable component)
		{
			_components.Add(component);
			AddToRenderLayerList(component, component.RenderLayer);
		}

		public void Remove(IRenderable component)
		{
			_components.Remove(component);
			_componentsByRenderLayer[component.RenderLayer].Remove(component);
		}

		public void UpdateRenderableRenderLayer(IRenderable component, int oldRenderLayer, int newRenderLayer)
		{
			// a bit of care needs to be taken in case a renderLayer is changed before the component is "live". this can happen when a component
			// changes its renderLayer immediately after being created
			if (_componentsByRenderLayer.ContainsKey(oldRenderLayer) && _componentsByRenderLayer[oldRenderLayer].Contains(component))
			{
				_componentsByRenderLayer[oldRenderLayer].Remove(component);
				AddToRenderLayerList(component, newRenderLayer);
			}
		}

		/// <summary>
		/// dirties a RenderLayers sort flag, causing a re-sort of all components to occur
		/// </summary>
		/// <param name="renderLayer"></param>
		public void SetRenderLayerNeedsComponentSort(int renderLayer)
		{
			if (!_unsortedRenderLayers.Contains(renderLayer))
				_unsortedRenderLayers.Add(renderLayer);
			_componentsNeedSort = true;
		}

		internal void SetNeedsComponentSort() => _componentsNeedSort = true;

		void AddToRenderLayerList(IRenderable component, int renderLayer)
		{
			var list = ComponentsWithRenderLayer(renderLayer);
			Insist.IsFalse(list.Contains(component), "Component renderLayer list already contains this component");

			list.Add(component);
			if (!_unsortedRenderLayers.Contains(renderLayer))
				_unsortedRenderLayers.Add(renderLayer);
			_componentsNeedSort = true;
		}

		/// <summary>
		/// fetches all the Components with the given renderLayer. The component list is pre-sorted.
		/// </summary>
		public FastList<IRenderable> ComponentsWithRenderLayer(int renderLayer)
		{
			if (!_componentsByRenderLayer.TryGetValue(renderLayer, out _))
				_componentsByRenderLayer[renderLayer] = new FastList<IRenderable>();

			return _componentsByRenderLayer[renderLayer];
		}

		public void UpdateLists()
		{
			if (_componentsNeedSort)
			{
				_components.Sort(CompareUpdatableOrder);
				_componentsNeedSort = false;
			}

			if (_unsortedRenderLayers.Count > 0)
			{
				for (int i = 0, count = _unsortedRenderLayers.Count; i < count; i++)
				{
					if (_componentsByRenderLayer.TryGetValue(_unsortedRenderLayers[i], out var renderLayerComponents))
						renderLayerComponents.Sort(CompareUpdatableOrder);
				}

				_unsortedRenderLayers.Clear();
			}
		}
	}
}