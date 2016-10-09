

namespace Nez.Verlet
{
	public abstract class Constraint
	{
		/// <summary>
		/// the Composite that owns this Constraint. Required so that Constraints can be broken.
		/// </summary>
		internal Composite composite;

		/// <summary>
		/// if true, the Constraint will check for collisions with standard Nez Colliders. Inner Constraints do not need to have this set to
		/// true.
		/// </summary>
		public bool collidesWithColliders = true;

		/// <summary>
		/// solves the Constraint
		/// </summary>
		public abstract void solve();

		/// <summary>
		/// if collidesWithColliders is true this will be called
		/// </summary>
		public virtual void handleCollisions( int collidesWithLayers )
		{}

		/// <summary>
		/// debug renders the Constraint
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		public virtual void debugRender( Batcher batcher )
		{}

	}
}
