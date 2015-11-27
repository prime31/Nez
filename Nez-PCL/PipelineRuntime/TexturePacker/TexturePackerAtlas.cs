using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.TextureAtlases
{
	public class TexturePackerAtlas
	{
		public Texture2D texture;
		public readonly List<Subtexture> subtextures;

		/// <summary>
		/// maps actual image names to the index in the subtextures list
		/// </summary>
		readonly Dictionary<string,int> _subtextureMap;


		public TexturePackerAtlas( Texture2D texture )
		{
			this.texture = texture;
			subtextures = new List<Subtexture>();
			_subtextureMap = new Dictionary<string,int>();
		}


		public static TexturePackerAtlas create( Texture2D texture, int regionWidth, int regionHeight, int maxRegionCount = int.MaxValue, int margin = 0, int spacing = 0 )
		{
			var textureAtlas = new TexturePackerAtlas( texture );
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
			if( _subtextureMap.ContainsKey( name ) )
				throw new InvalidOperationException( "Region {0} already exists in the texture atlas" );

			var region = new Subtexture( texture, x, y, width, height );
			var index = subtextures.Count;
			subtextures.Add( region );
			_subtextureMap.Add( name, index );

			return region;
		}


		public void removeSubtexture( int index )
		{
			subtextures.RemoveAt( index );
		}


		public void removeSubtexture( string name )
		{
			int index;

			if( _subtextureMap.TryGetValue( name, out index ) )
			{
				removeSubtexture( index );
				_subtextureMap.Remove( name );
			}
		}


		public Subtexture getSubtexture( int index )
		{
			if( index < 0 || index >= subtextures.Count )
				throw new IndexOutOfRangeException();

			return subtextures[index];
		}


		public Subtexture getSubtexture( string name )
		{
			int index;

			if( _subtextureMap.TryGetValue( name, out index ) )
				return getSubtexture( index );

			throw new KeyNotFoundException( name );
		}


		public Subtexture this[string name]
		{
			get { return getSubtexture( name ); }
		}


		public Subtexture this[int index]
		{
			get { return getSubtexture( index ); }
		}

	}
}
