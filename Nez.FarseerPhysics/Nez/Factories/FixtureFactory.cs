using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public static partial class Farseer
	{
		/// <summary>
		/// exactly the same as FarseerPhysics.Factories.FixtureFactory except all units are in display/Nez space as opposed to simulation space
		/// </summary>
		public static class FixtureFactory
		{
			public static Fixture attachEdge( Vector2 start, Vector2 end, Body body, object userData = null )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachEdge( ConvertUnits.toSimUnits( start ), ConvertUnits.toSimUnits( end ), body, userData );
			}


			public static Fixture attachChainShape( Vertices vertices, Body body, object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] = ConvertUnits.toSimUnits( vertices[i] );

				return FarseerPhysics.Factories.FixtureFactory.AttachChainShape( vertices, body, userData );
			}


			public static Fixture attachLoopShape( Vertices vertices, Body body, object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] = ConvertUnits.toSimUnits( vertices[i] );

				return FarseerPhysics.Factories.FixtureFactory.AttachLoopShape( vertices, body, userData );
			}


			public static Fixture attachRectangle( float width, float height, float density, Vector2 offset, Body body, object userData = null )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachRectangle( ConvertUnits.toSimUnits( width ), ConvertUnits.toSimUnits( height ), density, ConvertUnits.toSimUnits( offset ), body, userData );
			}


			public static Fixture attachCircle( float radius, float density, Body body, object userData = null )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachCircle( ConvertUnits.toSimUnits( radius ), density, body, userData );
			}


			public static Fixture attachCircle( float radius, float density, Body body, Vector2 offset, object userData = null )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachCircle( ConvertUnits.toSimUnits( radius ), density, body, ConvertUnits.toSimUnits( offset ), userData );
			}


			public static Fixture attachPolygon( Vertices vertices, float density, Body body, object userData = null )
			{
				for( var i = 0; i < vertices.Count; i++ )
					vertices[i] = ConvertUnits.toSimUnits( vertices[i] );

				return FarseerPhysics.Factories.FixtureFactory.AttachPolygon( vertices, density, body, userData );
			}


			public static Fixture attachEllipse( float xRadius, float yRadius, int edges, float density, Body body, object userData = null )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachEllipse( ConvertUnits.toSimUnits( xRadius ), ConvertUnits.toSimUnits( yRadius ), edges, density, body, userData );
			}


			public static List<Fixture> attachCompoundPolygon( List<Vertices> list, float density, Body body, object userData = null )
			{
				for( var i = 0; i < list.Count; i++ )
				{
					var vertices = list[i];
					for( var j = 0; j < vertices.Count; j++ )
						vertices[j] = ConvertUnits.toSimUnits( vertices[j] );

				}

				return FarseerPhysics.Factories.FixtureFactory.AttachCompoundPolygon( list, density, body, userData );
			}


			public static Fixture attachLineArc( float radians, int sides, float radius, bool closed, Body body )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachLineArc( radians, sides, ConvertUnits.toSimUnits( radius ), closed, body );
			}


			public static List<Fixture> attachSolidArc( float density, float radians, int sides, float radius, Body body )
			{
				return FarseerPhysics.Factories.FixtureFactory.AttachSolidArc( density, radians, sides, ConvertUnits.toSimUnits( radius ), body );
			}
		}
	}
}