using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A drawable knows how to draw itself at a given rectangular size. It provides border sizes and a minimum size so that other code
	/// can determine how to size and position content.
	/// </summary>
	public interface IDrawable
	{
		float LeftWidth { get; set; }
		float RightWidth { get; set; }
		float TopHeight { get; set; }
		float BottomHeight { get; set; }
		float MinWidth { get; set; }
		float MinHeight { get; set; }


		void SetPadding(float top, float bottom, float left, float right);

		void Draw(Batcher batcher, float x, float y, float width, float height, Color color);
	}
}