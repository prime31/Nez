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
		[XmlAttribute( "lineHeight" )]
		public int lineHeight;

		[XmlAttribute( "base" )]
		public int base_;

		[XmlAttribute( "scaleW" )]
		public int scaleW;

		[XmlAttribute( "scaleH" )]
		public int scaleH;

		[XmlAttribute( "pages" )]
		public int pages;

		[XmlAttribute( "packed" )]
		public int packed;

		// Littera doesnt seem to add these
		[XmlAttribute( "alphaChnl" )]
		public int alphaChannel;

		[XmlAttribute( "redChnl" )]
		public int redChannel;

		[XmlAttribute( "greenChnl" )]
		public int greenChannel;

		[XmlAttribute( "blueChnl" )]
		public int blueChannel;
	}
}
