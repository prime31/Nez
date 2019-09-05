using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Spatial
{
	/// <summary>
	/// A QuadTree Object that provides fast and efficient storage of objects in a world space.
	/// </summary>
	/// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
	public class QuadTreeNode<T> where T : IQuadTreeStorable
	{
		// How many objects can exist in a QuadTree before it sub divides itself
		const int maxObjectsPerNode = 2;


		/// <summary>
		/// The area this QuadTree represents.
		/// </summary>
		public Rectangle QuadRect
		{
			get { return _rect; }
		}

		/// <summary>
		/// The top left child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> TopLeftChild
		{
			get { return _childTL; }
		}

		/// <summary>
		/// The top right child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> TopRightChild
		{
			get { return _childTR; }
		}

		/// <summary>
		/// The bottom left child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> BottomLeftChild
		{
			get { return _childBL; }
		}

		/// <summary>
		/// The bottom right child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> BottomRightChild
		{
			get { return _childBR; }
		}

		/// <summary>
		/// This QuadTree's parent
		/// </summary>
		public QuadTreeNode<T> Parent
		{
			get { return _parent; }
		}

		/// <summary>
		/// How many total objects are contained within this QuadTree (ie, includes children)
		/// </summary>
		public int Count
		{
			get { return ObjectCount(); }
		}

		/// <summary>
		/// Returns true if this is a empty leaf node
		/// </summary>
		public bool IsEmptyLeaf
		{
			get { return Count == 0 && _childTL == null; }
		}


		// The objects in this QuadTree
		List<QuadTreeObject<T>> _objects;

		// The area this QuadTree represents
		Rectangle _rect;

		// The parent of this quad
		QuadTreeNode<T> _parent;

		// Top Left Child
		QuadTreeNode<T> _childTL;

		// Top Right Child
		QuadTreeNode<T> _childTR;

		// Bottom Left Child
		QuadTreeNode<T> _childBL;

		// Bottom Right Child
		QuadTreeNode<T> _childBR;


		/// <summary>
		/// Creates a QuadTree for the specified area.
		/// </summary>
		/// <param name="rect">The area this QuadTree object will encompass.</param>
		public QuadTreeNode(Rectangle rect)
		{
			_rect = rect;
		}


		/// <summary>
		/// Creates a QuadTree for the specified area.
		/// </summary>
		/// <param name="x">The top-left position of the area rectangle.</param>
		/// <param name="y">The top-right position of the area rectangle.</param>
		/// <param name="width">The width of the area rectangle.</param>
		/// <param name="height">The height of the area rectangle.</param>
		public QuadTreeNode(int x, int y, int width, int height)
		{
			_rect = new Rectangle(x, y, width, height);
		}


		QuadTreeNode(QuadTreeNode<T> parent, Rectangle rect) : this(rect)
		{
			_parent = parent;
		}


		/// <summary>
		/// Add an item to the object list.
		/// </summary>
		/// <param name="item">The item to add.</param>
		void Add(QuadTreeObject<T> item)
		{
			if (_objects == null)
				_objects = new List<QuadTreeObject<T>>();

			item.owner = this;
			_objects.Add(item);
		}


		/// <summary>
		/// Remove an item from the object list.
		/// </summary>
		/// <param name="item">The object to remove.</param>
		void Remove(QuadTreeObject<T> item)
		{
			if (_objects != null)
			{
				int removeIndex = _objects.IndexOf(item);
				if (removeIndex >= 0)
				{
					_objects[removeIndex] = _objects[_objects.Count - 1];
					_objects.RemoveAt(_objects.Count - 1);
				}
			}
		}


		/// <summary>
		/// Get the total for all objects in this QuadTree, including children.
		/// </summary>
		/// <returns>The number of objects contained within this QuadTree and its children.</returns>
		int ObjectCount()
		{
			var count = 0;

			// Add the objects at this level
			if (_objects != null)
			{
				count += _objects.Count;
			}

			// Add the objects that are contained in the children
			if (_childTL != null)
			{
				count += _childTL.ObjectCount();
				count += _childTR.ObjectCount();
				count += _childBL.ObjectCount();
				count += _childBR.ObjectCount();
			}

			return count;
		}


		/// <summary>
		/// Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
		/// </summary>
		void Subdivide()
		{
			// We've reached capacity, subdivide...
			var size = new Point(_rect.Width / 2, _rect.Height / 2);
			var mid = new Point(_rect.X + size.X, _rect.Y + size.Y);

			_childTL = new QuadTreeNode<T>(this, new Rectangle(_rect.Left, _rect.Top, size.X, size.Y));
			_childTR = new QuadTreeNode<T>(this, new Rectangle(mid.X, _rect.Top, size.X, size.Y));
			_childBL = new QuadTreeNode<T>(this, new Rectangle(_rect.Left, mid.Y, size.X, size.Y));
			_childBR = new QuadTreeNode<T>(this, new Rectangle(mid.X, mid.Y, size.X, size.Y));

			// If they're completely contained by the quad, bump objects down
			for (var i = 0; i < _objects.Count; i++)
			{
				var destTree = GetDestinationTree(_objects[i]);

				if (destTree != this)
				{
					// Insert to the appropriate tree, remove the object, and back up one in the loop
					destTree.Insert(_objects[i]);
					Remove(_objects[i]);
					i--;
				}
			}
		}


		/// <summary>
		/// Get the child Quad that would contain an object.
		/// </summary>
		/// <param name="item">The object to get a child for.</param>
		/// <returns></returns>
		QuadTreeNode<T> GetDestinationTree(QuadTreeObject<T> item)
		{
			// If a child can't contain an object, it will live in this Quad
			var destTree = this;

			if (_childTL.QuadRect.Contains(item.Data.Bounds))
				destTree = _childTL;
			else if (_childTR.QuadRect.Contains(item.Data.Bounds))
				destTree = _childTR;
			else if (_childBL.QuadRect.Contains(item.Data.Bounds))
				destTree = _childBL;
			else if (_childBR.QuadRect.Contains(item.Data.Bounds))
				destTree = _childBR;

			return destTree;
		}


		void Relocate(QuadTreeObject<T> item)
		{
			// Are we still inside our parent?
			if (QuadRect.Contains(item.Data.Bounds))
			{
				// Good, have we moved inside any of our children?
				if (_childTL != null)
				{
					var dest = GetDestinationTree(item);
					if (item.owner != dest)
					{
						// Delete the item from this quad and add it to our child
						// Note: Do NOT clean during this call, it can potentially delete our destination quad
						var formerOwner = item.owner;
						Delete(item, false);
						dest.Insert(item);

						// Clean up ourselves
						formerOwner.CleanUpwards();
					}
				}
			}
			else
			{
				// We don't fit here anymore, move up, if we can
				if (_parent != null)
					_parent.Relocate(item);
			}
		}


		void CleanUpwards()
		{
			if (_childTL != null)
			{
				// If all the children are empty leaves, delete all the children
				if (_childTL.IsEmptyLeaf && _childTR.IsEmptyLeaf && _childBL.IsEmptyLeaf && _childBR.IsEmptyLeaf)
				{
					_childTL = null;
					_childTR = null;
					_childBL = null;
					_childBR = null;

					if (_parent != null && Count == 0)
						_parent.CleanUpwards();
				}
			}
			else
			{
				// I could be one of 4 empty leaves, tell my parent to clean up
				if (_parent != null && Count == 0)
					_parent.CleanUpwards();
			}
		}


		/// <summary>
		/// Clears the QuadTree of all objects, including any objects living in its children.
		/// </summary>
		internal void Clear()
		{
			// Clear out the children, if we have any
			if (_childTL != null)
			{
				_childTL.Clear();
				_childTR.Clear();
				_childBL.Clear();
				_childBR.Clear();
			}

			// Clear any objects at this level
			if (_objects != null)
			{
				_objects.Clear();
				_objects = null;
			}

			// Set the children to null
			_childTL = null;
			_childTR = null;
			_childBL = null;
			_childBR = null;
		}


		/// <summary>
		/// Deletes an item from this QuadTree. If the object is removed causes this Quad to have no objects in its children, it's children will be removed as well.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <param name="clean">Whether or not to clean the tree</param>
		internal void Delete(QuadTreeObject<T> item, bool clean)
		{
			if (item.owner != null)
			{
				if (item.owner == this)
				{
					Remove(item);
					if (clean)
						CleanUpwards();
				}
				else
				{
					item.owner.Delete(item, clean);
				}
			}
		}


		/// <summary>
		/// Insert an item into this QuadTree object.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		internal void Insert(QuadTreeObject<T> item)
		{
			// If this quad doesn't contain the items rectangle, do nothing, unless we are the root
			if (!_rect.Contains(item.Data.Bounds))
			{
				Insist.IsNull(_parent, "We are not the root, and this object doesn't fit here. How did we get here?");
				if (_parent == null)
				{
					// This object is outside of the QuadTree bounds, we should add it at the root level
					Add(item);
				}
				else
				{
					return;
				}
			}

			if (_objects == null || (_childTL == null && _objects.Count + 1 <= maxObjectsPerNode))
			{
				// If there's room to add the object, just add it
				Add(item);
			}
			else
			{
				// No quads, create them and bump objects down where appropriate
				if (_childTL == null)
					Subdivide();

				// Find out which tree this object should go in and add it there
				var destTree = GetDestinationTree(item);
				if (destTree == this)
					Add(item);
				else
					destTree.Insert(item);
			}
		}


		/// <summary>
		/// Get the objects in this tree that intersect with the specified rectangle.
		/// </summary>
		/// <param name="searchRect">The rectangle to find objects in.</param>
		internal List<T> GetObjects(Rectangle searchRect)
		{
			var results = new List<T>();
			GetObjects(searchRect, ref results);
			return results;
		}


		/// <summary>
		/// Get the objects in this tree that intersect with the specified rectangle.
		/// </summary>
		/// <param name="searchRect">The rectangle to find objects in.</param>
		/// <param name="results">A reference to a list that will be populated with the results.</param>
		internal void GetObjects(Rectangle searchRect, ref List<T> results)
		{
			// We can't do anything if the results list doesn't exist
			if (results != null)
			{
				if (searchRect.Contains(this._rect))
				{
					// If the search area completely contains this quad, just get every object this quad and all it's children have
					GetAllObjects(ref results);
				}
				else if (searchRect.Intersects(this._rect))
				{
					// Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
					if (_objects != null)
					{
						for (int i = 0; i < _objects.Count; i++)
						{
							if (searchRect.Intersects(_objects[i].Data.Bounds))
								results.Add(_objects[i].Data);
						}
					}

					// Get the objects for the search rectangle from the children
					if (_childTL != null)
					{
						_childTL.GetObjects(searchRect, ref results);
						_childTR.GetObjects(searchRect, ref results);
						_childBL.GetObjects(searchRect, ref results);
						_childBR.GetObjects(searchRect, ref results);
					}
				}
			}
		}


		/// <summary>
		/// Get all objects in this Quad, and it's children.
		/// </summary>
		/// <param name="results">A reference to a list in which to store the objects.</param>
		internal void GetAllObjects(ref List<T> results)
		{
			// If this Quad has objects, add them
			if (_objects != null)
			{
				for (var i = 0; i < _objects.Count; i++)
					results.Add(_objects[i].Data);
			}

			// If we have children, get their objects too
			if (_childTL != null)
			{
				_childTL.GetAllObjects(ref results);
				_childTR.GetAllObjects(ref results);
				_childBL.GetAllObjects(ref results);
				_childBR.GetAllObjects(ref results);
			}
		}


		/// <summary>
		/// Moves the QuadTree object in the tree
		/// </summary>
		/// <param name="item">The item that has moved</param>
		internal void Move(QuadTreeObject<T> item)
		{
			if (item.owner != null)
				item.owner.Relocate(item);
			else
				Relocate(item);
		}
	}
}