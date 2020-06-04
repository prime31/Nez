using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// Renders a trail behind a moving object
	/// Adapted from http://www.paradeofrain.com/2010/01/28/update-on-continuous-2d-trails-in-xna/
	/// </summary>
	public class TrailRibbon : RenderableComponent, IUpdatable
	{
		public override RectangleF Bounds => _bounds;

		/// <summary>
		/// starting color of the ribbon
		/// </summary>
		public Color StartColor = Color.OrangeRed;

		/// <summary>
		/// end (tail) color of the ribbon
		/// </summary>
		public Color EndColor = new Color(255, 255, 0, 0);

		/// <summary>
		/// max pixel radius of the ribbon
		/// </summary>
		public float RibbonRadius = 20;

		// number of max segments
		readonly int _ribbonLength = 50;

		VertexPositionColor[] _vertices;
		LinkedList<RibbonSegment> _segments = new LinkedList<RibbonSegment>();
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;


		public TrailRibbon() : this(50)
		{
		}


		public TrailRibbon(int ribbonLength)
		{
			_ribbonLength = ribbonLength;
		}


		/// <summary>
		/// builds the intialial ribbon segments
		/// </summary>
		void InitializeVertices()
		{
			var radiusVec = new Vector3(0, -RibbonRadius, 0);
			_vertices = new VertexPositionColor[_ribbonLength * 2 + 3];

			// head of ribbon
			_vertices[0].Position = new Vector3(Entity.Transform.Position, 0f) + radiusVec;
			_vertices[0].Color = Color.Red;
			_vertices[1].Position = new Vector3(Entity.Transform.Position, 0f) + radiusVec;
			_vertices[1].Color = Color.Yellow;
			_vertices[2].Position = new Vector3(Entity.Transform.Position, 0f) + radiusVec;
			_vertices[2].Color = Color.Green;

			var pos = Entity.Transform.Position;
			for (var i = 0; i < _ribbonLength; i++)
			{
				var distanceRatio = 1 - (1 / (float) _ribbonLength * (i + 1));
				var segRadius = distanceRatio * RibbonRadius; // the radius size of this current segment
				var seg = new RibbonSegment(pos, segRadius);
				_segments.AddLast(seg);
			}

			CalculateVertices();
		}


		/// <summary>
		/// transfers the data from our segments to the vertices for display
		/// </summary>
		void CalculateVertices()
		{
			if (!_areVertsDirty)
				return;

			var center = new Vector3(Entity.Transform.Position, 0f);
			var radVec = new Vector3(0, -RibbonRadius, 0);

			// starting triangle, the head
			_vertices[0].Position = center + radVec;
			_vertices[0].Color = Color.Red;
			_vertices[1].Position = center + radVec;
			_vertices[1].Color = Color.Yellow;
			_vertices[2].Position = center + radVec;
			_vertices[2].Color = Color.Green;

			var maxX = float.MinValue;
			var minX = float.MaxValue;
			var maxY = float.MinValue;
			var minY = float.MaxValue;

			var index = 3;
			var segCount = 1;
			foreach (var seg in _segments)
			{
				var ratio = 1 - (1 / (float) _ribbonLength * segCount);
				seg.Radius = ratio * RibbonRadius;

				ColorExt.Lerp(ref StartColor, ref EndColor, out _vertices[index].Color, 1 - ratio);
				_vertices[index].Position = seg.TopPoint;
				_vertices[index + 1].Position = seg.BottomPoint;
				_vertices[index + 1].Color = _vertices[index].Color;

				// update min/max for any visible verts
				maxX = Mathf.MaxOf(maxX, _vertices[index].Position.X, _vertices[index + 1].Position.X);
				minX = Mathf.MinOf(minX, _vertices[index].Position.X, _vertices[index + 1].Position.X);
				maxY = Mathf.MaxOf(maxY, _vertices[index].Position.Y, _vertices[index + 1].Position.Y);
				minY = Mathf.MinOf(minY, _vertices[index].Position.Y, _vertices[index + 1].Position.Y);

				// increment counters
				index += 2;
				segCount++;
			}

			_bounds.X = minX;
			_bounds.Y = minY;
			_bounds.Width = maxX - minX;
			_bounds.Height = maxY - minY;

			_areVertsDirty = false;
		}


		#region Component/RenderableComponent/IUpdatable

		public override void OnEnabled()
		{
			base.OnEnabled();

			_segments.Clear();
			InitializeVertices();
		}


		public override void OnAddedToEntity()
		{
			InitializeVertices();

			_basicEffect = Entity.Scene.Content.LoadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}


		public override void OnRemovedFromEntity()
		{
			Entity.Scene.Content.UnloadEffect(_basicEffect);
			_basicEffect = null;
		}


		public virtual void Update()
		{
			// remove last node and put it at the front with new settings
			var seg = _segments.Last.Value;
			_segments.RemoveLast();
			var velocity = Entity.Transform.Position - _segments.First.Value.Position;

			// if the distance between the last segment and the current position is too tiny then just copy over the current head value
			if (velocity.LengthSquared() > float.Epsilon * float.Epsilon)
			{
				seg.Position = Entity.Transform.Position;
				seg.Radius = RibbonRadius;
				seg.RadiusDirection = new Vector2(-velocity.Y, velocity.X);
				seg.RadiusDirection.Normalize();
			}
			else
			{
				seg.Position = _segments.First.Value.Position;
				seg.Radius = _segments.First.Value.Radius;
				seg.RadiusDirection = _segments.First.Value.RadiusDirection;
			}

			_segments.AddFirst(seg);
			_areVertsDirty = true;
		}


		public override bool IsVisibleFromCamera(Camera camera)
		{
			CalculateVertices();
			return base.IsVisibleFromCamera(camera);
		}


		public override void Render(Batcher batcher, Camera camera)
		{
			CalculateVertices();
			_basicEffect.Projection = camera.ProjectionMatrix;
			_basicEffect.View = camera.TransformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			Core.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, _ribbonLength * 2 + 1);
		}

		#endregion


		class RibbonSegment
		{
			public Vector2 Position;

			public Vector2 RadiusDirection;

			// normalized
			public float Radius;

			public Vector3 TopPoint
			{
				get
				{
					var tp = (Position + RadiusDirection * Radius);
					return new Vector3(tp.X, tp.Y, 1);
				}
			}

			public Vector3 BottomPoint
			{
				get
				{
					var bp = Position - RadiusDirection * Radius;
					return new Vector3(bp.X, bp.Y, 1);
				}
			}


			public RibbonSegment(Vector2 position, float radius)
			{
				Position = position;
				Radius = radius;
			}
		}
	}
}