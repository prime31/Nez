using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontRegion
	{
		public char character;
		public Subtexture textureRegion;
		public int xOffset;
		public int yOffset;
		public int xAdvance;

		public int width
		{
			get { return textureRegion.sourceRect.Width; }
		}

		public int height
		{
			get { return textureRegion.sourceRect.Height; }
		}


		public BitmapFontRegion( Subtexture textureRegion, char character, int xOffset, int yOffset, int xAdvance )
		{
			this.textureRegion = textureRegion;
			this.character = character;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
			this.xAdvance = xAdvance;
		}

	}
}