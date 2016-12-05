using System;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;


namespace FarseerPhysics.Controllers
{
	[Flags]
	public enum ControllerType
	{
		GravityController = ( 1 << 0 ),
		VelocityLimitController = ( 1 << 1 ),
		AbstractForceController = ( 1 << 2 ),
		BuoyancyController = ( 1 << 3 ),
	}


	public struct ControllerFilter
	{
		public ControllerType controllerFlags;

		/// <summary>
		/// Ignores the controller. The controller has no effect on this body.
		/// </summary>
		/// <param name="controller">The controller type.</param>
		public void ignoreController( ControllerType controller )
		{
			controllerFlags |= controller;
		}

		/// <summary>
		/// Restore the controller. The controller affects this body.
		/// </summary>
		/// <param name="controller">The controller type.</param>
		public void restoreController( ControllerType controller )
		{
			controllerFlags &= ~controller;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="controller">The controller type.</param>
		/// <returns>
		/// 	<c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
		/// </returns>
		public bool isControllerIgnored( ControllerType controller )
		{
			return ( controllerFlags & controller ) == controller;
		}
	}


	public abstract class Controller : FilterData
	{
		public bool enabled;
		public World world;

		ControllerType _type;

		protected Controller( ControllerType controllerType )
		{
			_type = controllerType;
		}

		public override bool isActiveOn( Body body )
		{
			if( body.controllerFilter.isControllerIgnored( _type ) )
				return false;

			return base.isActiveOn( body );
		}

		public abstract void update( float dt );

	}

}