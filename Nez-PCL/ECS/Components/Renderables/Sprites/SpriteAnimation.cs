using System;
using System.Collections.Generic;
using Nez.TextureAtlases;


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
		public float fps = 5;
		public bool loop;
		public bool pingPong;
		public float delay = 0f;
		public AnimationCompletionBehavior completionBehavior;

		public List<Subtexture> frames = new List<Subtexture>();
		public float secondsPerFrame;
		public float iterationDuration;
		public float totalDuration;

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


		public void addFrame( Subtexture frame )
		{
			frames.Add( frame );
		}


		public void addFrames( List<Subtexture> frames )
		{
			this.frames.AddRange( frames );
		}

	}
}

