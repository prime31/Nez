using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Post Processing step for rendering actions after everthing done.
	/// </summary>
	public abstract class PostProcessor
	{
		/// <summary>
		/// Step is Enabled or not.
		/// </summary>
		public bool enabled { get; protected set; }


		public PostProcessor()
		{
			enabled = true;
		}


		abstract public void process();

	}
}

