using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontRegion
	{
		public char Character;
		public Subtexture Subtexture;
		public int XOffset;
		public int YOffset;
		public int XAdvance;

		public int Width
		{
			get { return Subtexture.SourceRect.Width; }
		}

		public int Height
		{
			get { return Subtexture.SourceRect.Height; }
		}


		public BitmapFontRegion(Subtexture textureRegion, char character, int xOffset, int yOffset, int xAdvance)
		{
			this.Subtexture = textureRegion;
			this.Character = character;
			this.XOffset = xOffset;
			this.YOffset = yOffset;
			this.XAdvance = xAdvance;
		}
	}
}