using System.Collections.Generic;
using FarseerPhysics;
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
			public static Body createBody( World world, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return new Body( world, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createEdge( World world, Vector2 start, Vector2 end, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateEdge( world, ConvertUnits.toSimUnits( start ), ConvertUnits.toSimUnits( end ), userData );
			}


			public static Body createChainShape( World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateChainShape( world, vertices, ConvertUnits.toSimUnits( position ), userData );
			}


			public static Body createLoopShape( World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateLoopShape( world, vertices, ConvertUnits.toSimUnits( position ), userData );
			}


			public static Body createRectangle( World world, float width, float height, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateRectangle( world, ConvertUnits.toSimUnits( width ), ConvertUnits.toSimUnits( height ), density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createCircle( World world, float radius, float density, Vector2 position = new Vector2(), BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCircle( world, ConvertUnits.toSimUnits( radius ), density, ConvertUnits.toSimUnits( position ), bodyType, userData );
			}


			public static Body createEllipse( World world, float xRadius, float yRadius, int edges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateEllipse( world, ConvertUnits.toSimUnits( xRadius ), ConvertUnits.toSimUnits( yRadius ), edges, density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createPolygon( World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreatePolygon( world, vertices, density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createCompoundPolygon( World world, List<Vertices> list, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCompoundPolygon( world, list, density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createGear( World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateGear( world, ConvertUnits.toSimUnits( radius ), numberOfTeeth, tipPercentage, ConvertUnits.toSimUnits( toothHeight ), density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createCapsule( World world, float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				height *= ConvertUnits.displayToSim;
				topRadius *= ConvertUnits.displayToSim;
				bottomRadius *= ConvertUnits.displayToSim;
				position *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateCapsule( world, height, topRadius, topEdges, bottomRadius, bottomEdges, density, position, rotation, bodyType, userData );
			}


			public static Body createCapsule( World world, float height, float endRadius, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateCapsule( world, ConvertUnits.toSimUnits( height ), ConvertUnits.toSimUnits( endRadius ), density, ConvertUnits.toSimUnits( position ), rotation, bodyType, userData );
			}


			public static Body createRoundedRectangle( World world, float width, float height, float xRadius, float yRadius, int segments, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				width *= ConvertUnits.displayToSim;
				height *= ConvertUnits.displayToSim;
				xRadius *= ConvertUnits.displayToSim;
				yRadius *= ConvertUnits.displayToSim;
				position *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateRoundedRectangle( world, width, height, xRadius, yRadius, segments, density, position, rotation, bodyType, userData );
			}


			public static Body createLineArc( World world, float radians, int sides, float radius, bool closed = false, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				var body = FarseerPhysics.Factories.BodyFactory.CreateLineArc( world, radians, sides, ConvertUnits.toSimUnits( radius ), closed, ConvertUnits.toSimUnits( position ), rotation, bodyType );
				body.UserData = userData;
				return body;
			}


			public static Body createSolidArc( World world, float density, float radians, int sides, float radius, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateSolidArc( world, density, radians, sides, ConvertUnits.toSimUnits( radius ), ConvertUnits.toSimUnits( position ), rotation, bodyType );
			}


			public static BreakableBody createBreakableBody( World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0 )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] *= ConvertUnits.displayToSim;

				return FarseerPhysics.Factories.BodyFactory.CreateBreakableBody( world, vertices, density, ConvertUnits.toSimUnits( position ), rotation );
			}


			public static BreakableBody createBreakableBody( World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(), float rotation = 0 )
			{
				return FarseerPhysics.Factories.BodyFactory.CreateBreakableBody( world, shapes, ConvertUnits.toSimUnits( position ), rotation );
			}
		}
	}
}