using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public partial class TmxMap : TmxDocument, IDisposable
    {
        public string Version {get; private set;}
        public string TiledVersion { get; private set; }
        public int Width;
        public int Height;
        public int WorldWidth => Width * TileWidth;
        public int WorldHeight => Height * TileHeight;
        public int TileWidth;
        public int TileHeight;
        public int? HexSideLength;
        public OrientationType Orientation;
        public StaggerAxisType StaggerAxis;
        public StaggerIndexType StaggerIndex;
        public RenderOrderType RenderOrder;
        public Color BackgroundColor;
        public int? NextObjectID;

        public TmxList<TmxTileset> Tilesets;
        public TmxList<TmxLayer> TileLayers;
        public TmxList<TmxObjectGroup> ObjectGroups;
        public TmxList<TmxImageLayer> ImageLayers;
        public TmxList<TmxGroup> Groups;
        public Dictionary<string, string> Properties;

        /// <summary>
        /// when we have an image tileset, tiles can be any size so we record the max size for culling
        /// </summary>
        public int MaxTileWidth;

        /// <summary>
        /// when we have an image tileset, tiles can be any size so we record the max size for culling
        /// </summary>
        public int MaxTileHeight;

        /// <summary>
        /// does this map have non-default tile sizes that would require special culling?
        /// </summary>
        public bool RequiresLargeTileCulling => MaxTileWidth > TileWidth || MaxTileHeight > TileHeight;

        /// <summary>
        /// contains all of the ITmxLayers, regardless of their specific type. Note that layers in a TmxGroup will not
        /// be in this list. TmxGroup manages its own layers list.
        /// </summary>
        public TmxList<ITmxLayer> Layers;

        public TmxMap(string filename) => Load(ReadXml(filename));

        public TmxMap(Stream inputStream, string tmxDirectory)
        {
			TmxDirectory = tmxDirectory;
			using (var xmlReader = XmlReader.Create(inputStream))
            {
                Load(XDocument.Load(xmlReader));
            }
        }

        public TmxMap(XDocument xDoc) => Load(xDoc);

        void Load(XDocument xDoc)
        {
            var xMap = xDoc.Element("map");
            Version = (string)xMap.Attribute("version");
            TiledVersion = (string)xMap.Attribute("tiledversion");

            Width = (int)xMap.Attribute("width");
            Height = (int)xMap.Attribute("height");
            TileWidth = (int)xMap.Attribute("tilewidth");
            TileHeight = (int)xMap.Attribute("tileheight");
            HexSideLength = (int?)xMap.Attribute("hexsidelength");

            // enum parsing
            Orientation = ParseOrientationType((string)xMap.Attribute("orientation"));
            StaggerAxis = ParseStaggerAxisType((string)xMap.Attribute("staggeraxis"));
            StaggerIndex = ParseStaggerIndexType((string)xMap.Attribute("staggerindex"));
            RenderOrder = ParaseRenderOrderType((string)xMap.Attribute("renderorder"));

            NextObjectID = (int?)xMap.Attribute("nextobjectid");
            BackgroundColor = TmxColor.ParseColor(xMap.Attribute("backgroundcolor"));

            Properties = PropertyDict.ParsePropertyDict(xMap.Element("properties"));

            // we keep a tally of the max tile size for the case of image tilesets with random sizes
            MaxTileWidth = TileWidth;
            MaxTileHeight = TileHeight;

            Tilesets = new TmxList<TmxTileset>();
            foreach (var e in xMap.Elements("tileset"))
            {
                var tileset = TmxTileset.ParseTmxTileset(this, e, TmxDirectory);
                Tilesets.Add(tileset);

                // we have to iterate the dictionary because tile.gid (the key) could be any number in any order
                foreach (var kvPair in tileset.Tiles)
                {
                    var tile = kvPair.Value;
                    if (tile.Image != null)
                    {
                        if (tile.Image.Width > MaxTileWidth)
                            MaxTileWidth = tile.Image.Width;
                        if (tile.Image.Height > MaxTileHeight)
                            MaxTileHeight = tile.Image.Height;
                    }
                }    
            }

            Layers = new TmxList<ITmxLayer>();
            TileLayers = new TmxList<TmxLayer>();
            ObjectGroups = new TmxList<TmxObjectGroup>();
            ImageLayers = new TmxList<TmxImageLayer>();
            Groups = new TmxList<TmxGroup>();
            foreach (var e in xMap.Elements().Where(x => x.Name == "layer" || x.Name == "objectgroup" || x.Name == "imagelayer" || x.Name == "group"))
            {
                ITmxLayer layer;
                switch (e.Name.LocalName)
                {
                    case "layer":
                        var tileLayer = new TmxLayer(this, e, Width, Height);
                        layer = tileLayer;
                        TileLayers.Add(tileLayer);
                        break;
                    case "objectgroup":
                        var objectgroup = new TmxObjectGroup(this, e);
                        layer = objectgroup;
                        ObjectGroups.Add(objectgroup);
                        break;
                    case "imagelayer":
                        var imagelayer = new TmxImageLayer(this, e, TmxDirectory);
                        layer = imagelayer;
                        ImageLayers.Add(imagelayer);
                        break;
                    case "group":
                        var group = new TmxGroup(this, e, Width, Height, TmxDirectory);
                        layer = group;
                        Groups.Add(group);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                Layers.Add(layer);
            }
        }

        OrientationType ParseOrientationType(string type)
        {
            if (type == "unknown")
                return OrientationType.Unknown;
            if (type == "orthogonal")
                return OrientationType.Orthogonal;
            if (type == "isometric")
                return OrientationType.Isometric;
            if (type == "staggered")
                return OrientationType.Staggered;
            if (type == "hexagonal")
                return OrientationType.Hexagonal;
            
            return OrientationType.Unknown;
        }

        StaggerAxisType ParseStaggerAxisType(string type)
        {
            if (type == "y")
                return StaggerAxisType.Y;
            return StaggerAxisType.X;
        }

        StaggerIndexType ParseStaggerIndexType(string type)
        {
            if (type == "even")
                return StaggerIndexType.Even;
            return StaggerIndexType.Odd;
        }

        RenderOrderType ParaseRenderOrderType(string type)
        {
            if (type == "right-up")
                return RenderOrderType.RightUp;
            if (type == "left-down")
                return RenderOrderType.LeftDown;
            if (type == "left-up")
                return RenderOrderType.LeftUp;
            return RenderOrderType.RightDown;
        }
    
        /// <summary>
        /// currently only used to tick all the Tilesets so they can update their animated tiles
        /// </summary>
        public void Update()
        {
            foreach (var tileset in Tilesets)
                tileset.Update();
        }

		#region IDisposable Support

		bool _isDisposed;

		void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					foreach (var tileset in Tilesets)
						tileset.Image.Dispose();

					foreach (var layer in ImageLayers)
						layer.Image.Dispose();
				}

				_isDisposed = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~TmxMap()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		void IDisposable.Dispose() => Dispose(true);

		#endregion
	}

    public enum OrientationType
    {
        Unknown,
        Orthogonal,
        Isometric,
        Staggered,
        Hexagonal
    }

    public enum StaggerAxisType
    {
        X,
        Y
    }

    public enum StaggerIndexType
    {
        Odd,
        Even
    }

    public enum RenderOrderType
    {
        RightDown,
        RightUp,
        LeftDown,
        LeftUp
    }
}
