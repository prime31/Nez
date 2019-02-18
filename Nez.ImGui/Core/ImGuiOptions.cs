using System;
using System.Collections.Generic;

namespace Nez.ImGuiTools
{
	public class ImGuiOptions
	{
		internal bool _includeDefaultFont = true;
		internal List<Tuple<string, float>> _fonts = new List<Tuple<string, float>>();


		public ImGuiOptions addFont( string path, float size )
		{
			_fonts.Add( new Tuple<string, float>( path, size ) );
			return this;
		}

		public ImGuiOptions includeDefaultFont( bool include )
		{
			_includeDefaultFont = include;
			return this;
		}
	}
}
