using System;
using System.Collections.Generic;
using System.Text;

namespace Nez.BitmapFonts
{
    /// <summary>
    /// Represents the font kerning between two characters.
    /// </summary>
    public struct Kerning : IEquatable<Kerning>
    {
        /// <summary>
        /// Gets or sets how much the x position should be adjusted when drawing the second character immediately following the first.
        /// </summary>
        public int amount;
        public char firstCharacter;
        public char secondCharacter;

        public Kerning(char firstCharacter, char secondCharacter, int amount)
        {
            this.firstCharacter = firstCharacter;
            this.secondCharacter = secondCharacter;
            this.amount = amount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(Kerning))
                return false;

            return Equals((Kerning)obj);
        }

        public bool Equals(Kerning other) => firstCharacter == other.firstCharacter && secondCharacter == other.secondCharacter;

        public override int GetHashCode() => (firstCharacter << 16) | secondCharacter;

        public override string ToString() => string.Format("{0} to {1} = {2}", firstCharacter, secondCharacter, amount);
    }
}
