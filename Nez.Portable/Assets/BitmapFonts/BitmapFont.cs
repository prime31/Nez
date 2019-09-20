using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.BitmapFonts
{
	public partial class BitmapFont : IDisposable, IFont
	{
        /// <summary>
        /// When used with MeasureString, specifies that no wrapping should occur.
        /// </summary>
        const int kNoMaxWidth = -1;

		#region Properties

		/// <summary>
		/// alpha channel.
		/// </summary>
		/// <remarks>Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if it's set to zero, and 4 if it's set to one.</remarks>
		public int AlphaChannel;

		/// <summary>
		/// number of pixels from the absolute top of the line to the base of the characters.
		/// </summary>
		public int BaseHeight;

		/// <summary>
		/// blue channel.
		/// </summary>
		/// <remarks>Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if it's set to zero, and 4 if it's set to one.</remarks>
		public int BlueChannel;

		public bool Bold;

		/// <summary>
		/// characters that comprise the font.
		/// </summary>
		public IDictionary<char, Character> Characters;

		/// <summary>
		/// name of the OEM charset used.
		/// </summary>
		public string Charset;

		/// <summary>
		/// name of the true type font.
		/// </summary>
		public string FamilyName;

		/// <summary>
		/// size of the font.
		/// </summary>
		public int FontSize;

		/// <summary>
		/// green channel.
		/// </summary>
		/// <remarks>Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if it's set to zero, and 4 if it's set to one.</remarks>
		public int GreenChannel;

		public bool Italic;

		/// <summary>
		/// character kernings for the font.
		/// </summary>
		public IDictionary<Kerning, int> Kernings;

		/// <summary>
		/// distance in pixels between each line of text.
		/// </summary>
		public int LineHeight;

		public float LineSpacing => LineHeight;

		/// <summary>
		/// outline thickness for the characters.
		/// </summary>
		public int OutlineSize;

		/// <summary>
		/// Gets or sets a value indicating whether the monochrome charaters have been packed into each of the texture channels.
		/// </summary>
		/// <remarks>
		/// When packed, the <see cref="AlphaChannel"/> property describes what is stored in each channel.
		/// </remarks>
		public bool Packed;

		/// <summary>
		/// padding for each character.
		/// </summary>
		public Padding Padding;

		/// <summary>
		/// texture pages for the font.
		/// </summary>
		public Page[] Pages;

		/// <summary>
		/// houses the Textures for each Page, with the same index as the Page.
		/// </summary>
		public Texture2D[] Textures;

		/// <summary>
		/// red channel.
		/// </summary>
		/// <remarks>Set to 0 if the channel holds the glyph data, 1 if it holds the outline, 2 if it holds the glyph and the outline, 3 if it's set to zero, and 4 if it's set to one.</remarks>
		public int RedChannel;

		/// <summary>
		/// Gets or sets a value indicating whether the font is smoothed.
		/// </summary>
		public bool Smoothed;

		/// <summary>
		/// spacing for each character.
		/// </summary>
		public Point Spacing;

		/// <summary>
		/// font height stretch.
		/// </summary>
		/// <remarks>100% means no stretch.</remarks>
		public int StretchedHeight;

		/// <summary>
		/// level of super sampling used by the font.
		/// </summary>
		/// <remarks>A value of 1 indicates no super sampling is in use.</remarks>
		public int SuperSampling;

		/// <summary>
		/// size of the texture images used by the font.
		/// </summary>
		public Point TextureSize;

		public bool Unicode;

		public Character DefaultCharacter;

		internal int _spaceWidth;

		#endregion

		/// <summary>
		/// Index to get items within thsi collection using array index syntax.
		/// </summary>
		public Character this[char character] => Characters[character];

		public void Initialize()
		{
			LoadTextures();
			if (Characters.TryGetValue(' ', out var defaultChar))
			{
				DefaultCharacter = defaultChar;
			}
			else
			{
				Debug.Log($"Font {FamilyName} has no space character!");
				DefaultCharacter = this['a'];
			}

			_spaceWidth = DefaultCharacter.Bounds.Width + DefaultCharacter.XAdvance;
		}

		/// <summary>
		/// Gets the kerning for the specified character combination.
		/// </summary>
		/// <param name="previous">The previous character.</param>
		/// <param name="current">The current character.</param>
		/// <returns>
		/// The spacing between the specified characters.
		/// </returns>
		public int GetKerning(char previous, char current)
		{
			var key = new Kerning(previous, current, 0);
			if (!Kernings.TryGetValue(key, out var result))
				return 0;

			return result;
		}

		public bool ContainsCharacter(char character) => Characters.ContainsKey(character);

		public bool HasCharacter(char character) => ContainsCharacter(character);

		public string WrapText(string text, float maxLineWidth)
		{
			var words = text.Split(' ');
			var sb = new StringBuilder();
			var lineWidth = 0f;

			if (maxLineWidth < _spaceWidth)
				return string.Empty;

			foreach (var word in words)
			{
				var size = MeasureString(word);
				if (lineWidth + size.X < maxLineWidth)
				{
					sb.Append(word + " ");
					lineWidth += size.X + _spaceWidth;
				}
				else
				{
					if (size.X > maxLineWidth)
					{
						if (sb.ToString() == "")
							sb.Append(WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
						else
							sb.Append("\n" + WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
					}
					else
					{
						sb.Append("\n" + word + " ");
						lineWidth = size.X + _spaceWidth;
					}
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// truncates text and returns a new string with ellipsis appended if necessary. This method ignores all
		/// line breaks.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="text">Text.</param>
		/// <param name="ellipsis">Ellipsis.</param>
		/// <param name="maxLineWidth">Max line width.</param>
		public string TruncateText(string text, string ellipsis, float maxLineWidth)
		{
			if (maxLineWidth < _spaceWidth)
				return string.Empty;

			var size = MeasureString(text);

			// do we even need to truncate?
			var ellipsisWidth = MeasureString(ellipsis).X;
			if (size.X > maxLineWidth)
			{
				var sb = new StringBuilder();

				var width = 0.0f;
				Character currentChar = null;
				var offsetX = 0.0f;

				// determine how many chars we can fit in maxLineWidth - ellipsisWidth
				for (var i = 0; i < text.Length; i++)
				{
					var c = text[i];

					// we don't deal with line breaks or tabs
					if (c == '\r' || c == '\n')
						continue;

					if (currentChar != null)
						offsetX += Spacing.X + currentChar.XAdvance;

					if (ContainsCharacter(c))
						currentChar = this[c];
					else
						currentChar = DefaultCharacter;

					var proposedWidth = offsetX + currentChar.XAdvance + Spacing.X;
					if (proposedWidth > width)
						width = proposedWidth;

					if (width < maxLineWidth - ellipsisWidth)
					{
						sb.Append(c);
					}
					else
					{
						// no more room. append our ellipsis and get out of here
						sb.Append(ellipsis);
						break;
					}
				}

				return sb.ToString();
			}

			return text;
		}

		public Vector2 MeasureString(string text)
		{
			var result = MeasureString(text, kNoMaxWidth);
			return new Vector2(result.X, result.Y);
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			var result = MeasureString(text, kNoMaxWidth);
			return new Vector2(result.X, result.Y);
		}

		public Point MeasureString(string text, float maxWidth = kNoMaxWidth)
		{
			var source = new FontCharacterSource(text);
			return MeasureString(ref source, maxWidth);
		}

		public Point MeasureString(StringBuilder text, float maxWidth = kNoMaxWidth)
		{
			var source = new FontCharacterSource(text);
			return MeasureString(ref source, maxWidth);
		}

		/// <summary>
		/// Provides the size, in pixels, of the specified text when drawn with this font, automatically wrapping to keep within the specified with.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="maxWidth">The maximum width.</param>
		/// <returns>
		/// The size, in pixels, of <paramref name="text"/> drawn with this font.
		/// </returns>
		/// <remarks>The MeasureString method uses the <paramref name="maxWidth"/> parameter to automatically wrap when determining text size.</remarks>
		public Point MeasureString(ref FontCharacterSource text, float maxWidth = kNoMaxWidth)
		{
			if (text.Length == 0)
				return Point.Zero;

			var length = text.Length;
			var previousCharacter = ' ';
			var currentLineWidth = 0;
			var currentLineHeight = LineHeight;
			var blockWidth = 0;
			var blockHeight = 0;
			var lineHeights = new List<int>();

			for (var i = 0; i < length; i++)
			{
				var character = text[i];
				if (character == '\n' || character == '\r')
				{
					if (character == '\n' || i + 1 == length || text[i + 1] != '\n')
					{
						lineHeights.Add(currentLineHeight);
						blockWidth = Math.Max(blockWidth, currentLineWidth);
						currentLineWidth = 0;
						currentLineHeight = LineHeight;
					}
				}
				else
				{
					var data = this[character];
					var width = data.XAdvance + GetKerning(previousCharacter, character) + Spacing.X;
					if (maxWidth != kNoMaxWidth && currentLineWidth + width >= maxWidth)
					{
						lineHeights.Add(currentLineHeight);
						blockWidth = Math.Max(blockWidth, currentLineWidth);
						currentLineWidth = 0;
						currentLineHeight = LineHeight;
					}

					currentLineWidth += width;
					currentLineHeight = Math.Max(currentLineHeight, data.Bounds.Height + data.Offset.Y);
					previousCharacter = character;
				}
			}

			// finish off the current line if required
			if (currentLineHeight != 0)
				lineHeights.Add(currentLineHeight);

			for (var i = 0; i < lineHeights.Count; i++)
			{
				// reduce any lines other than the last back to the base
				if (i < lineHeights.Count - 1)
					lineHeights[i] = LineHeight;

				// add height of each line to the overall block height
				blockHeight += lineHeights[i];
			}

			return new Point(Math.Max(currentLineWidth, blockWidth), blockHeight);
		}

		~BitmapFont() => Dispose();

		public void Dispose()
		{
			if (Textures == null)
				return;

			foreach (var tex in Textures)
				tex.Dispose();
			Textures = null;
		}

		public BitmapFontEnumerator GetGlyphs(string text)
		{
			var source = new FontCharacterSource(text);
			return GetGlyphs(ref source);
		}

		public BitmapFontEnumerator GetGlyphs(StringBuilder text)
		{
			var source = new FontCharacterSource(text);
			return GetGlyphs(ref source);
		}

		public BitmapFontEnumerator GetGlyphs(ref FontCharacterSource text) => new BitmapFontEnumerator(this, ref text);
	}
}