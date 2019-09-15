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
		/// array of all subtextures from the atlas
		/// </summary>
		public readonly Sprite[] Subtextures;

		/// <summary>
		/// image names for the subtextures. maps directly to the subtextures array
		/// </summary>
		public readonly string[] RegionNames;

		/// <summary>
		/// stores a map of the name of the sprite animation (derived from the folder name) to a Point. The Point x/y values are the
		/// start/end indexes of the subtextures for the animation frames.
		/// </summary>
		readonly Dictionary<string, Point> _spriteAnimationDetails;

		readonly int _animationFPS = 15;
		Dictionary<string, SpriteAnimation> _spriteAnimations;


		public TextureAtlas(string[] regionNames, Sprite[] subtextures,
		                    Dictionary<string, Point> spriteAnimationDetails, int animationFPS = 15)
		{
			RegionNames = regionNames;
			Subtextures = subtextures;
			_spriteAnimationDetails = spriteAnimationDetails;
			_animationFPS = animationFPS;
		}

		public TextureAtlas(string[] regionNames, Sprite[] subtextures) : this(regionNames, subtextures, null)
		{}

		/// <summary>
		/// gets the Sprite for the passed in image name
		/// </summary>
		/// <returns>The sprite.</returns>
		/// <param name="name">Name.</param>
		public Sprite GetSubtexture(string name) => Subtextures[Array.IndexOf(RegionNames, name)];

		/// <summary>
		/// checks whether the sprite is contained in this atlas.
		/// </summary>
		/// <returns><c>true</c>, if sprite is containsed, <c>false</c> otherwise.</returns>
		/// <param name="name">the image name</param>
		public bool ContainsSubtexture(string name) => RegionNames.Contains(name);

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
					animation.AddFrame(Subtextures[i]);

				_spriteAnimations[animationName] = animation;

				return animation;
			}

			throw new KeyNotFoundException(animationName);
		}

		public Sprite this[string name] => GetSubtexture(name);
	}
}