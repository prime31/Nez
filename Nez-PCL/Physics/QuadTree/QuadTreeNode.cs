using System;
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

		// The objects in this QuadTree
		List<QuadTreeObject<T>> _objects = null;
		Rectangle _rect;
		// The area this QuadTree represents

		QuadTreeNode<T> _parent = null;
		// The parent of this quad

		QuadTreeNode<T> _childTL = null;
		// Top Left Child
		QuadTreeNode<T> _childTR = null;
		// Top Right Child
		QuadTreeNode<T> _childBL = null;
		// Bottom Left Child
		QuadTreeNode<T> _childBR = null;
		// Bottom Right Child


		/// <summary>
		/// The area this QuadTree represents.
		/// </summary>
		public Rectangle quadRect
		{
			get { return _rect; }
		}

		/// <summary>
		/// The top left child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> topLeftChild
		{
			get { return _childTL; }
		}

		/// <summary>
		/// The top right child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> topRightChild
		{
			get { return _childTR; }
		}

		/// <summary>
		/// The bottom left child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> bottomLeftChild
		{
			get { return _childBL; }
		}

		/// <summary>
		/// The bottom right child for this QuadTree
		/// </summary>
		public QuadTreeNode<T> bottomRightChild
		{
			get { return _childBR; }
		}

		/// <summary>
		/// This QuadTree's parent
		/// </summary>
		public QuadTreeNode<T> parent
		{
			get { return _parent; }
		}

		/// <summary>
		/// The objects contained in this QuadTree at it's level (ie, excludes children)
		/// </summary>
		//public List<T> Objects { get { return m_objects; } }
		internal List<QuadTreeObject<T>> objects
		{
			get { return _objects; }
		}

		/// <summary>
		/// How many total objects are contained within this QuadTree (ie, includes children)
		/// </summary>
		public int count
		{
			get { return objectCount(); }
		}

		/// <summary>
		/// Returns true if this is a empty leaf node
		/// </summary>
		public bool isEmptyLeaf
		{
			get { return count == 0 && _childTL == null; }
		}


		/// <summary>
		/// Creates a QuadTree for the specified area.
		/// </summary>
		/// <param name="rect">The area this QuadTree object will encompass.</param>
		public QuadTreeNode( Rectangle rect )
		{
			this._rect = rect;
		}


		/// <summary>
		/// Creates a QuadTree for the specified area.
		/// </summary>
		/// <param name="x">The top-left position of the area rectangle.</param>
		/// <param name="y">The top-right position of the area rectangle.</param>
		/// <param name="width">The width of the area rectangle.</param>
		/// <param name="height">The height of the area rectangle.</param>
		public QuadTreeNode( int x, int y, int width, int height )
		{
			_rect = new Rectangle( x, y, width, height );
		}


		QuadTreeNode( QuadTreeNode<T> parent, Rectangle rect ) : this( rect )
		{
			this._parent = parent;
		}


		/// <summary>
		/// Add an item to the object list.
		/// </summary>
		/// <param name="item">The item to add.</param>
		void add( QuadTreeObject<T> item )
		{
			if( _objects == null )
			{
				//m_objects = new List<T>();
				_objects = new List<QuadTreeObject<T>>();
			}

			item.owner = this;
			_objects.Add( item );
		}


		/// <summary>
		/// Remove an item from the object list.
		/// </summary>
		/// <param name="item">The object to remove.</param>
		void remove( QuadTreeObject<T> item )
		{
			if( _objects != null )
			{
				int removeIndex = _objects.IndexOf( item );
				if( removeIndex >= 0 )
				{
					_objects[removeIndex] = _objects[_objects.Count - 1];
					_objects.RemoveAt( _objects.Count - 1 );
				}
			}
		}


		/// <summary>
		/// Get the total for all objects in this QuadTree, including children.
		/// </summary>
		/// <returns>The number of objects contained within this QuadTree and its children.</returns>
		int objectCount()
		{
			int count = 0;

			// Add the objects at this level
			if( _objects != null )
			{
				count += _objects.Count;
			}

			// Add the objects that are contained in the children
			if( _childTL != null )
			{
				count += _childTL.objectCount();
				count += _childTR.objectCount();
				count += _childBL.objectCount();
				count += _childBR.objectCount();
			}

			return count;
		}


		/// <summary>
		/// Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
		/// </summary>
		void subdivide()
		{
			// We've reached capacity, subdivide...
			Point size = new Point( _rect.Width / 2, _rect.Height / 2 );
			Point mid = new Point( _rect.X + size.X, _rect.Y + size.Y );

			_childTL = new QuadTreeNode<T>( this, new Rectangle( _rect.Left, _rect.Top, size.X, size.Y ) );
			_childTR = new QuadTreeNode<T>( this, new Rectangle( mid.X, _rect.Top, size.X, size.Y ) );
			_childBL = new QuadTreeNode<T>( this, new Rectangle( _rect.Left, mid.Y, size.X, size.Y ) );
			_childBR = new QuadTreeNode<T>( this, new Rectangle( mid.X, mid.Y, size.X, size.Y ) );

			// If they're completely contained by the quad, bump objects down
			for( int i = 0; i < _objects.Count; i++ )
			{
				QuadTreeNode<T> destTree = getDestinationTree( _objects[i] );

				if( destTree != this )
				{
					// Insert to the appropriate tree, remove the object, and back up one in the loop
					destTree.insert( _objects[i] );
					remove( _objects[i] );
					i--;
				}
			}
		}


		/// <summary>
		/// Get the child Quad that would contain an object.
		/// </summary>
		/// <param name="item">The object to get a child for.</param>
		/// <returns></returns>
		QuadTreeNode<T> getDestinationTree( QuadTreeObject<T> item )
		{
			// If a child can't contain an object, it will live in this Quad
			QuadTreeNode<T> destTree = this;

			if( _childTL.quadRect.Contains( item.data.bounds ) )
			{
				destTree = _childTL;
			}
			else if( _childTR.quadRect.Contains( item.data.bounds ) )
			{
				destTree = _childTR;
			}
			else if( _childBL.quadRect.Contains( item.data.bounds ) )
			{
				destTree = _childBL;
			}
			else if( _childBR.quadRect.Contains( item.data.bounds ) )
			{
				destTree = _childBR;
			}

			return destTree;
		}


		void relocate( QuadTreeObject<T> item )
		{
			// Are we still inside our parent?
			if( quadRect.Contains( item.data.bounds ) )
			{
				// Good, have we moved inside any of our children?
				if( _childTL != null )
				{
					QuadTreeNode<T> dest = getDestinationTree( item );
					if( item.owner != dest )
					{
						// Delete the item from this quad and add it to our child
						// Note: Do NOT clean during this call, it can potentially delete our destination quad
						QuadTreeNode<T> formerOwner = item.owner;
						delete( item, false );
						dest.insert( item );

						// Clean up ourselves
						formerOwner.cleanUpwards();
					}
				}
			}
			else
			{
				// We don't fit here anymore, move up, if we can
				if( _parent != null )
				{
					_parent.relocate( item );
				}
			}
		}


		void cleanUpwards()
		{
			if( _childTL != null )
			{
				// If all the children are empty leaves, delete all the children
				if( _childTL.isEmptyLeaf &&
					_childTR.isEmptyLeaf &&
					_childBL.isEmptyLeaf &&
					_childBR.isEmptyLeaf )
				{
					_childTL = null;
					_childTR = null;
					_childBL = null;
					_childBR = null;

					if( _parent != null && count == 0 )
					{
						_parent.cleanUpwards();
					}
				}
			}
			else
			{
				// I could be one of 4 empty leaves, tell my parent to clean up
				if( _parent != null && count == 0 )
				{
					_parent.cleanUpwards();
				}
			}
		}


		/// <summary>
		/// Clears the QuadTree of all objects, including any objects living in its children.
		/// </summary>
		internal void clear()
		{
			// Clear out the children, if we have any
			if( _childTL != null )
			{
				_childTL.clear();
				_childTR.clear();
				_childBL.clear();
				_childBR.clear();
			}

			// Clear any objects at this level
			if( _objects != null )
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
		internal void delete( QuadTreeObject<T> item, bool clean )
		{
			if( item.owner != null )
			{
				if( item.owner == this )
				{
					remove( item );
					if( clean )
					{
						cleanUpwards();
					}
				}
				else
				{
					item.owner.delete( item, clean );
				}
			}
		}



		/// <summary>
		/// Insert an item into this QuadTree object.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		internal void insert( QuadTreeObject<T> item )
		{
			// If this quad doesn't contain the items rectangle, do nothing, unless we are the root
			if( !_rect.Contains( item.data.bounds ) )
			{
				System.Diagnostics.Debug.Assert( _parent == null, "We are not the root, and this object doesn't fit here. How did we get here?" );
				if( _parent == null )
				{
					// This object is outside of the QuadTree bounds, we should add it at the root level
					add( item );
				}
				else
				{
					return;
				}
			}

			if( _objects == null ||
				( _childTL == null && _objects.Count + 1 <= maxObjectsPerNode ) )
			{
				// If there's room to add the object, just add it
				add( item );
			}
			else
			{
				// No quads, create them and bump objects down where appropriate
				if( _childTL == null )
				{
					subdivide();
				}

				// Find out which tree this object should go in and add it there
				QuadTreeNode<T> destTree = getDestinationTree( item );
				if( destTree == this )
				{
					add( item );
				}
				else
				{
					destTree.insert( item );
				}
			}
		}


		/// <summary>
		/// Get the objects in this tree that intersect with the specified rectangle.
		/// </summary>
		/// <param name="searchRect">The rectangle to find objects in.</param>
		internal List<T> getObjects( Rectangle searchRect )
		{
			List<T> results = new List<T>();
			getObjects( searchRect, ref results );
			return results;
		}


		/// <summary>
		/// Get the objects in this tree that intersect with the specified rectangle.
		/// </summary>
		/// <param name="searchRect">The rectangle to find objects in.</param>
		/// <param name="results">A reference to a list that will be populated with the results.</param>
		internal void getObjects( Rectangle searchRect, ref List<T> results )
		{
			// We can't do anything if the results list doesn't exist
			if( results != null )
			{
				if( searchRect.Contains( this._rect ) )
				{
					// If the search area completely contains this quad, just get every object this quad and all it's children have
					getAllObjects( ref results );
				}
				else if( searchRect.Intersects( this._rect ) )
				{
					// Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
					if( _objects != null )
					{
						for( int i = 0; i < _objects.Count; i++ )
						{
							if( searchRect.Intersects( _objects[i].data.bounds ) )
							{
								results.Add( _objects[i].data );
							}
						}
					}

					// Get the objects for the search rectangle from the children
					if( _childTL != null )
					{
						_childTL.getObjects( searchRect, ref results );
						_childTR.getObjects( searchRect, ref results );
						_childBL.getObjects( searchRect, ref results );
						_childBR.getObjects( searchRect, ref results );
					}
				}
			}
		}


		/// <summary>
		/// Get all objects in this Quad, and it's children.
		/// </summary>
		/// <param name="results">A reference to a list in which to store the objects.</param>
		internal void getAllObjects( ref List<T> results )
		{
			// If this Quad has objects, add them
			if( _objects != null )
			{
				foreach( QuadTreeObject<T> qto in _objects )
				{
					results.Add( qto.data );
				}
			}

			// If we have children, get their objects too
			if( _childTL != null )
			{
				_childTL.getAllObjects( ref results );
				_childTR.getAllObjects( ref results );
				_childBL.getAllObjects( ref results );
				_childBR.getAllObjects( ref results );
			}
		}


		/// <summary>
		/// Moves the QuadTree object in the tree
		/// </summary>
		/// <param name="item">The item that has moved</param>
		internal void move( QuadTreeObject<T> item )
		{
			if( item.owner != null )
			{
				item.owner.relocate( item );
			}
			else
			{
				relocate( item );
			}
		}

	}

}

