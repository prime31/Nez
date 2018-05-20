using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.UI.Containers
{
    public class Tree : Group, IInputListener
    {
        TreeStyle style;
        readonly List<Node> rootNodes = new List<Node>( 0 );
        readonly Selection<Node> selection;
        float ySpacing = 10, iconSpacingLeft = 2, iconSpacingRight = 2, padding = 0, indentSpacing;
        private float leftColumnWidth, prefWidth, prefHeight;
        private bool sizeInvalid = true;
        private Node foundNode;
        Node overNode, rangeStart;


        public override float preferredWidth
        {
            get
            {
                if( sizeInvalid )
                    computeSize();
                return prefWidth;
            }
        }

        public override float preferredHeight
        {
            get
            {
                if( sizeInvalid )
                    computeSize();
                return prefHeight;
            }
        }

        public Tree( TreeStyle style )
        {
            selection = new Selection<Node>();
            selection.setElement( this );
            selection.setMultiple( true );
            setStyle( style );
            initialize();
        }

        private void initialize()
        {
        }

        public void setStyle( TreeStyle style )
        {
            this.style = style;
            indentSpacing = Math.Max( style.plus.minWidth, style.minus.minWidth ) + iconSpacingLeft;
        }

        public void add( Node node )
        {
            insert( rootNodes.Count, node );
        }

        public void insert( int index, Node node )
        {
            remove( node );
            node.parent = null;
            rootNodes.Insert( index, node );
            node.addToTree( this );
            invalidateHierarchy();
        }

        public void remove( Node node )
        {
            if( node.parent != null )
            {
                node.parent.remove( node );
                return;
            }

            rootNodes.Remove( node );
            node.removeFromTree( this );
            invalidateHierarchy();
        }

        public override void clearChildren()
        {
            base.clearChildren();
            setOverNode( null );
            rootNodes.Clear();
            selection.clear();
        }

        public List<Node> getNodes()
        {
            return rootNodes;
        }

        public override void invalidate()
        {
            base.invalidate();
            sizeInvalid = true;
        }

        private void computeSize()
        {
            sizeInvalid = false;
            prefWidth = style.plus.minWidth;
            prefWidth = Math.Max( prefWidth, style.minus.minWidth );
            prefHeight = getHeight();
            leftColumnWidth = 0;
            computeSize( rootNodes, indentSpacing );
            leftColumnWidth += iconSpacingLeft + padding;
            prefWidth += leftColumnWidth + padding;
            prefHeight = getHeight() - prefHeight;
        }

        private void computeSize( List<Node> nodes, float indent )
        {
            float ySpacing = this.ySpacing;
            float spacing = iconSpacingLeft + iconSpacingRight;
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                float rowWidth = indent + iconSpacingRight;
                var actor = node.actor;
                if( actor is ILayout layout )
                {
                    rowWidth += layout.preferredWidth;
                    node.height = layout.preferredHeight;
                    layout.pack();
                }
                else
                {
                    rowWidth += actor.getWidth();
                    node.height = actor.getHeight();
                }

                if( node.icon != null )
                {
                    rowWidth += spacing + node.icon.minWidth;
                    node.height = Math.Max( node.height, node.icon.minHeight );
                }

                prefWidth = Math.Max( prefWidth, rowWidth );
                prefHeight -= node.height + ySpacing;
                if( node.expanded )
                    computeSize( node.children, indent + indentSpacing );
            }
        }

        public override void layout()
        {
            if( sizeInvalid )
                computeSize();
            layout( rootNodes, leftColumnWidth + indentSpacing + iconSpacingRight, ySpacing / 2 );
        }

        private float layout( List<Node> nodes, float indent, float y )
        {
            float ySpacing = this.ySpacing;
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                var node = nodes[i];
                var x = indent;
                if( node.icon != null )
                    x += node.icon.minWidth;
                node.actor.setPosition( x, y );
                y += node.getHeight();
                y += ySpacing;
                if( node.expanded )
                    y = layout( node.children, indent + indentSpacing, y );
            }

            return y;
        }

        public override void draw( Graphics graphics, float parentAlpha )
        {
            drawBackground( graphics, parentAlpha );
            var color = getColor();
            color = new Color( color.R, color.G, color.B, color.A * parentAlpha );
            draw( graphics, rootNodes, leftColumnWidth, color );
            base.draw( graphics, parentAlpha );
        }

        protected void drawBackground( Graphics Graphics, float parentAlpha )
        {
            if( style.background != null )
            {
                var color = getColor();
                color = new Color( color.R, color.G, color.B, color.A * parentAlpha );
                style.background.draw( Graphics, getX(), getY(), getWidth(), getHeight(), color );
            }
        }

        private void draw( Graphics batch, List<Node> nodes, float indent, Color color )
        {
            var plus = style.plus;
            var minus = style.minus;
            var x = getX();
            var y = getY();

            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                var node = nodes[i];
                var actor = node.actor;

                if( selection.contains( node ) && style.selection != null )
                {
                    style.selection.draw( batch, x, y + actor.getY() - ySpacing / 2, getWidth(), node.height + ySpacing, color );
                }
                else if( node == overNode )
                {
                    style.over?.draw( batch, x, y + actor.getY() - ySpacing / 2, getWidth(), node.height + ySpacing, color );
                }

                if( node.icon != null )
                {
                    var iconY = actor.getY() + Mathf.round( ( node.height - node.icon.minHeight ) / 2 );
                    node.icon.draw( batch, x + node.actor.getX() - iconSpacingRight - node.icon.minWidth, y + iconY, node.icon.minWidth, node.icon.minHeight, actor.getColor() );
                }

                if( node.children.Count == 0 )
                    continue;

                var expandIcon = node.expanded ? minus : plus;
                var expandIconY = actor.getY() + Mathf.round( ( node.height - expandIcon.minHeight ) / 2 );

                expandIcon.draw( batch,
                    x + indent - iconSpacingLeft,
                    y + expandIconY,
                    expandIcon.minWidth, expandIcon.minHeight, actor.getColor() );


                if( node.expanded )
                {
                    draw( batch, node.children, indent + indentSpacing, actor.getColor() );
                }
            }
        }

        public Node getNodeAt( float y )
        {
            foundNode = null;
            getNodeAt( rootNodes, y, 0 );
            return foundNode;
        }

        private float getNodeAt( IList<Node> nodes, float y, float rowY )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                var node = nodes[i];
                var height = node.getHeight();

                if( y > rowY && y < rowY + height + ySpacing )
                {
                    foundNode = node;
                    return -1;
                }

                rowY += height + ySpacing;

                if( node.expanded )
                {
                    rowY = getNodeAt( node.children, y, rowY );
                    if( rowY == -1 )
                        return -1;
                }
            }

            return rowY;
        }

        void selectNodes( IList<Node> nodes, float low, float high )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                if( node.actor.getY() < low )
                    break;
                if( !node.isSelectable() )
                    continue;
                if( node.actor.getY() <= high )
                    selection.add( node );
                if( node.expanded )
                    selectNodes( node.children, low, high );
            }
        }

        public Selection<Node> getSelection()
        {
            return selection;
        }

        public TreeStyle getStyle()
        {
            return style;
        }

        public List<Node> getRootNodes()
        {
            return rootNodes;
        }

        public Node getOverNode()
        {
            return overNode;
        }

        public object getOverObject()
        {
            if( overNode == null )
                return null;
            return overNode.getObject();
        }

        public void setOverNode( Node overNode )
        {
            this.overNode = overNode;
        }

        public void setPadding( float padding )
        {
            this.padding = padding;
        }

        public float getIndentSpacing()
        {
            return indentSpacing;
        }

        public void setYSpacing( float ySpacing )
        {
            this.ySpacing = ySpacing;
        }

        public float getYSpacing()
        {
            return ySpacing;
        }

        public void setIconSpacing( float left, float right )
        {
            this.iconSpacingLeft = left;
            this.iconSpacingRight = right;
        }

        public void findExpandedObjects( List<object> objects )
        {
            findExpandedObjects( rootNodes, objects );
        }

        public void restoreExpandedObjects( List<object> objects )
        {
            for( int i = 0, n = objects.Count; i < n; i++ )
            {
                Node node = findNode( objects[i] );
                if( node != null )
                {
                    node.setExpanded( true );
                    node.expandTo();
                }
            }
        }

        internal static bool findExpandedObjects( List<Node> nodes, List<object> objects )
        {
            var expanded = false;
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                if( node.expanded && !findExpandedObjects( node.children, objects ) )
                    objects.Add( node.o );
            }

            return expanded;
        }

        public Node findNode( object o )
        {
            if( o == null )
                throw new Exception( "object cannot be null." );
            return findNode( rootNodes, o );
        }

        internal static Node findNode( List<Node> nodes, object o )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                if( o.Equals( node.o ) )
                    return node;
            }

            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                Node found = findNode( node.children, o );
                if( found != null )
                    return found;
            }

            return null;
        }

        public void collapseAll()
        {
            collapseAll( rootNodes );
        }

        internal static void collapseAll( List<Node> nodes )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Node node = nodes[i];
                node.setExpanded( false );
                collapseAll( node.children );
            }
        }

        public void expandAll()
        {
            expandAll( rootNodes );
        }

        internal static void expandAll( List<Node> nodes )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
                nodes[i].expandAll();
        }

        void IInputListener.onMouseEnter()
        {
        }

        void IInputListener.onMouseExit()
        {
            //if (toActor == null || !toActor.isDescendantOf(Tree.this)) setOverNode(null);
        }

        bool IInputListener.onMousePressed( Vector2 mousePos )
        {
            var node = getNodeAt( mousePos.Y );
            if( node == null )
                return true;

            //if (node != getNodeAt(getTouchDownY())) return;
            if( selection.getMultiple() && selection.hasItems() && Keyboard.GetState().IsKeyDown( Keys.LeftShift ) )
            {
                // Select range (shift).
                if( rangeStart == null )
                    rangeStart = node;

                if( !Keyboard.GetState().IsKeyDown( Keys.LeftControl ) )
                    selection.clear();

                float start = rangeStart.actor.getY(), end = node.actor.getY();
                if( start > end )
                    selectNodes( rootNodes, end, start );
                else
                {
                    selectNodes( rootNodes, start, end );
                    selection.items().Reverse();
                }

                selection.fireChangeEvent();
                rangeStart = rangeStart;
                return true;
            }

            if( node.children.Count > 0 && ( !selection.getMultiple() || !Keyboard.GetState().IsKeyDown( Keys.LeftControl ) ) )
            {
                // Toggle expanded.
                var rowX = node.actor.getX();
                if( node.icon != null )
                    rowX -= iconSpacingRight + node.icon.minWidth;

                if( mousePos.X < rowX )
                {
                    node.setExpanded( !node.expanded );
                    return true;
                }
            }

            if( !node.isSelectable() )
                return true;

            selection.choose( node );
            if( !selection.isEmpty() )
                rangeStart = node;

            return true;
        }

        void IInputListener.onMouseMoved( Vector2 mousePos )
        {
            setOverNode( getNodeAt( mousePos.Y ) );
        }

        void IInputListener.onMouseUp( Vector2 mousePos )
        {
        }

        bool IInputListener.onMouseScrolled( int mouseWheelDelta )
        {
            return true;
        }
    }

    public class TreeStyle
    {
        public IDrawable plus, minus;

        /** Optional. */
        public IDrawable over, selection, background;
    }


    public class Node
    {
        internal Element actor;
        internal Node parent;
        internal List<Node> children = new List<Node>( 0 );
        internal bool selectable = true;
        internal bool expanded;
        internal IDrawable icon;
        internal float height;
        internal object o;

        public Node( Element element )
        {
            actor = element;
        }

        public void setExpanded( bool expanded )
        {
            if( expanded == this.expanded )
                return;
            this.expanded = expanded;
            if( children.Count == 0 )
                return;
            var tree = getTree();
            if( tree == null )
                return;
            if( expanded )
            {
                for( int i = 0, n = children.Count; i < n; i++ )
                    children[i].addToTree( tree );
            }
            else
            {
                for( var i = children.Count - 1; i >= 0; i-- )
                    children[i].removeFromTree( tree );
            }

            tree.invalidateHierarchy();
        }

        protected internal void addToTree( Tree tree )
        {
            tree.addElement( actor );
            if( !expanded )
                return;
            var children = this.children;
            for( var i = this.children.Count - 1; i >= 0; i-- )
                children[i].addToTree( tree );
        }

        protected internal void removeFromTree( Tree tree )
        {
            tree.removeElement( actor );
            if( !expanded )
                return;
            var children = this.children;
            for( var i = this.children.Count - 1; i >= 0; i-- )
                children[i].removeFromTree( tree );
        }

        public void add( Node node )
        {
            insert( children.Count, node );
        }

        public void addAll( List<Node> nodes )
        {
            for( int i = 0, n = nodes.Count; i < n; i++ )
                insert( children.Count, nodes[i] );
        }

        public void insert( int index, Node node )
        {
            node.parent = this;
            children.Insert( index, node );
            updateChildren();
        }

        public void remove()
        {
            var tree = getTree();
            if( tree != null )
                tree.remove( this );
            else if( parent != null ) //
                parent.remove( this );
        }

        public void remove( Node node )
        {
            children.Remove( node );
            if( !expanded )
                return;
            var tree = getTree();
            if( tree == null )
                return;
            node.removeFromTree( tree );
            if( children.Count == 0 )
                expanded = false;
        }

        public void removeAll()
        {
            var tree = getTree();
            if( tree != null )
            {
                var children = this.children;
                for( var i = this.children.Count - 1; i >= 0; i-- )
                    ( (Node)children[i] ).removeFromTree( tree );
            }

            children.Clear();
        }

        public Tree getTree()
        {
            var parent = actor.getParent();
            if( !( parent is Tree ) )
                return null;
            return (Tree)parent;
        }

        public Element getActor()
        {
            return actor;
        }

        public bool isExpanded()
        {
            return expanded;
        }

        public List<Node> getChildren()
        {
            return children;
        }

        public void updateChildren()
        {
            if( !expanded )
                return;
            var tree = getTree();
            if( tree == null )
                return;
            for( var i = children.Count - 1; i >= 0; i-- )
                children[i].removeFromTree( tree );
            for( int i = 0, n = children.Count; i < n; i++ )
                children[i].addToTree( tree );
        }

        public Node getParent()
        {
            return parent;
        }

        public void setIcon( IDrawable icon )
        {
            this.icon = icon;
        }

        public object getObject()
        {
            return o;
        }

        public void setObject( object o )
        {
            this.o = o;
        }

        public IDrawable getIcon()
        {
            return icon;
        }

        public int getLevel()
        {
            var level = 0;
            var current = this;
            do
            {
                level++;
                current = current.getParent();
            } while( current != null );

            return level;
        }

        public Node findNode( object o )
        {
            if( o == null )
                throw new Exception( "object cannot be null." );
            if( o.Equals( this.o ) )
                return this;
            return Tree.findNode( children, o );
        }

        public void collapseAll()
        {
            setExpanded( false );
            Tree.collapseAll( children );
        }

        public void expandAll()
        {
            setExpanded( true );
            if( children.Count > 0 )
                Tree.expandAll( children );
        }

        public void expandTo()
        {
            var node = parent;
            while( node != null )
            {
                node.setExpanded( true );
                node = node.parent;
            }
        }

        public bool isSelectable()
        {
            return selectable;
        }

        public void setSelectable( bool selectable )
        {
            this.selectable = selectable;
        }

        public void findExpandedObjects( List<object> objects )
        {
            if( expanded && !Tree.findExpandedObjects( children, objects ) )
                objects.Add( o );
        }

        public void restoreExpandedObjects( List<object> objects )
        {
            for( int i = 0, n = objects.Count; i < n; i++ )
            {
                var node = findNode( objects[i] );
                if( node != null )
                {
                    node.setExpanded( true );
                    node.expandTo();
                }
            }
        }

        public float getHeight()
        {
            return height;
        }
    }
}
