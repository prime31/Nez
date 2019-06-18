using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace Nez.BitmapFonts
{
    public partial class BitmapFont
    {
        Matrix2D _transformationMatrix = Matrix2D.identity;

        public void drawInto(Batcher batcher, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new FontCharacterSource(text);
            drawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
        }

        public void drawInto(Batcher batcher, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new FontCharacterSource(text);
            drawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
        }

        public void drawInto(Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
                                float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                var size = measureString(ref text);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = lineHeight - size.Y;
                }
            }


            var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != new Vector2(1);
            if (requiresTransformation)
            {
                Matrix2D temp;
                Matrix2D.createTranslation(-origin.X, -origin.Y, out _transformationMatrix);
                Matrix2D.createScale((flippedHorz ? -scale.X : scale.X), (flippedVert ? -scale.Y : scale.Y), out temp);
                Matrix2D.multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.createTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
                Matrix2D.multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
                Matrix2D.createRotation(rotation, out temp);
                Matrix2D.multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.createTranslation(position.X, position.Y, out temp);
                Matrix2D.multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
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
                    offset.Y += lineHeight;
                    currentChar = null;
                    continue;
                }

                if (currentChar != null)
                    offset.X += spacing.X + currentChar.xAdvance;

                currentChar = ContainsCharacter(c) ? this[c] : defaultCharacter;

                var p = offset;

                if (flippedHorz)
                    p.X += currentChar.bounds.Width;
                p.X += currentChar.offset.X + GetKerning(previousCharacter, currentChar.character);

                if (flippedVert)
                    p.Y += currentChar.bounds.Height - lineHeight;
                p.Y += currentChar.offset.Y;

                // transform our point if we need to
                if (requiresTransformation)
                    Vector2Ext.transform(ref p, ref _transformationMatrix, out p);

                var destRect = RectangleExt.fromFloats
                (
                   p.X, p.Y,
                   currentChar.bounds.Width * scale.X,
                   currentChar.bounds.Height * scale.Y
                   );

                batcher.draw(textures[currentChar.texturePage], destRect, currentChar.bounds, color, rotation, Vector2.Zero, effect, depth);
                previousCharacter = c;
            }
        }
    }
}
