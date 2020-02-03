using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontRegion
	{
		public char Character;
		public Sprite Sprite;
		public int XOffset;
		public int YOffset;
		public int XAdvance;

		public int Width => Sprite.SourceRect.Width;

		public int Height => Sprite.SourceRect.Height;


		public BitmapFontRegion(Sprite textureRegion, char character, int xOffset, int yOffset, int xAdvance)
		{
			Sprite = textureRegion;
			Character = character;
			XOffset = xOffset;
			YOffset = yOffset;
			XAdvance = xAdvance;
		}
	}
}