using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	internal class DebugDrawItem
	{
		public Vector2 start;
		public Vector2 end;
		public Color color;
		public float duration;


		public DebugDrawItem( Vector2 start, Vector2 end, Color color, float duration )
		{
			this.start = start;
			this.end = end;
			this.color = color;
			this.duration = duration;
		}


		/// <summary>
		/// returns true if we are done with this debug draw item
		/// </summary>
		public bool draw( Graphics graphics )
		{
			graphics.drawLine( start, end, color );
			duration -= Time.deltaTime;

			return duration < 0f;
		}
	}
}

