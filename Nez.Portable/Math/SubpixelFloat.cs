namespace Nez
{
	/// <summary>
	/// simple helper class that manages a float value. It stores the value until the total accumulated is greater than 1. Once it exceeds
	/// 1 the value will be added on to amount when update is called.
	/// 
	/// General usage would be something like the following.
	///  - calculate your objects velocity however you normally would
	///  - multiply by deltaTime to keep it framerate independent
	///  - pass the calculated delta movement for this frame to the SubpixelFloat.update method for both x and y. This will result in deltaMove
	///    being rounded to an int and the SubpixelFloat will deal with accumulating the excess value.
	/// 
	///    	var deltaMove = velocity * Time.deltaTime;
	///		_x.update( ref deltaMove.X );
	///		_y.update( ref deltaMove.Y );
	/// </summary>
	public struct SubpixelFloat
	{
		public float Remainder;


		/// <summary>
		/// increments remainder by amount, truncates the value to an int, stores off the new remainder and sets amount to the current value.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void Update(ref float amount)
		{
			Remainder += amount;
			var motion = Mathf.TruncateToInt(Remainder);
			Remainder -= motion;
			amount = motion;
		}


		/// <summary>
		/// resets the remainder to 0. Useful when an object collides with an immovable object. In that case you will want to zero out the
		/// subpixel remainder since it is null and void due to the collision.
		/// </summary>
		public void Reset()
		{
			Remainder = 0;
		}
	}
}