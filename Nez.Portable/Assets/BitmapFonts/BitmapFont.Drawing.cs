using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.BitmapFonts
{
	public partial class BitmapFont
	{
		Matrix2D _transformationMatrix = Matrix2D.Identity;

		public void DrawInto(Batcher batcher, string text, Vector2 position, Color color,
		                     float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
			var source = new FontCharacterSource(text);
			DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
		}

		public void DrawInto(Batcher batcher, StringBuilder text, Vector2 position, Color color,
		                     float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
			var source = new FontCharacterSource(text);
			DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
		}

		public void DrawInto(Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
		                     float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
			var flipAdjustment = Vector2.Zero;

			var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

			if (flippedVert || flippedHorz)
			{
				var size = MeasureString(ref text);

				if (flippedHorz)
				{
					origin.X *= -1;
					flipAdjustment.X = -size.X;
				}

				if (flippedVert)
				{
					origin.Y *= -1;
					flipAdjustment.Y = LineHeight - size.Y;
				}
			}


			var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != new Vector2(1);
			if (requiresTransformation)
			{
				Matrix2D temp;
				Matrix2D.CreateTranslation(-origin.X, -origin.Y, out _transformationMatrix);
				Matrix2D.CreateScale((flippedHorz ? -scale.X : scale.X), (flippedVert ? -scale.Y : scale.Y), out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
				Matrix2D.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
				Matrix2D.Multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
				Matrix2D.CreateRotation(rotation, out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
				Matrix2D.CreateTranslation(position.X, position.Y, out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
			}

			var previousCharacter = ' ';
			Character currentChar = null;
			var offset = requiresTransformation ? Vector2.Zero : position - origin;

			for (var i = 0; i < text.Length; ++i)
			{
				var c = text[i];
				if (c == '\r')
					continue;

				if (c == '\n')
				{
					offset.X = requiresTransformation ? 0f : position.X - origin.X;
					offset.Y += LineHeight;
					currentChar = null;
					continue;
				}

				if (currentChar != null)
					offset.X += Spacing.X + currentChar.XAdvance;

				currentChar = ContainsCharacter(c) ? this[c] : DefaultCharacter;

				var p = offset;

				if (flippedHorz)
					p.X += currentChar.Bounds.Width;
				p.X += currentChar.Offset.X + GetKerning(previousCharacter, currentChar.Char);

				if (flippedVert)
					p.Y += currentChar.Bounds.Height - LineHeight;
				p.Y += currentChar.Offset.Y;

				// transform our point if we need to
				if (requiresTransformation)
					Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

				var destRect = RectangleExt.FromFloats
				(
					p.X, p.Y,
					currentChar.Bounds.Width * scale.X,
					currentChar.Bounds.Height * scale.Y
				);

				batcher.Draw(Textures[currentChar.TexturePage], destRect, currentChar.Bounds, color, rotation,
					Vector2.Zero, effect, depth);
				previousCharacter = c;
			}
		}
		
		/// <summary>
		/// Submit a text string of sprites for drawing in the current batch.
		/// This method variant offers the ability to draw multicolored text by providing a colorMap.
		/// The colorMap is a dictionary where the key represents the zero-based index at which the text should
		/// start being displayed in a different color, and the value is the corresponding color.
		///  
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask. Will be used as the initial color.</param>
		/// <param name="rotation">A rotation of this string.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this string.</param>
		/// <param name="effect">Modificators for drawing. Can be combined.</param>
		/// <param name="depth">A depth of the layer of this string.</param>
		/// <param name="colorMap">A dictionary where the key represents the zero-based index at which the text should start being displayed in a different color, and the value is the corresponding color. Must not be null.</param>
		public void DrawInto(Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
		                     float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth, IDictionary<int, Color> colorMap)
		{
			Color currentColor = color;
			
			var flipAdjustment = Vector2.Zero;

			var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

			if (flippedVert || flippedHorz)
			{
				var size = MeasureString(ref text);

				if (flippedHorz)
				{
					origin.X *= -1;
					flipAdjustment.X = -size.X;
				}

				if (flippedVert)
				{
					origin.Y *= -1;
					flipAdjustment.Y = LineHeight - size.Y;
				}
			}
			
			var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != new Vector2(1);
			if (requiresTransformation)
			{
				ApplyTransformation(position, rotation, origin, scale, flippedHorz, flippedVert, flipAdjustment);
			}

			var previousCharacter = ' ';
			Character currentChar = null;
			var offset = requiresTransformation ? Vector2.Zero : position - origin;

			for (var i = 0; i < text.Length; ++i)
			{
				var character = text[i];

				if (colorMap.TryGetValue(i, out Color newColor)) {
					currentColor = newColor;
				}
				
				currentChar = DrawCurrentChar(batcher, currentColor, rotation, scale, effect, depth, currentChar, character, previousCharacter, flippedHorz, flippedVert, requiresTransformation, ref offset, position, origin);
				previousCharacter = character;
			}
		}
		
		/// <summary>
		/// Submits a number for drawing in the current batch.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="number">The number which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this number.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this number.</param>
		/// <param name="effect">Modificators for drawing. Can be combined.</param>
		/// <param name="depth">A depth of the layer of this number.</param>
		/// <param name="useDefaultLineHeight">If true, only the lineHeight attribute of the bitmap font will be used to calculate the height.
		/// If false, the height of the character image and the y offset will be considered when calculating the height.</param>
		public void DrawInto(Batcher batcher, int number, Vector2 position, Color color, 
							 float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth, bool useDefaultLineHeight = true) 
		{
			var flipAdjustment = Vector2.Zero;

			var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

			if (flippedVert || flippedHorz) {
				var size = MeasureInt(number, useDefaultLineHeight);

				if (flippedHorz) 
				{
					origin.X *= -1;
					flipAdjustment.X = -size.X;
				}

				if (flippedVert) 
				{
					origin.Y *= -1;
					flipAdjustment.Y = LineHeight - size.Y;
				}
			}

			var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != new Vector2(1);

			if (requiresTransformation) 
			{
				ApplyTransformation(position, rotation, origin, scale, flippedHorz, flippedVert, flipAdjustment);
			}

			var previousCharacter = ' ';
			Character currentChar = null;
			var offset = requiresTransformation ? Vector2.Zero : position - origin;

			char character;
			
			// We use uint because of special case int.minValue
			uint positiveNumber;

			if (number < 0) {
				character = '-';
				currentChar = DrawCurrentChar(batcher, color, rotation, scale, effect, depth, currentChar, character, previousCharacter, flippedHorz, flippedVert, requiresTransformation, ref offset, position, origin);
				previousCharacter = character;

				positiveNumber = (uint)-(long)number;
			}
			else {
				positiveNumber = (uint)number;
			}

			uint tempNumber = positiveNumber;
			uint divisor = 1;

			while (tempNumber >= 10) {
				divisor *= 10;
				tempNumber /= 10;
			}

			while (divisor > 0) {
				var digit = positiveNumber / divisor;
				positiveNumber %= divisor;
				divisor /= 10;

				character = (char)('0' + digit);
				currentChar = DrawCurrentChar(batcher, color, rotation, scale, effect, depth, currentChar, character, previousCharacter, flippedHorz, flippedVert, requiresTransformation, ref offset, position, origin);
				previousCharacter = character;
			}
		}

		private void ApplyTransformation(Vector2 position, float rotation, Vector2 origin, Vector2 scale, bool flippedHorz,
			bool flippedVert, Vector2 flipAdjustment) 
		{
			Matrix2D temp;
			Matrix2D.CreateTranslation(-origin.X, -origin.Y, out _transformationMatrix);
			Matrix2D.CreateScale(flippedHorz ? -scale.X : scale.X, flippedVert ? -scale.Y : scale.Y, out temp);
			Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
			Matrix2D.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
			Matrix2D.Multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
			Matrix2D.CreateRotation(rotation, out temp);
			Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
			Matrix2D.CreateTranslation(position.X, position.Y, out temp);
			Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
		}

		private Character DrawCurrentChar(Batcher batcher, Color color, float rotation, Vector2 scale, SpriteEffects effect,
			float depth, Character currentChar, char character, char previousCharacter, bool flippedHorz, bool flippedVert,
			bool requiresTransformation, ref Vector2 offset, Vector2 position, Vector2 origin) 
		{
			if (character == '\r')
				return currentChar;

			if (character == '\n')
			{
				offset.X = requiresTransformation ? 0f : position.X - origin.X;
				offset.Y += LineHeight;
				return null;
			}
			
			if (currentChar != null)
				offset.X += Spacing.X + currentChar.XAdvance;

			currentChar = ContainsCharacter(character) ? this[character] : DefaultCharacter;

			var p = offset;

			if (flippedHorz)
				p.X += currentChar.Bounds.Width;
			p.X += currentChar.Offset.X + GetKerning(previousCharacter, currentChar.Char);

			if (flippedVert)
				p.Y += currentChar.Bounds.Height - LineHeight;
			p.Y += currentChar.Offset.Y;

			// transform our point if we need to
			if (requiresTransformation)
				Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

			var destRect = RectangleExt.FromFloats
			(
				p.X, p.Y,
				currentChar.Bounds.Width * scale.X,
				currentChar.Bounds.Height * scale.Y
			);

			batcher.Draw(Textures[currentChar.TexturePage], destRect, currentChar.Bounds, color, rotation,
				Vector2.Zero, effect, depth);
			
			return currentChar;
		}

	}
}