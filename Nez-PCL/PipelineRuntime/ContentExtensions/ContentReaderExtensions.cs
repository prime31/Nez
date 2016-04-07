using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;


namespace Nez.Content
{
	internal static class ContentReaderExtensions
	{
		public static string getRelativeAssetPath( this ContentReader contentReader, string relativePath )
		{
			var assetName = contentReader.AssetName;
			var assetNodes = assetName.Split( new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries );
			var relativeNodes = relativePath.Split( new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries );
			var relativeIndex = assetNodes.Length - 1;
			var newPathNodes = new List<string>();


			foreach( var relativeNode in relativeNodes )
			{
				if( relativeNode == ".." )
					relativeIndex--;
				else
					newPathNodes.Add( relativeNode );
			}

			var values = assetNodes
                .Take( relativeIndex )
                .Concat( newPathNodes )
                .ToArray();

			return string.Join( "/", values );
		}


		public static Rectangle readRectangle( this ContentReader reader )
		{
			return new Rectangle( reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() );
		}
	}
}