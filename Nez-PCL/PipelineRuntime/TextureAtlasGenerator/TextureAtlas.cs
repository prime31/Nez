#region File Description
//-----------------------------------------------------------------------------
// SpriteSheet.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using Nez.Textures;


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
	/// A sprite sheet contains many individual sprite images, packed into different
	/// areas of a single larger texture, along with information describing where in
	/// that texture each sprite is located. Sprite sheets can make your game drawing
	/// more efficient, because they reduce the number of times the graphics hardware
	/// needs to switch from one texture to another.
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


		public TextureAtlas( Texture2D texture, List<Rectangle> spriteRectangles, Dictionary<string,int> subtextureMap )
		{
			this.texture = texture;
			subtextures = new List<Subtexture>();
			_subtextureMap = subtextureMap;

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
