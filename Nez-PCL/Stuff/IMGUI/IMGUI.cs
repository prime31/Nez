using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


namespace Nez
{
	/// <summary>
	/// Simple GUI tools for prototyping. init must be called before it will work. In your Draw method, call beginWindow followed
	/// by any other controls then call endWindow to complete the drawing.
	/// </summary>
	public class IMGUI
	{
		enum TextAlign
		{
			Left,
			Center,
			Right
		}

		static Graphics _graphics;

		// constants
		const float FONT_LINE_HEIGHT = 11;
		const float ELEMENT_HEIGHT = 20;
		const float SHORT_ELEMENT_HEIGHT = 15;
		const float ELEMENT_PADDING = 10;
		static Vector2 FONT_SCALE;

		// colors
		static Color FONT_COLOR = new Color( 255, 255, 255 );
		static Color WINDOW_COLOR = new Color( 17, 17, 17 );
		static Color BUTTON_COLOR = new Color( 78, 91, 98 );
		static Color BUTTON_COLOR_ACTIVE = new Color( 168, 207, 115 );
		static Color BUTTON_COLOR_DOWN = new Color( 244, 23, 135 );
		static Color TOGGLE_BG = new Color( 63, 63, 63 );
		static Color TOGGLE_BG_ACTIVE = new Color( 130, 130, 130 );
		static Color TOGGLE_ON = new Color( 168, 207, 115 );
		static Color TOGGLE_ON_ACTIVE = new Color( 244, 23, 135 );
		static Color SLIDER_BG = new Color( 78, 91, 98 );
		static Color SLIDER_THUMB_BG = new Color( 25, 144, 188 );
		static Color SLIDER_THUMB_BG_ACTIVE = new Color( 168, 207, 115 );
		static Color SLIDER_THUMB_BG_DOWN = new Color( 244, 23, 135 );
		static Color HEADER_BG = new Color( 40, 46, 50 );

		// state
		static float _lastY;
		static float _elementX;
		static float _windowWidth;
		#pragma warning disable 0414
		static float _windowHeight;
		static float _elementWidth;
		static Point _mouseInWorldCoords;


		public static void init( GraphicsDevice device, BitmapFont font )
		{
			_graphics = new Graphics( device, font );

			var scale = FONT_LINE_HEIGHT / _graphics.bitmapFont.lineHeight;
			FONT_SCALE = new Vector2( scale, scale );
		}


		#region Helpers

		static void drawString( string text, Color color, TextAlign align = TextAlign.Center, float elementHeight = ELEMENT_HEIGHT )
		{
			// center align the text
			var textSize = _graphics.bitmapFont.measureString( text ) * FONT_SCALE.Y;
			float x = _elementX;
			switch( align )
			{
				case TextAlign.Center:
					x += ( _elementWidth - textSize.X ) * 0.5f;
					break;
				case TextAlign.Right:
					x = _elementX + _elementWidth - textSize.X;
					break;
			}

			var y = _lastY + ELEMENT_PADDING + ( elementHeight - FONT_LINE_HEIGHT ) * 0.7f;

			_graphics.spriteBatch.DrawString( _graphics.bitmapFont, text, new Vector2( x, y ), color, 0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0 );
		}


		static bool isMouseOverElement()
		{
			var rect = new Rectangle( (int)_elementX, (int)_lastY + (int)ELEMENT_PADDING, (int)_elementWidth, (int)ELEMENT_HEIGHT );
			return rect.Contains( _mouseInWorldCoords );
		}


		static bool isMouseBetween( float left, float right )
		{
			var rect = new Rectangle( (int)left, (int)_lastY + (int)ELEMENT_PADDING, (int)right - (int)left, (int)ELEMENT_HEIGHT );
			return rect.Contains( _mouseInWorldCoords );
		}


		static void endElement( float elementHeight = ELEMENT_HEIGHT )
		{
			_lastY += elementHeight + ELEMENT_PADDING;
		}

		#endregion


		public static void beginWindow( float x, float y, float width, float height )
		{
			_graphics.spriteBatch.Begin();

			_graphics.drawRect( x, y, width, height, WINDOW_COLOR );

			_elementX = x + ELEMENT_PADDING;
			_lastY = y;
			_windowWidth = width;
			_windowHeight = height;
			_elementWidth = _windowWidth - 2f * ELEMENT_PADDING;
			_mouseInWorldCoords = Input.mousePosition - new Point( _graphics.graphicsDevice.Viewport.X, _graphics.graphicsDevice.Viewport.Y );
		}


		public static void endWindow()
		{
			_graphics.spriteBatch.End();
		}


		public static bool button( string text )
		{
			var ret = false;

			var color = BUTTON_COLOR;
			if( isMouseOverElement() )
			{
				ret = Input.leftMouseButtonReleased;
				color = Input.leftMouseButtonDown ? BUTTON_COLOR_DOWN : BUTTON_COLOR_ACTIVE;
			}

			_graphics.drawRect( _elementX, _lastY + ELEMENT_PADDING, _elementWidth, ELEMENT_HEIGHT, color );
			drawString( text, FONT_COLOR );
			endElement();

			return ret;
		}


		public static bool toggle( string text, bool isChecked )
		{
			var toggleX = _elementX + _elementWidth - ELEMENT_HEIGHT;
			var color = TOGGLE_BG;
			var toggleCheckColor = TOGGLE_ON;
			var isToggleActive = false;

			if( isMouseBetween( toggleX, toggleX + ELEMENT_HEIGHT ) )
			{
				color = TOGGLE_BG_ACTIVE;
				if( Input.leftMouseButtonDown )
				{
					isToggleActive = true;
					toggleCheckColor = TOGGLE_ON_ACTIVE;
				}

				if( Input.leftMouseButtonReleased )
					isChecked = !isChecked;
			}

			drawString( text, FONT_COLOR, TextAlign.Left );
			_graphics.drawRect( toggleX, _lastY + ELEMENT_PADDING, ELEMENT_HEIGHT, ELEMENT_HEIGHT, color );

			if( isChecked || isToggleActive )
				_graphics.drawRect( toggleX + 3, _lastY + ELEMENT_PADDING + 3, ELEMENT_HEIGHT - 6, ELEMENT_HEIGHT - 6, toggleCheckColor );

			endElement();

			return isChecked;
		}


		public static float slider( float value )
		{
			var workingWidth = _elementWidth - SHORT_ELEMENT_HEIGHT;
			var thumbPos = workingWidth * value;
			var color = SLIDER_THUMB_BG;

			if( isMouseOverElement() )
			{
				if( Input.leftMouseButtonDown )
				{
					var localMouseX = _mouseInWorldCoords.X - _elementX - SHORT_ELEMENT_HEIGHT * 0.5f;
					value = MathHelper.Clamp( localMouseX / workingWidth, 0, 1 );
					thumbPos = workingWidth * value;
					color = SLIDER_THUMB_BG_DOWN;
				}
				else
				{
					color = SLIDER_THUMB_BG_ACTIVE;
				}
			}
				
			_graphics.drawRect( _elementX, _lastY + ELEMENT_PADDING, _elementWidth, SHORT_ELEMENT_HEIGHT, SLIDER_BG );
			_graphics.drawRect( _elementX + thumbPos, _lastY + ELEMENT_PADDING, SHORT_ELEMENT_HEIGHT, SHORT_ELEMENT_HEIGHT, color );
			drawString( value.ToString( "F" ), FONT_COLOR, TextAlign.Center, SHORT_ELEMENT_HEIGHT );
			endElement();

			return value;
		}


		public static float bar( float value )
		{
			var thumbPos = _elementWidth * value;
			var color = SLIDER_THUMB_BG;

			if( isMouseOverElement() )
			{
				if( Input.leftMouseButtonDown )
				{
					var localMouseX = _mouseInWorldCoords.X - _elementX;
					value = MathHelper.Clamp( localMouseX / _elementWidth, 0, 1 );
					thumbPos = _elementWidth * value;
					color = SLIDER_THUMB_BG_DOWN;
				}
				else
				{
					color = SLIDER_THUMB_BG_ACTIVE;
				}
			}

			_graphics.drawRect( _elementX, _lastY + ELEMENT_PADDING, _elementWidth, ELEMENT_HEIGHT, SLIDER_BG );
			_graphics.drawRect( _elementX, _lastY + ELEMENT_PADDING, thumbPos, ELEMENT_HEIGHT, color );
			drawString( value.ToString( "F" ), FONT_COLOR );
			endElement();

			return value;
		}


		public static void header( string text )
		{
			// expand the header to full width and use a shorter element height
			_graphics.drawRect( _elementX - ELEMENT_PADDING, _lastY + ELEMENT_PADDING, _elementWidth + ELEMENT_PADDING * 2, SHORT_ELEMENT_HEIGHT, HEADER_BG );
			drawString( text, FONT_COLOR, TextAlign.Center, SHORT_ELEMENT_HEIGHT );
			endElement( SHORT_ELEMENT_HEIGHT );
		}
	
	}
}

