/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision
{
	/// <summary>
	/// A node in the dynamic tree. The client does not interact with this directly.
	/// </summary>
	class TreeNode
	{
		/// <summary>
		/// Enlarged AABB
		/// </summary>
		internal AABB aabb;

		internal int child1;
		internal int child2;

		internal int height;
		internal int parentOrNext;
		internal FixtureProxy userData;

		internal bool IsLeaf()
		{
			return child1 == DynamicTree.nullNode;
		}
	}


	/// <summary>
	/// A dynamic tree arranges data in a binary tree to accelerate
	/// queries such as volume queries and ray casts. Leafs are proxies
	/// with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
	/// so that the proxy AABB is bigger than the client object. This allows the client
	/// object to move by small amounts without triggering a tree update.
	///
	/// Nodes are pooled and relocatable, so we use node indices rather than pointers.
	/// </summary>
	public class DynamicTree
	{
		#region Properties/Fields

		/// <summary>
		/// Compute the height of the binary tree in O(N) time. Should not be called often.
		/// </summary>
		public int Height
		{
			get
			{
				if (_root == nullNode)
					return 0;

				return _nodes[_root].height;
			}
		}

		/// <summary>
		/// Get the ratio of the sum of the node areas to the root area.
		/// </summary>
		public float AreaRatio
		{
			get
			{
				if (_root == nullNode)
					return 0.0f;

				var root = _nodes[_root];
				float rootArea = root.aabb.Perimeter;

				float totalArea = 0.0f;
				for (int i = 0; i < _nodeCapacity; ++i)
				{
					var node = _nodes[i];
					if (node.height < 0)
					{
						// Free node in pool
						continue;
					}

					totalArea += node.aabb.Perimeter;
				}

				return totalArea / rootArea;
			}
		}

		/// <summary>
		/// Get the maximum balance of an node in the tree. The balance is the difference
		/// in height of the two children of a node.
		/// </summary>
		public int MaxBalance
		{
			get
			{
				int maxBalance = 0;
				for (int i = 0; i < _nodeCapacity; ++i)
				{
					var node = _nodes[i];
					if (node.height <= 1)
						continue;

					Debug.Assert(node.IsLeaf() == false);

					int child1 = node.child1;
					int child2 = node.child2;
					int balance = Math.Abs(_nodes[child2].height - _nodes[child1].height);
					maxBalance = Math.Max(maxBalance, balance);
				}

				return maxBalance;
			}
		}

		Stack<int> _raycastStack = new Stack<int>(256);
		Stack<int> _queryStack = new Stack<int>(256);
		int _freeList;
		int _nodeCapacity;
		int _nodeCount;
		TreeNode[] _nodes;
		int _root;
		internal const int nullNode = -1;

		#endregion


		/// <summary>
		/// Constructing the tree initializes the node pool.
		/// </summary>
		public DynamicTree()
		{
			_root = nullNode;

			_nodeCapacity = 16;
			_nodeCount = 0;
			_nodes = new TreeNode[_nodeCapacity];

			// Build a linked list for the free list.
			for (var i = 0; i < _nodeCapacity - 1; ++i)
			{
				_nodes[i] = new TreeNode();
				_nodes[i].parentOrNext = i + 1;
				_nodes[i].height = 1;
			}

			_nodes[_nodeCapacity - 1] = new TreeNode();
			_nodes[_nodeCapacity - 1].parentOrNext = nullNode;
			_nodes[_nodeCapacity - 1].height = 1;
			_freeList = 0;
		}

		/// <summary>
		/// Create a proxy in the tree as a leaf node. We return the index
		/// of the node instead of a pointer so that we can grow
		/// the node pool.        
		/// /// </summary>
		/// <param name="aabb">The aabb.</param>
		/// <param name="userData">The user data.</param>
		/// <returns>Index of the created proxy</returns>
		public int AddProxy(ref AABB aabb, FixtureProxy userData)
		{
			int proxyId = AllocateNode();

			// Fatten the aabb.
			var r = new Vector2(Settings.AabbExtension, Settings.AabbExtension);
			_nodes[proxyId].aabb.LowerBound = aabb.LowerBound - r;
			_nodes[proxyId].aabb.UpperBound = aabb.UpperBound + r;
			_nodes[proxyId].userData = userData;
			_nodes[proxyId].height = 0;

			InsertLeaf(proxyId);

			return proxyId;
		}

		/// <summary>
		/// Destroy a proxy. This asserts if the id is invalid.
		/// </summary>
		/// <param name="proxyId">The proxy id.</param>
		public void RemoveProxy(int proxyId)
		{
			Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
			Debug.Assert(_nodes[proxyId].IsLeaf());

			RemoveLeaf(proxyId);
			FreeNode(proxyId);
		}

		/// <summary>
		/// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
		/// then the proxy is removed from the tree and re-inserted. Otherwise
		/// the function returns immediately.
		/// </summary>
		/// <param name="proxyId">The proxy id.</param>
		/// <param name="aabb">The aabb.</param>
		/// <param name="displacement">The displacement.</param>
		/// <returns>true if the proxy was re-inserted.</returns>
		public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
		{
			Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);

			Debug.Assert(_nodes[proxyId].IsLeaf());

			if (_nodes[proxyId].aabb.Contains(ref aabb))
			{
				return false;
			}

			RemoveLeaf(proxyId);

			// Extend AABB.
			AABB b = aabb;
			var r = new Vector2(Settings.AabbExtension, Settings.AabbExtension);
			b.LowerBound = b.LowerBound - r;
			b.UpperBound = b.UpperBound + r;

			// Predict AABB displacement.
			var d = Settings.AabbMultiplier * displacement;
			if (d.X < 0.0f)
				b.LowerBound.X += d.X;
			else
				b.UpperBound.X += d.X;

			if (d.Y < 0.0f)
				b.LowerBound.Y += d.Y;
			else
				b.UpperBound.Y += d.Y;

			_nodes[proxyId].aabb = b;

			InsertLeaf(proxyId);
			return true;
		}

		/// <summary>
		/// Get proxy user data.
		/// </summary>
		/// <param name="proxyId">The proxy id.</param>
		/// <returns>the proxy user data or 0 if the id is invalid.</returns>
		public FixtureProxy GetUserData(int proxyId)
		{
			Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
			return _nodes[proxyId].userData;
		}

		/// <summary>
		/// Get the fat AABB for a proxy.
		/// </summary>
		/// <param name="proxyId">The proxy id.</param>
		/// <param name="fatAABB">The fat AABB.</param>
		public void GetFatAABB(int proxyId, out AABB fatAABB)
		{
			Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
			fatAABB = _nodes[proxyId].aabb;
		}

		/// <summary>
		/// Query an AABB for overlapping proxies. The callback class is called for each proxy that overlaps the supplied AABB.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="aabb">The aabb.</param>
		public void Query(Func<int, bool> callback, ref AABB aabb)
		{
			_queryStack.Clear();
			_queryStack.Push(_root);

			while (_queryStack.Count > 0)
			{
				var nodeId = _queryStack.Pop();
				if (nodeId == nullNode)
					continue;

				var node = _nodes[nodeId];
				if (AABB.TestOverlap(ref node.aabb, ref aabb))
				{
					if (node.IsLeaf())
					{
						var proceed = callback(nodeId);
						if (!proceed)
							return;
					}
					else
					{
						_queryStack.Push(node.child1);
						_queryStack.Push(node.child2);
					}
				}
			}
		}

		/// <summary>
		/// Query an AABB for overlapping proxies
		/// </summary>
		/// <param name="aabb">Aabb.</param>
		/// <param name="fixtures">Fixtures.</param>
		public void Query(ref AABB aabb, List<Fixture> fixtures)
		{
			_queryStack.Clear();
			_queryStack.Push(_root);

			while (_queryStack.Count > 0)
			{
				var nodeId = _queryStack.Pop();
				if (nodeId == nullNode)
					continue;

				var node = _nodes[nodeId];
				if (AABB.TestOverlap(ref node.aabb, ref aabb))
				{
					if (node.IsLeaf())
					{
						fixtures.Add(GetUserData(nodeId).Fixture);
					}
					else
					{
						_queryStack.Push(node.child1);
						_queryStack.Push(node.child2);
					}
				}
			}
		}

		/// <summary>
		/// Ray-cast against the proxies in the tree. This relies on the callback
		/// to perform a exact ray-cast in the case were the proxy contains a Shape.
		/// The callback also performs the any collision filtering. This has performance
		/// roughly equal to k * log(n), where k is the number of collisions and n is the
		/// number of proxies in the tree.
		/// </summary>
		/// <param name="callback">A callback class that is called for each proxy that is hit by the ray.</param>
		/// <param name="input">The ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).</param>
		public void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input)
		{
			var p1 = input.Point1;
			var p2 = input.Point2;
			var r = p2 - p1;
			Debug.Assert(r.LengthSquared() > 0.0f);
			Nez.Vector2Ext.Normalize(ref r);

			// v is perpendicular to the segment.
			var absV = MathUtils.Abs(new Vector2(-r.Y, r.X)); //FPE: Inlined the 'v' variable

			// Separating axis for segment (Gino, p80).
			// |dot(v, p1 - c)| > dot(|v|, h)

			var maxFraction = input.MaxFraction;

			// Build a bounding box for the segment.
			var segmentAABB = new AABB();
			{
				var t = p1 + maxFraction * (p2 - p1);
				Vector2.Min(ref p1, ref t, out segmentAABB.LowerBound);
				Vector2.Max(ref p1, ref t, out segmentAABB.UpperBound);
			}

			_raycastStack.Clear();
			_raycastStack.Push(_root);

			while (_raycastStack.Count > 0)
			{
				int nodeId = _raycastStack.Pop();
				if (nodeId == nullNode)
					continue;

				var node = _nodes[nodeId];
				if (AABB.TestOverlap(ref node.aabb, ref segmentAABB) == false)
					continue;

				// Separating axis for segment (Gino, p80).
				// |dot(v, p1 - c)| > dot(|v|, h)
				var c = node.aabb.Center;
				var h = node.aabb.Extents;
				var separation = Math.Abs(Vector2.Dot(new Vector2(-r.Y, r.X), p1 - c)) - Vector2.Dot(absV, h);
				if (separation > 0.0f)
					continue;

				if (node.IsLeaf())
				{
					RayCastInput subInput;
					subInput.Point1 = input.Point1;
					subInput.Point2 = input.Point2;
					subInput.MaxFraction = maxFraction;

					float value = callback(subInput, nodeId);

					if (value == 0.0f)
					{
						// the client has terminated the raycast.
						return;
					}

					if (value > 0.0f)
					{
						// Update segment bounding box.
						maxFraction = value;
						var t = p1 + maxFraction * (p2 - p1);
						segmentAABB.LowerBound = Vector2.Min(p1, t);
						segmentAABB.UpperBound = Vector2.Max(p1, t);
					}
				}
				else
				{
					_raycastStack.Push(node.child1);
					_raycastStack.Push(node.child2);
				}
			}
		}

		int AllocateNode()
		{
			// Expand the node pool as needed.
			if (_freeList == nullNode)
			{
				Debug.Assert(_nodeCount == _nodeCapacity);

				// The free list is empty. Rebuild a bigger pool.
				var oldNodes = _nodes;
				_nodeCapacity *= 2;
				_nodes = new TreeNode[_nodeCapacity];
				Array.Copy(oldNodes, _nodes, _nodeCount);

				// Build a linked list for the free list. The parent
				// pointer becomes the "next" pointer.
				for (var i = _nodeCount; i < _nodeCapacity - 1; ++i)
				{
					_nodes[i] = new TreeNode();
					_nodes[i].parentOrNext = i + 1;
					_nodes[i].height = -1;
				}

				_nodes[_nodeCapacity - 1] = new TreeNode();
				_nodes[_nodeCapacity - 1].parentOrNext = nullNode;
				_nodes[_nodeCapacity - 1].height = -1;
				_freeList = _nodeCount;
			}

			// Peel a node off the free list.
			int nodeId = _freeList;
			_freeList = _nodes[nodeId].parentOrNext;
			_nodes[nodeId].parentOrNext = nullNode;
			_nodes[nodeId].child1 = nullNode;
			_nodes[nodeId].child2 = nullNode;
			_nodes[nodeId].height = 0;
			_nodes[nodeId].userData = default(FixtureProxy);
			++_nodeCount;
			return nodeId;
		}

		void FreeNode(int nodeId)
		{
			Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
			Debug.Assert(0 < _nodeCount);
			_nodes[nodeId].parentOrNext = _freeList;
			_nodes[nodeId].height = -1;
			_freeList = nodeId;
			--_nodeCount;
		}

		void InsertLeaf(int leaf)
		{
			if (_root == nullNode)
			{
				_root = leaf;
				_nodes[_root].parentOrNext = nullNode;
				return;
			}

			// Find the best sibling for this node
			AABB leafAABB = _nodes[leaf].aabb;
			int index = _root;
			while (_nodes[index].IsLeaf() == false)
			{
				int child1 = _nodes[index].child1;
				int child2 = _nodes[index].child2;

				float area = _nodes[index].aabb.Perimeter;

				var combinedAABB = new AABB();
				combinedAABB.Combine(ref _nodes[index].aabb, ref leafAABB);
				float combinedArea = combinedAABB.Perimeter;

				// Cost of creating a new parent for this node and the new leaf
				float cost = 2.0f * combinedArea;

				// Minimum cost of pushing the leaf further down the tree
				float inheritanceCost = 2.0f * (combinedArea - area);

				// Cost of descending into child1
				float cost1;
				if (_nodes[child1].IsLeaf())
				{
					var aabb = new AABB();
					aabb.Combine(ref leafAABB, ref _nodes[child1].aabb);
					cost1 = aabb.Perimeter + inheritanceCost;
				}
				else
				{
					var aabb = new AABB();
					aabb.Combine(ref leafAABB, ref _nodes[child1].aabb);
					float oldArea = _nodes[child1].aabb.Perimeter;
					float newArea = aabb.Perimeter;
					cost1 = (newArea - oldArea) + inheritanceCost;
				}

				// Cost of descending into child2
				float cost2;
				if (_nodes[child2].IsLeaf())
				{
					var aabb = new AABB();
					aabb.Combine(ref leafAABB, ref _nodes[child2].aabb);
					cost2 = aabb.Perimeter + inheritanceCost;
				}
				else
				{
					var aabb = new AABB();
					aabb.Combine(ref leafAABB, ref _nodes[child2].aabb);
					float oldArea = _nodes[child2].aabb.Perimeter;
					float newArea = aabb.Perimeter;
					cost2 = newArea - oldArea + inheritanceCost;
				}

				// Descend according to the minimum cost.
				if (cost < cost1 && cost1 < cost2)
				{
					break;
				}

				// Descend
				if (cost1 < cost2)
					index = child1;
				else
					index = child2;
			}

			var sibling = index;

			// Create a new parent.
			int oldParent = _nodes[sibling].parentOrNext;
			int newParent = AllocateNode();
			_nodes[newParent].parentOrNext = oldParent;
			_nodes[newParent].userData = default(FixtureProxy);
			_nodes[newParent].aabb.Combine(ref leafAABB, ref _nodes[sibling].aabb);
			_nodes[newParent].height = _nodes[sibling].height + 1;

			if (oldParent != nullNode)
			{
				// The sibling was not the root.
				if (_nodes[oldParent].child1 == sibling)
				{
					_nodes[oldParent].child1 = newParent;
				}
				else
				{
					_nodes[oldParent].child2 = newParent;
				}

				_nodes[newParent].child1 = sibling;
				_nodes[newParent].child2 = leaf;
				_nodes[sibling].parentOrNext = newParent;
				_nodes[leaf].parentOrNext = newParent;
			}
			else
			{
				// The sibling was the root.
				_nodes[newParent].child1 = sibling;
				_nodes[newParent].child2 = leaf;
				_nodes[sibling].parentOrNext = newParent;
				_nodes[leaf].parentOrNext = newParent;
				_root = newParent;
			}

			// Walk back up the tree fixing heights and AABBs
			index = _nodes[leaf].parentOrNext;
			while (index != nullNode)
			{
				index = Balance(index);

				int child1 = _nodes[index].child1;
				int child2 = _nodes[index].child2;

				Debug.Assert(child1 != nullNode);
				Debug.Assert(child2 != nullNode);

				_nodes[index].height = 1 + Math.Max(_nodes[child1].height, _nodes[child2].height);
				_nodes[index].aabb.Combine(ref _nodes[child1].aabb, ref _nodes[child2].aabb);

				index = _nodes[index].parentOrNext;
			}

			//Validate();
		}

		void RemoveLeaf(int leaf)
		{
			if (leaf == _root)
			{
				_root = nullNode;
				return;
			}

			int parent = _nodes[leaf].parentOrNext;
			int grandParent = _nodes[parent].parentOrNext;
			int sibling;
			if (_nodes[parent].child1 == leaf)
			{
				sibling = _nodes[parent].child2;
			}
			else
			{
				sibling = _nodes[parent].child1;
			}

			if (grandParent != nullNode)
			{
				// Destroy parent and connect sibling to grandParent.
				if (_nodes[grandParent].child1 == parent)
				{
					_nodes[grandParent].child1 = sibling;
				}
				else
				{
					_nodes[grandParent].child2 = sibling;
				}

				_nodes[sibling].parentOrNext = grandParent;
				FreeNode(parent);

				// Adjust ancestor bounds.
				int index = grandParent;
				while (index != nullNode)
				{
					index = Balance(index);

					int child1 = _nodes[index].child1;
					int child2 = _nodes[index].child2;

					_nodes[index].aabb.Combine(ref _nodes[child1].aabb, ref _nodes[child2].aabb);
					_nodes[index].height = 1 + Math.Max(_nodes[child1].height, _nodes[child2].height);

					index = _nodes[index].parentOrNext;
				}
			}
			else
			{
				_root = sibling;
				_nodes[sibling].parentOrNext = nullNode;
				FreeNode(parent);
			}

			//Validate();
		}

		/// <summary>
		/// Perform a left or right rotation if node A is imbalanced.
		/// </summary>
		/// <param name="iA"></param>
		/// <returns>the new root index.</returns>
		int Balance(int iA)
		{
			Debug.Assert(iA != nullNode);

			var A = _nodes[iA];
			if (A.IsLeaf() || A.height < 2)
				return iA;

			var iB = A.child1;
			var iC = A.child2;
			Debug.Assert(0 <= iB && iB < _nodeCapacity);
			Debug.Assert(0 <= iC && iC < _nodeCapacity);

			var B = _nodes[iB];
			var C = _nodes[iC];

			var balance = C.height - B.height;

			// Rotate C up
			if (balance > 1)
			{
				int iF = C.child1;
				int iG = C.child2;
				var F = _nodes[iF];
				var G = _nodes[iG];
				Debug.Assert(0 <= iF && iF < _nodeCapacity);
				Debug.Assert(0 <= iG && iG < _nodeCapacity);

				// Swap A and C
				C.child1 = iA;
				C.parentOrNext = A.parentOrNext;
				A.parentOrNext = iC;

				// A's old parent should point to C
				if (C.parentOrNext != nullNode)
				{
					if (_nodes[C.parentOrNext].child1 == iA)
					{
						_nodes[C.parentOrNext].child1 = iC;
					}
					else
					{
						Debug.Assert(_nodes[C.parentOrNext].child2 == iA);
						_nodes[C.parentOrNext].child2 = iC;
					}
				}
				else
				{
					_root = iC;
				}

				// Rotate
				if (F.height > G.height)
				{
					C.child2 = iF;
					A.child2 = iG;
					G.parentOrNext = iA;
					A.aabb.Combine(ref B.aabb, ref G.aabb);
					C.aabb.Combine(ref A.aabb, ref F.aabb);

					A.height = 1 + Math.Max(B.height, G.height);
					C.height = 1 + Math.Max(A.height, F.height);
				}
				else
				{
					C.child2 = iG;
					A.child2 = iF;
					F.parentOrNext = iA;
					A.aabb.Combine(ref B.aabb, ref F.aabb);
					C.aabb.Combine(ref A.aabb, ref G.aabb);

					A.height = 1 + Math.Max(B.height, F.height);
					C.height = 1 + Math.Max(A.height, G.height);
				}

				return iC;
			}

			// Rotate B up
			if (balance < -1)
			{
				int iD = B.child1;
				int iE = B.child2;
				var D = _nodes[iD];
				var E = _nodes[iE];
				Debug.Assert(0 <= iD && iD < _nodeCapacity);
				Debug.Assert(0 <= iE && iE < _nodeCapacity);

				// Swap A and B
				B.child1 = iA;
				B.parentOrNext = A.parentOrNext;
				A.parentOrNext = iB;

				// A's old parent should point to B
				if (B.parentOrNext != nullNode)
				{
					if (_nodes[B.parentOrNext].child1 == iA)
					{
						_nodes[B.parentOrNext].child1 = iB;
					}
					else
					{
						Debug.Assert(_nodes[B.parentOrNext].child2 == iA);
						_nodes[B.parentOrNext].child2 = iB;
					}
				}
				else
				{
					_root = iB;
				}

				// Rotate
				if (D.height > E.height)
				{
					B.child2 = iD;
					A.child1 = iE;
					E.parentOrNext = iA;
					A.aabb.Combine(ref C.aabb, ref E.aabb);
					B.aabb.Combine(ref A.aabb, ref D.aabb);

					A.height = 1 + Math.Max(C.height, E.height);
					B.height = 1 + Math.Max(A.height, D.height);
				}
				else
				{
					B.child2 = iE;
					A.child1 = iD;
					D.parentOrNext = iA;
					A.aabb.Combine(ref C.aabb, ref D.aabb);
					B.aabb.Combine(ref A.aabb, ref E.aabb);

					A.height = 1 + Math.Max(C.height, D.height);
					B.height = 1 + Math.Max(A.height, E.height);
				}

				return iB;
			}

			return iA;
		}

		/// <summary>
		/// Compute the height of a sub-tree.
		/// </summary>
		/// <param name="nodeId">The node id to use as parent.</param>
		/// <returns>The height of the tree.</returns>
		public int ComputeHeight(int nodeId)
		{
			Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
			var node = _nodes[nodeId];

			if (node.IsLeaf())
			{
				return 0;
			}

			int height1 = ComputeHeight(node.child1);
			int height2 = ComputeHeight(node.child2);
			return 1 + Math.Max(height1, height2);
		}

		/// <summary>
		/// Compute the height of the entire tree.
		/// </summary>
		/// <returns>The height of the tree.</returns>
		public int ComputeHeight()
		{
			int height = ComputeHeight(_root);
			return height;
		}

		public void ValidateStructure(int index)
		{
			if (index == nullNode)
				return;

			if (index == _root)
				Debug.Assert(_nodes[index].parentOrNext == nullNode);

			var node = _nodes[index];

			int child1 = node.child1;
			int child2 = node.child2;

			if (node.IsLeaf())
			{
				Debug.Assert(child1 == nullNode);
				Debug.Assert(child2 == nullNode);
				Debug.Assert(node.height == 0);
				return;
			}

			Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
			Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

			Debug.Assert(_nodes[child1].parentOrNext == index);
			Debug.Assert(_nodes[child2].parentOrNext == index);

			ValidateStructure(child1);
			ValidateStructure(child2);
		}

		public void ValidateMetrics(int index)
		{
			if (index == nullNode)
				return;

			var node = _nodes[index];

			int child1 = node.child1;
			int child2 = node.child2;

			if (node.IsLeaf())
			{
				Debug.Assert(child1 == nullNode);
				Debug.Assert(child2 == nullNode);
				Debug.Assert(node.height == 0);
				return;
			}

			Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
			Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

			int height1 = _nodes[child1].height;
			int height2 = _nodes[child2].height;
			int height = 1 + Math.Max(height1, height2);
			Debug.Assert(node.height == height);

			var AABB = new AABB();
			AABB.Combine(ref _nodes[child1].aabb, ref _nodes[child2].aabb);

			Debug.Assert(AABB.LowerBound == node.aabb.LowerBound);
			Debug.Assert(AABB.UpperBound == node.aabb.UpperBound);

			ValidateMetrics(child1);
			ValidateMetrics(child2);
		}

		/// <summary>
		/// Validate this tree. For testing.
		/// </summary>
		public void Validate()
		{
			ValidateStructure(_root);
			ValidateMetrics(_root);

			int freeCount = 0;
			int freeIndex = _freeList;
			while (freeIndex != nullNode)
			{
				Debug.Assert(0 <= freeIndex && freeIndex < _nodeCapacity);
				freeIndex = _nodes[freeIndex].parentOrNext;
				++freeCount;
			}

			Debug.Assert(Height == ComputeHeight());

			Debug.Assert(_nodeCount + freeCount == _nodeCapacity);
		}

		/// <summary>
		/// Build an optimal tree. Very expensive. For testing.
		/// </summary>
		public void RebuildBottomUp()
		{
			int[] nodes = new int[_nodeCount];
			int count = 0;

			// Build array of leaves. Free the rest.
			for (int i = 0; i < _nodeCapacity; ++i)
			{
				if (_nodes[i].height < 0)
				{
					// free node in pool
					continue;
				}

				if (_nodes[i].IsLeaf())
				{
					_nodes[i].parentOrNext = nullNode;
					nodes[count] = i;
					++count;
				}
				else
				{
					FreeNode(i);
				}
			}

			while (count > 1)
			{
				float minCost = Settings.MaxFloat;
				int iMin = -1, jMin = -1;
				for (int i = 0; i < count; ++i)
				{
					AABB AABBi = _nodes[nodes[i]].aabb;

					for (int j = i + 1; j < count; ++j)
					{
						AABB AABBj = _nodes[nodes[j]].aabb;
						var b = new AABB();
						b.Combine(ref AABBi, ref AABBj);
						float cost = b.Perimeter;
						if (cost < minCost)
						{
							iMin = i;
							jMin = j;
							minCost = cost;
						}
					}
				}

				int index1 = nodes[iMin];
				int index2 = nodes[jMin];
				var child1 = _nodes[index1];
				var child2 = _nodes[index2];

				var parentIndex = AllocateNode();
				var parent = _nodes[parentIndex];
				parent.child1 = index1;
				parent.child2 = index2;
				parent.height = 1 + Math.Max(child1.height, child2.height);
				parent.aabb.Combine(ref child1.aabb, ref child2.aabb);
				parent.parentOrNext = nullNode;

				child1.parentOrNext = parentIndex;
				child2.parentOrNext = parentIndex;

				nodes[jMin] = nodes[count - 1];
				nodes[iMin] = parentIndex;
				--count;
			}

			_root = nodes[0];

			Validate();
		}

		/// <summary>
		/// Shift the origin of the nodes
		/// </summary>
		/// <param name="newOrigin">The displacement to use.</param>
		public void ShiftOrigin(Vector2 newOrigin)
		{
			// Build array of leaves. Free the rest.
			for (int i = 0; i < _nodeCapacity; ++i)
			{
				_nodes[i].aabb.LowerBound -= newOrigin;
				_nodes[i].aabb.UpperBound -= newOrigin;
			}
		}
	}
}