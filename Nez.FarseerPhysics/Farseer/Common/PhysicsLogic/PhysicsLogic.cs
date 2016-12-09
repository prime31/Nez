using System;
using FarseerPhysics.Dynamics;


namespace FarseerPhysics.Common.PhysicsLogic
{
	[Flags]
	public enum PhysicsLogicType
	{
		Explosion = ( 1 << 0 )
	}


	public struct PhysicsLogicFilter
	{
		public PhysicsLogicType controllerIgnores;

		/// <summary>
		/// Ignores the controller. The controller has no effect on this body.
		/// </summary>
		/// <param name="type">The logic type.</param>
		public void IgnorePhysicsLogic( PhysicsLogicType type )
		{
			controllerIgnores |= type;
		}

		/// <summary>
		/// Restore the controller. The controller affects this body.
		/// </summary>
		/// <param name="type">The logic type.</param>
		public void RestorePhysicsLogic( PhysicsLogicType type )
		{
			controllerIgnores &= ~type;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="type">The logic type.</param>
		/// <returns>
		/// 	<c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
		/// </returns>
		public bool IsPhysicsLogicIgnored( PhysicsLogicType type )
		{
			return ( controllerIgnores & type ) == type;
		}
	}


	public abstract class PhysicsLogic : FilterData
	{
		public World world;
		PhysicsLogicType _type;

		public override bool isActiveOn( Body body )
		{
			if( body.physicsLogicFilter.IsPhysicsLogicIgnored( _type ) )
				return false;

			return base.isActiveOn( body );
		}

		protected PhysicsLogic( World world, PhysicsLogicType type )
		{
			_type = type;
			this.world = world;
		}
	}
}