using System;


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
		public int Amount;

		public readonly char FirstCharacter;
		public readonly char SecondCharacter;

		public Kerning(char firstCharacter, char secondCharacter, int amount)
		{
			FirstCharacter = firstCharacter;
			SecondCharacter = secondCharacter;
			Amount = amount;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj.GetType() != typeof(Kerning))
				return false;

			return Equals((Kerning) obj);
		}

		public bool Equals(Kerning other) => FirstCharacter == other.FirstCharacter && SecondCharacter == other.SecondCharacter;

		public override int GetHashCode() => (FirstCharacter << 16) | SecondCharacter;

		public override string ToString() => string.Format("{0} to {1} = {2}", FirstCharacter, SecondCharacter, Amount);
	}
}