using System;


namespace Nez.UI
{
	public interface IGamepadFocusable
	{
		void onFocused();

		void onUnfocused();

		void onActionButtonPressed();

		void onActionButtonReleased();
	}
}

