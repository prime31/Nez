using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontRegion
	{
		public char character;
		public Subtexture subtexture;
		public int xOffset;
		public int yOffset;
		public int xAdvance;

		public int width
		{
			get { return subtexture.sourceRect.Width; }
		}

		public int height
		{
			get { return subtexture.sourceRect.Height; }
		}


		public BitmapFontRegion( Subtexture textureRegion, char character, int xOffset, int yOffset, int xAdvance )
		{
			this.subtexture = textureRegion;
			this.character = character;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
			this.xAdvance = xAdvance;
		}

	}
}