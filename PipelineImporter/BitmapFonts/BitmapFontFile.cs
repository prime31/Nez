using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	[XmlRoot("font")]
    public class BitmapFontFile
	{
		[XmlElement("info")]
		public BitmapFontInfo Info;
		
		[XmlElement("common")]
		public BitmapFontCommon Common;

		[XmlArray("pages")]
		[XmlArrayItem("page")]
		public List<BitmapFontPage> Pages;

		[XmlArray("chars")]
		[XmlArrayItem("char")]
		public List<BitmapFontChar> Chars;
		
		[XmlArray("kernings")]
		[XmlArrayItem("kerning")]
		public List<BitmapFontKerning> Kernings;
	}	
}
