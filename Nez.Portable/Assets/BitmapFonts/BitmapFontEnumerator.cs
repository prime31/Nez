using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.BitmapFonts
{
	public struct BitmapFontGlyph
	{
		public Vector2 Position;
		public Character Character;
		public Texture2D Texture;
	}

    /// <summary>
    /// returned by <seealso cref="BitmapFont.GetGlyphs"/>, providing a way to iterate over a string. Can be drawn with
    /// <seealso cref="BatcherBitmapFontExt"/> or by iterating and drawing each glyph.
    /// </summary>
    public struct BitmapFontEnumerator : IEnumerator<BitmapFontGlyph>
	{
		BitmapFont _font;
		FontCharacterSource _text;

		BitmapFontGlyph _currentGlyph;
		BitmapFontGlyph? _previousGlyph;
		char? _previousChar;
		Vector2 _runningPosition;
		int _index;

		object IEnumerator.Current => throw new InvalidOperationException();

		public BitmapFontGlyph Current => _currentGlyph;

		public BitmapFontEnumerator(BitmapFont font, ref FontCharacterSource text)
		{
			_font = font;
			_text = text;
			_currentGlyph = new BitmapFontGlyph();
			_previousGlyph = null;
			_previousChar = null;
			_runningPosition = Vector2.Zero;
			_index = -1;
		}

		public void Reset()
		{
			_index = -1;
			_previousGlyph = null;
			_runningPosition = Vector2.Zero;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_text.Length == ++_index)
				return false;

			var currentChar = _text[_index];
			if (currentChar == '\r')
				return true;

			if (currentChar == '\n')
			{
				_runningPosition.X = 0;
				_runningPosition.Y += _font.LineHeight;
				_previousGlyph = null;
				_previousChar = null;
				return true;
			}

			_currentGlyph = new BitmapFontGlyph
			{
				Position = _runningPosition,
				Character = _font[currentChar],
				Texture = _font.Textures[_font[currentChar].TexturePage]
			};

			if (_currentGlyph.Character != null)
			{
				_currentGlyph.Position.X += _currentGlyph.Character.Offset.X;
				_currentGlyph.Position.Y += _currentGlyph.Character.Offset.Y;
				_runningPosition.X += _currentGlyph.Character.XAdvance + _font.Spacing.X;
			}

			if (_previousChar.HasValue && _previousGlyph.Value.Character != null)
			{
				var amount = _font.GetKerning(_previousChar.Value, currentChar);
				_runningPosition.X += amount;
				_currentGlyph.Position.X += amount;
			}

			_previousGlyph = _currentGlyph;
			_previousChar = currentChar;

			return true;
		}

		public BitmapFontEnumerator GetEnumerator() => this;
	}
}