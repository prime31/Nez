using System;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	public class BitmapFontInfo
	{
		[XmlAttribute("face")] public string Face;

		[XmlAttribute("size")] public int Size;

		[XmlAttribute("bold")] public int Bold;

		[XmlAttribute("italic")] public int Italic;

		[XmlAttribute("charset")] public string CharSet;

		[XmlAttribute("unicode")] public string Unicode;

		[XmlAttribute("stretchH")] public int StretchHeight;

		[XmlAttribute("smooth")] public int Smooth;

		[XmlAttribute("aa")] public int SuperSampling;

		[XmlAttribute("padding")] public string Padding;

		[XmlAttribute("spacing")] public string Spacing;

		[XmlAttribute("outline")] public int OutLine;
	}
}