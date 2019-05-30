using ImGuiNET;
using System;
using System.Collections.Generic;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	public class ImGuiOptions
	{
		internal bool _includeDefaultFont = true;
		internal List<Tuple<string, float>> _fonts = new List<Tuple<string, float>>();
		internal string _gameWindowTitle = "Game Window";
		internal Num.Vector2 _gameWindowFirstPosition = new Num.Vector2( 345f, 25f );
		internal ImGuiWindowFlags _gameWindowFlags = 0;


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

		public ImGuiOptions setGameWindowTitle( string title )
		{
			_gameWindowTitle = title;
			return this;
		}

		public ImGuiOptions setGameWindowFirstPosition( float x, float y )
		{
			_gameWindowFirstPosition = new Num.Vector2( x, y );
			return this;
		}

		public ImGuiOptions setGameWindowFlag( ImGuiWindowFlags flag )
		{
			_gameWindowFlags |= flag;
			return this;
		}
	}
}
