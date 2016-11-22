using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Overlap2D
{
	public class O2DMainItem
	{
		public int uniqueId;
		public string itemIdentifier;
		public string itemName;
		public string customVars;
		public float x;
		public float y;
		public float scaleX;
		public float scaleY;
		public float originX;
		public float originY;
		public float rotation;
		public int zIndex;

		/// <summary>
		/// layerDepth is calculated by the Pipeline processor. It is derrived by getting the max zIndex and converting it to the MonoGame
		/// 0 - 1 range. If sorting issues arise the CompositeItemVO.calculateLayerDepthForChild method is where to look. The default value
		/// probably just needs to be increased a bit.
		/// </summary>
		public float layerDepth;

		/// <summary>
		/// renderLayer is derived from the layer name set in Overlap2D. If the layer name contains an integer that value will be parsed and set.
		/// </summary>
		public int renderLayer;
		public string layerName;
		public Color tint;

		Dictionary<string,string> _customVarsDict;


		/// <summary>
		/// translates the bottom-left based origin of Overlap2D to a top-left based origin
		/// </summary>
		/// <returns>The for image size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public Vector2 orginForImageSize( float width, float height )
		{
			var origin = new Vector2( 0, height );
			return origin; 
		}


		/// <summary>
		/// helper to translate zIndex to layerDepth. zIndexMax should be at least equal to the highest zIndex
		/// </summary>
		/// <returns>The depth.</returns>
		/// <param name="zIndexMax">Z index max.</param>
		public float calculateLayerDepth( float zIndexMax )
		{
			return ( zIndexMax - (float)zIndex ) / zIndexMax;
		}


		public Dictionary<string,string> getCustomVars()
		{
			if( _customVarsDict == null )
				parseCustomVars();

			return _customVarsDict;
		}


		public string getCustomVarString( string key )
		{
			if( _customVarsDict == null )
				parseCustomVars();

			string value = null;
			_customVarsDict.TryGetValue( key, out value );

			return value;
		}


		public float getCustomVarFloat( string key, float defaultValue = 0f )
		{
			if( _customVarsDict == null )
				parseCustomVars();

			string value = null;
			if( _customVarsDict.TryGetValue( key, out value ) )
				return float.Parse( value );

			return defaultValue;
		}


		public int getCustomVarInt( string key, int defaultValue = 0 )
		{
			if( _customVarsDict == null )
				parseCustomVars();

			string value = null;
			if( _customVarsDict.TryGetValue( key, out value ) )
				return int.Parse( value );

			return defaultValue;
		}


		public bool getCustomVarBool( string key, bool defaultValue = true )
		{
			if( _customVarsDict == null )
				parseCustomVars();

			string value = null;
			if( _customVarsDict.TryGetValue( key, out value ) )
				return bool.Parse( value );

			return defaultValue;
		}


		void parseCustomVars()
		{
			_customVarsDict = new Dictionary<string,string>();

			var vars = customVars.Split( ';' );
			for( int i = 0; i < vars.Length; i++ )
			{
				var tmp = vars[i].Split( ':' );
				if( tmp.Length > 1 )
					_customVarsDict.Add( tmp[0], tmp[1] );
			}
		}
	
	}
}

