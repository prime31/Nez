using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class Group : Element, ICullable
	{
		internal List<Element> children = new List<Element>();
		protected bool transform = false;
		Matrix _previousBatcherTransform;
		Rectangle? _cullingArea;

		public T addElement<T>( T element ) where T : Element
		{
			if( element.parent != null )
				element.parent.removeElement( element );
			
			children.Add( element );
			element.setParent( this );
			element.setStage( stage );
			onChildrenChanged();

			return element;
		}


		public T insertElement<T>( int index, T element ) where T : Element
		{
			if( element.parent != null )
				element.parent.removeElement( element );

			if( index >= children.Count )
				return addElement( element );
			
			children.Insert( index, element );
			element.setParent( this );
			element.setStage( stage );
			onChildrenChanged();

			return element;
		}


		public virtual bool removeElement( Element element )
		{
			if( !children.Contains( element ) )
				return false;
			
			element.parent = null;
			children.Remove( element );
			onChildrenChanged();
			return true;
		}


		/// <summary>
		/// Returns an ordered list of child elements in this group
		/// </summary>
		/// <returns>The children.</returns>
		public List<Element> getChildren()
		{
			return children;
		}


		public void setTransform( bool transform )
		{
			this.transform = transform;
		}


		/// <summary>
		/// sets the stage on all children in case the Group is added to the Stage after it is configured
		/// </summary>
		/// <param name="stage">Stage.</param>
		internal override void setStage( Stage stage )
		{
			this.stage = stage;
			for( var i = 0; i < children.Count; i++ )
				children[i].setStage( stage );
		}


		void setLayoutEnabled( Group parent, bool enabled )
		{
			for( var i = 0; i < parent.children.Count; i++ )
			{
				if( parent.children[i] is ILayout )
					( (ILayout)parent.children[i] ).layoutEnabled = enabled;
				else if( parent.children[i] is Group )
					setLayoutEnabled( parent.children[i] as Group, enabled );
			}
		}


		/// <summary>
		/// Removes all children
		/// </summary>
		public void clear()
		{
			clearChildren();
		}


		/// <summary>
		/// Removes all elements from this group
		/// </summary>
		public virtual void clearChildren()
		{
			for( var i = 0; i < children.Count; i++ )
				children[i].parent = null;

			children.Clear();
			onChildrenChanged();
		}


		/// <summary>
		/// Called when elements are added to or removed from the group.
		/// </summary>
		protected virtual void onChildrenChanged()
		{
			invalidateHierarchy();
		}


		public override Element hit( Vector2 point )
		{
			if( touchable == Touchable.Disabled )
				return null;

			for( var i = children.Count - 1; i >= 0; i-- )
			{
				var child = children[i];
				if( !child.isVisible() )
					continue;

				var childLocalPoint = child.parentToLocalCoordinates( point );
				var hit = child.hit( childLocalPoint );
				if( hit != null )
					return hit;
			}

			return base.hit( point );
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			if( !isVisible() )
				return;

			validate();

			if( transform )
				applyTransform( graphics, computeTransform() );

			drawChildren( graphics, parentAlpha );

			if( transform )
				resetTransform( graphics );
		}


		public void drawChildren( Graphics graphics, float parentAlpha )
		{
			parentAlpha *= color.A / 255.0f;
		
			if( _cullingArea.HasValue )
            {
                float cullLeft = _cullingArea.Value.X;
                float cullRight = cullLeft + _cullingArea.Value.Width;
                float cullBottom = _cullingArea.Value.Y;
                float cullTop = cullBottom + _cullingArea.Value.Height;

                if( transform )
                {
                    for( int i = 0, n = children.Count; i < n; i++ )
                    {
                        var child = children[i];
                        if (!child.isVisible()) continue;
                        float cx = child.x, cy = child.y;
                        if( cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
                            cy + child.height >= cullBottom )
                        {
                            child.draw(graphics, parentAlpha);
                        }
                    }
                }
                else
                {
                    float offsetX = x, offsetY = y;
                    x = 0;
                    y = 0;
                    for( int i = 0, n = children.Count; i < n; i++ )
                    {
                        var child = children[i];
                        if( !child.isVisible() ) continue;
                        float cx = child.x, cy = child.y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.width >= cullLeft &&
                            cy + child.height >= cullBottom)
                        {
                            child.x = cx + offsetX;
                            child.y = cy + offsetY;
                            child.draw(graphics, parentAlpha);
                            child.x = cx;
                            child.y = cy;
                        }
                    }
                    x = offsetX;
                    y = offsetY;
                }
            }
            else
            {
                // No culling, draw all children.
                if ( transform )
                {
                    for( int i = 0, n = children.Count; i < n; i++ )
                    {
                        var child = children[i];
                        if( !child.isVisible() ) continue;
                        child.draw(graphics, parentAlpha);
                    }
                }
                else
                {
                    // No transform for this group, offset each child.
                    float offsetX = x, offsetY = y;
                    x = 0;
                    y = 0;
                    for( int i = 0, n = children.Count; i < n; i++ )
                    {
                        var child = children[i];
                        if( !child.isVisible() ) continue;
                        float cx = child.x, cy = child.y;
                        child.x = cx + offsetX;
                        child.y = cy + offsetY;
                        child.draw(graphics, parentAlpha);
                        child.x = cx;
                        child.y = cy;
                    }
                    x = offsetX;
                    y = offsetY;
                }
            }
		}


		public override void debugRender( Graphics graphics )
		{
			if( transform )
				applyTransform( graphics, computeTransform() );

			debugRenderChildren( graphics, 1f );

			if( transform )
				resetTransform( graphics );

			if( this is Button )
				base.debugRender( graphics );
		}


		public void debugRenderChildren( Graphics graphics, float parentAlpha )
		{
			parentAlpha *= color.A / 255.0f;
			if( transform )
			{
				for( var i = 0; i < children.Count; i++ )
				{
					if( !children[i].isVisible() )
						continue;

					if( !children[i].getDebug() && !( children[i] is Group ) )
						continue;

					children[i].debugRender( graphics );
				}
			}
			else
			{
				// No transform for this group, offset each child.
				float offsetX = x, offsetY = y;
				x = 0;
				y = 0;
				for( var i = 0; i < children.Count; i++ )
				{
					if( !children[i].isVisible() )
						continue;

					if( !children[i].getDebug() && !( children[i] is Group ) )
						continue;

					children[i].x += offsetX;
					children[i].y += offsetY;
					children[i].debugRender( graphics );
					children[i].x -= offsetX;
					children[i].y -= offsetY;
				}
				x = offsetX;
				y = offsetY;
			}
		}


		/// <summary>
		/// Returns the transform for this group's coordinate system
		/// </summary>
		/// <returns>The transform.</returns>
		protected Matrix2D computeTransform()
		{
			var mat = Matrix2D.identity;

			if( originX != 0 || originY != 0 )
				mat = Matrix2D.multiply( mat, Matrix2D.createTranslation( -originX, -originY ) );
			
			if( rotation != 0 )
				mat = Matrix2D.multiply( mat, Matrix2D.createRotation( MathHelper.ToRadians( rotation ) ) );

			if( scaleX != 1 || scaleY != 1 )
				mat = Matrix2D.multiply( mat, Matrix2D.createScale( scaleX, scaleY ) );

			mat = Matrix2D.multiply( mat, Matrix2D.createTranslation( x + originX, y + originY ) );

			// Find the first parent that transforms
			Group parentGroup = parent;
			while( parentGroup != null )
			{
				if( parentGroup.transform )
					break;
				parentGroup = parentGroup.parent;
			}

			if( parentGroup != null )
				mat = Matrix2D.multiply( mat, parentGroup.computeTransform() );

			return mat;
		}


		/// <summary>
		/// Set the batch's transformation matrix, often with the result of {@link #computeTransform()}. Note this causes the batch to 
		/// be flushed. {@link #resetTransform(Batch)} will restore the transform to what it was before this call.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="transform">Transform.</param>
		protected void applyTransform( Graphics graphics, Matrix transform )
		{
			_previousBatcherTransform = graphics.batcher.transformMatrix;
			graphics.batcher.end();
			graphics.batcher.begin( transform );
		}


		/// <summary>
		/// Restores the batch transform to what it was before {@link #applyTransform(Batch, Matrix4)}. Note this causes the batch to
		/// be flushed
		/// </summary>
		/// <param name="batch">Batch.</param>
		protected void resetTransform( Graphics graphics )
		{
			graphics.batcher.end();
			graphics.batcher.begin( _previousBatcherTransform );
		}


		/// <summary>
		/// If true, drawDebug() will be called for this group and, optionally, all children recursively.
		/// </summary>
		/// <param name="enabled">If set to <c>true</c> enabled.</param>
		/// <param name="recursively">If set to <c>true</c> recursively.</param>
		public void setDebug( bool enabled, bool recursively )
		{
			_debug = enabled;
			if( recursively )
			{
				foreach( var child in children )
				{
					if( child is Group )
						( (Group)child ).setDebug( enabled, recursively );
					else
						child.setDebug( enabled );
				}
			}
		}


		/// <summary>
		/// Calls {setDebug(true, true)
		/// </summary>
		/// <returns>The all.</returns>
		public virtual Group debugAll()
		{
			setDebug( true, true );
			return this;
		}


		#region ILayout

		public override bool layoutEnabled
		{
			get { return _layoutEnabled; }
			set
			{
				if( _layoutEnabled != value )
				{
					_layoutEnabled = value;

					setLayoutEnabled( this, _layoutEnabled );
					if( _layoutEnabled )
						invalidateHierarchy();
				}
			}
		}


		public override void pack()
		{
			setSize( preferredWidth, preferredHeight );
			validate();

			// Some situations require another layout. Eg, a wrapped label doesn't know its pref height until it knows its width, so it
			// calls invalidateHierarchy() in layout() if its pref height has changed.
			if( _needsLayout )
			{
				setSize( preferredWidth, preferredHeight );
				validate();
			}
		}

		#endregion

		public void setCullingArea( Rectangle cullingArea )
		{
			_cullingArea = cullingArea;
		}
	}
}

