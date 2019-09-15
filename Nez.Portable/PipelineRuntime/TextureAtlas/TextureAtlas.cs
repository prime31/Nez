using Nez.Textures;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.TextureAtlases
{
	/// <summary>
	/// runtime component of the TextureAtlasGenerator. Stores the main Texture2D and all relevant metadata
	/// </summary>
	public class TextureAtlas
	{
		/// <summary>
		/// array of all sprites from the atlas
		/// </summary>
		public readonly Sprite[] Sprites;

		/// <summary>
		/// image names for the sprites. maps directly to the sprites array
		/// </summary>
		public readonly string[] RegionNames;

		/// <summary>
		/// stores a map of the name of the sprite animation (derived from the folder name) to a Point. The Point x/y values are the
		/// start/end indexes of the sprites for the animation frames.
		/// </summary>
		readonly Dictionary<string, Point> _spriteAnimationDetails;

		readonly int _animationFPS = 15;
		Dictionary<string, SpriteAnimation> _spriteAnimations;


		public TextureAtlas(string[] regionNames, Sprite[] sprites,
		                    Dictionary<string, Point> spriteAnimationDetails, int animationFPS = 15)
		{
			RegionNames = regionNames;
			Sprites = sprites;
			_spriteAnimationDetails = spriteAnimationDetails;
			_animationFPS = animationFPS;
		}

		public TextureAtlas(string[] regionNames, Sprite[] sprites) : this(regionNames, sprites, null)
		{}

		/// <summary>
		/// gets the Sprite for the passed in image name
		/// </summary>
		/// <returns>The sprite.</returns>
		/// <param name="name">Name.</param>
		public Sprite GetSprite(string name) => Sprites[Array.IndexOf(RegionNames, name)];

		/// <summary>
		/// checks whether the sprite is contained in this atlas.
		/// </summary>
		/// <returns><c>true</c>, if sprite is containsed, <c>false</c> otherwise.</returns>
		/// <param name="name">the image name</param>
		public bool ContainsSprite(string name) => RegionNames.Contains(name);

		/// <summary>
		/// returns a SpriteAnimation given an animationName where the animationName is the folder that contains the images
		/// </summary>
		/// <returns>The sprite animation.</returns>
		/// <param name="animationName">Animation name.</param>
		public SpriteAnimation GetSpriteAnimation(string animationName)
		{
			// create the cache Dictionary if necessary. Return the animation direction if already cached.
			if (_spriteAnimations == null)
				_spriteAnimations = new Dictionary<string, SpriteAnimation>();
			else if (_spriteAnimations.ContainsKey(animationName))
				return _spriteAnimations[animationName];

			if (_spriteAnimationDetails.TryGetValue(animationName, out Point point))
			{
				var animation = new SpriteAnimation
				{
					Fps = _animationFPS
				};

				for (var i = point.X; i <= point.Y; i++)
					animation.AddFrame(Sprites[i]);

				_spriteAnimations[animationName] = animation;

				return animation;
			}

			throw new KeyNotFoundException(animationName);
		}

		public Sprite this[string name] => GetSprite(name);
	}
}