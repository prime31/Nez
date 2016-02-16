using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace Nez
{
	/// <summary>
	/// grid of springs
	/// based on the tutorial: http://gamedevelopment.tutsplus.com/tutorials/make-a-neon-vector-shooter-for-ios-the-warping-grid--gamedev-14637
	/// </summary>
	public class SpringGrid : RenderableComponent, IUpdatable
	{
		#region internal classes

		class PointMass
		{
			public Vector3 position;
			public Vector3 velocity;
			public float inverseMass;

			Vector3 _acceleration;
			float _damping = 0.98f;

			public PointMass( Vector3 position, float invMass )
			{
				this.position = position;
				this.inverseMass = invMass;
			}


			public void applyForce( Vector3 force )
			{
				_acceleration += force * inverseMass;
			}


			public void increaseDamping( float factor )
			{
				_damping *= factor;
			}


			public void update()
			{
				velocity += _acceleration;
				position += velocity;
				_acceleration = Vector3.Zero;
				if( velocity.LengthSquared() < 0.001f * 0.001f )
					velocity = Vector3.Zero;

				velocity *= _damping;
				_damping = 0.98f;
			}
		}


		class Spring
		{
			public PointMass end1;
			public PointMass end2;
			public float targetLength;
			public float stiffness;
			public float damping;


			public Spring( PointMass end1, PointMass end2, float stiffness, float damping )
			{
				this.end1 = end1;
				this.end2 = end2;
				this.stiffness = stiffness;
				this.damping = damping;
				targetLength = Vector3.Distance( end1.position, end2.position ) * 0.95f;
			}


			public void update()
			{
				var x = end1.position - end2.position;

				var length = x.Length();
				// these springs can only pull, not push
				if( length <= targetLength )
					return;

				x = ( x / length ) * ( length - targetLength );
				var dv = end2.velocity - end1.velocity;
				var force = stiffness * x - dv * damping;

				end1.applyForce( -force );
				end2.applyForce( force );
			}
		}

		#endregion

		public Color gridColor = Color.OrangeRed;

		Spring[] _springs;
		PointMass[,] _points;
		Vector2 _screenSize;


		public SpringGrid( Rectangle size, Vector2 spacing )
		{
			var springList = new List<Spring>();
			
			var numColumns = (int)( size.Width / spacing.X ) + 1;
			var numRows = (int)( size.Height / spacing.Y ) + 1;
			_points = new PointMass[numColumns, numRows];

			// these fixed points will be used to anchor the grid to fixed positions on the screen
			var fixedPoints = new PointMass[numColumns, numRows];

			// create the point masses
			int column = 0, row = 0;
			for( float y = size.Top; y <= size.Bottom; y += spacing.Y )
			{
				for( float x = size.Left; x <= size.Right; x += spacing.X )
				{
					_points[column, row] = new PointMass( new Vector3( x, y, 0 ), 1 );
					fixedPoints[column, row] = new PointMass( new Vector3( x, y, 0 ), 0 );
					column++;
				}
				row++;
				column = 0;
			}

			// link the point masses with springs
			for( var y = 0; y < numRows; y++ )
			{
				for( var x = 0; x < numColumns; x++ )
				{
					if( x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1 ) // anchor the border of the grid
						springList.Add( new Spring( fixedPoints[x, y], _points[x, y], 0.1f, 0.1f ) );
					else if( x % 3 == 0 && y % 3 == 0 ) // loosely anchor 1/9th of the point masses
						springList.Add( new Spring( fixedPoints[x, y], _points[x, y], 0.002f, 0.02f ) );

					const float stiffness = 0.28f;
					const float damping = 0.06f;

					if( x > 0 )
						springList.Add( new Spring( _points[x - 1, y], _points[x, y], stiffness, damping ) );
					if( y > 0 )
						springList.Add( new Spring( _points[x, y - 1], _points[x, y], stiffness, damping ) );
				}
			}

			_springs = springList.ToArray();
		}


		public void applyDirectedForce( Vector2 force, Vector2 position, float radius )
		{
			applyDirectedForce( new Vector3( force, 0 ), new Vector3( position, 0 ), radius );
		}


		public void applyDirectedForce( Vector3 force, Vector3 position, float radius )
		{
			foreach( var mass in _points )
			{
				if( Vector3.DistanceSquared( position, mass.position ) < radius * radius )
					mass.applyForce( 10 * force / ( 10 + Vector3.Distance( position, mass.position ) ) );
			}
		}


		public void applyImplosiveForce( float force, Vector2 position, float radius )
		{
			applyImplosiveForce( force, new Vector3( position, 0 ), radius );
		}


		public void applyImplosiveForce( float force, Vector3 position, float radius )
		{
			foreach( var mass in _points )
			{
				var dist2 = Vector3.DistanceSquared( position, mass.position );
				if( dist2 < radius * radius )
				{
					mass.applyForce( 10 * force * ( position - mass.position ) / ( 100 + dist2 ) );
					mass.increaseDamping( 0.6f );
				}
			}
		}


		public void applyExplosiveForce( float force, Vector2 position, float radius )
		{
			applyExplosiveForce( force, new Vector3( position, 0 ), radius );
		}


		public void applyExplosiveForce( float force, Vector3 position, float radius )
		{
			foreach( var mass in _points )
			{
				var dist2 = Vector3.DistanceSquared( position, mass.position );
				if( dist2 < radius * radius )
				{
					mass.applyForce( 100 * force * ( mass.position - position ) / ( 10000 + dist2 ) );
					mass.increaseDamping( 0.6f );
				}
			}
		}


		public void update()
		{
			foreach( var spring in _springs )
				spring.update();

			foreach( var mass in _points )
				mass.update();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			_screenSize.X = Screen.width;
			_screenSize.Y = Screen.height;

			var width = _points.GetLength( 0 );
			var height = _points.GetLength( 1 );

			for( var y = 1; y < height; y++ )
			{
				for( var x = 1; x < width; x++ )
				{
					var left = new Vector2();
					var up = new Vector2();
					var p = projectToVector2( _points[x, y].position );

					if( x > 1 )
					{
						left = projectToVector2( _points[x - 1, y].position );
						float thickness = y % 3 == 1 ? 3f : 1f;
						
						// use Catmull-Rom interpolation to help smooth bends in the grid
						var clampedX = Math.Min( x + 1, width - 1 );
						var mid = Vector2.CatmullRom( projectToVector2( _points[x - 2, y].position ), left, p, projectToVector2( _points[clampedX, y].position ), 0.5f );

						// If the grid is very straight here, draw a single straight line. Otherwise, draw lines to our
						// new interpolated midpoint
						if( Vector2.DistanceSquared( mid, ( left + p ) / 2 ) > 1 )
						{
							drawLine( graphics.spriteBatch, left, mid, gridColor, thickness );
							drawLine( graphics.spriteBatch, mid, p, gridColor, thickness );
						}
						else
						{
							drawLine( graphics.spriteBatch, left, p, gridColor, thickness );
						}
					}

					if( y > 1 )
					{
						up = projectToVector2( _points[x, y - 1].position );
						var thickness = x % 3 == 1 ? 3f : 1f;
						var clampedY = Math.Min( y + 1, height - 1 );
						var mid = Vector2.CatmullRom( projectToVector2( _points[x, y - 2].position ), up, p, projectToVector2( _points[x, clampedY].position ), 0.5f );

						if( Vector2.DistanceSquared( mid, ( up + p ) / 2 ) > 1 )
						{
							drawLine( graphics.spriteBatch, up, mid, gridColor, thickness );
							drawLine( graphics.spriteBatch, mid, p, gridColor, thickness );
						}
						else
						{
							drawLine( graphics.spriteBatch, up, p, gridColor, thickness );
						}
					}

					// Add interpolated lines halfway between our point masses. This makes the grid look
					// denser without the cost of simulating more springs and point masses.
					if( x > 1 && y > 1 )
					{
						var upLeft = projectToVector2( _points[x - 1, y - 1].position );
						drawLine( graphics.spriteBatch, 0.5f * ( upLeft + up ), 0.5f * ( left + p ), gridColor, 1f );	// vertical line
						drawLine( graphics.spriteBatch, 0.5f * ( upLeft + left ), 0.5f * ( up + p ), gridColor, 1f );	// horizontal line
					}
				}
			}
		}


		Vector2 projectToVector2( Vector3 v )
		{
			// do a perspective projection
			var factor = ( v.Z + 2000 ) * 0.0005f;
			return ( new Vector2( v.X, v.Y ) - _screenSize * 0.5f ) * factor + _screenSize * 0.5f;
		}


		static void drawLine( SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 2f )
		{
			var delta = end - start;
			var angle = (float)Math.Atan2( delta.Y, delta.X );
			spriteBatch.Draw( Nez.Graphics.instance.pixelTexture, start, null, color, angle, new Vector2( 0, 0.5f ), new Vector2( delta.Length(), thickness ), SpriteEffects.None, 0f );
		}
	
	}
}
