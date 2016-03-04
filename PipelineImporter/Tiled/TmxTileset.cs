using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;

namespace Nez.TiledMaps
{
	[XmlRoot( ElementName = "tileset" )]
	public class TmxTileset
	{
		public TmxTileset()
		{
			tileOffset = new TmxTileOffset();
			tiles = new List<TmxTilesetTile>();
			properties = new List<TmxProperty>();
			terrainTypes = new List<TmxTerrain>();

		}


		[XmlAttribute( AttributeName = "firstgid" )]
		public int firstGid;

		[XmlAttribute( AttributeName = "source" )]
		public string source;

		[XmlAttribute( AttributeName = "name" )]
		public string name;

		[XmlAttribute( AttributeName = "tilewidth" )]
		public int tileWidth;

		[XmlAttribute( AttributeName = "tileheight" )]
		public int tileHeight;

		[XmlAttribute( AttributeName = "spacing" )]
		public int spacing;

		[XmlAttribute( AttributeName = "margin" )]
		public int margin;

		[XmlElement( ElementName = "tileoffset" )]
		public TmxTileOffset tileOffset;

		[XmlElement( ElementName = "tile" )]
		public List<TmxTilesetTile> tiles;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties;

		[XmlElement( ElementName = "image" )]
		public TmxImage image;

		[XmlArray( "terraintypes" )]
		[XmlArrayItem( "terrain" )]
		public List<TmxTerrain> terrainTypes;


		public override string ToString()
		{
			return string.Format( "{0}: {1}", name, image );
		}


		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static string makeRelativePath( string fromPath, string toPath )
		{
			var fromUri = new Uri( fromPath );
			var toUri = new Uri( toPath );

			if( fromUri.Scheme != toUri.Scheme )
				return toPath; // path can't be made relative.

			var relativeUri = fromUri.MakeRelativeUri( toUri );
			var relativePath = Uri.UnescapeDataString( relativeUri.ToString() );

			if( toUri.Scheme.Equals( "file", StringComparison.InvariantCultureIgnoreCase ) )
				relativePath = relativePath.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );

			return relativePath;
		}


		public void fixImagePath( string mapPath, string tilesetSource )
		{
			var mapDirectory = Path.GetDirectoryName( mapPath );
			var tilesetDirectory = Path.GetDirectoryName( tilesetSource );
			var imageDirectory = Path.GetDirectoryName( this.image.source );
			var imageFile = Path.GetFileName( this.image.source );
            
			var newPath = Path.GetFullPath( Path.Combine( mapDirectory, tilesetDirectory, imageDirectory, imageFile ) );                        
			image.source = Path.Combine( makeRelativePath( mapPath, newPath ) );
		}
	}
}