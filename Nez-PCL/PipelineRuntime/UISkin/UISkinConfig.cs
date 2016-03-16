using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.TextureAtlases;


namespace Nez.UI
{
	public class UISkinConfig
	{
		public Dictionary<string,Color> colors;
		public string[] textureAtlases;
		public string[] libGdxAtlases;
		public UISkinStyleConfig styles;


		public bool containsColor( string name )
		{
			return colors.ContainsKey( name );
		}
	}


	public class UISkinStyleConfig : Dictionary<string,Dictionary<string,Dictionary<string,string>>>
	{
		/// <summary>
		/// gets all the style class names included in the config object
		/// </summary>
		/// <returns>The styles.</returns>
		/// <param name="styleType">Style type.</param>
		public List<string> getStyleClasses()
		{
			return new List<string>( Keys );
		}


		public List<string> getStyleNames( string styleType )
		{
			return new List<string>( this[styleType].Keys );
		}


		/// <summary>
		/// gets a style config dict for the styleType -> styleName
		/// </summary>
		/// <returns>The style.</returns>
		/// <param name="styleType">Style type.</param>
		/// <param name="styleName">Style name.</param>
		public Dictionary<string,string> getStyleDict( string styleType, string styleName )
		{
			return this[styleType][styleName];
		}
	}
}

