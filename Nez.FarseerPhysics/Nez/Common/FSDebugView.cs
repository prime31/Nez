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
		public override RectangleF Bounds => _bounds;

		/// <summary>
		/// Gets or sets the debug view flags
		/// </summary>
		public DebugViewFlags Flags;

		/// <summary>
		/// the World we are drawing
		/// </summary>
		protected World world;

		//Shapes
		public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);
		public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);
		public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);
		public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);
		public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);
		public Color TextColor = Color.White;

		//Drawing
		PrimitiveBatch _primitiveBatch;
		Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];
		List<StringData> _stringData = new List<StringData>();

		Matrix _localProjection;
		Matrix _localView;


		//Contacts
		int _pointCount;
		const int maxContactPoints = 2048;
		ContactPoint[] _points = new ContactPoint[maxContactPoints];

		//Debug panel
		public Vector2 DebugPanelPosition = new Vector2(5, 5);
		float _max;
		float _avg;
		float _min;
		StringBuilder _debugPanelSb = new StringBuilder();

		//Performance graph
		public bool AdaptiveLimits = true;
		public int ValuesToGraph = 500;
		public float MinimumValue;
		public float MaximumValue = 10;
		public Rectangle PerformancePanelBounds = new Rectangle(Screen.Width - 300, 5, 200, 100);
		List<float> _graphValues = new List<float>(500);
		Vector2[] _background = new Vector2[4];

		public const int CircleSegments = 24;


		public FSDebugView()
		{
			_bounds = RectangleF.MaxRect;

			//Default flags
			AppendFlags(DebugViewFlags.Shape);
			AppendFlags(DebugViewFlags.Controllers);
			AppendFlags(DebugViewFlags.Joint);
		}


		public FSDebugView(World world) : this()
		{
			this.world = world;
		}


		/// <summary>
		/// Append flags to the current flags
		/// </summary>
		/// <param name="flags">Flags.</param>
		public void AppendFlags(DebugViewFlags flags)
		{
			this.Flags |= flags;
		}


		/// <summary>
		/// Remove flags from the current flags
		/// </summary>
		/// <param name="flags">Flags.</param>
		public void RemoveFlags(DebugViewFlags flags)
		{
			this.Flags &= ~flags;
		}


		#region IDisposable Members

		public void Dispose()
		{
			world.ContactManager.OnPreSolve -= PreSolve;
		}

		#endregion


		public override void OnAddedToEntity()
		{
			if (world == null)
				world = Entity.Scene.GetOrCreateSceneComponent<FSWorld>();
			world.ContactManager.OnPreSolve += PreSolve;

			Transform.SetPosition(new Vector2(-float.MaxValue, -float.MaxValue) * 0.5f);
			_primitiveBatch = new PrimitiveBatch(1000);

			_localProjection = Matrix.CreateOrthographicOffCenter(0f, Core.GraphicsDevice.Viewport.Width,
				Core.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
			_localView = Matrix.Identity;
		}


		void PreSolve(Contact contact, ref Manifold oldManifold)
		{
			if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
			{
				Manifold manifold = contact.Manifold;

				if (manifold.PointCount == 0)
					return;

				Fixture fixtureA = contact.FixtureA;

				FixedArray2<PointState> state1, state2;
				FarseerPhysics.Collision.Collision.GetPointStates(out state1, out state2, ref oldManifold,
					ref manifold);

				FixedArray2<Vector2> points;
				Vector2 normal;
				contact.GetWorldManifold(out normal, out points);

				for (int i = 0; i < manifold.PointCount && _pointCount < maxContactPoints; ++i)
				{
					if (fixtureA == null)
						_points[i] = new ContactPoint();

					ContactPoint cp = _points[_pointCount];
					cp.Position = points[i];
					cp.Normal = normal;
					cp.State = state2[i];
					_points[_pointCount] = cp;
					++_pointCount;
				}
			}
		}


		/// <summary>
		/// Call this to draw shapes and other debug draw data.
		/// </summary>
		void DrawDebugData()
		{
			if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
			{
				foreach (Body b in world.BodyList)
				{
					FarseerPhysics.Common.Transform xf;
					b.GetTransform(out xf);
					foreach (Fixture f in b.FixtureList)
					{
						if (b.Enabled == false)
							DrawShape(f, xf, InactiveShapeColor);
						else if (b.BodyType == BodyType.Static)
							DrawShape(f, xf, StaticShapeColor);
						else if (b.BodyType == BodyType.Kinematic)
							DrawShape(f, xf, KinematicShapeColor);
						else if (b.IsAwake == false)
							DrawShape(f, xf, SleepingShapeColor);
						else
							DrawShape(f, xf, DefaultShapeColor);
					}
				}
			}

			if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
			{
				const float axisScale = 0.3f;

				for (int i = 0; i < _pointCount; ++i)
				{
					ContactPoint point = _points[i];

					if (point.State == PointState.Add)
						DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
					else if (point.State == PointState.Persist)
						DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));

					if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
					{
						Vector2 p1 = point.Position;
						Vector2 p2 = p1 + axisScale * point.Normal;
						DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
					}
				}

				_pointCount = 0;
			}

			if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
			{
				foreach (Body body in world.BodyList)
				{
					foreach (Fixture f in body.FixtureList)
					{
						var polygon = f.Shape as PolygonShape;
						if (polygon != null)
						{
							FarseerPhysics.Common.Transform xf;
							body.GetTransform(out xf);

							for (int i = 0; i < polygon.Vertices.Count; i++)
							{
								Vector2 tmp = MathUtils.Mul(ref xf, polygon.Vertices[i]);
								DrawPoint(tmp, 0.1f, Color.Red);
							}
						}
					}
				}
			}

			if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
			{
				foreach (var j in world.JointList)
					FSDebugView.DrawJoint(this, j);
			}

			if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
			{
				var color = new Color(0.9f, 0.3f, 0.9f);
				var bp = world.ContactManager.BroadPhase;

				foreach (var body in world.BodyList)
				{
					if (body.Enabled == false)
						continue;

					foreach (var f in body.FixtureList)
					{
						for (var t = 0; t < f.ProxyCount; ++t)
						{
							var proxy = f.Proxies[t];
							AABB aabb;
							bp.GetFatAABB(proxy.ProxyId, out aabb);

							DrawAABB(ref aabb, color);
						}
					}
				}
			}

			if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
			{
				foreach (Body b in world.BodyList)
				{
					FarseerPhysics.Common.Transform xf;
					b.GetTransform(out xf);
					xf.P = b.WorldCenter;
					DrawTransform(ref xf);
				}
			}

			if ((Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
			{
				for (int i = 0; i < world.ControllerList.Count; i++)
				{
					Controller controller = world.ControllerList[i];

					var buoyancy = controller as BuoyancyController;
					if (buoyancy != null)
					{
						AABB container = buoyancy.Container;
						DrawAABB(ref container, Color.LightBlue);
					}
				}
			}

			if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
				DrawDebugPanel();
		}


		void DrawPerformanceGraph()
		{
			_graphValues.Add(world.UpdateTime / TimeSpan.TicksPerMillisecond);

			if (_graphValues.Count > ValuesToGraph + 1)
				_graphValues.RemoveAt(0);

			float x = PerformancePanelBounds.X;
			float deltaX = PerformancePanelBounds.Width / (float) ValuesToGraph;
			float yScale = PerformancePanelBounds.Bottom - (float) PerformancePanelBounds.Top;

			// we must have at least 2 values to start rendering
			if (_graphValues.Count > 2)
			{
				_max = _graphValues.Max();
				_avg = _graphValues.Average();
				_min = _graphValues.Min();

				if (AdaptiveLimits)
				{
					MaximumValue = _max;
					MinimumValue = 0;
				}

				// start at last value (newest value added)
				// continue until no values are left
				for (int i = _graphValues.Count - 1; i > 0; i--)
				{
					float y1 = PerformancePanelBounds.Bottom -
					           ((_graphValues[i] / (MaximumValue - MinimumValue)) * yScale);
					float y2 = PerformancePanelBounds.Bottom -
					           ((_graphValues[i - 1] / (MaximumValue - MinimumValue)) * yScale);

					var x1 = new Vector2(MathHelper.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
						MathHelper.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
					var x2 = new Vector2(
						MathHelper.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right),
						MathHelper.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

					DrawSegment(FSConvert.ToSimUnits(x1), FSConvert.ToSimUnits(x2), Color.LightGreen);

					x += deltaX;
				}
			}

			DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top,
				string.Format("Max: {0} ms", _max));
			DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7,
				string.Format("Avg: {0} ms", _avg));
			DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15,
				string.Format("Min: {0} ms", _min));

			//Draw background.
			_background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
			_background[1] = new Vector2(PerformancePanelBounds.X,
				PerformancePanelBounds.Y + PerformancePanelBounds.Height);
			_background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width,
				PerformancePanelBounds.Y + PerformancePanelBounds.Height);
			_background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width,
				PerformancePanelBounds.Y);

			_background[0] = FSConvert.ToSimUnits(_background[0]);
			_background[1] = FSConvert.ToSimUnits(_background[1]);
			_background[2] = FSConvert.ToSimUnits(_background[2]);
			_background[3] = FSConvert.ToSimUnits(_background[3]);

			DrawSolidPolygon(_background, 4, Color.DarkGray, true);
		}


		void DrawDebugPanel()
		{
			int fixtureCount = 0;
			for (int i = 0; i < world.BodyList.Count; i++)
				fixtureCount += world.BodyList[i].FixtureList.Count;

			var x = (int) DebugPanelPosition.X;
			var y = (int) DebugPanelPosition.Y;

			_debugPanelSb.Clear();
			_debugPanelSb.AppendLine("Objects:");
			_debugPanelSb.Append("- Bodies: ").AppendLine(world.BodyList.Count.ToString());
			_debugPanelSb.Append("- Fixtures: ").AppendLine(fixtureCount.ToString());
			_debugPanelSb.Append("- Contacts: ").AppendLine(world.ContactList.Count.ToString());
			_debugPanelSb.Append("- Joints: ").AppendLine(world.JointList.Count.ToString());
			_debugPanelSb.Append("- Controllers: ").AppendLine(world.ControllerList.Count.ToString());
			_debugPanelSb.Append("- Proxies: ").AppendLine(world.ProxyCount.ToString());
			DrawString(x, y, _debugPanelSb.ToString());

			_debugPanelSb.Clear();
			_debugPanelSb.AppendLine("Update time:");
			_debugPanelSb.Append("- Body: ")
				.AppendLine(string.Format("{0} ms", world.SolveUpdateTime / TimeSpan.TicksPerMillisecond));
			_debugPanelSb.Append("- Contact: ")
				.AppendLine(string.Format("{0} ms", world.ContactsUpdateTime / TimeSpan.TicksPerMillisecond));
			_debugPanelSb.Append("- CCD: ")
				.AppendLine(string.Format("{0} ms", world.ContinuousPhysicsTime / TimeSpan.TicksPerMillisecond));
			_debugPanelSb.Append("- Joint: ")
				.AppendLine(string.Format("{0} ms", world.Island.JointUpdateTime / TimeSpan.TicksPerMillisecond));
			_debugPanelSb.Append("- Controller: ")
				.AppendLine(string.Format("{0} ms", world.ControllersUpdateTime / TimeSpan.TicksPerMillisecond));
			_debugPanelSb.Append("- Total: ")
				.AppendLine(string.Format("{0} ms", world.UpdateTime / TimeSpan.TicksPerMillisecond));
			DrawString(x + 110, y, _debugPanelSb.ToString());
		}


		#region Drawing methods

		public void DrawAABB(ref AABB aabb, Color color)
		{
			Vector2[] verts = new Vector2[4];
			verts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
			verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
			verts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
			verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

			DrawPolygon(verts, 4, color);
		}


		static void DrawJoint(FSDebugView instance, Joint joint)
		{
			if (!joint.Enabled)
				return;

			var b1 = joint.BodyA;
			var b2 = joint.BodyB;
			FarseerPhysics.Common.Transform xf1;
			b1.GetTransform(out xf1);

			var x2 = Vector2.Zero;

			if (b2 != null || !joint.IsFixedType())
			{
				FarseerPhysics.Common.Transform xf2;
				b2.GetTransform(out xf2);
				x2 = xf2.P;
			}

			var p1 = joint.WorldAnchorA;
			var p2 = joint.WorldAnchorB;
			var x1 = xf1.P;

			var color = new Color(0.5f, 0.8f, 0.8f);

			switch (joint.JointType)
			{
				case JointType.Distance:
				{
					instance.DrawSegment(p1, p2, color);
					break;
				}
				case JointType.Pulley:
				{
					var pulley = (PulleyJoint) joint;
					var s1 = b1.GetWorldPoint(pulley.LocalAnchorA);
					var s2 = b2.GetWorldPoint(pulley.LocalAnchorB);
					instance.DrawSegment(p1, p2, color);
					instance.DrawSegment(p1, s1, color);
					instance.DrawSegment(p2, s2, color);
					break;
				}
				case JointType.FixedMouse:
				{
					instance.DrawPoint(p1, 0.2f, new Color(0.0f, 1.0f, 0.0f));
					instance.DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
					break;
				}
				case JointType.Revolute:
				{
					instance.DrawSegment(x1, p1, color);
					instance.DrawSegment(p1, p2, color);
					instance.DrawSegment(x2, p2, color);

					instance.DrawSolidCircle(p2, 0.1f, Vector2.Zero, Color.Red);
					instance.DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color.Blue);
					break;
				}
				case JointType.Gear:
				{
					instance.DrawSegment(x1, x2, color);
					break;
				}
				default:
				{
					instance.DrawSegment(x1, p1, color);
					instance.DrawSegment(p1, p2, color);
					instance.DrawSegment(x2, p2, color);
					break;
				}
			}
		}


		public void DrawShape(Fixture fixture, FarseerPhysics.Common.Transform xf, Color color)
		{
			switch (fixture.Shape.ShapeType)
			{
				case ShapeType.Circle:
				{
					var circle = (CircleShape) fixture.Shape;

					Vector2 center = MathUtils.Mul(ref xf, circle.Position);
					float radius = circle.Radius;
					Vector2 axis = MathUtils.Mul(xf.Q, new Vector2(1.0f, 0.0f));

					DrawSolidCircle(center, radius, axis, color);
				}
					break;

				case ShapeType.Polygon:
				{
					var poly = (PolygonShape) fixture.Shape;
					int vertexCount = poly.Vertices.Count;
					System.Diagnostics.Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);

					if (vertexCount > _tempVertices.Length)
						_tempVertices = new Vector2[vertexCount];

					for (int i = 0; i < vertexCount; ++i)
					{
						_tempVertices[i] = MathUtils.Mul(ref xf, poly.Vertices[i]);
					}

					DrawSolidPolygon(_tempVertices, vertexCount, color);
				}
					break;

				case ShapeType.Edge:
				{
					var edge = (EdgeShape) fixture.Shape;
					var v1 = MathUtils.Mul(ref xf, edge.Vertex1);
					var v2 = MathUtils.Mul(ref xf, edge.Vertex2);
					DrawSegment(v1, v2, color);
				}
					break;

				case ShapeType.Chain:
				{
					var chain = (ChainShape) fixture.Shape;
					for (int i = 0; i < chain.Vertices.Count - 1; ++i)
					{
						var v1 = MathUtils.Mul(ref xf, chain.Vertices[i]);
						var v2 = MathUtils.Mul(ref xf, chain.Vertices[i + 1]);
						DrawSegment(v1, v2, color);
					}
				}
					break;
			}
		}


		public void DrawPolygon(Vector2[] vertices, int count, float red, float green, float blue, bool closed = true)
		{
			DrawPolygon(vertices, count, new Color(red, green, blue), closed);
		}


		public void DrawPolygon(Vector2[] vertices, int count, Color color, bool closed = true)
		{
			for (int i = 0; i < count - 1; i++)
			{
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[i]), color, PrimitiveType.LineList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[i + 1]), color, PrimitiveType.LineList);
			}

			if (closed)
			{
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[count - 1]), color, PrimitiveType.LineList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[0]), color, PrimitiveType.LineList);
			}
		}


		public void DrawSolidPolygon(Vector2[] vertices, int count, float red, float green, float blue)
		{
			DrawSolidPolygon(vertices, count, new Color(red, green, blue));
		}


		public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline = true)
		{
			if (count == 2)
			{
				DrawPolygon(vertices, count, color);
				return;
			}

			var colorFill = color * (outline ? 0.5f : 1.0f);

			for (int i = 1; i < count - 1; i++)
			{
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[0]), colorFill, PrimitiveType.TriangleList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[i]), colorFill, PrimitiveType.TriangleList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(vertices[i + 1]), colorFill,
					PrimitiveType.TriangleList);
			}

			if (outline)
				DrawPolygon(vertices, count, color);
		}


		public void DrawCircle(Vector2 center, float radius, float red, float green, float blue)
		{
			DrawCircle(center, radius, new Color(red, green, blue));
		}


		public void DrawCircle(Vector2 center, float radius, Color color)
		{
			const double increment = Math.PI * 2.0 / CircleSegments;
			double theta = 0.0;

			for (int i = 0; i < CircleSegments; i++)
			{
				Vector2 v1 = center + radius * new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
				Vector2 v2 = center + radius * new Vector2((float) Math.Cos(theta + increment),
					             (float) Math.Sin(theta + increment));

				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(v1), color, PrimitiveType.LineList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(v2), color, PrimitiveType.LineList);

				theta += increment;
			}
		}


		public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float green, float blue)
		{
			DrawSolidCircle(center, radius, axis, new Color(red, green, blue));
		}


		public void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
		{
			const double increment = Math.PI * 2.0 / CircleSegments;
			double theta = 0.0;

			Color colorFill = color * 0.5f;

			Vector2 v0 = center + radius * new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
			FSConvert.ToDisplayUnits(ref v0, out v0);
			theta += increment;

			for (int i = 1; i < CircleSegments - 1; i++)
			{
				Vector2 v1 = center + radius * new Vector2((float) Math.Cos(theta), (float) Math.Sin(theta));
				Vector2 v2 = center + radius * new Vector2((float) Math.Cos(theta + increment),
					             (float) Math.Sin(theta + increment));

				_primitiveBatch.AddVertex(v0, colorFill, PrimitiveType.TriangleList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(v1), colorFill, PrimitiveType.TriangleList);
				_primitiveBatch.AddVertex(FSConvert.ToDisplayUnits(v2), colorFill, PrimitiveType.TriangleList);

				theta += increment;
			}

			DrawCircle(center, radius, color);
			DrawSegment(center, center + axis * radius, color);
		}


		public void DrawSegment(Vector2 start, Vector2 end, float red, float green, float blue)
		{
			DrawSegment(start, end, new Color(red, green, blue));
		}


		public void DrawSegment(Vector2 start, Vector2 end, Color color)
		{
			start = FSConvert.ToDisplayUnits(start);
			end = FSConvert.ToDisplayUnits(end);
			_primitiveBatch.AddVertex(start, color, PrimitiveType.LineList);
			_primitiveBatch.AddVertex(end, color, PrimitiveType.LineList);
		}


		public void DrawTransform(ref FarseerPhysics.Common.Transform transform)
		{
			const float axisScale = 0.4f;
			Vector2 p1 = transform.P;

			Vector2 p2 = p1 + axisScale * transform.Q.GetXAxis();
			DrawSegment(p1, p2, Color.Red);

			p2 = p1 + axisScale * transform.Q.GetYAxis();
			DrawSegment(p1, p2, Color.Green);
		}


		public void DrawPoint(Vector2 p, float size, Color color)
		{
			Vector2[] verts = new Vector2[4];
			float hs = size / 2.0f;
			verts[0] = p + new Vector2(-hs, -hs);
			verts[1] = p + new Vector2(hs, -hs);
			verts[2] = p + new Vector2(hs, hs);
			verts[3] = p + new Vector2(-hs, hs);

			DrawSolidPolygon(verts, 4, color, true);
		}


		public void DrawString(int x, int y, string text)
		{
			DrawString(new Vector2(x, y), text);
		}


		public void DrawString(Vector2 position, string text)
		{
			_stringData.Add(new StringData(position, text, TextColor));
		}


		public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator,
		                      Color color)
		{
			// Draw connection segment between start- and end-point
			DrawSegment(start, end, color);

			// Precalculate halfwidth
			var halfWidth = width / 2;

			// Create directional reference
			var rotation = (start - end);
			Nez.Vector2Ext.Normalize(ref rotation);

			// Calculate angle of directional vector
			var angle = (float) Math.Atan2(rotation.X, -rotation.Y);

			// Create matrix for rotation
			var rotMatrix = Matrix.CreateRotationZ(angle);

			// Create translation matrix for end-point
			var endMatrix = Matrix.CreateTranslation(end.X, end.Y, 0);

			// Setup arrow end shape
			var verts = new Vector2[3];
			verts[0] = new Vector2(0, 0);
			verts[1] = new Vector2(-halfWidth, -length);
			verts[2] = new Vector2(halfWidth, -length);

			// Rotate end shape
			Vector2.Transform(verts, ref rotMatrix, verts);

			// Translate end shape
			Vector2.Transform(verts, ref endMatrix, verts);

			// Draw arrow end shape
			DrawSolidPolygon(verts, 3, color, false);

			if (drawStartIndicator)
			{
				// Create translation matrix for start
				var startMatrix = Matrix.CreateTranslation(start.X, start.Y, 0);

				// Setup arrow start shape
				var baseVerts = new Vector2[4];
				baseVerts[0] = new Vector2(-halfWidth, length / 4);
				baseVerts[1] = new Vector2(halfWidth, length / 4);
				baseVerts[2] = new Vector2(halfWidth, 0);
				baseVerts[3] = new Vector2(-halfWidth, 0);

				// Rotate start shape
				Vector2.Transform(baseVerts, ref rotMatrix, baseVerts);

				// Translate start shape
				Vector2.Transform(baseVerts, ref startMatrix, baseVerts);

				// Draw start shape
				DrawSolidPolygon(baseVerts, 4, color, false);
			}
		}

		#endregion


		public void BeginCustomDraw()
		{
			_primitiveBatch.Begin(Entity.Scene.Camera.ProjectionMatrix, Entity.Scene.Camera.TransformMatrix);
		}


		public void EndCustomDraw()
		{
			_primitiveBatch.End();
		}


		public override void Render(Batcher batcher, Camera camera)
		{
			// nothing is enabled - don't draw the debug view.
			if (Flags == 0)
				return;

			Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			_primitiveBatch.Begin(camera.ProjectionMatrix, camera.TransformMatrix);
			DrawDebugData();
			_primitiveBatch.End();

			if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
			{
				_primitiveBatch.Begin(ref _localProjection, ref _localView);
				DrawPerformanceGraph();
				_primitiveBatch.End();
			}

			// draw any strings we have
			for (int i = 0; i < _stringData.Count; i++)
				batcher.DrawString(Graphics.Instance.BitmapFont, _stringData[i].Text, _stringData[i].Position,
					_stringData[i].Color);

			_stringData.Clear();
		}


		#region Nested types

		[Flags]
		public enum DebugViewFlags
		{
			/// <summary>
			/// Draw shapes.
			/// </summary>
			Shape = (1 << 0),

			/// <summary>
			/// Draw joint connections.
			/// </summary>
			Joint = (1 << 1),

			/// <summary>
			/// Draw axis aligned bounding boxes.
			/// </summary>
			AABB = (1 << 2),

			// Draw broad-phase pairs.
			//Pair = (1 << 3),

			/// <summary>
			/// Draw center of mass frame.
			/// </summary>
			CenterOfMass = (1 << 4),

			/// <summary>
			/// Draw useful debug data such as timings and number of bodies, joints, contacts and more.
			/// </summary>
			DebugPanel = (1 << 5),

			/// <summary>
			/// Draw contact points between colliding bodies.
			/// </summary>
			ContactPoints = (1 << 6),

			/// <summary>
			/// Draw contact normals. Need ContactPoints to be enabled first.
			/// </summary>
			ContactNormals = (1 << 7),

			/// <summary>
			/// Draws the vertices of polygons.
			/// </summary>
			PolygonPoints = (1 << 8),

			/// <summary>
			/// Draws the performance graph.
			/// </summary>
			PerformanceGraph = (1 << 9),

			/// <summary>
			/// Draws controllers.
			/// </summary>
			Controllers = (1 << 10)
		}

		struct ContactPoint
		{
			public Vector2 Normal;
			public Vector2 Position;
			public PointState State;
		}

		struct StringData
		{
			public Color Color;
			public string Text;
			public Vector2 Position;

			public StringData(Vector2 position, string text, Color color)
			{
				this.Position = position;
				this.Text = text;
				this.Color = color;
			}
		}

		#endregion
	}
}