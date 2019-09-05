using System;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlasRegion
	{
		public string Name = "";
		public string Page = "";
		public LibGdxAtlasRect SourceRectangle = new LibGdxAtlasRect();
		public LibGdxAtlasPoint OriginalSize = new LibGdxAtlasPoint();
		public LibGdxAtlasPoint Offset = new LibGdxAtlasPoint();

		/// <summary>
		/// nine patch details in this order: left, right, top, bottom
		/// </summary>
		public int[] Splits;

		public int[] Pads;
		public bool Rotate = false;
		public int Index;
		public bool Flip = false;


		public LibGdxAtlasRegion()
		{
		}


		public override string ToString()
		{
			return string.Format("{0}", Name);
		}
	}
}