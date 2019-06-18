using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nez.BitmapFonts
{
    /// <summary>
    /// Represents the definition of a single character in a <see cref="BitmapFont"/>
    /// </summary>
    public class Character
    {
        /// <summary>
        /// bounds of the character image in the source texture.
        /// </summary>
        public Rectangle bounds;

        /// <summary>
        /// texture channel where the character image is found.
        /// </summary>
        public int channel;

        /// <summary>
        /// character.
        /// </summary>
        public char character;

        /// <summary>
        /// offset when copying the image from the texture to the screen.
        /// </summary>
        public Point offset;

        /// <summary>
        /// texture page where the character image is found.
        /// </summary>
        public int texturePage;

        /// <summary>
        /// value used to advance the current position after drawing the character.
        /// </summary>
        public int xAdvance;

        public override string ToString() => character.ToString();
    }
}
