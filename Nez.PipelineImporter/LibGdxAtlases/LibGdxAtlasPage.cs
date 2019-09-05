using System;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasPage
	{
		public string TextureFile;
		public float Width;
		public float Height;
		public bool UseMipMaps;
		public string Format;
		public string MinFilter;
		public string MagFilter;
		public bool UWrap;
		public bool VWrap;


		public LibGdxAtlasPage(string textureFile, float width, float height, bool useMipMaps, string format,
		                       string minFilter, string magFilter, bool uWrap, bool vWrap)
		{
			this.TextureFile = textureFile;
			this.Width = width;
			this.Height = height;
			this.UseMipMaps = useMipMaps;
			this.Format = format;
			this.MinFilter = minFilter;
			this.MagFilter = magFilter;
			this.UWrap = uWrap;
			this.VWrap = vWrap;
		}
	}
}