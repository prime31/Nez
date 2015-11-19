using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/*
	public struct Bounds
	{
		public float x;
		public float y;
		public float width;
		public float height;

		public Vector2 min
		{
			get { return new Vector2( x, y ); }
		}

		public Vector2 max
		{
			get { return new Vector2( x + width, y + height ); }
		}

		public Vector2 position
		{
			get { return new Vector2( x, y ); }
		}

		public Vector2 size
		{
			get { return new Vector2( width, height ); }
		}

		public Vector2 center
		{
			get { return new Vector2( x + width * 0.5f, y + height * 0.5f ); }
		}


		public Bounds( float x, float y, float width, float height )
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}


		public Bounds( Vector2 center, Vector2 size )
		{
			x = center.X - size.X * 0.5f;
			y = center.Y - size.Y * 0.5f;
			width = size.X;
			height = size.Y;
		}


		/// <summary>
		/// performs a simple AABB intersection check returning true if the two Bounds intersect
		/// </summary>
		/// <param name="other">Other.</param>
		public bool intersects( Bounds other )
		{
			var maxX = x + width;
			var maxY = y + height;
			var otherMax = other.max;
			return x <= otherMax.X && maxX >= other.x && y <= otherMax.Y && maxY >= other.y;
		}


		public bool rayIntersects( Ray2D ray, out float distance )
		{
			distance = 0f;
			var maxValue = float.MaxValue;

			if( Math.Abs( ray.direction.X ) < 1E-06f )
			{
				if( ( ray.position.X < x ) || ( ray.position.X > x + width ) )
					return false;
			}
			else
			{
				float num11 = 1f / ray.direction.X;
				float num8 = ( x - ray.position.X ) * num11;
				float num7 = ( x + width - ray.position.X ) * num11;
				if( num8 > num7 )
				{
					float num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				distance = MathHelper.Max( num8, distance );
				maxValue = MathHelper.Min( num7, maxValue );
				if( distance > maxValue )
				{
					return false;  
				}  
			}

			if( Math.Abs( ray.direction.Y ) < 1E-06f )
			{
				if( ( ray.position.Y < y ) || ( ray.position.Y > y + height ) )
				{
					return false;
				}
			}
			else
			{
				float num10 = 1f / ray.direction.Y;
				float num6 = ( y - ray.position.Y ) * num10;
				float num5 = ( y + height - ray.position.Y ) * num10;
				if( num6 > num5 )
				{
					float num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				distance = MathHelper.Max( num6, distance );
				maxValue = MathHelper.Min( num5, maxValue );
				if( distance > maxValue )
					return false;
			}

			return true;
		}


		/// <summary>
		/// returns a Bounds the spans the current bounds and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public Bounds getSweptBroadphaseBounds( float deltaX, float deltaY )
		{
			var broadphasebox = new Bounds( 0.0f, 0.0f, 0.0f, 0.0f );

			broadphasebox.x = deltaX > 0 ? x : x + deltaX;
			broadphasebox.y = deltaY > 0 ? y : y + deltaY;
			broadphasebox.width = deltaX > 0 ? deltaX + width : width - deltaX;
			broadphasebox.height = deltaY > 0 ? deltaY + height : height - deltaY;

			return broadphasebox;
		}


		/// <summary>
		/// returns true if the boxes are colliding (velocities are not used).
		/// moveX and moveY will return the movement that b1 must move to avoid the collision
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="moveX">Move x.</param>
		/// <param name="moveY">Move y.</param>
		public bool collisionCheck( Bounds other, out float moveX, out float moveY )
		{
			moveX = moveY = 0.0f;

			var l = other.x - ( x + width );
			var r = ( other.x + other.width ) - x;
			var t = other.y - ( y + height );
			var b = ( other.y + other.height ) - y;

			// check that there was a collision
			if( l > 0 || r < 0 || t > 0 || b < 0 )
				return false;

			// find the offset of both sides
			moveX = Math.Abs( l ) < r ? l : r;
			moveY = Math.Abs( t ) < b ? t : b;

			// only use whichever offset is the smallest
			if( Math.Abs( moveX ) < Math.Abs( moveY ) )
				moveY = 0.0f;
			else
				moveX = 0.0f;

			return true;
		}


		/// <summary>
		/// performs collision detection on moving box and static box other.
		/// returns the time that the collision occured (where 0 is the start of the movement and 1 is the destination).
		/// getting the new position can be retrieved by box.x = box.x + box.vx * collisiontime.
		/// normalx and normaly return the normal of the collided surface (this can be used to do a response)
		/// </summary>
		/// <returns>The collision check.</returns>
		/// <param name="other">Other.</param>
		/// <param name="vx">Vx.</param>
		/// <param name="vy">Vy.</param>
		/// <param name="normalx">Normalx.</param>
		/// <param name="normaly">Normaly.</param>
		public float sweptCollisionCheck( Bounds other, float deltaX, float deltaY, out float normalX, out float normalY )
		{
			// how far away the closest edges of the objects are from each other
			float xInvEntry, yInvEntry;
			// the distance to the far side of the object
			float xInvExit, yInvExit;

			// find the distance between the objects on the near and far sides for both x and y
			if( deltaX > 0.0f )
			{
				xInvEntry = other.x - ( x + width );
				xInvExit = ( other.x + other.width ) - x;
			}
			else
			{
				xInvEntry = ( other.x + other.width ) - x;
				xInvExit = other.x - ( x + width );
			}

			if( deltaY > 0.0f )
			{
				yInvEntry = other.y - ( y + height );
				yInvExit = ( other.y + other.height ) - y;
			}
			else
			{
				yInvEntry = ( other.y + other.height ) - y;
				yInvExit = other.y - ( y + height );
			}

			// find time of collision and time of leaving for each axis (if statement is to prevent divide by zero)
			float xEntry, yEntry;
			float xExit, yExit;

			if( deltaX == 0.0f )
			{
				xEntry = -float.PositiveInfinity;
				xExit = float.PositiveInfinity;
			}
			else
			{
				xEntry = xInvEntry / deltaX;
				xExit = xInvExit / deltaX;
			}

			if( deltaY == 0.0f )
			{
				yEntry = -float.PositiveInfinity;
				yExit = float.PositiveInfinity;
			}
			else
			{
				yEntry = yInvEntry / deltaY;
				yExit = yInvExit / deltaY;
			}


			// TODO: I dont think this is needed
			if( xEntry > 1.0f )
				xEntry = float.MinValue;
			if( yEntry > 1.0f )
				yEntry = float.MinValue;


			// find the earliest/latest times of collision
			var entryTime = Math.Max( xEntry, yEntry );
			var exitTime = Math.Min( xExit, yExit );

			// if there was no collision
			if( entryTime > exitTime || xEntry < 0.0f && yEntry < 0.0f || xEntry > 1.0f || yEntry > 1.0f )
			{
				normalX = 0.0f;
				normalY = 0.0f;

				return 1.0f;
			}
			else // if there was a collision
			{
				// calculate normal of collided surface
				if( xEntry > yEntry )
				{
					if( xInvEntry < 0.0f )
					{
						normalX = 1.0f;
						normalY = 0.0f;
					}
					else
					{
						normalX = -1.0f;
						normalY = 0.0f;
					}
				}
				else
				{
					if( yInvEntry < 0.0f )
					{
						normalX = 0.0f;
						normalY = 1.0f;
					}
					else
					{
						normalX = 0.0f;
						normalY = -1.0f;
					}
				}

				// return the time of collision
				return entryTime;
			}
		}


		/// <summary>
		/// returns a copy of this Bounds
		/// </summary>
		public Bounds clone()
		{
			return new Bounds( x, y, width, height );
		}


		public static implicit operator Rectangle( Bounds source )
		{
			return new Rectangle( (int)source.x, (int)source.y, (int)source.width, (int)source.height );
		}


		public override string ToString()
		{
			return String.Format( "x: {0}, y: {1}, width: {2}, height: {3}", x, y, width, height );
		}
	
	}
	*/
}

