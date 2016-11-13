using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	/// <summary>
	/// simple Component that will sync the position/rotation of the physics Body with the Transform. The GenericBody.transform.position
	/// will always match Body.Position. Note that scale is not considered here.
	/// </summary>
	public class FSGenericBody : Component, IUpdatable
	{
		public Body body;

		bool _ignoreTransformChanges;


		public FSGenericBody( World wold, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
		{
			body = new Body( wold, position * ConvertUnits.displayToSim, 0, bodyType );
		}


		public FSGenericBody( Body body )
		{
			this.body = body;
		}


		public override void onAddedToEntity()
		{
			// if scale is not 1 or rotation is not 0 then trigger a scale/rotation change for the Shape
			if( transform.scale.X != 1 )
				onEntityTransformChanged( Transform.Component.Scale );
			if( transform.rotation != 0 )
				onEntityTransformChanged( Transform.Component.Rotation );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			if( _ignoreTransformChanges )
				return;

			if( comp == Transform.Component.Position )
				body.Position = transform.position * ConvertUnits.displayToSim;
			else if( comp == Transform.Component.Rotation )
				body.Rotation = transform.rotation;
		}


		public override void onRemovedFromEntity()
		{
			if( body != null )
			{
				body.World.RemoveBody( body );
				body = null;
			}
		}


		void IUpdatable.update()
		{
			if( !body.Awake )
				return;

			_ignoreTransformChanges = true;
			transform.position = ConvertUnits.simToDisplay * body.Position;
			transform.rotation = body.Rotation;
			_ignoreTransformChanges = false;
		}


		#region Shapes

		public Fixture attachEdge( Vector2 start, Vector2 end )
		{
			return Farseer.FixtureFactory.attachEdge( start, end, body );
		}


		public Fixture attachChainShape( List<Vector2> vertices )
		{
			return Farseer.FixtureFactory.attachChainShape( new Vertices( vertices ), body );
		}


		public Fixture attachLoopShape( List<Vector2> vertices )
		{
			return Farseer.FixtureFactory.attachLoopShape( new Vertices( vertices ), body );
		}


		public Fixture attachCircle( float radius, float density, Vector2 offset = default( Vector2 ) )
		{
			return Farseer.FixtureFactory.attachCircle( radius, density, body, offset );
		}


		public Fixture attachRectangle( float width, float height, float density, Vector2 offset = default( Vector2 ) )
		{
			return Farseer.FixtureFactory.attachRectangle( width, height, density, offset, body );
		}


		public Fixture attachPolygon( List<Vector2> vertices, float density )
		{
			return Farseer.FixtureFactory.attachPolygon( new Vertices( vertices ), density, body );
		}


		public Fixture attachEllipse( float xRadius, float yRadius, int edges, float density )
		{
			return Farseer.FixtureFactory.attachEllipse( xRadius, yRadius, edges, density, body );
		}


		public List<Fixture> attachCompoundPolygon( List<Vertices> list, float density )
		{
			return Farseer.FixtureFactory.attachCompoundPolygon( list, density, body );
		}


		public Fixture attachLineArc( float radians, int sides, float radius, bool closed )
		{
			return Farseer.FixtureFactory.attachLineArc( radians, sides, radius, closed, body );
		}


		public List<Fixture> attachSolidArc( float density, float radians, int sides, float radius )
		{
			return Farseer.FixtureFactory.attachSolidArc( density, radians, sides, radius, body );
		}


		public List<Fixture> attachGear( float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density )
		{
			var gearPolygon = PolygonTools.CreateGear( ConvertUnits.displayToSim * radius, numberOfTeeth, tipPercentage, ConvertUnits.displayToSim * toothHeight );

			// Gears can in some cases be convex
			if( !gearPolygon.IsConvex() )
			{
				//Decompose the gear:
				var list = Triangulate.ConvexPartition( gearPolygon, TriangulationAlgorithm.Earclip );
				return attachCompoundPolygon( list, density );
			}

			var fixtures = new List<Fixture>();
			fixtures.Add( attachPolygon( gearPolygon, density ) );
			return fixtures;
		}


		public void attachCapsule( float height, float endRadius, float density )
		{
			// Create the middle rectangle
			attachRectangle( endRadius, height / 2, density );

			// create the two circles
			Farseer.FixtureFactory.attachCircle( endRadius, density, body, new Vector2( 0, height / 2 ) );
			Farseer.FixtureFactory.attachCircle( endRadius, density, body, new Vector2( 0, -height / 2 ) );
		}

		#endregion


		#region Joints

		public MotorJoint createMotorJoint( Body bodyB, bool useWorldCoordinates = false )
		{
			return Farseer.JointFactory.createMotorJoint( body.World, body, bodyB, useWorldCoordinates );
		}


		public RevoluteJoint createRevoluteJoint( Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
		{
			return Farseer.JointFactory.createRevoluteJoint( body.World, body, bodyB, anchorA, anchorB, useWorldCoordinates );
		}

		#endregion


		public static implicit operator Body( FSGenericBody self )
		{
			return self.body;
		}

	}
}
