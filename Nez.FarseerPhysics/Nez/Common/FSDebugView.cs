using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Farseer
{
	/// <summary>
	/// A debug view shows you what happens inside the physics engine. You can view
	/// bodies, joints, fixtures and more.
	/// </summary>
	public class FSDebugView : RenderableComponent, IDisposable
	{
		public override RectangleF bounds { get { return _bounds; } }

		/// <summary>
		/// Gets or sets the debug view flags
		/// </summary>
		public DebugViewFlags flags;

		/// <summary>
		/// the World we are drawing
		/// </summary>
		protected World world;

		//Shapes
		public Color defaultShapeColor = new Color( 0.9f, 0.7f, 0.7f );
		public Color inactiveShapeColor = new Color( 0.5f, 0.5f, 0.3f );
		public Color kinematicShapeColor = new Color( 0.5f, 0.5f, 0.9f );
		public Color sleepingShapeColor = new Color( 0.6f, 0.6f, 0.6f );
		public Color staticShapeColor = new Color( 0.5f, 0.9f, 0.5f );
		public Color textColor = Color.White;

		//Drawing
		PrimitiveBatch _primitiveBatch;
		Vector2[] _tempVertices = new Vector2[Settings.maxPolygonVertices];
		List<StringData> _stringData = new List<StringData>();

		Matrix _localProjection;
		Matrix _localView;


		//Contacts
		int _pointCount;
		const int maxContactPoints = 2048;
		ContactPoint[] _points = new ContactPoint[maxContactPoints];

		//Debug panel
		public Vector2 debugPanelPosition = new Vector2( 5, 5 );
		float _max;
		float _avg;
		float _min;
		StringBuilder _debugPanelSb = new StringBuilder();

		//Performance graph
		public bool adaptiveLimits = true;
		public int valuesToGraph = 500;
		public float minimumValue;
		public float maximumValue = 10;
		public Rectangle performancePanelBounds = new Rectangle( Screen.width - 300, 5, 200, 100 );
		List<float> _graphValues = new List<float>( 500 );
		Vector2[] _background = new Vector2[4];

		public const int circleSegments = 24;


		public FSDebugView()
		{
			_bounds = RectangleF.maxRect;

			//Default flags
			appendFlags( DebugViewFlags.Shape );
			appendFlags( DebugViewFlags.Controllers );
			appendFlags( DebugViewFlags.Joint );
		}


		public FSDebugView( World world ) : this()
		{
			this.world = world;
		}


		/// <summary>
		/// Append flags to the current flags
		/// </summary>
		/// <param name="flags">Flags.</param>
		public void appendFlags( DebugViewFlags flags )
		{
			this.flags |= flags;
		}


		/// <summary>
		/// Remove flags from the current flags
		/// </summary>
		/// <param name="flags">Flags.</param>
		public void removeFlags( DebugViewFlags flags )
		{
			this.flags &= ~flags;
		}


		#region IDisposable Members

		public void Dispose()
		{
			world.contactManager.onPreSolve -= preSolve;
		}

		#endregion


		public override void onAddedToEntity()
		{
			if( world == null )
				world = entity.scene.getOrCreateSceneComponent<FSWorld>();
			world.contactManager.onPreSolve += preSolve;

			transform.setPosition( new Vector2( -float.MaxValue, -float.MaxValue ) * 0.5f );
			_primitiveBatch = new PrimitiveBatch( 1000 );

			_localProjection = Matrix.CreateOrthographicOffCenter( 0f, Core.graphicsDevice.Viewport.Width, Core.graphicsDevice.Viewport.Height, 0f, 0f, 1f );
			_localView = Matrix.Identity;
		}


		void preSolve( Contact contact, ref Manifold oldManifold )
		{
			if( ( flags & DebugViewFlags.ContactPoints ) == DebugViewFlags.ContactPoints )
			{
				Manifold manifold = contact.manifold;

				if( manifold.pointCount == 0 )
					return;

				Fixture fixtureA = contact.fixtureA;

				FixedArray2<PointState> state1, state2;
				FarseerPhysics.Collision.Collision.getPointStates( out state1, out state2, ref oldManifold, ref manifold );

				FixedArray2<Vector2> points;
				Vector2 normal;
				contact.getWorldManifold( out normal, out points );

				for( int i = 0; i < manifold.pointCount && _pointCount < maxContactPoints; ++i )
				{
					if( fixtureA == null )
						_points[i] = new ContactPoint();

					ContactPoint cp = _points[_pointCount];
					cp.position = points[i];
					cp.normal = normal;
					cp.state = state2[i];
					_points[_pointCount] = cp;
					++_pointCount;
				}
			}
		}


		/// <summary>
		/// Call this to draw shapes and other debug draw data.
		/// </summary>
		void drawDebugData()
		{
			if( ( flags & DebugViewFlags.Shape ) == DebugViewFlags.Shape )
			{
				foreach( Body b in world.bodyList )
				{
					FarseerPhysics.Common.Transform xf;
					b.getTransform( out xf );
					foreach( Fixture f in b.fixtureList )
					{
						if( b.enabled == false )
							drawShape( f, xf, inactiveShapeColor );
						else if( b.bodyType == BodyType.Static )
							drawShape( f, xf, staticShapeColor );
						else if( b.bodyType == BodyType.Kinematic )
							drawShape( f, xf, kinematicShapeColor );
						else if( b.isAwake == false )
							drawShape( f, xf, sleepingShapeColor );
						else
							drawShape( f, xf, defaultShapeColor );
					}
				}
			}

			if( ( flags & DebugViewFlags.ContactPoints ) == DebugViewFlags.ContactPoints )
			{
				const float axisScale = 0.3f;

				for( int i = 0; i < _pointCount; ++i )
				{
					ContactPoint point = _points[i];

					if( point.state == PointState.Add )
						drawPoint( point.position, 0.1f, new Color( 0.3f, 0.95f, 0.3f ) );
					else if( point.state == PointState.Persist )
						drawPoint( point.position, 0.1f, new Color( 0.3f, 0.3f, 0.95f ) );

					if( ( flags & DebugViewFlags.ContactNormals ) == DebugViewFlags.ContactNormals )
					{
						Vector2 p1 = point.position;
						Vector2 p2 = p1 + axisScale * point.normal;
						drawSegment( p1, p2, new Color( 0.4f, 0.9f, 0.4f ) );
					}
				}

				_pointCount = 0;
			}

			if( ( flags & DebugViewFlags.PolygonPoints ) == DebugViewFlags.PolygonPoints )
			{
				foreach( Body body in world.bodyList )
				{
					foreach( Fixture f in body.fixtureList )
					{
						var polygon = f.shape as PolygonShape;
						if( polygon != null )
						{
							FarseerPhysics.Common.Transform xf;
							body.getTransform( out xf );

							for( int i = 0; i < polygon.vertices.Count; i++ )
							{
								Vector2 tmp = MathUtils.mul( ref xf, polygon.vertices[i] );
								drawPoint( tmp, 0.1f, Color.Red );
							}
						}
					}
				}
			}

			if( ( flags & DebugViewFlags.Joint ) == DebugViewFlags.Joint )
			{
				foreach( var j in world.jointList )
					FSDebugView.drawJoint( this, j );
			}

			if( ( flags & DebugViewFlags.AABB ) == DebugViewFlags.AABB )
			{
				var color = new Color( 0.9f, 0.3f, 0.9f );
				var bp = world.contactManager.broadPhase;

				foreach( var body in world.bodyList )
				{
					if( body.enabled == false )
						continue;

					foreach( var f in body.fixtureList )
					{
						for( var t = 0; t < f.proxyCount; ++t )
						{
							var proxy = f.proxies[t];
							AABB aabb;
							bp.getFatAABB( proxy.proxyId, out aabb );

							drawAABB( ref aabb, color );
						}
					}
				}
			}

			if( ( flags & DebugViewFlags.CenterOfMass ) == DebugViewFlags.CenterOfMass )
			{
				foreach( Body b in world.bodyList )
				{
					FarseerPhysics.Common.Transform xf;
					b.getTransform( out xf );
					xf.p = b.worldCenter;
					drawTransform( ref xf );
				}
			}

			if( ( flags & DebugViewFlags.Controllers ) == DebugViewFlags.Controllers )
			{
				for( int i = 0; i < world.controllerList.Count; i++ )
				{
					Controller controller = world.controllerList[i];

					var buoyancy = controller as BuoyancyController;
					if( buoyancy != null )
					{
						AABB container = buoyancy.container;
						drawAABB( ref container, Color.LightBlue );
					}
				}
			}

			if( ( flags & DebugViewFlags.DebugPanel ) == DebugViewFlags.DebugPanel )
				drawDebugPanel();
		}


		void drawPerformanceGraph()
		{
			_graphValues.Add( world.updateTime / TimeSpan.TicksPerMillisecond );

			if( _graphValues.Count > valuesToGraph + 1 )
				_graphValues.RemoveAt( 0 );

			float x = performancePanelBounds.X;
			float deltaX = performancePanelBounds.Width / (float)valuesToGraph;
			float yScale = performancePanelBounds.Bottom - (float)performancePanelBounds.Top;

			// we must have at least 2 values to start rendering
			if( _graphValues.Count > 2 )
			{
				_max = _graphValues.Max();
				_avg = _graphValues.Average();
				_min = _graphValues.Min();

				if( adaptiveLimits )
				{
					maximumValue = _max;
					minimumValue = 0;
				}

				// start at last value (newest value added)
				// continue until no values are left
				for( int i = _graphValues.Count - 1; i > 0; i-- )
				{
					float y1 = performancePanelBounds.Bottom - ( ( _graphValues[i] / ( maximumValue - minimumValue ) ) * yScale );
					float y2 = performancePanelBounds.Bottom - ( ( _graphValues[i - 1] / ( maximumValue - minimumValue ) ) * yScale );

					var x1 = new Vector2( MathHelper.Clamp( x, performancePanelBounds.Left, performancePanelBounds.Right ), MathHelper.Clamp( y1, performancePanelBounds.Top, performancePanelBounds.Bottom ) );
					var x2 = new Vector2( MathHelper.Clamp( x + deltaX, performancePanelBounds.Left, performancePanelBounds.Right ), MathHelper.Clamp( y2, performancePanelBounds.Top, performancePanelBounds.Bottom ) );

					drawSegment( FSConvert.toSimUnits( x1 ), FSConvert.toSimUnits( x2 ), Color.LightGreen );

					x += deltaX;
				}
			}

			drawString( performancePanelBounds.Right + 10, performancePanelBounds.Top, string.Format( "Max: {0} ms", _max ) );
			drawString( performancePanelBounds.Right + 10, performancePanelBounds.Center.Y - 7, string.Format( "Avg: {0} ms", _avg ) );
			drawString( performancePanelBounds.Right + 10, performancePanelBounds.Bottom - 15, string.Format( "Min: {0} ms", _min ) );

			//Draw background.
			_background[0] = new Vector2( performancePanelBounds.X, performancePanelBounds.Y );
			_background[1] = new Vector2( performancePanelBounds.X, performancePanelBounds.Y + performancePanelBounds.Height );
			_background[2] = new Vector2( performancePanelBounds.X + performancePanelBounds.Width, performancePanelBounds.Y + performancePanelBounds.Height );
			_background[3] = new Vector2( performancePanelBounds.X + performancePanelBounds.Width, performancePanelBounds.Y );

			_background[0] = FSConvert.toSimUnits( _background[0] );
			_background[1] = FSConvert.toSimUnits( _background[1] );
			_background[2] = FSConvert.toSimUnits( _background[2] );
			_background[3] = FSConvert.toSimUnits( _background[3] );

			drawSolidPolygon( _background, 4, Color.DarkGray, true );
		}


		void drawDebugPanel()
		{
			int fixtureCount = 0;
			for( int i = 0; i < world.bodyList.Count; i++ )
				fixtureCount += world.bodyList[i].fixtureList.Count;

			var x = (int)debugPanelPosition.X;
			var y = (int)debugPanelPosition.Y;

			_debugPanelSb.Clear();
			_debugPanelSb.AppendLine( "Objects:" );
			_debugPanelSb.Append( "- Bodies: " ).AppendLine( world.bodyList.Count.ToString() );
			_debugPanelSb.Append( "- Fixtures: " ).AppendLine( fixtureCount.ToString() );
			_debugPanelSb.Append( "- Contacts: " ).AppendLine( world.contactList.Count.ToString() );
			_debugPanelSb.Append( "- Joints: " ).AppendLine( world.jointList.Count.ToString() );
			_debugPanelSb.Append( "- Controllers: " ).AppendLine( world.controllerList.Count.ToString() );
			_debugPanelSb.Append( "- Proxies: " ).AppendLine( world.proxyCount.ToString() );
			drawString( x, y, _debugPanelSb.ToString() );

			_debugPanelSb.Clear();
			_debugPanelSb.AppendLine( "Update time:" );
			_debugPanelSb.Append( "- Body: " ).AppendLine( string.Format( "{0} ms", world.solveUpdateTime / TimeSpan.TicksPerMillisecond ) );
			_debugPanelSb.Append( "- Contact: " ).AppendLine( string.Format( "{0} ms", world.contactsUpdateTime / TimeSpan.TicksPerMillisecond ) );
			_debugPanelSb.Append( "- CCD: " ).AppendLine( string.Format( "{0} ms", world.continuousPhysicsTime / TimeSpan.TicksPerMillisecond ) );
			_debugPanelSb.Append( "- Joint: " ).AppendLine( string.Format( "{0} ms", world.island.JointUpdateTime / TimeSpan.TicksPerMillisecond ) );
			_debugPanelSb.Append( "- Controller: " ).AppendLine( string.Format( "{0} ms", world.controllersUpdateTime / TimeSpan.TicksPerMillisecond ) );
			_debugPanelSb.Append( "- Total: " ).AppendLine( string.Format( "{0} ms", world.updateTime / TimeSpan.TicksPerMillisecond ) );
			drawString( x + 110, y, _debugPanelSb.ToString() );
		}


		#region Drawing methods

		public void drawAABB( ref AABB aabb, Color color )
		{
			Vector2[] verts = new Vector2[4];
			verts[0] = new Vector2( aabb.lowerBound.X, aabb.lowerBound.Y );
			verts[1] = new Vector2( aabb.upperBound.X, aabb.lowerBound.Y );
			verts[2] = new Vector2( aabb.upperBound.X, aabb.upperBound.Y );
			verts[3] = new Vector2( aabb.lowerBound.X, aabb.upperBound.Y );

			drawPolygon( verts, 4, color );
		}


		static void drawJoint( FSDebugView instance, Joint joint )
		{
			if( !joint.enabled )
				return;

			var b1 = joint.bodyA;
			var b2 = joint.bodyB;
			FarseerPhysics.Common.Transform xf1;
			b1.getTransform( out xf1 );

			var x2 = Vector2.Zero;

			if( b2 != null || !joint.isFixedType() )
			{
				FarseerPhysics.Common.Transform xf2;
				b2.getTransform( out xf2 );
				x2 = xf2.p;
			}

			var p1 = joint.worldAnchorA;
			var p2 = joint.worldAnchorB;
			var x1 = xf1.p;

			var color = new Color( 0.5f, 0.8f, 0.8f );

			switch( joint.jointType )
			{
				case JointType.Distance:
				{
					instance.drawSegment( p1, p2, color );
					break;
				}
				case JointType.Pulley:
				{
					var pulley = (PulleyJoint)joint;
					var s1 = b1.getWorldPoint( pulley.localAnchorA );
					var s2 = b2.getWorldPoint( pulley.localAnchorB );
					instance.drawSegment( p1, p2, color );
					instance.drawSegment( p1, s1, color );
					instance.drawSegment( p2, s2, color );
					break;
				}
				case JointType.FixedMouse:
				{
					instance.drawPoint( p1, 0.2f, new Color( 0.0f, 1.0f, 0.0f ) );
					instance.drawSegment( p1, p2, new Color( 0.8f, 0.8f, 0.8f ) );
					break;
				}
				case JointType.Revolute:
				{
					instance.drawSegment( x1, p1, color );
					instance.drawSegment( p1, p2, color );
					instance.drawSegment( x2, p2, color );

					instance.drawSolidCircle( p2, 0.1f, Vector2.Zero, Color.Red );
					instance.drawSolidCircle( p1, 0.1f, Vector2.Zero, Color.Blue );
					break;
				}
				case JointType.Gear:
				{
					instance.drawSegment( x1, x2, color );
					break;
				}
				default:
				{
					instance.drawSegment( x1, p1, color );
					instance.drawSegment( p1, p2, color );
					instance.drawSegment( x2, p2, color );
					break;
				}
			}
		}


		public void drawShape( Fixture fixture, FarseerPhysics.Common.Transform xf, Color color )
		{
			switch( fixture.shape.shapeType )
			{
				case ShapeType.Circle:
				{
					var circle = (CircleShape)fixture.shape;

					Vector2 center = MathUtils.mul( ref xf, circle.position );
					float radius = circle.radius;
					Vector2 axis = MathUtils.mul( xf.q, new Vector2( 1.0f, 0.0f ) );

					drawSolidCircle( center, radius, axis, color );
				}
				break;

				case ShapeType.Polygon:
				{
					var poly = (PolygonShape)fixture.shape;
					int vertexCount = poly.vertices.Count;
					System.Diagnostics.Debug.Assert( vertexCount <= Settings.maxPolygonVertices );

					if( vertexCount > _tempVertices.Length )
						_tempVertices = new Vector2[vertexCount];

					for( int i = 0; i < vertexCount; ++i )
					{
						_tempVertices[i] = MathUtils.mul( ref xf, poly.vertices[i] );
					}

					drawSolidPolygon( _tempVertices, vertexCount, color );
				}
				break;

				case ShapeType.Edge:
				{
					var edge = (EdgeShape)fixture.shape;
					var v1 = MathUtils.mul( ref xf, edge.vertex1 );
					var v2 = MathUtils.mul( ref xf, edge.vertex2 );
					drawSegment( v1, v2, color );
				}
				break;

				case ShapeType.Chain:
				{
					var chain = (ChainShape)fixture.shape;
					for( int i = 0; i < chain.vertices.Count - 1; ++i )
					{
						var v1 = MathUtils.mul( ref xf, chain.vertices[i] );
						var v2 = MathUtils.mul( ref xf, chain.vertices[i + 1] );
						drawSegment( v1, v2, color );
					}
				}
				break;
			}
		}


		public void drawPolygon( Vector2[] vertices, int count, float red, float green, float blue, bool closed = true )
		{
			drawPolygon( vertices, count, new Color( red, green, blue ), closed );
		}


		public void drawPolygon( Vector2[] vertices, int count, Color color, bool closed = true )
		{
			for( int i = 0; i < count - 1; i++ )
			{
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[i] ), color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[i + 1] ), color, PrimitiveType.LineList );
			}
			if( closed )
			{
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[count - 1] ), color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[0] ), color, PrimitiveType.LineList );
			}
		}


		public void drawSolidPolygon( Vector2[] vertices, int count, float red, float green, float blue )
		{
			drawSolidPolygon( vertices, count, new Color( red, green, blue ) );
		}


		public void drawSolidPolygon( Vector2[] vertices, int count, Color color, bool outline = true )
		{
			if( count == 2 )
			{
				drawPolygon( vertices, count, color );
				return;
			}

			var colorFill = color * ( outline ? 0.5f : 1.0f );

			for( int i = 1; i < count - 1; i++ )
			{
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[0] ), colorFill, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[i] ), colorFill, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( vertices[i + 1] ), colorFill, PrimitiveType.TriangleList );
			}

			if( outline )
				drawPolygon( vertices, count, color );
		}


		public void drawCircle( Vector2 center, float radius, float red, float green, float blue )
		{
			drawCircle( center, radius, new Color( red, green, blue ) );
		}


		public void drawCircle( Vector2 center, float radius, Color color )
		{
			const double increment = Math.PI * 2.0 / circleSegments;
			double theta = 0.0;

			for( int i = 0; i < circleSegments; i++ )
			{
				Vector2 v1 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
				Vector2 v2 = center + radius * new Vector2( (float)Math.Cos( theta + increment ), (float)Math.Sin( theta + increment ) );

				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( v1 ), color, PrimitiveType.LineList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( v2 ), color, PrimitiveType.LineList );

				theta += increment;
			}
		}


		public void drawSolidCircle( Vector2 center, float radius, Vector2 axis, float red, float green, float blue )
		{
			drawSolidCircle( center, radius, axis, new Color( red, green, blue ) );
		}


		public void drawSolidCircle( Vector2 center, float radius, Vector2 axis, Color color )
		{
			const double increment = Math.PI * 2.0 / circleSegments;
			double theta = 0.0;

			Color colorFill = color * 0.5f;

			Vector2 v0 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
			FSConvert.toDisplayUnits( ref v0, out v0 );
			theta += increment;

			for( int i = 1; i < circleSegments - 1; i++ )
			{
				Vector2 v1 = center + radius * new Vector2( (float)Math.Cos( theta ), (float)Math.Sin( theta ) );
				Vector2 v2 = center + radius * new Vector2( (float)Math.Cos( theta + increment ), (float)Math.Sin( theta + increment ) );

				_primitiveBatch.addVertex( v0, colorFill, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( v1 ), colorFill, PrimitiveType.TriangleList );
				_primitiveBatch.addVertex( FSConvert.toDisplayUnits( v2 ), colorFill, PrimitiveType.TriangleList );

				theta += increment;
			}

			drawCircle( center, radius, color );
			drawSegment( center, center + axis * radius, color );
		}


		public void drawSegment( Vector2 start, Vector2 end, float red, float green, float blue )
		{
			drawSegment( start, end, new Color( red, green, blue ) );
		}


		public void drawSegment( Vector2 start, Vector2 end, Color color )
		{
			start = FSConvert.toDisplayUnits( start );
			end = FSConvert.toDisplayUnits( end );
			_primitiveBatch.addVertex( start, color, PrimitiveType.LineList );
			_primitiveBatch.addVertex( end, color, PrimitiveType.LineList );
		}


		public void drawTransform( ref FarseerPhysics.Common.Transform transform )
		{
			const float axisScale = 0.4f;
			Vector2 p1 = transform.p;

			Vector2 p2 = p1 + axisScale * transform.q.GetXAxis();
			drawSegment( p1, p2, Color.Red );

			p2 = p1 + axisScale * transform.q.GetYAxis();
			drawSegment( p1, p2, Color.Green );
		}


		public void drawPoint( Vector2 p, float size, Color color )
		{
			Vector2[] verts = new Vector2[4];
			float hs = size / 2.0f;
			verts[0] = p + new Vector2( -hs, -hs );
			verts[1] = p + new Vector2( hs, -hs );
			verts[2] = p + new Vector2( hs, hs );
			verts[3] = p + new Vector2( -hs, hs );

			drawSolidPolygon( verts, 4, color, true );
		}


		public void drawString( int x, int y, string text )
		{
			drawString( new Vector2( x, y ), text );
		}


		public void drawString( Vector2 position, string text )
		{
			_stringData.Add( new StringData( position, text, textColor ) );
		}


		public void drawArrow( Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color )
		{
			// Draw connection segment between start- and end-point
			drawSegment( start, end, color );

			// Precalculate halfwidth
			var halfWidth = width / 2;

			// Create directional reference
			var rotation = ( start - end );
			Nez.Vector2Ext.normalize( ref rotation );

			// Calculate angle of directional vector
			var angle = (float)Math.Atan2( rotation.X, -rotation.Y );
			// Create matrix for rotation
			var rotMatrix = Matrix.CreateRotationZ( angle );
			// Create translation matrix for end-point
			var endMatrix = Matrix.CreateTranslation( end.X, end.Y, 0 );

			// Setup arrow end shape
			var verts = new Vector2[3];
			verts[0] = new Vector2( 0, 0 );
			verts[1] = new Vector2( -halfWidth, -length );
			verts[2] = new Vector2( halfWidth, -length );

			// Rotate end shape
			Vector2.Transform( verts, ref rotMatrix, verts );
			// Translate end shape
			Vector2.Transform( verts, ref endMatrix, verts );

			// Draw arrow end shape
			drawSolidPolygon( verts, 3, color, false );

			if( drawStartIndicator )
			{
				// Create translation matrix for start
				var startMatrix = Matrix.CreateTranslation( start.X, start.Y, 0 );
				// Setup arrow start shape
				var baseVerts = new Vector2[4];
				baseVerts[0] = new Vector2( -halfWidth, length / 4 );
				baseVerts[1] = new Vector2( halfWidth, length / 4 );
				baseVerts[2] = new Vector2( halfWidth, 0 );
				baseVerts[3] = new Vector2( -halfWidth, 0 );

				// Rotate start shape
				Vector2.Transform( baseVerts, ref rotMatrix, baseVerts );
				// Translate start shape
				Vector2.Transform( baseVerts, ref startMatrix, baseVerts );
				// Draw start shape
				drawSolidPolygon( baseVerts, 4, color, false );
			}
		}

		#endregion


		public void beginCustomDraw()
		{
			_primitiveBatch.begin( entity.scene.camera.projectionMatrix, entity.scene.camera.transformMatrix );
		}


		public void endCustomDraw()
		{
			_primitiveBatch.end();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// nothing is enabled - don't draw the debug view.
			if( flags == 0 )
				return;

			Core.graphicsDevice.RasterizerState = RasterizerState.CullNone;
			Core.graphicsDevice.DepthStencilState = DepthStencilState.Default;

			_primitiveBatch.begin( camera.projectionMatrix, camera.transformMatrix );
			drawDebugData();
			_primitiveBatch.end();

			if( ( flags & DebugViewFlags.PerformanceGraph ) == DebugViewFlags.PerformanceGraph )
			{
				_primitiveBatch.begin( ref _localProjection, ref _localView );
				drawPerformanceGraph();
				_primitiveBatch.end();
			}

			// draw any strings we have
			for( int i = 0; i < _stringData.Count; i++ )
				graphics.batcher.drawString( graphics.bitmapFont, _stringData[i].text, _stringData[i].position, _stringData[i].color );

			_stringData.Clear();
		}


		#region Nested types

		[Flags]
		public enum DebugViewFlags
		{
			/// <summary>
			/// Draw shapes.
			/// </summary>
			Shape = ( 1 << 0 ),

			/// <summary>
			/// Draw joint connections.
			/// </summary>
			Joint = ( 1 << 1 ),

			/// <summary>
			/// Draw axis aligned bounding boxes.
			/// </summary>
			AABB = ( 1 << 2 ),

			// Draw broad-phase pairs.
			//Pair = (1 << 3),

			/// <summary>
			/// Draw center of mass frame.
			/// </summary>
			CenterOfMass = ( 1 << 4 ),

			/// <summary>
			/// Draw useful debug data such as timings and number of bodies, joints, contacts and more.
			/// </summary>
			DebugPanel = ( 1 << 5 ),

			/// <summary>
			/// Draw contact points between colliding bodies.
			/// </summary>
			ContactPoints = ( 1 << 6 ),

			/// <summary>
			/// Draw contact normals. Need ContactPoints to be enabled first.
			/// </summary>
			ContactNormals = ( 1 << 7 ),

			/// <summary>
			/// Draws the vertices of polygons.
			/// </summary>
			PolygonPoints = ( 1 << 8 ),

			/// <summary>
			/// Draws the performance graph.
			/// </summary>
			PerformanceGraph = ( 1 << 9 ),

			/// <summary>
			/// Draws controllers.
			/// </summary>
			Controllers = ( 1 << 10 )
		}

		struct ContactPoint
		{
			public Vector2 normal;
			public Vector2 position;
			public PointState state;
		}

		struct StringData
		{
			public Color color;
			public string text;
			public Vector2 position;

			public StringData( Vector2 position, string text, Color color )
			{
				this.position = position;
				this.text = text;
				this.color = color;
			}
		}

		#endregion

	}

}