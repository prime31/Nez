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
		/// called when a collider intersects a trigger collider. This is called on the trigger collider and the collider that touched
		/// the trigger. Movement must be handled by the Mover/ProjectileMover methods for this to function automatically.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void OnTriggerEnter(Collider other, Collider local);

		/// <summary>
		/// called when another collider leaves a trigger collider.
		/// </summary>
		/// <param name="remote">Remote.</param>
		/// <param name="local">Local.</param>
		void OnTriggerExit(Collider other, Collider local);
	}
}