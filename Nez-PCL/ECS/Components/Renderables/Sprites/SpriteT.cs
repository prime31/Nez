using System;
using Nez.Textures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Sprites
{
	/// <summary>
	/// Sprite class handles the display and animation of a sprite. It uses a suggested Enum as a key (you can use an int as well if you
	/// prefer). If you do use an Enum it is recommended to pass in a IEqualityComparer when using an enum like CoreEvents does.
	/// </summary>
	public class Sprite<TEnum> : Sprite, IUpdatable where TEnum : struct, IComparable, IFormattable
	{
		public System.Action<TEnum> onAnimationCompletedEvent;
		public bool isPlaying { get; private set; }
		public int currentFrame { get; private set; }

		/// <summary>
		/// gets/sets the currently playing animation
		/// </summary>
		/// <value>The current animation.</value>
		public TEnum currentAnimation
		{
			get { return _currentAnimationKey; }
			set { play( value ); }
		}

		Dictionary<TEnum,SpriteAnimation> _animations;

		// playback state
		SpriteAnimation _currentAnimation;
		TEnum _currentAnimationKey;
		float _totalElapsedTime;
		float _elapsedDelay;
		int _completedIterations;
		bool _delayComplete;
		bool _isReversed;
		bool _isLoopingBackOnPingPong;


		/// <summary>
		/// beware the beast man! If you use this constructor you must set the subtexture or set animations so that this sprite has proper bounds
		/// when the Scene is running.
		/// </summary>
		/// <param name="customComparer">Custom comparer.</param>
		public Sprite( IEqualityComparer<TEnum> customComparer = null ) : base( new Subtexture( null, 0, 0, 0, 0 ) )
		{
			_animations = new Dictionary<TEnum,SpriteAnimation>( customComparer );
		}


		public Sprite( IEqualityComparer<TEnum> customComparer, Subtexture subtexture ) : base( subtexture )
		{
			_animations = new Dictionary<TEnum,SpriteAnimation>( customComparer );
		}


		/// <summary>
		/// Sprite needs a Subtexture at constructor time so that it knows how to size itself
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public Sprite( Subtexture subtexture ) : this( null, subtexture )
		{}


		/// <summary>
		/// Sprite needs a Subtexture at constructor time so the first frame of the passed in animation will be used for this constructor
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public Sprite( TEnum animationKey, SpriteAnimation animation ) : this( null, animation.frames[0].subtexture )
		{
			addAnimation( animationKey, animation );
		}


		#region Component overrides

		void IUpdatable.update()
		{
			if( _currentAnimation == null || !isPlaying )
				return;

			// handle delay
			if( !_delayComplete && _elapsedDelay < _currentAnimation.delay )
			{
				_elapsedDelay += Time.deltaTime;
				if( _elapsedDelay >= _currentAnimation.delay )
					_delayComplete = true;

				return;
			}

			// count backwards if we are going in reverse
			if( _isReversed )
				_totalElapsedTime -= Time.deltaTime;
			else
				_totalElapsedTime += Time.deltaTime;


			_totalElapsedTime = Mathf.clamp( _totalElapsedTime, 0f, _currentAnimation.totalDuration );
			_completedIterations = Mathf.floorToInt( _totalElapsedTime / _currentAnimation.iterationDuration );
			_isLoopingBackOnPingPong = false;


			// handle ping pong loops. if loop is false but pingPongLoop is true we allow a single forward-then-backward iteration
			if( _currentAnimation.pingPong )
			{
				if( _currentAnimation.loop || _completedIterations < 2 )
					_isLoopingBackOnPingPong = _completedIterations % 2 != 0;
			}


			var elapsedTime = 0f;
			if( _totalElapsedTime < _currentAnimation.iterationDuration )
			{
				elapsedTime = _totalElapsedTime;
			}
			else
			{
				elapsedTime = _totalElapsedTime % _currentAnimation.iterationDuration;

				// if we arent looping and elapsedTime is 0 we are done. Handle it appropriately
				if( !_currentAnimation.loop && elapsedTime == 0 )
				{
					// the animation is done so fire our event
					if( onAnimationCompletedEvent != null )
						onAnimationCompletedEvent( _currentAnimationKey );

					isPlaying = false;

					switch( _currentAnimation.completionBehavior )
					{
						case AnimationCompletionBehavior.RemainOnFinalFrame:
							return;
						case AnimationCompletionBehavior.RevertToFirstFrame:
							subtexture = _currentAnimation.frames[0].subtexture;
							origin = _currentAnimation.frames[0].origin;
							return;
						case AnimationCompletionBehavior.HideSprite:
							subtexture = null;
							_currentAnimation = null;
							return;
					}
				}
			}


			// if we reversed the animation and we reached 0 total elapsed time handle un-reversing things and loop continuation
			if( _isReversed && _totalElapsedTime <= 0 )
			{
				_isReversed = false;

				if( _currentAnimation.loop )
				{
					_totalElapsedTime = 0f;
				}
				else
				{
					// the animation is done so fire our event
					if( onAnimationCompletedEvent != null )
						onAnimationCompletedEvent( _currentAnimationKey );

					isPlaying = false;
					return;
				}
			}

			// time goes backwards when we are reversing a ping-pong loop
			if( _isLoopingBackOnPingPong )
				elapsedTime = _currentAnimation.iterationDuration - elapsedTime;


			// fetch our desired frame
			var desiredFrame = Mathf.floorToInt( elapsedTime / _currentAnimation.secondsPerFrame );
			if( desiredFrame != currentFrame )
			{
				currentFrame = desiredFrame;
				subtexture = _currentAnimation.frames[currentFrame].subtexture;
				origin = _currentAnimation.frames[currentFrame].origin;
				handleFrameChanged();

				// ping-pong needs special care. we don't want to double the frame time when wrapping so we man-handle the totalElapsedTime
				if( _currentAnimation.pingPong && ( currentFrame == 0 || currentFrame == _currentAnimation.frames.Count - 1 ) )
				{
					if( _isReversed )
						_totalElapsedTime -= _currentAnimation.secondsPerFrame;
					else
						_totalElapsedTime += _currentAnimation.secondsPerFrame;
				}
			}
		}

		#endregion


		public Sprite<TEnum> addAnimation( TEnum key, SpriteAnimation animation )
		{
			// if we have no subtexture use the first frame we find
			if( subtexture == null && animation.frames.Count > 0 )
				subtexture = animation.frames[0].subtexture;
			_animations[key] = animation;

			return this;
		}


		#region Playback

		/// <summary>
		/// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
		/// </summary>
		/// <param name="animationKey">Animation key.</param>
		/// <param name="startFrame">Start frame.</param>
		public void play( TEnum animationKey, int startFrame = 0 )
		{
			Assert.isTrue( _animations.ContainsKey( animationKey ), "Attempted to play an animation that doesnt exist" );

			var animation = _animations[animationKey];
			animation.prepareForUse();

			_currentAnimationKey = animationKey;
			_currentAnimation = animation;
			isPlaying = true;
			_isReversed = false;
			currentFrame = startFrame;
			subtexture = _currentAnimation.frames[currentFrame].subtexture;
			origin = _currentAnimation.frames[currentFrame].origin;

			_totalElapsedTime = (float)startFrame * _currentAnimation.secondsPerFrame;
		}


		public bool isAnimationPlaying( TEnum animationKey )
		{
			return _currentAnimation != null && _currentAnimationKey.Equals( animationKey );
		}


		public void pause()
		{
			isPlaying = false;
		}


		public void unPause()
		{
			isPlaying = true;
		}


		public void reverseAnimation()
		{
			_isReversed = !_isReversed;
		}


		public void stop()
		{
			isPlaying = false;
			_currentAnimation = null;
		}

		#endregion


		void handleFrameChanged()
		{
			// TODO: add animation frame triggers
		}

	}
}

