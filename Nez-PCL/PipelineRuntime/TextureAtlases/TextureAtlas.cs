using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.TextureAtlases
{
	public class TextureAtlas
	{
		public Texture2D texture;
		public readonly List<Subtexture> regions;

		readonly Dictionary<string,int> _regionMap;


		public TextureAtlas( Texture2D texture )
		{
			this.texture = texture;
			regions = new List<Subtexture>();
			_regionMap = new Dictionary<string, int>();
		}


		public static TextureAtlas create( Texture2D texture, int regionWidth, int regionHeight, int maxRegionCount = int.MaxValue, int margin = 0, int spacing = 0 )
		{
			var textureAtlas = new TextureAtlas( texture );
			var count = 0;
			var width = texture.Width - margin;
			var height = texture.Height - margin;
			var xIncrement = regionWidth + spacing;
			var yIncrement = regionHeight + spacing;

			for( var y = margin; y < height; y += yIncrement )
			{
				for( var x = margin; x < width; x += xIncrement )
				{
					var regionName = string.Format( "{0}{1}", texture.Name ?? "region", count );
					textureAtlas.createRegion( regionName, x, y, regionWidth, regionHeight );
					count++;

					if( count >= maxRegionCount )
						return textureAtlas;
				}
			}

			return textureAtlas;
		}


		public Subtexture createRegion( string name, int x, int y, int width, int height )
		{
			if( _regionMap.ContainsKey( name ) )
				throw new InvalidOperationException( "Region {0} already exists in the texture atlas" );

			var region = new Subtexture( texture, x, y, width, height );
			var index = regions.Count;
			regions.Add( region );
			_regionMap.Add( name, index );

			return region;
		}


		public void removeRegion( int index )
		{
			regions.RemoveAt( index );
		}


		public void removeRegion( string name )
		{
			int index;

			if( _regionMap.TryGetValue( name, out index ) )
			{
				removeRegion( index );
				_regionMap.Remove( name );
			}
		}


		public Subtexture getRegion( int index )
		{
			if( index < 0 || index >= regions.Count )
				throw new IndexOutOfRangeException();

			return regions[index];
		}


		public Subtexture getRegion( string name )
		{
			int index;

			if( _regionMap.TryGetValue( name, out index ) )
				return getRegion( index );

			throw new KeyNotFoundException( name );
		}


		public Subtexture this[string name]
		{
			get { return getRegion( name ); }
		}


		public Subtexture this[int index]
		{
			get { return getRegion( index ); }
		}

	}
}
