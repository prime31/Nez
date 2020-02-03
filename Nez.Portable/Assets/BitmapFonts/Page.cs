using System.IO;


namespace Nez.BitmapFonts
{
	/// <summary>
	/// Represents a texture page.
	/// </summary>
	public struct Page
	{
		public string Filename;
		public int Id;

		public Page(int id, string filename)
		{
			Filename = filename;
			Id = id;
		}

		public override string ToString() => string.Format("{0} ({1})", Id, Path.GetFileName(Filename));
	}
}