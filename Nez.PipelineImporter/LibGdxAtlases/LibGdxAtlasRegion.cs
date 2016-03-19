using System;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasRegion
	{
		public string name = "";
		public string page = "";
		public LibGdxAtlasRect sourceRectangle = new LibGdxAtlasRect();
		public LibGdxAtlasPoint originalSize = new LibGdxAtlasPoint();
		public LibGdxAtlasPoint offset = new LibGdxAtlasPoint();

		/// <summary>
		/// nine patch details in this order: left, right, top, bottom
		/// </summary>
		public int[] splits;
		public int[] pads;
		public bool rotate = false;
		public int index;
		public bool flip = false;


		public LibGdxAtlasRegion()
		{}


		public override string ToString()
		{
			return string.Format( "{0}", name );
		}

	}
}

