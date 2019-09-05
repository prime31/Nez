using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public static partial class Farseer
	{
		/// <summary>
		/// exactly the same as FarseerPhysics.Factories.BodyFactory except all units are in display/Nez space as opposed to simulation space
		/// </summary>
		public static class BodyFactory
		{
			public static Body CreateBody(World world, Vector2 position = new Vector2(), float rotation = 0,
			                              BodyType bodyType = BodyType.Static, object userData = null)
			{
				return new Body(world, FSConvert.ToSimUnits(position), rotation, bodyType, userData);
			}


			public static Body CreateEdge(World world, Vector2 start, Vector2 end, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateEdge(world, FSConvert.ToSimUnits(start),
					FSConvert.ToSimUnits(end), userData);
			}


			public static Body CreateChainShape(World world, Vertices vertices, Vector2 position = new Vector2(),
			                                    object userData = null)
			{
				for (var i = 0; i < vertices.Count; i++)
					vertices[i] *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateChainShape(world, vertices,
					FSConvert.ToSimUnits(position), userData);
			}


			public static Body CreateLoopShape(World world, Vertices vertices, Vector2 position = new Vector2(),
			                                   object userData = null)
			{
				for (var i = 0; i < vertices.Count; i++)
					vertices[i] *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateLoopShape(world, vertices,
					FSConvert.ToSimUnits(position), userData);
			}


			public static Body CreateRectangle(World world, float width, float height, float density,
			                                   Vector2 position = new Vector2(), float rotation = 0,
			                                   BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateRectangle(world, FSConvert.ToSimUnits(width),
					FSConvert.ToSimUnits(height), density, FSConvert.ToSimUnits(position), rotation, bodyType,
					userData);
			}


			public static Body CreateCircle(World world, float radius, float density, Vector2 position = new Vector2(),
			                                BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCircle(world, FSConvert.ToSimUnits(radius), density,
					FSConvert.ToSimUnits(position), bodyType, userData);
			}


			public static Body CreateEllipse(World world, float xRadius, float yRadius, int edges, float density,
			                                 Vector2 position = new Vector2(), float rotation = 0,
			                                 BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateEllipse(world, FSConvert.ToSimUnits(xRadius),
					FSConvert.ToSimUnits(yRadius), edges, density, FSConvert.ToSimUnits(position), rotation, bodyType,
					userData);
			}


			public static Body CreatePolygon(World world, Vertices vertices, float density,
			                                 Vector2 position = new Vector2(), float rotation = 0,
			                                 BodyType bodyType = BodyType.Static, object userData = null)
			{
				for (var i = 0; i < vertices.Count; i++)
					vertices[i] *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreatePolygon(world, vertices, density,
					FSConvert.ToSimUnits(position), rotation, bodyType, userData);
			}


			public static Body CreateCompoundPolygon(World world, List<Vertices> list, float density,
			                                         Vector2 position = new Vector2(), float rotation = 0,
			                                         BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCompoundPolygon(world, list, density,
					FSConvert.ToSimUnits(position), rotation, bodyType, userData);
			}


			public static Body CreateGear(World world, float radius, int numberOfTeeth, float tipPercentage,
			                              float toothHeight, float density, Vector2 position = new Vector2(),
			                              float rotation = 0, BodyType bodyType = BodyType.Static,
			                              object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateGear(world, FSConvert.ToSimUnits(radius),
					numberOfTeeth, tipPercentage, FSConvert.ToSimUnits(toothHeight), density,
					FSConvert.ToSimUnits(position), rotation, bodyType, userData);
			}


			public static Body CreateCapsule(World world, float height, float topRadius, int topEdges,
			                                 float bottomRadius, int bottomEdges, float density,
			                                 Vector2 position = new Vector2(), float rotation = 0,
			                                 BodyType bodyType = BodyType.Static, object userData = null)
			{
				height *= FSConvert.DisplayToSim;
				topRadius *= FSConvert.DisplayToSim;
				bottomRadius *= FSConvert.DisplayToSim;
				position *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateCapsule(world, height, topRadius, topEdges,
					bottomRadius, bottomEdges, density, position, rotation, bodyType, userData);
			}


			public static Body CreateCapsule(World world, float height, float endRadius, float density,
			                                 Vector2 position = new Vector2(), float rotation = 0,
			                                 BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCapsule(world, FSConvert.ToSimUnits(height),
					FSConvert.ToSimUnits(endRadius), density, FSConvert.ToSimUnits(position), rotation, bodyType,
					userData);
			}


			public static Body CreateRoundedRectangle(World world, float width, float height, float xRadius,
			                                          float yRadius, int segments, float density,
			                                          Vector2 position = new Vector2(), float rotation = 0,
			                                          BodyType bodyType = BodyType.Static, object userData = null)
			{
				width *= FSConvert.DisplayToSim;
				height *= FSConvert.DisplayToSim;
				xRadius *= FSConvert.DisplayToSim;
				yRadius *= FSConvert.DisplayToSim;
				position *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateRoundedRectangle(world, width, height, xRadius,
					yRadius, segments, density, position, rotation, bodyType, userData);
			}


			public static Body CreateLineArc(World world, float radians, int sides, float radius, bool closed = false,
			                                 Vector2 position = new Vector2(), float rotation = 0,
			                                 BodyType bodyType = BodyType.Static, object userData = null)
			{
				var body = FarseerPhysics.Factories.BodyFactory.CreateLineArc(world, radians, sides,
					FSConvert.ToSimUnits(radius), closed, FSConvert.ToSimUnits(position), rotation, bodyType);
				body.UserData = userData;
				return body;
			}


			public static Body CreateSolidArc(World world, float density, float radians, int sides, float radius,
			                                  Vector2 position = new Vector2(), float rotation = 0,
			                                  BodyType bodyType = BodyType.Static, object userData = null)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateSolidArc(world, density, radians, sides,
					FSConvert.ToSimUnits(radius), FSConvert.ToSimUnits(position), rotation, bodyType);
			}


			public static BreakableBody CreateBreakableBody(World world, Vertices vertices, float density,
			                                                Vector2 position = new Vector2(), float rotation = 0)
			{
				for (var i = 0; i < vertices.Count; i++)
					vertices[i] *= FSConvert.DisplayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateBreakableBody(world, vertices, density,
					FSConvert.ToSimUnits(position), rotation);
			}


			public static BreakableBody CreateBreakableBody(World world, IEnumerable<Shape> shapes,
			                                                Vector2 position = new Vector2(), float rotation = 0)
			{
				return FarseerPhysics.Factories.BodyFactory.CreateBreakableBody(world, shapes,
					FSConvert.ToSimUnits(position), rotation);
			}
		}
	}
}