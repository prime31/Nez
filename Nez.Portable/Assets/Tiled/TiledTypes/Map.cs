using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public partial class TmxMap : TmxDocument, IDisposable
    {
		public string Version;
		public string TiledVersion;
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

		/// <summary>
		/// contains all of the ITmxLayers, regardless of their specific type. Note that layers in a TmxGroup will not
		/// be in this list. TmxGroup manages its own layers list.
		/// </summary>
		public TmxList<ITmxLayer> Layers;

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
        /// currently only used to tick all the Tilesets so they can update their animated tiles
        /// </summary>
        public void Update() {
	        for (var i = 0; i < Tilesets.Count; i++) {
		        Tilesets[i].Update();
	        }
        }

		#region IDisposable Support

		bool _isDisposed;

		void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing) {
					for (var i = 0; i < Tilesets.Count; i++) {
						Tilesets[i].Image?.Dispose();
					}

					for (var i = 0; i < ImageLayers.Count; i++) {
						ImageLayers[i].Image?.Dispose();
					}
				}

				_isDisposed = true;
			}
		}

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
