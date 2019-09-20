using Microsoft.Xna.Framework.Input;


namespace Nez.UI
{
	public interface IKeyboardListener
	{
		/// <summary>
		/// called when a key is first pressed
		/// </summary>
		/// <param name="key">Key.</param>
		void KeyDown(Keys key);

		/// <summary>
		/// called the same frame as keyDown and for repeating keys (key held down). This is only called for non-modifier keys.
		/// </summary>
		/// <param name="key">Key.</param>
		void KeyPressed(Keys key, char character);

		/// <summary>
		/// called when a key is released
		/// </summary>
		/// <param name="key">Key.</param>
		void KeyReleased(Keys key);

		/// <summary>
		/// called when keyboard focus is gained
		/// </summary>
		void GainedFocus();

		/// <summary>
		/// called when keyboard focus is lost
		/// </summary>
		void LostFocus();
	}
}