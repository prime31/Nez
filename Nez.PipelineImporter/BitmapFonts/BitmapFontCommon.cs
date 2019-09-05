using System;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	public class BitmapFontCommon
	{
		[XmlAttribute("lineHeight")] public int LineHeight;

		[XmlAttribute("base")] public int Base_;

		[XmlAttribute("scaleW")] public int ScaleW;

		[XmlAttribute("scaleH")] public int ScaleH;

		[XmlAttribute("pages")] public int Pages;

		[XmlAttribute("packed")] public int Packed;

		// Littera doesnt seem to add these
		[XmlAttribute("alphaChnl")] public int AlphaChannel;

		[XmlAttribute("redChnl")] public int RedChannel;

		[XmlAttribute("greenChnl")] public int GreenChannel;

		[XmlAttribute("blueChnl")] public int BlueChannel;
	}
}