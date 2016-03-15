using System;


namespace Nez
{
	public interface IClipboard
	{
		string getContents();
		void setContents( string text );
	}
}

