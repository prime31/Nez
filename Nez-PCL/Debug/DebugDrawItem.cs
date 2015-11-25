using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	internal class DebugDrawItem
	{
		enum DebugDrawType
		{
			Line,
			HollowRectangle
		}
		
		public Vector2 start;
		public Vector2 end;
		public Rectangle rectangle;
		public Color color;
		public float duration;

		DebugDrawType _drawType;


		public DebugDrawItem( Vector2 start, Vector2 end, Color color, float duration )
		{
			this.start = start;
			this.end = end;
			this.color = color;
			this.duration = duration;
			_drawType = DebugDrawType.Line;
		}


		public DebugDrawItem( Rectangle rectangle, Color color, float duration )
		{
			this.rectangle = rectangle;
			this.color = color;
			this.duration = duration;
			_drawType = DebugDrawType.HollowRectangle;
		}


		/// <summary>
		/// returns true if we are done with this debug draw item
		/// </summary>
		public bool draw( Graphics graphics )
		{
			switch( _drawType )
			{
				case DebugDrawType.Line:
					graphics.drawLine( start, end, color );
					break;
				case DebugDrawType.HollowRectangle:
					graphics.drawHollowRect( rectangle, color );
					break;
			}

			duration -= Time.deltaTime;

			return duration < 0f;
		}
	}
}

