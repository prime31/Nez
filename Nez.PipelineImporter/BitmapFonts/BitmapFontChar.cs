using System;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	public class BitmapFontChar
	{
		[XmlAttribute("id")] public int Id;

		[XmlAttribute("x")] public int X;

		[XmlAttribute("y")] public int Y;

		[XmlAttribute("width")] public int Width;

		[XmlAttribute("height")] public int Height;

		[XmlAttribute("xoffset")] public int XOffset;

		[XmlAttribute("yoffset")] public int YOffset;

		[XmlAttribute("xadvance")] public int XAdvance;

		[XmlAttribute("page")] public int Page;

		[XmlAttribute("chnl")] public int Channel;
	}
}