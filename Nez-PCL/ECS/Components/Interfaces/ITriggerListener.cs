using System;


namespace Nez
{
	/// <summary>
	/// when added to a Component, whenever a Collider on the Entity overlaps/exits another Component these methods will be called.
	/// </summary>
	public interface ITriggerListener
	{
		/// <summary>
		/// called when remote intersects local. One or both of the Colliders must be triggers and movement must be handled by the
		/// Entity.move methods for this to function automatically.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void onTriggerEnter( Collider remote, Collider local );

		/// <summary>
		/// called when the intersection between local and remote breaks. One or both of the Colliders must be triggers and movement
		/// must be handled by the Entity.move methods for this to function automatically.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void onTriggerExiting( Collider remote, Collider local );
	}
}

