using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace Nez.UI.Containers
{
	public class Tree : Group, IInputListener
	{
		TreeStyle style;
		readonly List<Node> rootNodes = new List<Node>(0);
		readonly Selection<Node> selection;
		float ySpacing = 10, iconSpacingLeft = 2, iconSpacingRight = 2, padding = 0, indentSpacing;
		private float leftColumnWidth, prefWidth, prefHeight;
		private bool sizeInvalid = true;
		private Node foundNode;
		Node overNode, rangeStart;


		public override float PreferredWidth
		{
			get
			{
				if (sizeInvalid)
					ComputeSize();
				return prefWidth;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				if (sizeInvalid)
					ComputeSize();
				return prefHeight;
			}
		}

		public Tree(TreeStyle style)
		{
			selection = new Selection<Node>();
			selection.SetElement(this);
			selection.SetMultiple(true);
			SetStyle(style);
			Initialize();
		}

		private void Initialize()
		{
		}

		public void SetStyle(TreeStyle style)
		{
			this.style = style;
			indentSpacing = Math.Max(style.Plus.MinWidth, style.Minus.MinWidth) + iconSpacingLeft;
		}

		public void Add(Node node)
		{
			Insert(rootNodes.Count, node);
		}

		public void Insert(int index, Node node)
		{
			Remove(node);
			node.parent = null;
			rootNodes.Insert(index, node);
			node.AddToTree(this);
			InvalidateHierarchy();
		}

		public void Remove(Node node)
		{
			if (node.parent != null)
			{
				node.parent.Remove(node);
				return;
			}

			rootNodes.Remove(node);
			node.RemoveFromTree(this);
			InvalidateHierarchy();
		}

		public override void ClearChildren()
		{
			base.ClearChildren();
			SetOverNode(null);
			rootNodes.Clear();
			selection.Clear();
		}

		public List<Node> GetNodes()
		{
			return rootNodes;
		}

		public override void Invalidate()
		{
			base.Invalidate();
			sizeInvalid = true;
		}

		private void ComputeSize()
		{
			sizeInvalid = false;
			prefWidth = style.Plus.MinWidth;
			prefWidth = Math.Max(prefWidth, style.Minus.MinWidth);
			prefHeight = GetHeight();
			leftColumnWidth = 0;
			ComputeSize(rootNodes, indentSpacing);
			leftColumnWidth += iconSpacingLeft + padding;
			prefWidth += leftColumnWidth + padding;
			prefHeight = GetHeight() - prefHeight;
		}

		private void ComputeSize(List<Node> nodes, float indent)
		{
			float ySpacing = this.ySpacing;
			float spacing = iconSpacingLeft + iconSpacingRight;
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				float rowWidth = indent + iconSpacingRight;
				var actor = node.actor;
				if (actor is ILayout layout)
				{
					rowWidth += layout.PreferredWidth;
					node.height = layout.PreferredHeight;
					layout.Pack();
				}
				else
				{
					rowWidth += actor.GetWidth();
					node.height = actor.GetHeight();
				}

				if (node.icon != null)
				{
					rowWidth += spacing + node.icon.MinWidth;
					node.height = Math.Max(node.height, node.icon.MinHeight);
				}

				prefWidth = Math.Max(prefWidth, rowWidth);
				prefHeight -= node.height + ySpacing;
				if (node.expanded)
					ComputeSize(node.children, indent + indentSpacing);
			}
		}

		public override void Layout()
		{
			if (sizeInvalid)
				ComputeSize();
			Layout(rootNodes, leftColumnWidth + indentSpacing + iconSpacingRight, ySpacing / 2);
		}

		private float Layout(List<Node> nodes, float indent, float y)
		{
			float ySpacing = this.ySpacing;
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				var node = nodes[i];
				var x = indent;
				if (node.icon != null)
					x += node.icon.MinWidth;
				node.actor.SetPosition(x, y);
				y += node.GetHeight();
				y += ySpacing;
				if (node.expanded)
					y = Layout(node.children, indent + indentSpacing, y);
			}

			return y;
		}

		public override void Draw(Batcher batcher, float parentAlpha)
		{
			DrawBackground(batcher, parentAlpha);
			var color = GetColor();
			color = new Color(color.R, color.G, color.B, color.A * parentAlpha);
			Draw(batcher, rootNodes, leftColumnWidth, color);
			base.Draw(batcher, parentAlpha);
		}

		protected void DrawBackground(Batcher batcher, float parentAlpha)
		{
			if (style.Background != null)
			{
				var color = GetColor();
				color = new Color(color.R, color.G, color.B, color.A * parentAlpha);
				style.Background.Draw(batcher, GetX(), GetY(), GetWidth(), GetHeight(), color);
			}
		}

		private void Draw(Batcher batch, List<Node> nodes, float indent, Color color)
		{
			var plus = style.Plus;
			var minus = style.Minus;
			var x = GetX();
			var y = GetY();

			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				var node = nodes[i];
				var actor = node.actor;

				if (selection.Contains(node) && style.Selection != null)
				{
					style.Selection.Draw(batch, x, y + actor.GetY() - ySpacing / 2, GetWidth(), node.height + ySpacing,
						color);
				}
				else if (node == overNode)
				{
					style.Over?.Draw(batch, x, y + actor.GetY() - ySpacing / 2, GetWidth(), node.height + ySpacing,
						color);
				}

				if (node.icon != null)
				{
					var iconY = actor.GetY() + Mathf.Round((node.height - node.icon.MinHeight) / 2);
					node.icon.Draw(batch, x + node.actor.GetX() - iconSpacingRight - node.icon.MinWidth, y + iconY,
						node.icon.MinWidth, node.icon.MinHeight, actor.GetColor());
				}

				if (node.children.Count == 0)
					continue;

				var expandIcon = node.expanded ? minus : plus;
				var expandIconY = actor.GetY() + Mathf.Round((node.height - expandIcon.MinHeight) / 2);

				expandIcon.Draw(batch,
					x + indent - iconSpacingLeft,
					y + expandIconY,
					expandIcon.MinWidth, expandIcon.MinHeight, actor.GetColor());


				if (node.expanded)
				{
					Draw(batch, node.children, indent + indentSpacing, actor.GetColor());
				}
			}
		}

		public Node GetNodeAt(float y)
		{
			foundNode = null;
			GetNodeAt(rootNodes, y, 0);
			return foundNode;
		}

		private float GetNodeAt(IList<Node> nodes, float y, float rowY)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				var node = nodes[i];
				var height = node.GetHeight();

				if (y > rowY && y < rowY + height + ySpacing)
				{
					foundNode = node;
					return -1;
				}

				rowY += height + ySpacing;

				if (node.expanded)
				{
					rowY = GetNodeAt(node.children, y, rowY);
					if (rowY == -1)
						return -1;
				}
			}

			return rowY;
		}

		void SelectNodes(IList<Node> nodes, float low, float high)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				if (node.actor.GetY() < low)
					break;

				if (!node.IsSelectable())
					continue;

				if (node.actor.GetY() <= high)
					selection.Add(node);
				if (node.expanded)
					SelectNodes(node.children, low, high);
			}
		}

		public Selection<Node> GetSelection()
		{
			return selection;
		}

		public TreeStyle GetStyle()
		{
			return style;
		}

		public List<Node> GetRootNodes()
		{
			return rootNodes;
		}

		public Node GetOverNode()
		{
			return overNode;
		}

		public object GetOverObject()
		{
			if (overNode == null)
				return null;

			return overNode.GetObject();
		}

		public void SetOverNode(Node overNode)
		{
			this.overNode = overNode;
		}

		public void SetPadding(float padding)
		{
			this.padding = padding;
		}

		public float GetIndentSpacing()
		{
			return indentSpacing;
		}

		public void SetYSpacing(float ySpacing)
		{
			this.ySpacing = ySpacing;
		}

		public float GetYSpacing()
		{
			return ySpacing;
		}

		public void SetIconSpacing(float left, float right)
		{
			iconSpacingLeft = left;
			iconSpacingRight = right;
		}

		public void FindExpandedObjects(List<object> objects)
		{
			FindExpandedObjects(rootNodes, objects);
		}

		public void RestoreExpandedObjects(List<object> objects)
		{
			for (int i = 0, n = objects.Count; i < n; i++)
			{
				Node node = FindNode(objects[i]);
				if (node != null)
				{
					node.SetExpanded(true);
					node.ExpandTo();
				}
			}
		}

		internal static bool FindExpandedObjects(List<Node> nodes, List<object> objects)
		{
			var expanded = false;
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				if (node.expanded && !FindExpandedObjects(node.children, objects))
					objects.Add(node.o);
			}

			return expanded;
		}

		public Node FindNode(object o)
		{
			if (o == null)
				throw new Exception("object cannot be null.");

			return FindNode(rootNodes, o);
		}

		internal static Node FindNode(List<Node> nodes, object o)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				if (o.Equals(node.o))
					return node;
			}

			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				Node found = FindNode(node.children, o);
				if (found != null)
					return found;
			}

			return null;
		}

		public void CollapseAll()
		{
			CollapseAll(rootNodes);
		}

		internal static void CollapseAll(List<Node> nodes)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
			{
				Node node = nodes[i];
				node.SetExpanded(false);
				CollapseAll(node.children);
			}
		}

		public void ExpandAll()
		{
			ExpandAll(rootNodes);
		}

		internal static void ExpandAll(List<Node> nodes)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
				nodes[i].ExpandAll();
		}

		void IInputListener.OnMouseEnter()
		{
		}

		void IInputListener.OnMouseExit()
		{
			//if (toActor == null || !toActor.isDescendantOf(Tree.this)) setOverNode(null);
		}

		bool IInputListener.OnMousePressed(Vector2 mousePos)
		{
			var node = GetNodeAt(mousePos.Y);
			if (node == null)
				return true;

			//if (node != getNodeAt(getTouchDownY())) return;
			if (selection.GetMultiple() && selection.HasItems() && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
			{
				// Select range (shift).
				if (rangeStart == null)
					rangeStart = node;

				if (!Keyboard.GetState().IsKeyDown(Keys.LeftControl))
					selection.Clear();

				float start = rangeStart.actor.GetY(), end = node.actor.GetY();
				if (start > end)
					SelectNodes(rootNodes, end, start);
				else
				{
					SelectNodes(rootNodes, start, end);
					selection.Items().Reverse();
				}

				selection.FireChangeEvent();
				return true;
			}

			if (node.children.Count > 0 &&
			    (!selection.GetMultiple() || !Keyboard.GetState().IsKeyDown(Keys.LeftControl)))
			{
				// Toggle expanded.
				var rowX = node.actor.GetX();
				if (node.icon != null)
					rowX -= iconSpacingRight + node.icon.MinWidth;

				if (mousePos.X < rowX)
				{
					node.SetExpanded(!node.expanded);
					return true;
				}
			}

			if (!node.IsSelectable())
				return true;

			selection.Choose(node);
			if (!selection.IsEmpty())
				rangeStart = node;

			return true;
		}

		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
			SetOverNode(GetNodeAt(mousePos.Y));
		}

		void IInputListener.OnMouseUp(Vector2 mousePos)
		{
		}

		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			return true;
		}
	}

	public class TreeStyle
	{
		public IDrawable Plus, Minus;

		/** Optional. */
		public IDrawable Over, Selection, Background;
	}


	public class Node
	{
		internal Element actor;
		internal Node parent;
		internal List<Node> children = new List<Node>(0);
		internal bool selectable = true;
		internal bool expanded;
		internal IDrawable icon;
		internal float height;
		internal object o;

		public Node(Element element)
		{
			actor = element;
		}

		public void SetExpanded(bool expanded)
		{
			if (expanded == this.expanded)
				return;

			this.expanded = expanded;
			if (children.Count == 0)
				return;

			var tree = GetTree();
			if (tree == null)
				return;

			if (expanded)
			{
				for (int i = 0, n = children.Count; i < n; i++)
					children[i].AddToTree(tree);
			}
			else
			{
				for (var i = children.Count - 1; i >= 0; i--)
					children[i].RemoveFromTree(tree);
			}

			tree.InvalidateHierarchy();
		}

		protected internal void AddToTree(Tree tree)
		{
			tree.AddElement(actor);
			if (!expanded)
				return;

			var children = this.children;
			for (var i = this.children.Count - 1; i >= 0; i--)
				children[i].AddToTree(tree);
		}

		protected internal void RemoveFromTree(Tree tree)
		{
			tree.RemoveElement(actor);
			if (!expanded)
				return;

			var children = this.children;
			for (var i = this.children.Count - 1; i >= 0; i--)
				children[i].RemoveFromTree(tree);
		}

		public void Add(Node node)
		{
			Insert(children.Count, node);
		}

		public void AddAll(List<Node> nodes)
		{
			for (int i = 0, n = nodes.Count; i < n; i++)
				Insert(children.Count, nodes[i]);
		}

		public void Insert(int index, Node node)
		{
			node.parent = this;
			children.Insert(index, node);
			UpdateChildren();
		}

		public void Remove()
		{
			var tree = GetTree();
			if (tree != null)
				tree.Remove(this);
			else if (parent != null) //
				parent.Remove(this);
		}

		public void Remove(Node node)
		{
			children.Remove(node);
			if (!expanded)
				return;

			var tree = GetTree();
			if (tree == null)
				return;

			node.RemoveFromTree(tree);
			if (children.Count == 0)
				expanded = false;
		}

		public void RemoveAll()
		{
			var tree = GetTree();
			if (tree != null)
			{
				var children = this.children;
				for (var i = this.children.Count - 1; i >= 0; i--)
					((Node) children[i]).RemoveFromTree(tree);
			}

			children.Clear();
		}

		public Tree GetTree()
		{
			var parent = actor.GetParent();
			if (!(parent is Tree))
				return null;

			return (Tree) parent;
		}

		public Element GetActor()
		{
			return actor;
		}

		public bool IsExpanded()
		{
			return expanded;
		}

		public List<Node> GetChildren()
		{
			return children;
		}

		public void UpdateChildren()
		{
			if (!expanded)
				return;

			var tree = GetTree();
			if (tree == null)
				return;

			for (var i = children.Count - 1; i >= 0; i--)
				children[i].RemoveFromTree(tree);
			for (int i = 0, n = children.Count; i < n; i++)
				children[i].AddToTree(tree);
		}

		public Node GetParent()
		{
			return parent;
		}

		public void SetIcon(IDrawable icon)
		{
			this.icon = icon;
		}

		public object GetObject()
		{
			return o;
		}

		public void SetObject(object o)
		{
			this.o = o;
		}

		public IDrawable GetIcon()
		{
			return icon;
		}

		public int GetLevel()
		{
			var level = 0;
			var current = this;
			do
			{
				level++;
				current = current.GetParent();
			} while (current != null);

			return level;
		}

		public Node FindNode(object o)
		{
			if (o == null)
				throw new Exception("object cannot be null.");

			if (o.Equals(this.o))
				return this;

			return Tree.FindNode(children, o);
		}

		public void CollapseAll()
		{
			SetExpanded(false);
			Tree.CollapseAll(children);
		}

		public void ExpandAll()
		{
			SetExpanded(true);
			if (children.Count > 0)
				Tree.ExpandAll(children);
		}

		public void ExpandTo()
		{
			var node = parent;
			while (node != null)
			{
				node.SetExpanded(true);
				node = node.parent;
			}
		}

		public bool IsSelectable()
		{
			return selectable;
		}

		public void SetSelectable(bool selectable)
		{
			this.selectable = selectable;
		}

		public void FindExpandedObjects(List<object> objects)
		{
			if (expanded && !Tree.FindExpandedObjects(children, objects))
				objects.Add(o);
		}

		public void RestoreExpandedObjects(List<object> objects)
		{
			for (int i = 0, n = objects.Count; i < n; i++)
			{
				var node = FindNode(objects[i]);
				if (node != null)
				{
					node.SetExpanded(true);
					node.ExpandTo();
				}
			}
		}

		public float GetHeight()
		{
			return height;
		}
	}
}