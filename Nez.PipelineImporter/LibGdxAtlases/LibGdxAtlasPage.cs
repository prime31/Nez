using System;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasPage
	{
		public string textureFile;
		public float width;
		public float height;
		public bool useMipMaps;
		public string format;
		public string minFilter;
		public string magFilter;
		public bool uWrap;
		public bool vWrap;


		public LibGdxAtlasPage( string textureFile, float width, float height, bool useMipMaps, string format, string minFilter, string magFilter, bool uWrap, bool vWrap )
		{
			this.textureFile = textureFile;
			this.width = width;
			this.height = height;
			this.useMipMaps = useMipMaps;
			this.format = format;
			this.minFilter = minFilter;
			this.magFilter = magFilter;
			this.uWrap = uWrap;
			this.vWrap = vWrap;
		}
	}
}

