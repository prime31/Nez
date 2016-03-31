using System;


namespace Nez
{
	/// <summary>
	/// when added to a Component, whenever a Collider on the Entity overlaps/exits another Component these methods will be called.
	/// The ITriggerListener method will be called on any Component on the Entity that is a trigger that implement the interface.
	/// Note that this interface works only in conjunction with the Mover class
	/// </summary>
	public interface ITriggerListener
	{
		/// <summary>
		/// called when another collider intersects a trigger collider attached to this Entity. Movement must be handled by the
		/// Mover methods for this to function automatically.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void onTriggerEnter( Collider other );

		/// <summary>
		/// called when another collider leaves a trigger collider attached to this Entity. Movement must be handled by the Entity.move
		/// methods for this to function automatically.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void onTriggerExit( Collider other );
	}
}

