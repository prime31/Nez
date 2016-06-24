using System;
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
		public override RectangleF bounds
		{
			// we calculate bounds in update so no need to mess with anything here
			get { return _bounds; }
		}

		/// <summary>
		/// starting color of the ribbon
		/// </summary>
		public Color startColor = Color.OrangeRed;

		/// <summary>
		/// end (tail) color of the ribbon
		/// </summary>
		public Color endColor = new Color( 255, 255, 0, 0 );

		/// <summary>
		/// max pixel radius of the ribbon
		/// </summary>
		public float ribbonRadius = 20;

		// number of max segments
		readonly int _ribbonLength = 50;

		VertexPositionColor[] _vertices;
		LinkedList<RibbonSegment> _segments = new LinkedList<RibbonSegment>();
		BasicEffect _basicEffect;
		bool _areVertsDirty = true;


		public TrailRibbon() : this( 50 )
		{}


		public TrailRibbon( int ribbonLength )
		{
			_ribbonLength = ribbonLength;
		}


		/// <summary>
		/// builds the intialial ribbon segments
		/// </summary>
		void initializeVertices()
		{
			var radiusVec = new Vector3( 0, -ribbonRadius, 0 );
			_vertices = new VertexPositionColor[_ribbonLength * 2 + 3];

			// head of ribbon
			_vertices[0].Position = new Vector3( entity.transform.position, 0f ) + radiusVec;
			_vertices[0].Color = Color.Red;
			_vertices[1].Position = new Vector3( entity.transform.position, 0f ) + radiusVec;
			_vertices[1].Color = Color.Yellow;
			_vertices[2].Position = new Vector3( entity.transform.position, 0f ) + radiusVec;
			_vertices[2].Color = Color.Green;

			var pos = entity.transform.position;
			for( var i = 0; i < _ribbonLength; i++ )
			{
				var distanceRatio = 1 - ( 1 / (float)_ribbonLength * ( i + 1 ) );
				var segRadius = distanceRatio * ribbonRadius; // the radius size of this current segment
				var seg = new RibbonSegment( pos, segRadius );
				_segments.AddLast( seg );

			}
			calculateVertices();
		}


		/// <summary>
		/// transfers the data from our segments to the vertices for display
		/// </summary>
		void calculateVertices()
		{
			if( !_areVertsDirty )
				return;
			
			var center = new Vector3( entity.transform.position, 0f );
			var radVec = new Vector3( 0, -ribbonRadius, 0 );
			
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
			foreach( var seg in _segments )
			{
				var ratio = 1 - ( 1 / (float)_ribbonLength * segCount );
				seg.radius = ratio * ribbonRadius;

				ColorExt.lerp( ref startColor, ref endColor, out _vertices[index].Color, 1 - ratio );
				_vertices[index].Position = seg.topPoint;
				_vertices[index + 1].Position = seg.bottomPoint;
				_vertices[index + 1].Color = _vertices[index].Color;

				// update min/max for any visible verts
				maxX = Mathf.maxOf( maxX, _vertices[index].Position.X, _vertices[index + 1].Position.X );
				minX = Mathf.minOf( minX, _vertices[index].Position.X, _vertices[index + 1].Position.X );
				maxY = Mathf.maxOf( maxY, _vertices[index].Position.Y, _vertices[index + 1].Position.Y );
				minY = Mathf.minOf( minY, _vertices[index].Position.Y, _vertices[index + 1].Position.Y );

				// increment counters
				index += 2;
				segCount++;
			}

			_bounds.x = minX;
			_bounds.y = minY;
			_bounds.width = maxX - minX;
			_bounds.height = maxY - minY;

			_areVertsDirty = false;
		}

		public override void onEnabled()
		{
			base.onEnabled();

			_segments.Clear();
			initializeVertices();
		}

		public override void onAddedToEntity()
		{
			initializeVertices();

			_basicEffect = entity.scene.contentManager.loadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
		}


		void IUpdatable.update()
		{
			// remove last node and put it at the front with new settings
			var seg = _segments.Last.Value;
			_segments.RemoveLast();
			var velocity = entity.transform.position - _segments.First.Value.position;

			// if the distance between the last segment and the current position is too tiny then just copy over the current head value
			if( velocity.LengthSquared() > float.Epsilon * float.Epsilon )
			{
				seg.position = entity.transform.position;
				seg.radius = ribbonRadius;
				seg.radiusDirection = new Vector2( -velocity.Y, velocity.X );
				seg.radiusDirection.Normalize();
			}
			else
			{
				seg.position = _segments.First.Value.position;
				seg.radius = _segments.First.Value.radius;
				seg.radiusDirection = _segments.First.Value.radiusDirection;
			}

			_segments.AddFirst( seg );
			_areVertsDirty = true;
		}


		public override bool isVisibleFromCamera( Camera camera )
		{
			calculateVertices();
			return base.isVisibleFromCamera( camera );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			calculateVertices();
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleStrip, _vertices, 0, _ribbonLength * 2 + 1 );
		}


		class RibbonSegment
		{
			public Vector2 position;
			public Vector2 radiusDirection;
			// normalized
			public float radius;

			public Vector3 topPoint
			{
				get
				{
					var tp = ( position + radiusDirection * radius );
					return new Vector3( tp.X, tp.Y, 1 );
				}
			}

			public Vector3 bottomPoint
			{
				get
				{
					var bp = position - radiusDirection * radius;
					return new Vector3( bp.X, bp.Y, 1 );
				}
			}


			public RibbonSegment( Vector2 position, float radius )
			{
				this.position = position;
				this.radius = radius;
			}
		}

	}
}

