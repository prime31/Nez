using System.Collections.Generic;


namespace Nez.BitmapFontImporter
{
	public class BitmapFontProcessorResult
	{
		public List<string> textureAssets;
		public BitmapFontFile fontFile;


		public BitmapFontProcessorResult( BitmapFontFile fontFile )
		{
			this.fontFile = fontFile;
			this.textureAssets = new List<string>();
		}
	}
}