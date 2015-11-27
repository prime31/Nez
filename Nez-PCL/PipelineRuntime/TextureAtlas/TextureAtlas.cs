#region File Description
//-----------------------------------------------------------------------------
// SpriteSheet.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using Nez.Textures;
using Nez.Sprites;


#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


#endregion


namespace Nez.TextureAtlases
{
	/// <summary>
	/// runtime component of the TextureAtlasGenerator. Stores the main Texture2D and all relevant metadata
	/// </summary>
	public class TextureAtlas
	{
		/// <summary>
		/// the single large texture used by this sprite sheet
		/// </summary>
		public Texture2D texture;

		/// <summary>
		/// list of all our subtextures. It can be indexed via the image name with the help of the subtextureMap
		/// </summary>
		public readonly List<Subtexture> subtextures;

		/// <summary>
		/// maps actual image names to the index in the subtextures list
		/// </summary>
		readonly Dictionary<string,int> _subtextureMap;

		/// <summary>
		/// stores a map of the name of the sprite animation (derived from the folder name) to a Point. The Point x/y values are the
		/// start/end indexes of the subtextures for the animation frames.
		/// </summary>
		readonly Dictionary<string,Point> _spriteAnimationDetails;

		readonly int _animationFPS;


		public TextureAtlas( Texture2D texture, List<Rectangle> spriteRectangles, Dictionary<string,int> subtextureMap, Dictionary<string,Point> spriteAnimationDetails, int animationFPS )
		{
			this.texture = texture;
			subtextures = new List<Subtexture>();
			_subtextureMap = subtextureMap;
			_spriteAnimationDetails = spriteAnimationDetails;
			_animationFPS = animationFPS;

			// create subtextures for reach rect for easy access
			for( var i = 0; i < spriteRectangles.Count; i++ )
				subtextures.Add( new Subtexture( texture, spriteRectangles[i] ) );
		}


		public Subtexture getSubtexture( int index )
		{
			if( index < 0 || index >= subtextures.Count )
				throw new IndexOutOfRangeException();

			return subtextures[index];
		}


		/// <summary>
		/// gets the Subtexture for the passed in image name
		/// </summary>
		/// <returns>The subtexture.</returns>
		/// <param name="name">Name.</param>
		public Subtexture getSubtexture( string name )
		{
			int index;
			if( _subtextureMap.TryGetValue( name, out index ) )
				return getSubtexture( index );

			throw new KeyNotFoundException( name );
		}


		/// <summary>
		/// returns a SpriteAnimation given an animationName where the animationName is the folder that contains the images
		/// </summary>
		/// <returns>The sprite animation.</returns>
		/// <param name="animationName">Animation name.</param>
		public SpriteAnimation getSpriteAnimation( string animationName )
		{
			Point point;
			if( _spriteAnimationDetails.TryGetValue( animationName, out point ) )
			{
				var animation = new SpriteAnimation
				{
					fps = _animationFPS
				};

				for( var i = point.X; i < point.Y; i++ )
					animation.addFrame( subtextures[i] );

				return animation;
			}

			throw new KeyNotFoundException( animationName );
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
