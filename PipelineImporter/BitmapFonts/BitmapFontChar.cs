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
		[XmlAttribute( "id" )]
		public int id;
		
		[XmlAttribute( "x" )]
		public int x;
		
		[XmlAttribute( "y" )]
		public int y;
		
		[XmlAttribute( "width" )]
		public int width;
		
		[XmlAttribute( "height" )]
		public int height;
		
		[XmlAttribute( "xoffset" )]
		public int xOffset;
		
		[XmlAttribute( "yoffset" )]
		public int yOffset;
		
		[XmlAttribute( "xadvance" )]
		public int xAdvance;
		
		[XmlAttribute( "page" )]
		public int page;
		
		[XmlAttribute( "chnl" )]
		public int channel;
	}
}
