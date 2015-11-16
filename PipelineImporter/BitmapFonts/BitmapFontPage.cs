using System;
using System.Xml.Serialization;

namespace Nez.Content.Pipeline.BitmapFonts
{
	// ---- AngelCode BmFont XML serializer ----------------------
	// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
	// ---- There's no license restrictions, use as you will. ----
	// ---- Credits to http://www.angelcode.com/ -----------------	
    public class BitmapFontPage
	{
		[XmlAttribute ( "id" )]
        public int ID;
		
		[XmlAttribute ( "file" )]
		public string File;
	}	
}
