using System;
using System.Runtime.Serialization;

namespace Nez.Tools.Atlases
{
	/// <summary>Insufficient space left in packing area to contain a given object</summary>
	/// <remarks>
	///   An exception being sent to you from deep space. Erm, no, wait, it's an exception
	///   that occurs when a packing algorithm runs out of space and is unable to fit
	///   the object you tried to pack into the remaining packing area.
	/// </remarks>
	[Serializable]
	internal class OutOfSpaceException : Exception 
	{
		public OutOfSpaceException()
		{ }

		/// <summary>Initializes the exception with an error message</summary>
		/// <param name="message">Error message describing the cause of the exception</param>
		public OutOfSpaceException(string message) : base(message) 
		{ 
		}
	}
}