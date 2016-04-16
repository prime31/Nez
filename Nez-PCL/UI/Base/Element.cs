using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class Element : ILayout
	{
		protected Stage stage;
		internal Group parent;

		/// <summary>
		/// true if the widget's layout has been {@link #invalidate() invalidated}.
		/// </summary>
		/// <value><c>true</c> if needs layout; otherwise, <c>false</c>.</value>
		public bool needsLayout { get { return _needsLayout; } }

		internal float x, y;
		internal float width, height;
		internal Color color = Color.White;

		protected float originX, originY;
		protected float scaleX = 1, scaleY = 1;
		protected float rotation;
		protected bool _visible = true;
		protected bool _debug = false;
		protected Touchable touchable = Touchable.Enabled;

		protected bool _needsLayout = true;
		protected bool _layoutEnabled = true;


		/// <summary>
		/// If this method is overridden, the super method or {@link #validate()} should be called to ensure the widget is laid out.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="parentAlpha">Parent alpha.</param>
		public virtual void draw( Graphics graphics, float parentAlpha )
		{
			validate();
		}


		protected virtual void sizeChanged()
		{
			invalidate();
		}


		protected virtual void positionChanged()
		{
		}


		protected virtual void rotationChanged()
		{
		}


		#region Getters/Setters

		/// <summary>
		/// Returns the stage that this element is currently in, or null if not in a stage.
		/// </summary>
		/// <returns>The stage.</returns>
		public Stage getStage()
		{
			return stage;
		}

	
		/// <summary>
		/// Called by the framework when this element or any parent is added to a group that is in the stage.
		/// stage May be null if the element or any parent is no longer in a stage
		/// </summary>
		/// <param name="stage">Stage.</param>
		internal virtual void setStage( Stage stage )
		{
			this.stage = stage;
		}


		/// <summary>
		/// Returns true if the element's parent is not null
		/// </summary>
		/// <returns><c>true</c>, if parent was hased, <c>false</c> otherwise.</returns>
		public bool hasParent()
		{
			return parent != null;
		}


		/// <summary>
		/// Returns the parent element, or null if not in a group
		/// </summary>
		/// <returns>The parent.</returns>
		public Group getParent()
		{
			return parent;
		}


		/// <summary>
		/// Called by the framework when an element is added to or removed from a group.
		/// </summary>
		/// <param name="parent">parent May be null if the element has been removed from the parent</param>
		internal void setParent( Group parent )
		{
			this.parent = parent;
		}


		/// <summary>
		/// Returns true if input events are processed by this element.
		/// </summary>
		/// <returns>The touchable.</returns>
		public bool isTouchable()
		{
			return touchable == Touchable.Enabled;
		}


		public Touchable getTouchable()
		{
			return touchable;
		}


		/// <summary>
		/// Determines how touch events are distributed to this element. Default is {@link Touchable#enabled}.
		/// </summary>
		/// <param name="touchable">Touchable.</param>
		public void setTouchable( Touchable touchable )
		{
			this.touchable = touchable;
		}


		public void setIsVisible( bool visible )
		{
			_visible = visible;
		}


		public bool isVisible()
		{
			return _visible;
		}


		/// <summary>
		/// If false, the element will not be drawn and will not receive touch events. Default is true.
		/// </summary>
		/// <param name="visible">Visible.</param>
		public void setVisible( bool visible )
		{
			this._visible = visible;
		}


		/// <summary>
		/// Returns the X position of the element's left edge
		/// </summary>
		/// <returns>The x.</returns>
		public float getX()
		{
			return x;
		}


		/// <summary>
		/// Returns the X position of the specified {@link Align alignment}.
		/// </summary>
		/// <returns>The x.</returns>
		/// <param name="alignment">Alignment.</param>
		public float getX( int alignment )
		{
			float x = this.x;
			if( ( alignment & AlignInternal.right ) != 0 )
				x += width;
			else if( ( alignment & AlignInternal.left ) == 0 )
				x += width / 2;
			return x;
		}


		public void setX( float x )
		{
			if( this.x != x )
			{
				this.x = x;
				positionChanged();
			}
		}


		/// <summary>
		/// Returns the Y position of the element's bottom edge
		/// </summary>
		/// <returns>The y.</returns>
		public float getY()
		{
			return y;
		}


		/// <summary>
		/// Returns the Y position of the specified {@link Align alignment}
		/// </summary>
		/// <returns>The y.</returns>
		/// <param name="alignment">Alignment.</param>
		public float getY( int alignment )
		{
			float y = this.y;
			if( ( alignment & AlignInternal.bottom ) != 0 )
				y += height;
			else if( ( alignment & AlignInternal.top ) == 0 )
				y += height / 2;
			return y;
		}


		public void setY( float y )
		{
			if( this.y != y )
			{
				this.y = y;
				positionChanged();
			}
		}


		/// <summary>
		/// Sets the position of the element's bottom left corner
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void setPosition( float x, float y )
		{
			if( this.x != x || this.y != y )
			{
				this.x = x;
				this.y = y;
				positionChanged();
			}
		}


		/// <summary>
		/// Sets the position using the specified {@link Align alignment}. Note this may set the position to non-integer coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="alignment">Alignment.</param>
		public void setPosition( float x, float y, int alignment )
		{
			if( ( alignment & AlignInternal.right ) != 0 )
				x -= width;
			else if( ( alignment & AlignInternal.left ) == 0 ) //
				x -= width / 2;

			if( ( alignment & AlignInternal.top ) != 0 )
				y -= height;
			else if( ( alignment & AlignInternal.bottom ) == 0 ) //
				y -= height / 2;

			if( this.x != x || this.y != y )
			{
				this.x = x;
				this.y = y;
				positionChanged();
			}
		}


		/// <summary>
		/// Add x and y to current position
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void moveBy( float x, float y )
		{
			if( x != 0 || y != 0 )
			{
				this.x += x;
				this.y += y;
				positionChanged();
			}
		}


		public float getWidth()
		{
			return width;
		}


		public void setWidth( float width )
		{
			if( this.width != width )
			{
				this.width = width;
				sizeChanged();
			}
		}


		public float getHeight()
		{
			return height;
		}


		public void setHeight( float height )
		{
			if( this.height != height )
			{
				this.height = height;
				sizeChanged();
			}
		}


		public void setSize( float width, float height )
		{
			if( this.width == width && this.height == height )
				return;

			this.width = width;
			this.height = height;
			sizeChanged();
		}


		/// <summary>
		/// Returns y plus height
		/// </summary>
		/// <returns>The top.</returns>
		public float getBottom()
		{
			return y + height;
		}


		/// <summary>
		/// Returns x plus width
		/// </summary>
		/// <returns>The right.</returns>
		public float getRight()
		{
			return x + width;
		}


		/// <summary>
		/// Sets the x, y, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void setBounds( float x, float y, float width, float height )
		{
			if( this.x != x || this.y != y )
			{
				this.x = x;
				this.y = y;
				positionChanged();
			}

			if( this.width != width || this.height != height )
			{
				this.width = width;
				this.height = height;
				sizeChanged();
			}
		}


		public float getOriginX()
		{
			return originX;
		}


		public void setOriginX( float originX )
		{
			this.originX = originX;
		}


		public float getOriginY()
		{
			return originY;
		}


		public void setOriginY( float originY )
		{
			this.originY = originY;
		}


		/// <summary>
		/// Sets the origin position which is relative to the element's bottom left corner
		/// </summary>
		/// <param name="originX">Origin x.</param>
		/// <param name="originY">Origin y.</param>
		public void setOrigin( float originX, float originY )
		{
			this.originX = originX;
			this.originY = originY;
		}


		/// <summary>
		/// Sets the origin position to the specified {@link Align alignment}.
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public void setOrigin( int alignment )
		{
			if( ( alignment & AlignInternal.left ) != 0 )
				originX = 0;
			else if( ( alignment & AlignInternal.right ) != 0 )
				originX = width;
			else
				originX = width / 2;

			if( ( alignment & AlignInternal.top ) != 0 )
				originY = 0;
			else if( ( alignment & AlignInternal.bottom ) != 0 )
				originY = height;
			else
				originY = height / 2;
		}


		public float getScaleX()
		{
			return scaleX;
		}


		public void setScaleX( float scaleX )
		{
			this.scaleX = scaleX;
		}


		public float getScaleY()
		{
			return scaleY;
		}


		public void setScaleY( float scaleY )
		{
			this.scaleY = scaleY;
		}


		/// <summary>
		/// Sets the scale for both X and Y
		/// </summary>
		/// <param name="scaleXY">Scale X.</param>
		public void setScale( float scaleXY )
		{
			this.scaleX = scaleXY;
			this.scaleY = scaleXY;
		}


		/// <summary>
		/// Sets the scale X and scale Y
		/// </summary>
		/// <param name="scaleX">Scale x.</param>
		/// <param name="scaleY">Scale y.</param>
		public void setScale( float scaleX, float scaleY )
		{
			this.scaleX = scaleX;
			this.scaleY = scaleY;
		}


		/// <summary>
		/// Adds the specified scale to the current scale
		/// </summary>
		/// <param name="scale">Scale.</param>
		public void scaleBy( float scale )
		{
			scaleX += scale;
			scaleY += scale;
		}


		/// <summary>
		/// Adds the specified scale to the current scale
		/// </summary>
		/// <param name="scaleX">Scale x.</param>
		/// <param name="scaleY">Scale y.</param>
		public void scaleBy( float scaleX, float scaleY )
		{
			this.scaleX += scaleX;
			this.scaleY += scaleY;
		}


		public float getRotation()
		{
			return rotation;
		}


		public void setRotation( float degrees )
		{
			if( this.rotation != degrees )
			{
				this.rotation = degrees;
				rotationChanged();
			}
		}


		/// <summary>
		/// Adds the specified rotation to the current rotation
		/// </summary>
		/// <param name="amountInDegrees">Amount in degrees.</param>
		public void rotateBy( float amountInDegrees )
		{
			if( amountInDegrees != 0 )
			{
				rotation += amountInDegrees;
				rotationChanged();
			}
		}


		public void setColor( Color color )
		{
			this.color = color;
		}


		/// <summary>
		/// Returns the color the element will be tinted when drawn
		/// </summary>
		/// <returns>The color.</returns>
		public Color getColor()
		{
			return color;
		}


		/// <summary>
		/// Changes the z-order for this element so it is in front of all siblings
		/// </summary>
		public void toFront()
		{
			setZIndex( int.MaxValue );
		}


		/// <summary>
		/// Changes the z-order for this element so it is in back of all siblings
		/// </summary>
		public void toBack()
		{
			setZIndex( 0 );
		}


		/// <summary>
		/// Sets the z-index of this element. The z-index is the index into the parent's {@link Group#getChildren() children}, where a
		/// lower index is below a higher index. Setting a z-index higher than the number of children will move the child to the front.
		/// Setting a z-index less than zero is invalid.
		/// </summary>
		/// <param name="index">Index.</param>
		public void setZIndex( int index )
		{
			var parent = this.parent;
			if( parent == null )
				return;
			
			var children = parent.children;
			if( children.Count == 1 )
				return;
			
			index = Math.Min( index, children.Count - 1 );
			if( index == children.IndexOf( this ) )
				return;
			
			if( !children.Remove( this ) )
				return;

			children.Insert( index, this );
		}


		/// <summary>
		/// If true, {@link #debugDraw} will be called for this element
		/// </summary>
		/// <param name="enabled">Enabled.</param>
		public virtual void setDebug( bool enabled )
		{
			_debug = enabled;
			if( enabled )
				Stage.debug = true;
		}


		public bool getDebug()
		{
			return _debug;
		}

		#endregion


		#region Coordinate conversion

		/// <summary>
		/// Transforms the specified point in screen coordinates to the element's local coordinate system
		/// </summary>
		/// <returns>The to local coordinates.</returns>
		/// <param name="screenCoords">Screen coords.</param>
		public Vector2 screenToLocalCoordinates( Vector2 screenCoords )
		{
			if( stage == null )
				return screenCoords;
			return stageToLocalCoordinates( stage.screenToStageCoordinates( screenCoords ) );
		}


		/// <summary>
		/// Transforms the specified point in the stage's coordinates to the element's local coordinate system.
		/// </summary>
		/// <returns>The to local coordinates.</returns>
		/// <param name="stageCoords">Stage coords.</param>
		public Vector2 stageToLocalCoordinates( Vector2 stageCoords )
		{
			if( parent != null )
				stageCoords = parent.stageToLocalCoordinates( stageCoords );
			
			stageCoords = parentToLocalCoordinates( stageCoords );
			return stageCoords;
		}


		/// <summary>
		/// Transforms the specified point in the element's coordinates to be in the stage's coordinates
		/// </summary>
		/// <returns>The to stage coordinates.</returns>
		/// <param name="localCoords">Local coords.</param>
		public Vector2 localToStageCoordinates( Vector2 localCoords )
		{
			return localToAscendantCoordinates( null, localCoords );
		}


		/// <summary>
		/// Converts coordinates for this element to those of a parent element. The ascendant does not need to be a direct parent
		/// </summary>
		/// <returns>The to ascendant coordinates.</returns>
		/// <param name="ascendant">Ascendant.</param>
		/// <param name="localCoords">Local coords.</param>
		public Vector2 localToAscendantCoordinates( Element ascendant, Vector2 localCoords )
		{
			Element element = this;
			while( element != null )
			{
				localCoords = element.localToParentCoordinates( localCoords );
				element = element.parent;
				if( element == ascendant )
					break;
			}
			return localCoords;
		}


		/// <summary>
		/// Converts the coordinates given in the parent's coordinate system to this element's coordinate system.
		/// </summary>
		/// <returns>The to local coordinates.</returns>
		/// <param name="parentCoords">Parent coords.</param>
		public Vector2 parentToLocalCoordinates( Vector2 parentCoords )
		{
			if( rotation == 0 )
			{
				if( scaleX == 1 && scaleY == 1 )
				{
					parentCoords.X -= x;
					parentCoords.Y -= y;
				}
				else
				{
					parentCoords.X = ( parentCoords.X - x - originX ) / scaleX + originX;
					parentCoords.Y = ( parentCoords.Y - y - originY ) / scaleY + originY;
				}
			}
			else
			{
				var cos = Mathf.cos( MathHelper.ToRadians( rotation ) );
				var sin = Mathf.sin( MathHelper.ToRadians( rotation ) );
				var tox = parentCoords.X - x - originX;
				var toy = parentCoords.Y - y - originY;
				parentCoords.X = ( tox * cos + toy * sin ) / scaleX + originX;
				parentCoords.Y = ( tox * -sin + toy * cos ) / scaleY + originY;
			}

			return parentCoords;
		}


		/// <summary>
		/// Transforms the specified point in the element's coordinates to be in the parent's coordinates.
		/// </summary>
		/// <returns>The to parent coordinates.</returns>
		/// <param name="localCoords">Local coords.</param>
		public Vector2 localToParentCoordinates( Vector2 localCoords )
		{
			var rotation = -this.rotation;

			if( rotation == 0 )
			{
				if( scaleX == 1 && scaleY == 1 )
				{
					localCoords.X += x;
					localCoords.Y += y;
				}
				else
				{
					localCoords.X = ( localCoords.X - originX ) * scaleX + originX + x;
					localCoords.Y = ( localCoords.Y - originY ) * scaleY + originY + y;
				}
			}
			else
			{
				var cos = Mathf.cos( MathHelper.ToRadians( rotation ) );
				var sin = Mathf.sin( MathHelper.ToRadians( rotation ) );

				var tox = ( localCoords.X - originX ) * scaleX;
				var toy = ( localCoords.Y - originY ) * scaleY;
				localCoords.X = ( tox * cos + toy * sin ) + originX + x;
				localCoords.Y = ( tox * -sin + toy * cos ) + originY + y;
			}

			return localCoords;
		}

		#endregion


		/// <summary>
		/// returns the distance from point to the bounds of element in the largest dimension or a negative number if the point is inside the bounds.
		/// Note that point should be in the element's coordinate system already.
		/// </summary>
		/// <returns>The outside bounds to point.</returns>
		/// <param name="Point">Point.</param>
		protected float distanceOutsideBoundsToPoint( Vector2 point )
		{
			var offsetX = Math.Max( -point.X, point.X - width );
			var offsetY = Math.Max( -point.Y, point.Y - height );

			return Math.Max( offsetX, offsetY );
		}


		/// <summary>
		/// Draws this element's debug lines
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void debugRender( Graphics graphics )
		{
			if( _debug )
				graphics.batcher.drawHollowRect( x, y, width, height, Color.Red );
		}


		public virtual Element hit( Vector2 point )
		{
			if( touchable != Touchable.Enabled )
				return null;

			if( point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height )
				return this;
			return null;
		}


		/// <summary>
		/// Removes this element from its parent, if it has a parent
		/// </summary>
		public bool remove()
		{
			if( parent != null )
				return parent.removeElement( this );
			return false;
		}
			

		#region ILayout

		public bool fillParent { get; set; }

		public virtual bool layoutEnabled
		{
			get { return _layoutEnabled; }
			set
			{
				if( _layoutEnabled != value )
				{
					_layoutEnabled = value;

					if( _layoutEnabled )
						invalidateHierarchy();
				}
			}
		}

		public virtual float minWidth
		{
			get { return preferredWidth; }
		}

		public virtual float minHeight
		{
			get { return preferredHeight; }
		}

		public virtual float preferredWidth
		{
			get { return 0; }
		}

		public virtual float preferredHeight
		{
			get { return 0; }
		}

		public virtual float maxWidth
		{
			get { return 0; }
		}

		public virtual float maxHeight
		{
			get { return 0; }
		}


		public virtual void layout()
		{}


		public virtual void invalidate()
		{
			_needsLayout = true;
		}


		public virtual void invalidateHierarchy()
		{
			if( !_layoutEnabled )
				return;

			invalidate();

			if( parent is ILayout )
				( (ILayout)parent ).invalidateHierarchy();
		}


		public void validate()
		{
			if( !_layoutEnabled )
				return;

			if( fillParent && parent != null )
			{
				var stage = getStage();
				float parentWidth, parentHeight;

				if( stage != null && parent == stage.getRoot() )
				{
					parentWidth = stage.getWidth();
					parentHeight = stage.getHeight();
				}
				else
				{
					parentWidth = parent.getWidth();
					parentHeight = parent.getHeight();
				}

				if( width != parentWidth || height != parentHeight )
				{
					setSize( parentWidth, parentHeight );
					invalidate();
				}
			}

			if( !_needsLayout )
				return;

			_needsLayout = false;
			layout();
		}


		public virtual void pack()
		{
			setSize( preferredWidth, preferredHeight );
			validate();
		}

		#endregion

	}
}

