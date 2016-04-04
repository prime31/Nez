using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public struct CollisionResult
	{
		/// <summary>
		/// the collider that was collided with
		/// </summary>
		public Collider collider;

		/// <summary>
		/// The normal vector of the surface hit by the shape
		/// </summary>
		public Vector2 normal;

		/// <summary>
		/// The translation to apply to the first shape to push the shapes appart
		/// </summary>
		public Vector2 minimumTranslationVector;

		internal Vector2 point;


		/// <summary>
		/// alters the minimumTranslationVector so that it removes the x-component of the translation if there was no movement in
		/// the same direction.
		/// </summary>
		/// <param name="deltaMovement">the original movement that caused the collision</param>
		public void removeHorizontalTranslation( Vector2 deltaMovement )
		{
			// http://dev.yuanworks.com/2013/03/19/little-ninja-physics-and-collision-detection/
			// fix is the vector that is only in the y-direction that we want. Projecting it on the normal gives us the
			// responseDistance that we already have (MTV). We know fix.x should be 0 so it simplifies to fix = r / normal.y
			// fix dot normal = responseDistance

			// check if the lateral motion is undesirable and if so remove it and fix the response
			if( Math.Sign( normal.X ) != Math.Sign( deltaMovement.X ) || ( deltaMovement.X == 0f && normal.X != 0f ) )
			{
				var responseDistance = minimumTranslationVector.Length();
				var fix = responseDistance / normal.Y;

				// check some edge cases. make sure we dont have normal.x == 1 and a super small y which will result in a huge
				// fix value since we divide by normal
				if( Math.Abs( normal.X ) != 1f && Math.Abs( fix ) < Math.Abs( deltaMovement.Y * 3f ) )
				{
					minimumTranslationVector = new Vector2( 0f, -fix );
				}
			}
		}


		/// <summary>
		/// inverts the normal and MTV
		/// </summary>
		public void invertResult()
		{
			var inverse = new Vector2( -1f );
			minimumTranslationVector *= inverse;
			normal *= inverse;
		}


		public override string ToString()
		{
			return string.Format( "[ShapeCollisionResult] normal: {0}, minimumTranslationVector: {1}", normal, minimumTranslationVector );
		}

	}
}

