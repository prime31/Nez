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
		/// <param name="rotation">>A rotation of this string.</param>
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

				if (colorMap.TryGetValue(i, out Color newColor)) {
					currentColor = newColor;
				}
				
				batcher.Draw(Textures[currentChar.TexturePage], destRect, currentChar.Bounds, currentColor, rotation,
					Vector2.Zero, effect, depth);
				previousCharacter = c;
			}
		}
	}
}