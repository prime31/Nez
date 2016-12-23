using System;


namespace Nez
{
	/// <summary>
	/// prep for a proper multi-platform clipboard system. For now it just mocks the clipboard and will only work in-app
	/// </summary>
	public class Clipboard : IClipboard
	{
		static IClipboard _instance;
		string _text;


		public static string getContents()
		{
			if( _instance == null )
				_instance = new Clipboard();
			return _instance.getContents();
		}


		public static void setContents( string text )
		{
			if( _instance == null )
				_instance = new Clipboard();
			_instance.setContents( text );
		}


		
		#region IClipboard implementation

		string IClipboard.getContents()
		{
			return _text;
		}


		void IClipboard.setContents( string text )
		{
			_text = text;
		}

		#endregion


	}
}

