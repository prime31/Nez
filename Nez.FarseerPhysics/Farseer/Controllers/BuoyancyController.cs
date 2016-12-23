using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Controllers
{
	public sealed class BuoyancyController : Controller
	{
		#region Properties/Fields

		/// <summary>
		/// Controls the rotational drag that the fluid exerts on the bodies within it. Use higher values will simulate thick fluid, like honey, lower values to
		/// simulate water-like fluids. 
		/// </summary>
		public float angularDragCoefficient;

		/// <summary>
		/// Density of the fluid. Higher values will make things more buoyant, lower values will cause things to sink.
		/// </summary>
		public float density;

		/// <summary>
		/// Controls the linear drag that the fluid exerts on the bodies within it. Use higher values will simulate thick fluid, like honey, lower values to
		/// simulate water-like fluids.
		/// </summary>
		public float linearDragCoefficient;

		/// <summary>
		/// Acts like waterflow. Defaults to 0,0.
		/// </summary>
		public Vector2 velocity;

		AABB _container;

		Vector2 _gravity;
		Vector2 _normal;
		float _offset;
		Dictionary<int, Body> _uniqueBodies = new Dictionary<int, Body>();

		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="BuoyancyController"/> class.
		/// </summary>
		/// <param name="container">Only bodies inside this AABB will be influenced by the controller</param>
		/// <param name="density">Density of the fluid</param>
		/// <param name="linearDragCoefficient">Linear drag coefficient of the fluid</param>
		/// <param name="rotationalDragCoefficient">Rotational drag coefficient of the fluid</param>
		/// <param name="gravity">The direction gravity acts. Buoyancy force will act in opposite direction of gravity.</param>
		public BuoyancyController( AABB container, float density, float linearDragCoefficient, float rotationalDragCoefficient, Vector2 gravity )
			: base( ControllerType.BuoyancyController )
		{
			this.container = container;
			_normal = new Vector2( 0, 1 );
			this.density = density;
			this.linearDragCoefficient = linearDragCoefficient;
			angularDragCoefficient = rotationalDragCoefficient;
			_gravity = gravity;
		}

		public AABB container
		{
			get { return _container; }
			set
			{
				_container = value;
				_offset = _container.upperBound.Y;
			}
		}

		public override void update( float dt )
		{
			_uniqueBodies.Clear();
			world.queryAABB( fixture =>
								 {
									 if( fixture.body.isStatic || !fixture.body.isAwake )
										 return true;

									 if( !_uniqueBodies.ContainsKey( fixture.body.bodyId ) )
										 _uniqueBodies.Add( fixture.body.bodyId, fixture.body );

									 return true;
								 }, ref _container );

			foreach( KeyValuePair<int, Body> kv in _uniqueBodies )
			{
				Body body = kv.Value;

				Vector2 areac = Vector2.Zero;
				Vector2 massc = Vector2.Zero;
				float area = 0;
				float mass = 0;

				for( int j = 0; j < body.fixtureList.Count; j++ )
				{
					Fixture fixture = body.fixtureList[j];

					if( fixture.shape.shapeType != ShapeType.Polygon && fixture.shape.shapeType != ShapeType.Circle )
						continue;

					Shape shape = fixture.shape;

					Vector2 sc;
					float sarea = shape.computeSubmergedArea( ref _normal, _offset, ref body._xf, out sc );
					area += sarea;
					areac.X += sarea * sc.X;
					areac.Y += sarea * sc.Y;

					mass += sarea * shape.density;
					massc.X += sarea * sc.X * shape.density;
					massc.Y += sarea * sc.Y * shape.density;
				}

				areac.X /= area;
				areac.Y /= area;
				massc.X /= mass;
				massc.Y /= mass;

				if( area < Settings.epsilon )
					continue;

				//Buoyancy
				var buoyancyForce = -density * area * _gravity;
				body.applyForce( buoyancyForce, massc );

				//Linear drag
				var dragForce = body.getLinearVelocityFromWorldPoint( areac ) - velocity;
				dragForce *= -linearDragCoefficient * area;
				body.applyForce( dragForce, areac );

				//Angular drag
				body.applyTorque( -body.inertia / body.mass * area * body.angularVelocity * angularDragCoefficient );
			}
		}
	
	}
}