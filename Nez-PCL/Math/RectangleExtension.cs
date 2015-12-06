using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class RectangleExtension
	{
		/// <summary>
		/// returns a rectangle from the passed in floats
		/// </summary>
		/// <returns>The floats.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static Rectangle fromFloats( float x, float y, float width, float height )
		{
			return new Rectangle( (int)x, (int)y, (int)width, (int)height );
		}


		/// <summary>
		/// creates a Rectangle given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static Rectangle fromMinMaxPoints( Point min, Point max )
		{
			return new Rectangle( min.X, min.Y, max.X - min.X, max.Y - min.Y );
		}


		public static void calculateBounds( ref Rectangle rect, Vector2 parentPosition, Vector2 position, Vector2 origin, Vector2 scale, float rotation, float width, float height )
		{
			if( rotation == 0f )
			{
				rect.X = (int)( parentPosition.X + position.X - origin.X * scale.X );
				rect.Y = (int)( parentPosition.Y + position.Y - origin.Y * scale.Y );
				rect.Width = (int)( width * scale.X );
				rect.Height = (int)( height * scale.Y );
			}
			else
			{
				// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
				var worldPosX = parentPosition.X + position.X;
				var worldPosY = parentPosition.Y + position.Y;

				Matrix tempMat;
				// set the reference point to world reference taking origin into account
				var transformMatrix = Matrix.CreateTranslation( -worldPosX - origin.X, -worldPosY - origin.Y, 0f );
				Matrix.CreateScale( scale.X, scale.Y, 1f, out tempMat ); // scale ->
				Matrix.Multiply( ref transformMatrix, ref tempMat, out transformMatrix );
				Matrix.CreateRotationZ( rotation, out tempMat ); // rotate ->
				Matrix.Multiply( ref transformMatrix, ref tempMat, out transformMatrix );
				Matrix.CreateTranslation( worldPosX, worldPosY, 0f, out tempMat ); // translate back
				Matrix.Multiply( ref transformMatrix, ref tempMat, out transformMatrix );

				// TODO: this is a bit silly. we can just leave the worldPos translation in the Matrix and avoid this
				// get all four corners in world space
				var topLeft = new Vector2( worldPosX, worldPosY );
				var topRight = new Vector2( worldPosX + width, worldPosY );
				var bottomLeft = new Vector2( worldPosX, worldPosY + height );
				var bottomRight = new Vector2( worldPosX + width, worldPosY + height );

				// transform the corners into our work space
				Vector2.Transform( ref topLeft, ref transformMatrix, out topLeft );
				Vector2.Transform( ref topRight,ref transformMatrix, out topRight );
				Vector2.Transform( ref bottomLeft, ref transformMatrix, out bottomLeft );
				Vector2.Transform( ref bottomRight, ref transformMatrix, out bottomRight );

				// find the min and max values so we can concoct our bounding box
				var minX = (int)Mathf.minOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
				var maxX = (int)Mathf.maxOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
				var minY = (int)Mathf.minOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );
				var maxY = (int)Mathf.maxOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );

				rect.Location = new Point( minX, minY );
				rect.Width = (int)( maxX - minX );
				rect.Height = (int)( maxY - minY );
			}
		}


		/// <summary>
		/// clones and returns a new Rectangle with the same data as the current rectangle
		/// </summary>
		/// <param name="rect">Rect.</param>
		public static Rectangle clone( this Rectangle rect )
		{
			return new Rectangle( rect.X, rect.Y, rect.Width, rect.Height );
		}


		/// <summary>
		/// scales the rect
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="scale">Scale.</param>
		public static void scale( ref Rectangle rect, Vector2 scale )
		{
			rect.X = (int)( rect.X * scale.X );
			rect.Y = (int)( rect.Y * scale.Y );
			rect.Width = (int)( rect.Width * scale.X );
			rect.Height = (int)( rect.Height * scale.Y );
		}


		public static void translate( ref Rectangle rect, Vector2 vec )
		{
			rect.Location += vec.ToPoint();
		}

		
		public static bool rayIntersects( this Rectangle rect, Ray2D ray, out float distance )
		{
			distance = 0f;
			var maxValue = float.MaxValue;

			if( Math.Abs( ray.direction.X ) < 1E-06f )
			{
				if( ( ray.position.X < rect.X ) || ( ray.position.X > rect.X + rect.Width ) )
					return false;
			}
			else
			{
				float num11 = 1f / ray.direction.X;
				float num8 = ( rect.X - ray.position.X ) * num11;
				float num7 = ( rect.X + rect.Width - ray.position.X ) * num11;
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
				if( ( ray.position.Y < rect.Y ) || ( ray.position.Y > rect.Y + rect.Height ) )
				{
					return false;
				}
			}
			else
			{
				float num10 = 1f / ray.direction.Y;
				float num6 = ( rect.Y - ray.position.Y ) * num10;
				float num5 = ( rect.Y + rect.Height - ray.position.Y ) * num10;
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


		public static float? rayIntersects( this Rectangle rectangle, Ray ray )
		{
			var num = 0f;
			var maxValue = float.MaxValue;

			if( Math.Abs( ray.Direction.X ) < 1E-06f )
			{
				if( ( ray.Position.X < rectangle.Left ) || ( ray.Position.X > rectangle.Right ) )
					return null;
			}
			else
			{
				float num11 = 1f / ray.Direction.X;
				float num8 = ( rectangle.Left - ray.Position.X ) * num11;
				float num7 = ( rectangle.Right - ray.Position.X ) * num11;
				if( num8 > num7 )
				{
					float num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				num = MathHelper.Max( num8, num );
				maxValue = MathHelper.Min( num7, maxValue );
				if( num > maxValue )
				{
					return null;  
				}  
			}

			if( Math.Abs( ray.Direction.Y ) < 1E-06f )
			{
				if( ( ray.Position.Y < rectangle.Top ) || ( ray.Position.Y > rectangle.Bottom ) )
				{
					return null;
				}
			}
			else
			{
				float num10 = 1f / ray.Direction.Y;
				float num6 = ( rectangle.Top - ray.Position.Y ) * num10;
				float num5 = ( rectangle.Bottom - ray.Position.Y ) * num10;
				if( num6 > num5 )
				{
					float num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				num = MathHelper.Max( num6, num );
				maxValue = MathHelper.Min( num5, maxValue );
				if( num > maxValue )
					return null;
			}

			return new float?( num );
		}
	

		/// <summary>
		/// returns a Bounds the spans the current bounds and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public static Rectangle getSweptBroadphaseBounds( this Rectangle rect, float deltaX, float deltaY )
		{
			return rect.getSweptBroadphaseBounds( (int)deltaX, (int)deltaY );
		}


		/// <summary>
		/// returns a Bounds the spans the current bounds and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public static Rectangle getSweptBroadphaseBounds( this Rectangle rect, int deltaX, int deltaY )
		{
			var broadphasebox = Rectangle.Empty;

			broadphasebox.X = deltaX > 0 ? rect.X : rect.X + deltaX;
			broadphasebox.Y = deltaY > 0 ? rect.Y : rect.Y + deltaY;
			broadphasebox.Width = deltaX > 0 ? deltaX + rect.Width : rect.Width - deltaX;
			broadphasebox.Height = deltaY > 0 ? deltaY + rect.Height : rect.Height - deltaY;

			return broadphasebox;
		}


		/// <summary>
		/// returns true if the boxes are colliding (velocities are not used).
		/// moveX and moveY will return the movement that b1 must move to avoid the collision
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="moveX">Move x.</param>
		/// <param name="moveY">Move y.</param>
		public static bool collisionCheck( this Rectangle rect, Rectangle other, out float moveX, out float moveY )
		{
			moveX = moveY = 0.0f;

			var l = other.X - ( rect.X + rect.Width );
			var r = ( other.X + other.Width ) - rect.X;
			var t = other.Y - ( rect.Y + rect.Height );
			var b = ( other.Y + other.Height ) - rect.Y;

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
		/// gets the center point of the rectangle as a Vector2
		/// </summary>
		/// <returns>The center.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 getCenter( this Rectangle rect )
		{
			return new Vector2( rect.X + rect.Width / 2, rect.Y + rect.Height / 2 );
		}


		/// <summary>
		/// gets the position of the rectangle as a Vector2
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="rect">Rect.</param>
		public static Vector2 getPosition( this Rectangle rect )
		{
			return new Vector2( rect.X, rect.Y );
		}

	}
}

