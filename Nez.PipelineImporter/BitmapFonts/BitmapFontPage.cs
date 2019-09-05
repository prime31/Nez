using System;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	public class BitmapFontPage
	{
		[XmlAttribute("id")] public int Id;

		[XmlAttribute("file")] public string File;

		/// <summary>
		/// not part of fnt spec. this is manually added.
		/// </summary>
		[XmlAttribute("x")] public int X;

		/// <summary>
		/// not part of fnt spec. this is manually added.
		/// </summary>
		[XmlAttribute("y")] public int Y;
	}
}