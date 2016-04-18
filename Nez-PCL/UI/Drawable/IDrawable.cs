using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A drawable knows how to draw itself at a given rectangular size. It provides border sizes and a minimum size so that other code
	/// can determine how to size and position content.
	/// </summary>
	public interface IDrawable
	{
		float leftWidth { get; set; }
		float rightWidth { get; set; }
		float topHeight { get; set; }
		float bottomHeight { get; set; }
		float minWidth { get; set; }
		float minHeight { get; set; }


		void setPadding( float top, float bottom, float left, float right );

		void draw( Graphics graphics, float x, float y, float width, float height, Color color );
	}
}

