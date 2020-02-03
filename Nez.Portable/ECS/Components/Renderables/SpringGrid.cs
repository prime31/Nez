using System.Collections.Generic;
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
			public Vector3 Position;
			public Vector3 Velocity;
			public float InverseMass;

			Vector3 _acceleration;
			float _damping = 0.98f;

			public PointMass(Vector3 position, float invMass)
			{
				Position = position;
				InverseMass = invMass;
			}


			public void ApplyForce(Vector3 force)
			{
				_acceleration += force * InverseMass;
			}


			public void IncreaseDamping(float factor)
			{
				_damping *= factor;
			}


			public void Update()
			{
				Velocity += _acceleration;
				Position += Velocity;
				_acceleration = Vector3.Zero;
				if (Velocity.LengthSquared() < 0.001f * 0.001f)
					Velocity = Vector3.Zero;

				Velocity *= _damping;
				_damping = 0.98f;
			}
		}


		class Spring
		{
			public PointMass End1;
			public PointMass End2;
			public float TargetLength;
			public float Stiffness;
			public float Damping;


			public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
			{
				End1 = end1;
				End2 = end2;
				Stiffness = stiffness;
				Damping = damping;
				TargetLength = Vector3.Distance(end1.Position, end2.Position) * 0.95f;
			}


			public void Update()
			{
				var x = End1.Position - End2.Position;

				var length = x.Length();

				// these springs can only pull, not push
				if (length <= TargetLength)
					return;

				x = (x / length) * (length - TargetLength);
				var dv = End2.Velocity - End1.Velocity;
				var force = Stiffness * x - dv * Damping;

				End1.ApplyForce(-force);
				End2.ApplyForce(force);
			}
		}

		#endregion

		/// <summary>
		/// width of the grid
		/// </summary>
		/// <value>The width.</value>
		public override float Width => _gridSize.Width;

		/// <summary>
		/// height of the grid
		/// </summary>
		/// <value>The height.</value>
		public override float Height => _gridSize.Height;

		/// <summary>
		/// color of all major grid lines
		/// </summary>
		public Color GridMajorColor = Color.OrangeRed;

		/// <summary>
		/// color of all minor grid lines
		/// </summary>
		public Color GridMinorColor = Color.PaleVioletRed;

		/// <summary>
		/// thickness of all major grid lines
		/// </summary>
		[Range(1, 10)] public float GridMajorThickness = 3f;

		/// <summary>
		/// thickness of all minor grid lines
		/// </summary>
		[Range(1, 10)] public float GridMinorThickness = 1f;

		/// <summary>
		/// how often a major grid line should appear on the x axis
		/// </summary>
		[Range(1, 10)] public int GridMajorPeriodX = 3;

		/// <summary>
		/// how often a major grid line should appear on the y axis
		/// </summary>
		[Range(1, 10)] public int GridMajorPeriodY = 3;

		Spring[] _springs;
		PointMass[,] _points;
		Rectangle _gridSize;
		Vector2 _screenSize;


		public SpringGrid() : this(new Rectangle(0, 0, Screen.Width, Screen.Height), new Vector2(30))
		{ }

		public SpringGrid(Rectangle gridSize, Vector2 spacing)
		{
			SetGridSizeAndSpacing(gridSize, spacing);
		}

		/// <summary>
		/// sets up the SpringGrid springs and points so that it can be drawn
		/// </summary>
		/// <param name="gridSize"></param>
		/// <param name="spacing"></param>
		public void SetGridSizeAndSpacing(Rectangle gridSize, Vector2 spacing)
		{
			_gridSize = gridSize;
			var springList = new List<Spring>();

			// we offset the gridSize location by half-spacing so the padding is applied evenly all around
			gridSize.Location -= spacing.ToPoint();
			gridSize.Width += (int) spacing.X;
			gridSize.Height += (int) spacing.Y;

			var numColumns = (int) (gridSize.Width / spacing.X) + 1;
			var numRows = (int) (gridSize.Height / spacing.Y) + 1;
			_points = new PointMass[numColumns, numRows];

			// these fixed points will be used to anchor the grid to fixed positions on the screen
			var fixedPoints = new PointMass[numColumns, numRows];

			// create the point masses
			int column = 0, row = 0;
			for (float y = gridSize.Top; y <= gridSize.Bottom; y += spacing.Y)
			{
				for (float x = gridSize.Left; x <= gridSize.Right; x += spacing.X)
				{
					_points[column, row] = new PointMass(new Vector3(x, y, 0), 1);
					fixedPoints[column, row] = new PointMass(new Vector3(x, y, 0), 0);
					column++;
				}

				row++;
				column = 0;
			}

			// link the point masses with springs
			for (var y = 0; y < numRows; y++)
			{
				for (var x = 0; x < numColumns; x++)
				{
					if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1) // anchor the border of the grid
						springList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.1f, 0.1f));
					else if (x % 3 == 0 && y % 3 == 0) // loosely anchor 1/9th of the point masses
						springList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.002f, 0.02f));

					const float stiffness = 0.28f;
					const float damping = 0.06f;

					if (x > 0)
						springList.Add(new Spring(_points[x - 1, y], _points[x, y], stiffness, damping));
					if (y > 0)
						springList.Add(new Spring(_points[x, y - 1], _points[x, y], stiffness, damping));
				}
			}

			_springs = springList.ToArray();
		}

		#region Force application

		/// <summary>
		/// applies a force in a 3-dimensional direction
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyDirectedForce(Vector2 force, Vector2 position, float radius)
		{
			ApplyDirectedForce(new Vector3(force, 0), new Vector3(position, 0), radius);
		}

		/// <summary>
		/// applies a force in a 3-dimensional direction
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
		{
			// translate position into our coordinate space
			position -= new Vector3(Entity.Transform.Position + LocalOffset, 0);
			foreach (var mass in _points)
			{
				if (Vector3.DistanceSquared(position, mass.Position) < radius * radius)
					mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
			}
		}

		/// <summary>
		/// applies a force that sucks the grid in towards the point
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyImplosiveForce(float force, Vector2 position, float radius)
		{
			ApplyImplosiveForce(force, new Vector3(position, 0), radius);
		}

		/// <summary>
		/// applies a force that sucks the grid in towards the point
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyImplosiveForce(float force, Vector3 position, float radius)
		{
			// translate position into our coordinate space
			position -= new Vector3(Entity.Transform.Position + LocalOffset, 0);
			foreach (var mass in _points)
			{
				var dist2 = Vector3.DistanceSquared(position, mass.Position);
				if (dist2 < radius * radius)
				{
					mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
					mass.IncreaseDamping(0.6f);
				}
			}
		}

		/// <summary>
		/// applies a force the pushes the grid out aware from the point
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyExplosiveForce(float force, Vector2 position, float radius)
		{
			ApplyExplosiveForce(force, new Vector3(position, 0), radius);
		}

		/// <summary>
		/// applies a force the pushes the grid out aware from the point
		/// </summary>
		/// <param name="force">Force.</param>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void ApplyExplosiveForce(float force, Vector3 position, float radius)
		{
			// translate position into our coordinate space
			position -= new Vector3(Entity.Transform.Position + LocalOffset, 0);
			foreach (var mass in _points)
			{
				var dist2 = Vector3.DistanceSquared(position, mass.Position);
				if (dist2 < radius * radius)
				{
					mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
					mass.IncreaseDamping(0.6f);
				}
			}
		}

		#endregion


		void IUpdatable.Update()
		{
			_screenSize.X = Screen.Width;
			_screenSize.Y = Screen.Height;

			foreach (var spring in _springs)
				spring.Update();

			foreach (var mass in _points)
				mass.Update();
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			// TODO: make culling smarter and only render the lines that are actually on the screen rather than all or nothing
			var width = _points.GetLength(0);
			var height = _points.GetLength(1);

			for (var y = 1; y < height; y++)
			{
				for (var x = 1; x < width; x++)
				{
					var left = new Vector2();
					var up = new Vector2();
					var p = ProjectToVector2(_points[x, y].Position);

					if (x > 1)
					{
						float thickness;
						Color gridColor;
						if (y % GridMajorPeriodY == 1)
						{
							thickness = GridMajorThickness;
							gridColor = GridMajorColor;
						}
						else
						{
							thickness = GridMinorThickness;
							gridColor = GridMinorColor;
						}


						// use Catmull-Rom interpolation to help smooth bends in the grid
						left = ProjectToVector2(_points[x - 1, y].Position);
						var clampedX = Math.Min(x + 1, width - 1);
						var mid = Vector2.CatmullRom(ProjectToVector2(_points[x - 2, y].Position), left, p,
							ProjectToVector2(_points[clampedX, y].Position), 0.5f);

						// If the grid is very straight here, draw a single straight line. Otherwise, draw lines to our new interpolated midpoint
						if (Vector2.DistanceSquared(mid, (left + p) / 2) > 1)
						{
							DrawLine(batcher, left, mid, gridColor, thickness);
							DrawLine(batcher, mid, p, gridColor, thickness);
						}
						else
						{
							DrawLine(batcher, left, p, gridColor, thickness);
						}
					}

					if (y > 1)
					{
						float thickness;
						Color gridColor;
						if (x % GridMajorPeriodX == 1)
						{
							thickness = GridMajorThickness;
							gridColor = GridMajorColor;
						}
						else
						{
							thickness = GridMinorThickness;
							gridColor = GridMinorColor;
						}

						up = ProjectToVector2(_points[x, y - 1].Position);
						var clampedY = Math.Min(y + 1, height - 1);
						var mid = Vector2.CatmullRom(ProjectToVector2(_points[x, y - 2].Position), up, p,
							ProjectToVector2(_points[x, clampedY].Position), 0.5f);

						if (Vector2.DistanceSquared(mid, (up + p) / 2) > 1)
						{
							DrawLine(batcher, up, mid, gridColor, thickness);
							DrawLine(batcher, mid, p, gridColor, thickness);
						}
						else
						{
							DrawLine(batcher, up, p, gridColor, thickness);
						}
					}

					// Add interpolated lines halfway between our point masses. This makes the grid look
					// denser without the cost of simulating more springs and point masses.
					if (x > 1 && y > 1)
					{
						var upLeft = ProjectToVector2(_points[x - 1, y - 1].Position);
						DrawLine(batcher, 0.5f * (upLeft + up), 0.5f * (left + p), GridMinorColor,
							GridMinorThickness); // vertical line
						DrawLine(batcher, 0.5f * (upLeft + left), 0.5f * (up + p), GridMinorColor,
							GridMinorThickness); // horizontal line
					}
				}
			}
		}

		Vector2 ProjectToVector2(Vector3 v)
		{
			// do a perspective projection
			var factor = (v.Z + 2000) * 0.0005f;
			return (new Vector2(v.X, v.Y) - _screenSize * 0.5f) * factor + _screenSize * 0.5f;
		}

		void DrawLine(Batcher batcher, Vector2 start, Vector2 end, Color color, float thickness = 2f)
		{
			var delta = end - start;
			var angle = (float) Math.Atan2(delta.Y, delta.X);
			batcher.Draw(Graphics.Instance.PixelTexture, start + Entity.Transform.Position + LocalOffset,
				Graphics.Instance.PixelTexture.SourceRect, color, angle, new Vector2(0, 0.5f),
				new Vector2(delta.Length(), thickness), SpriteEffects.None, LayerDepth);
		}
	}
}