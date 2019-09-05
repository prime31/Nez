using Microsoft.Xna.Framework;


namespace Nez
{
	public struct SubpixelVector2
	{
		SubpixelFloat _x;
		SubpixelFloat _y;

		/// <summary>
		/// increments s/y remainders by amount, truncates the values to an int, stores off the new remainders and sets amount to the current value.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void Update(ref Vector2 amount)
		{
			_x.Update(ref amount.X);
			_y.Update(ref amount.Y);
		}


		/// <summary>
		/// resets the remainder to 0. Useful when an object collides with an immovable object. In that case you will want to zero out the
		/// subpixel remainder since it is null and void due to the collision.
		/// </summary>
		public void Reset()
		{
			_x.Reset();
			_y.Reset();
		}
	}
}