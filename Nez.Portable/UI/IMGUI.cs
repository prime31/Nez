using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez
{
	/// <summary>
	/// IMGUI is a very simple class with only static methods designed to make sticking buttons, checkboxes, sliders and progress bars on screen
	/// in quick and dirty fashion. It is not designed to be a full and proper UI system.
	/// </summary>
	[System.Obsolete("This class is deprecated in favor of Nez.ImGui and will be removed in the future")]
	public class IMGUI
	{
		enum TextAlign
		{
			Left,
			Center,
			Right
		}

		static Batcher _batcher;
		static BitmapFont _font;

		// constants
		const float FONT_LINE_HEIGHT = 10;
		const float ELEMENT_HEIGHT = 20;
		const float SHORT_ELEMENT_HEIGHT = 15;
		const float ELEMENT_PADDING = 10;
		static Vector2 FONT_SCALE;

		// colors
		static Color FONT_COLOR = new Color(255, 255, 255);
		static Color WINDOW_COLOR = new Color(17, 17, 17);
		static Color BUTTON_COLOR = new Color(78, 91, 98);
		static Color BUTTON_COLOR_ACTIVE = new Color(168, 207, 115);
		static Color BUTTON_COLOR_DOWN = new Color(244, 23, 135);
		static Color TOGGLE_BG = new Color(63, 63, 63);
		static Color TOGGLE_BG_ACTIVE = new Color(130, 130, 130);
		static Color TOGGLE_ON = new Color(168, 207, 115);
		static Color TOGGLE_ON_ACTIVE = new Color(244, 23, 135);
		static Color SLIDER_BG = new Color(78, 91, 98);
		static Color SLIDER_THUMB_BG = new Color(25, 144, 188);
		static Color SLIDER_THUMB_BG_ACTIVE = new Color(168, 207, 115);
		static Color SLIDER_THUMB_BG_DOWN = new Color(244, 23, 135);
		static Color HEADER_BG = new Color(40, 46, 50);

		// state
		static float _lastY;
		static float _elementX;
		static float _windowWidth;
		#pragma warning disable 0414
		static float _windowHeight;
		static float _elementWidth;
		static Point _mouseInWorldCoords;


		static IMGUI()
		{
			_batcher = new Batcher(Core.GraphicsDevice);
			_font = Graphics.Instance.BitmapFont;

			var scale = FONT_LINE_HEIGHT / _font.LineHeight;
			FONT_SCALE = new Vector2(scale, scale);
		}


		#region Helpers

		static void DrawString(string text, Color color, TextAlign align = TextAlign.Center,
		                       float elementHeight = ELEMENT_HEIGHT)
		{
			// center align the text
			var textSize = _font.MeasureString(text) * FONT_SCALE.Y;
			float x = _elementX;
			switch (align)
			{
				case TextAlign.Center:
					x += (_elementWidth - textSize.X) * 0.5f;
					break;
				case TextAlign.Right:
					x = _elementX + _elementWidth - textSize.X;
					break;
			}

			var y = _lastY + ELEMENT_PADDING + (elementHeight - FONT_LINE_HEIGHT) * 0.5f;

			BitmapFonts.BatcherBitmapFontExt.DrawString(_batcher, _font, text, new Vector2(x, y), color, 0,
				Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0);
		}


		static bool IsMouseOverElement()
		{
			var rect = new Rectangle((int) _elementX, (int) _lastY + (int) ELEMENT_PADDING, (int) _elementWidth,
				(int) ELEMENT_HEIGHT);
			return rect.Contains(_mouseInWorldCoords);
		}


		static bool IsMouseBetween(float left, float right)
		{
			var rect = new Rectangle((int) left, (int) _lastY + (int) ELEMENT_PADDING, (int) right - (int) left,
				(int) ELEMENT_HEIGHT);
			return rect.Contains(_mouseInWorldCoords);
		}


		static void EndElement(float elementHeight = ELEMENT_HEIGHT)
		{
			_lastY += elementHeight + ELEMENT_PADDING;
		}

		#endregion


		/// <summary>
		/// begins an IMGUI window specifying where and how large it should be. If you are not using IMGUI in world space (for example, inside
		/// a Scene with a scaled resolution policy) passing false for useRawMousePosition will use the Input.scaledMousePosition.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="useRawMousePosition">If set to <c>true</c> use raw mouse position.</param>
		public static void BeginWindow(float x, float y, float width, float height, bool useRawMousePosition = true)
		{
			_batcher.Begin();

			_batcher.DrawRect(x, y, width, height, WINDOW_COLOR);

			_elementX = x + ELEMENT_PADDING;
			_lastY = y;
			_windowWidth = width;
			_windowHeight = height;
			_elementWidth = _windowWidth - 2f * ELEMENT_PADDING;

			var mousePos = useRawMousePosition ? Input.RawMousePosition : Input.ScaledMousePosition.ToPoint();
			_mouseInWorldCoords = mousePos - new Point(Core.GraphicsDevice.Viewport.X, Core.GraphicsDevice.Viewport.Y);
		}


		public static void EndWindow()
		{
			_batcher.End();
		}


		public static bool Button(string text)
		{
			var ret = false;

			var color = BUTTON_COLOR;
			if (IsMouseOverElement())
			{
				ret = Input.LeftMouseButtonReleased;
				color = Input.LeftMouseButtonDown ? BUTTON_COLOR_DOWN : BUTTON_COLOR_ACTIVE;
			}

			_batcher.DrawRect(_elementX, _lastY + ELEMENT_PADDING, _elementWidth, ELEMENT_HEIGHT, color);
			DrawString(text, FONT_COLOR);
			EndElement();

			return ret;
		}


		/// <summary>
		/// creates a checkbox/toggle
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="isChecked">If set to <c>true</c> is checked.</param>
		public static bool Toggle(string text, bool isChecked)
		{
			var toggleX = _elementX + _elementWidth - ELEMENT_HEIGHT;
			var color = TOGGLE_BG;
			var toggleCheckColor = TOGGLE_ON;
			var isToggleActive = false;

			if (IsMouseBetween(toggleX, toggleX + ELEMENT_HEIGHT))
			{
				color = TOGGLE_BG_ACTIVE;
				if (Input.LeftMouseButtonDown)
				{
					isToggleActive = true;
					toggleCheckColor = TOGGLE_ON_ACTIVE;
				}

				if (Input.LeftMouseButtonReleased)
					isChecked = !isChecked;
			}

			DrawString(text, FONT_COLOR, TextAlign.Left);
			_batcher.DrawRect(toggleX, _lastY + ELEMENT_PADDING, ELEMENT_HEIGHT, ELEMENT_HEIGHT, color);

			if (isChecked || isToggleActive)
				_batcher.DrawRect(toggleX + 3, _lastY + ELEMENT_PADDING + 3, ELEMENT_HEIGHT - 6, ELEMENT_HEIGHT - 6,
					toggleCheckColor);

			EndElement();

			return isChecked;
		}


		/// <summary>
		/// value should be between 0 and 1
		/// </summary>
		/// <param name="value">Value.</param>
		public static float Slider(float value, string name = "")
		{
			var workingWidth = _elementWidth - SHORT_ELEMENT_HEIGHT;
			var thumbPos = workingWidth * value;
			var color = SLIDER_THUMB_BG;

			if (IsMouseOverElement())
			{
				if (Input.LeftMouseButtonDown)
				{
					var localMouseX = _mouseInWorldCoords.X - _elementX - SHORT_ELEMENT_HEIGHT * 0.5f;
					value = MathHelper.Clamp(localMouseX / workingWidth, 0, 1);
					thumbPos = workingWidth * value;
					color = SLIDER_THUMB_BG_DOWN;
				}
				else
				{
					color = SLIDER_THUMB_BG_ACTIVE;
				}
			}

			_batcher.DrawRect(_elementX, _lastY + ELEMENT_PADDING, _elementWidth, SHORT_ELEMENT_HEIGHT, SLIDER_BG);
			_batcher.DrawRect(_elementX + thumbPos, _lastY + ELEMENT_PADDING, SHORT_ELEMENT_HEIGHT,
				SHORT_ELEMENT_HEIGHT, color);
			DrawString(name + value.ToString("F"), FONT_COLOR, TextAlign.Center, SHORT_ELEMENT_HEIGHT);
			EndElement();

			return value;
		}


		/// <summary>
		/// value should be between 0 and 1
		/// </summary>
		/// <returns>The bar.</returns>
		/// <param name="value">Value.</param>
		public static float ProgressBar(float value)
		{
			var thumbPos = _elementWidth * value;
			var color = SLIDER_THUMB_BG;

			if (IsMouseOverElement())
			{
				if (Input.LeftMouseButtonDown)
				{
					var localMouseX = _mouseInWorldCoords.X - _elementX;
					value = MathHelper.Clamp(localMouseX / _elementWidth, 0, 1);
					thumbPos = _elementWidth * value;
					color = SLIDER_THUMB_BG_DOWN;
				}
				else
				{
					color = SLIDER_THUMB_BG_ACTIVE;
				}
			}

			_batcher.DrawRect(_elementX, _lastY + ELEMENT_PADDING, _elementWidth, ELEMENT_HEIGHT, SLIDER_BG);
			_batcher.DrawRect(_elementX, _lastY + ELEMENT_PADDING, thumbPos, ELEMENT_HEIGHT, color);
			DrawString(value.ToString("F"), FONT_COLOR);
			EndElement();

			return value;
		}


		/// <summary>
		/// creates a full width header with text
		/// </summary>
		/// <param name="text">Text.</param>
		public static void Header(string text)
		{
			// expand the header to full width and use a shorter element height
			_batcher.DrawRect(_elementX - ELEMENT_PADDING, _lastY + ELEMENT_PADDING,
				_elementWidth + ELEMENT_PADDING * 2, SHORT_ELEMENT_HEIGHT, HEADER_BG);
			DrawString(text, FONT_COLOR, TextAlign.Center, SHORT_ELEMENT_HEIGHT);
			EndElement(SHORT_ELEMENT_HEIGHT);
		}


		/// <summary>
		/// adds some vertical space
		/// </summary>
		/// <param name="verticalSpace">Vertical space.</param>
		public static void Space(float verticalSpace)
		{
			_lastY += verticalSpace;
		}
	}
}