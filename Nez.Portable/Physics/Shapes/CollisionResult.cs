using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public struct CollisionResult
	{
		/// <summary>
		/// the collider that was collided with
		/// </summary>
		public Collider Collider;

		/// <summary>
		/// The normal vector of the surface hit by the shape
		/// </summary>
		public Vector2 Normal;

		/// <summary>
		/// The translation to apply to the first shape to push the shapes appart
		/// </summary>
		public Vector2 MinimumTranslationVector;

		/// <summary>
		/// not used for all collisions types! Check the ShapeCollisions class before relying on this field!
		/// </summary>
		public Vector2 Point;


		/// <summary>
		/// alters the minimumTranslationVector so that it removes the x-component of the translation if there was no movement in
		/// the same direction.
		/// </summary>
		/// <param name="deltaMovement">the original movement that caused the collision</param>
		public void RemoveHorizontalTranslation(Vector2 deltaMovement)
		{
			// http://dev.yuanworks.com/2013/03/19/little-ninja-physics-and-collision-detection/
			// fix is the vector that is only in the y-direction that we want. Projecting it on the normal gives us the
			// responseDistance that we already have (MTV). We know fix.x should be 0 so it simplifies to fix = r / normal.y
			// fix dot normal = responseDistance

			// check if the lateral motion is undesirable and if so remove it and fix the response
			if (Math.Sign(Normal.X) != Math.Sign(deltaMovement.X) || (deltaMovement.X == 0f && Normal.X != 0f))
			{
				var responseDistance = MinimumTranslationVector.Length();
				var fix = responseDistance / Normal.Y;

				// check some edge cases. make sure we dont have normal.x == 1 and a super small y which will result in a huge
				// fix value since we divide by normal
				if (Math.Abs(Normal.X) != 1f && Math.Abs(fix) < Math.Abs(deltaMovement.Y * 3f))
				{
					MinimumTranslationVector = new Vector2(0f, -fix);
				}
			}
		}


		/// <summary>
		/// inverts the normal and MTV
		/// </summary>
		public void InvertResult()
		{
			Vector2.Negate(ref MinimumTranslationVector, out MinimumTranslationVector);
			Vector2.Negate(ref Normal, out Normal);
		}


		public override string ToString()
		{
			return string.Format("[CollisionResult] normal: {0}, minimumTranslationVector: {1}", Normal,
				MinimumTranslationVector);
		}
	}
}