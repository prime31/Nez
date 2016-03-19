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
		[XmlAttribute( "id" )]
		public int id;
		
		[XmlAttribute( "file" )]
		public string file;
	}
}
