using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.BitmapFontImporter
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------
	[XmlRoot( "font" )]
	public class BitmapFontFile
	{
		/// <summary>
		/// the full path to the fnt font
		/// </summary>
		public string file;

		[XmlElement( "info" )]
		public BitmapFontInfo info;
		
		[XmlElement( "common" )]
		public BitmapFontCommon common;

		[XmlArray( "pages" )]
		[XmlArrayItem( "page" )]
		public List<BitmapFontPage> pages;

		[XmlArray( "chars" )]
		[XmlArrayItem( "char" )]
		public List<BitmapFontChar> chars;
		
		[XmlArray( "kernings" )]
		[XmlArrayItem( "kerning" )]
		public List<BitmapFontKerning> kernings;
	}
}
