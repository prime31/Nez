using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class UISkinConfig
	{
		public Dictionary<string, Color> colors;
		public string[] textureAtlases;
		public string[] libGdxAtlases;
		public UISkinStyleConfig styles;


		public bool containsColor( string name )
		{
			return colors.ContainsKey( name );
		}
	}


	public class UISkinStyleConfig : Dictionary<string, object>
	{
		/// <summary>
		/// gets all the style class names included in the config object
		/// </summary>
		/// <returns>The styles.</returns>
		public List<string> getStyleClasses()
		{
			return new List<string>( Keys );
		}


		/// <summary>
		/// gets all the style names in the config object for the given styleType
		/// </summary>
		/// <returns>The style names.</returns>
		/// <param name="styleType">Style type.</param>
		public List<string> getStyleNames( string styleType )
		{
			var type = this[styleType] as Dictionary<string, object>;
			return new List<string>( type.Keys );
		}


		/// <summary>
		/// gets a style config dict for the styleType -> styleName
		/// </summary>
		/// <returns>The style.</returns>
		/// <param name="styleType">Style type.</param>
		/// <param name="styleName">Style name.</param>
		public Dictionary<string, object> getStyleDict( string styleType, string styleName )
		{
			var styleDict = this[styleType] as Dictionary<string, object>;
			return styleDict[styleName] as Dictionary<string, object>;
		}
	}
}

