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
		[XmlAttribute( "face" )]
		public string face;

		[XmlAttribute( "size" )]
		public int size;
		
		[XmlAttribute( "bold" )]
		public int bold;
		
		[XmlAttribute( "italic" )]
		public int italic;
		
		[XmlAttribute( "charset" )]
		public string charSet;
		
		[XmlAttribute( "unicode" )]
		public string unicode;
		
		[XmlAttribute( "stretchH" )]
		public int stretchHeight;
		
		[XmlAttribute( "smooth" )]
		public int smooth;

		[XmlAttribute( "aa" )]
		public int superSampling;

		[XmlAttribute( "padding" )]
		public string padding;

		[XmlAttribute( "spacing" )]
		public string spacing;
		
		[XmlAttribute( "outline" )]
		public int outLine;
	}
}
