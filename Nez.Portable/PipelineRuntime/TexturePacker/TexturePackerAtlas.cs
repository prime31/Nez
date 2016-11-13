using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Nez.Sprites;


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

		/// <summary>
		/// stores a map of the name of the sprite animation (derived from texturepacker filename metadata) to an array. 
		/// each entry in the list refers to index of the corresponding subtexture
		/// </summary>
		public Dictionary<string,List<int>> spriteAnimationDetails;

		readonly int _animationFPS = 15;
		Dictionary<string,SpriteAnimation> _spriteAnimations;


		public TexturePackerAtlas( Texture2D texture )
		{
			this.texture = texture;
			subtextures = new List<Subtexture>();
			_subtextureMap = new Dictionary<string, int>();
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


		public Subtexture createRegion( string name, int x, int y, int width, int height, float pivotX = 0.5f, float pivotY = 0.5f )
		{
			Assert.isFalse( _subtextureMap.ContainsKey( name ), "Region {0} already exists in the texture atlas", name );

			var region = new Subtexture( texture, new Rectangle( x, y, width, height ), new Vector2( pivotX * width, pivotY * height ) );
			var index = subtextures.Count;
			subtextures.Add( region );
			_subtextureMap.Add( name, index );

			return region;
		}


		/// <summary>
		/// returns a SpriteAnimation given an animationName where the animationName is the region's "filename" metadata 
		/// in the TexturePacker atlas minus the framenumbers at the end
		/// </summary>
		/// <returns>The sprite animation.</returns>
		/// <param name="animationName">Animation name.</param>
		public SpriteAnimation getSpriteAnimation( string animationName )
		{
			// create the cache Dictionary if necessary. Return the animation direction if already cached.
			if( _spriteAnimations == null )
				_spriteAnimations = new Dictionary<string,SpriteAnimation>();
			else if( _spriteAnimations.ContainsKey( animationName ) )
				return _spriteAnimations[animationName];

			if( spriteAnimationDetails.ContainsKey( animationName ) )
			{
				var frames = spriteAnimationDetails[animationName];
				var animation = new SpriteAnimation
				{
					fps = _animationFPS
				};

				for( var i = 0; i < frames.Count; i++ )
					animation.addFrame( subtextures[frames[i]] );

				_spriteAnimations[animationName] = animation;

				return animation;

			}

			throw new KeyNotFoundException( animationName );
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
			Assert.isFalse( index < 0 || index >= subtextures.Count, "index out of range" );
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
