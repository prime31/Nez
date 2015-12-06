using System;


namespace Nez.Spatial
{
	/// <summary>
	/// Used internally to attach an Owner to each object stored in the QuadTree
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class QuadTreeObject<T> where T : IQuadTreeStorable
	{
		/// <summary>
		/// The wrapped data value
		/// </summary>
		public T data;

		/// <summary>
		/// The QuadTreeNode that owns this object
		/// </summary>
		internal QuadTreeNode<T> owner;

		/// <summary>
		/// Wraps the data value
		/// </summary>
		/// <param name="data">The data value to wrap</param>
		public QuadTreeObject( T data )
		{
			this.data = data;
		}
	}
}

