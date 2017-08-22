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


	/// <summary>
	/// houses the information that a SpriteT requires for animation
	/// </summary>
	public class SpriteAnimation
	{
		/// <summary>
		/// frames per second for the animations
		/// </summary>
		/// <value>The fps.</value>
		public float fps
		{
			get { return _fps; }
			set { setFps( value ); }
		}

		/// <summary>
		/// controls whether the animation should loop
		/// </summary>
		/// <value>The loop.</value>
		public bool loop
		{
			get { return _loop; }
			set { setLoop( value ); }
		}

		/// <summary>
		/// if loop is true, this controls if an animation loops sequentially or back and forth
		/// </summary>
		/// <value>The ping pong.</value>
		public bool pingPong
		{
			get { return _pingPong; }
			set { setPingPong( value ); }
		}

		public float delay = 0f;
		public float totalDuration;
		public AnimationCompletionBehavior completionBehavior;
		public List<Subtexture> frames = new List<Subtexture>();

		// calculated values used by SpriteT
		public float secondsPerFrame;
		public float iterationDuration;

		float _fps = 10;
		bool _loop = true;
		bool _pingPong = false;
		bool _isDirty = true;


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
			if( !_isDirty )
				return;

			secondsPerFrame = 1f / fps;
			iterationDuration = secondsPerFrame * (float)frames.Count;

			if( loop )
				totalDuration = float.PositiveInfinity;
			else if( pingPong )
				totalDuration = iterationDuration * 2f;
			else
				totalDuration = iterationDuration;

			_isDirty = false;
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


		public SpriteAnimation setFps( float fps )
		{
			_fps = fps;
			_isDirty = true;
			return this;
		}


		public SpriteAnimation setLoop( bool loop )
		{
			_loop = loop;
			_isDirty = true;
			return this;
		}


		public SpriteAnimation setPingPong( bool pingPong )
		{
			_pingPong = pingPong;
			_isDirty = true;
			return this;
		}


		/// <summary>
		/// adds a frame to this animation
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public SpriteAnimation addFrame( Subtexture subtexture )
		{
			frames.Add( subtexture );
			return this;
		}


		/// <summary>
		/// adds multiple frames to this animation
		/// </summary>
		/// <returns>The frames.</returns>
		/// <param name="subtextures">Subtextures.</param>
		public SpriteAnimation addFrames( List<Subtexture> subtextures )
		{
			for( var i = 0; i < subtextures.Count; i++ )
				addFrame( subtextures[i] );
			return this;
		}

	}
}

