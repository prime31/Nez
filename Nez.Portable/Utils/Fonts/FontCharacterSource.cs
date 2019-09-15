using System.Text;


namespace Nez
{
	/// <summary>
	/// helper that wraps either a string or StringBuilder and provides a common API to read them for measuring/drawing
	/// </summary>
	public struct FontCharacterSource
	{
		readonly string _string;
		readonly StringBuilder _builder;
		public readonly int Length;


		public FontCharacterSource(string s)
		{
			_string = s;
			_builder = null;
			Length = s.Length;
		}


		public FontCharacterSource(StringBuilder builder)
		{
			_builder = builder;
			_string = null;
			Length = _builder.Length;
		}


		public char this[int index]
		{
			get
			{
				if (_string != null)
					return _string[index];

				return _builder[index];
			}
		}
	}
}