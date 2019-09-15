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

		public int Width
		{
			get { return Sprite.SourceRect.Width; }
		}

		public int Height
		{
			get { return Sprite.SourceRect.Height; }
		}


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