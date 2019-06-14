using System.IO;

namespace Nez.BitmapFonts
{
    /// <summary>
    /// Represents a texture page.
    /// </summary>
    public struct Page
    {
        public string filename;
        public int id;

        public Page(int id, string filename)
        {
            this.filename = filename;
            this.id = id;
        }

        public override string ToString() => string.Format("{0} ({1})", id, Path.GetFileName(filename));
    }
}
