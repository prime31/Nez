/*
The MIT License (MIT)

Copyright (c) 2015 Valerio Santinelli

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;


namespace Nez.Overlap2D.Runtime
{
	public class CustomVariables
	{
		Dictionary<String,String> variables = new Dictionary<String,String>();


		public CustomVariables()
		{}


		public void loadFromString( String varString )
		{
			variables.Clear();
			String[] vars = varString.Split( ';' );
			for( int i = 0; i < vars.Length; i++ )
			{
				String[] tmp = vars[i].Split( ':' );
				if( tmp.Length > 1 )
				{
					setVariable( tmp[0], tmp[1] );
				}
			}
		}

		public String saveAsString()
		{
			String result = "";
			foreach( var entry in variables )
			{
				String key = entry.Key;
				String value = entry.Value;
				result += key + ":" + value + ";";
			}
			if( result.Length > 0 )
			{
				result = result.Substring( 0, result.Length - 1 );
			}

			return result;
		}


		public void setVariable( String key, String value )
		{
			variables.Add( key, value );
		}


		public void removeVariable( String key )
		{
			variables.Remove( key );
		}


		public String getStringVariable( String key )
		{
			return variables[key];
		}


		public int getIntegerVariable( String key )
		{
			int result = 0;
			try
			{
				result = int.Parse( variables[key] );
			}
			catch( Exception )
			{
			}

			return result;
		}


		public float getFloatVariable( String key )
		{
			float result = 0;
			try
			{
				result = float.Parse( variables[key] );
			}
			catch( Exception )
			{
			}

			return result;
		}


		public Dictionary<String, String> getHashMap()
		{
			return variables;
		}


		public int getCount()
		{
			return variables.Count;
		}
	}
}

