using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public interface IInputListener
	{
		void OnMouseEnter();
		void OnMouseExit();

		/// <summary>
		/// if true is returned then onMouseDown/Up will be called else they will not be called
		/// </summary>
		/// <returns><c>true</c>, if mouse pressed was oned, <c>false</c> otherwise.</returns>
		/// <param name="mousePos">Mouse position.</param>
		bool OnLeftMousePressed(Vector2 mousePos);

		/// <summary>
		/// if true is returned then onMouseDown/Up will be called else they will not be called
		/// </summary>
		/// <returns><c>true</c>, if right mouse pressed was oned, <c>false</c> otherwise.</returns>
		/// <param name="mousePos">Mouse position.</param>
		bool OnRightMousePressed(Vector2 mousePos);

		/// <summary>
		/// called when the mouse moves only on an element that returned true for onMousePressed. It is safe to call stage.removeInputFocusListener
		/// here if you are uninterested in the onMouseUp event.
		/// </summary>
		/// <param name="mousePos">Mouse position.</param>
		void OnMouseMoved(Vector2 mousePos);

		/// <summary>
		/// called when the left mouse button is released
		/// </summary>
		/// <param name="mousePos">Mouse position.</param>
		void OnLeftMouseUp(Vector2 mousePos);

		/// <summary>
		/// called when the right mouse button is released
		/// </summary>
		/// <param name="mousePos">Mouse position.</param>
		void OnRightMouseUp(Vector2 mousePos);

		/// <summary>
		/// if true is returned the scroll event will be consumed by the Element
		/// </summary>
		/// <returns>The mouse scrolled.</returns>
		bool OnMouseScrolled(int mouseWheelDelta);
	}
}