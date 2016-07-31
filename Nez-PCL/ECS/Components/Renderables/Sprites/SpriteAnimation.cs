using System.Collections.Generic;
using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	public enum AnimationCompletionBehavior
	{
		RemainOnFinalFrame,
		RevertToFirstFrame,
		HideSprite
	}


	public class SpriteAnimation
	{
		public float fps = 10;
		public bool loop = true;
		public bool pingPong;
		public float delay = 0f;
		public float totalDuration;
		public AnimationCompletionBehavior completionBehavior;
		public List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();

		internal float secondsPerFrame;
		internal float iterationDuration;

		bool _hasBeenPreparedForUse;


		public SpriteAnimation()
		{}


		public SpriteAnimation( Subtexture frame )
		{
			addFrame( frame );
		}


		public SpriteAnimation( List<Subtexture> frames )
		{
			addFrames( frames );
		}


		/// <summary>
		/// called by SpriteT to calculate the secondsPerFrame and totalDuration based on the loop details and frame count
		/// </summary>
		/// <returns>The for use.</returns>
		public void prepareForUse()
		{
			if( _hasBeenPreparedForUse )
				return;

			secondsPerFrame = 1f / fps;
			iterationDuration = secondsPerFrame * (float)frames.Count;

			if( loop )
				totalDuration = float.PositiveInfinity;
			else if( pingPong )
				totalDuration = iterationDuration * 2f;
			else
				totalDuration = iterationDuration;

			_hasBeenPreparedForUse = true;
		}


		/// <summary>
		/// sets the origin for all frames in this animation
		/// </summary>
		/// <param name="origin"></param>
		public SpriteAnimation setOrigin( Vector2 origin )
		{
			for( var i = 0; i < frames.Count; i++ )
				frames[i].origin = origin;
			return this;
		}


		/// <summary>
		/// adds a frame to this animation
		/// </summary>
		/// <param name="frame">Frame.</param>
		public SpriteAnimation addFrame( SpriteAnimationFrame frame )
		{
			frames.Add( frame );
			return this;
		}


		/// <summary>
		/// adds a frame to this animation with an origin at subtexture.center
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public SpriteAnimation addFrame( Subtexture subtexture )
		{
			addFrame( new SpriteAnimationFrame( subtexture ) );
			return this;
		}


		/// <summary>
		/// adds a frame to this animation with the specified subtexture and origin
		/// </summary>
		/// <returns>The frame.</returns>
		/// <param name="subtexture">Subtexture.</param>
		/// <param name="origin">Origin.</param>
		public SpriteAnimation addFrame( Subtexture subtexture, Vector2 origin )
		{
			addFrame( new SpriteAnimationFrame( subtexture, origin ) );
			return this;
		}


		public SpriteAnimation addFrames( List<Subtexture> subtextures )
		{
			for( var i = 0; i < subtextures.Count; i++ )
				addFrame( subtextures[i] );
			return this;
		}

	}
}

