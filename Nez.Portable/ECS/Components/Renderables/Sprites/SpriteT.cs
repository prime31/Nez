using System;
using Nez.Textures;
using System.Collections.Generic;


namespace Nez.Sprites
{
	/// <summary>
	/// Sprite class handles the display and animation of a sprite. It uses a suggested Enum as a key (you can use an int as well if you
	/// prefer). If you do use an Enum it is recommended to pass in a IEqualityComparer when using an enum like CoreEvents does. See also
	/// the EnumEqualityComparerGenerator.tt T4 template for automatically generating the IEqualityComparer.
	/// </summary>
	public class Sprite<TEnum> : Sprite, IUpdatable where TEnum : struct, IComparable, IFormattable
	{
		public event Action<TEnum> OnAnimationCompletedEvent;
		public bool IsPlaying { get; private set; }
		public int CurrentFrame { get; private set; }

		/// <summary>
		/// gets/sets the currently playing animation
		/// </summary>
		/// <value>The current animation.</value>
		public TEnum CurrentAnimation
		{
			get => _currentAnimationKey;
			set => Play(value);
		}

		Dictionary<TEnum, SpriteAnimation> _animations;

		// playback state
		SpriteAnimation _currentAnimation;
		TEnum _currentAnimationKey;
		float _totalElapsedTime;
		float _elapsedDelay;
		int _completedIterations;
		bool _delayComplete;
		bool _isReversed;
		bool _isLoopingBackOnPingPong;


		public Sprite() : this(default(IEqualityComparer<TEnum>))
		{
		}

		/// <summary>
		/// beware the beast man! If you use this constructor you must set the subtexture or set animations so that this sprite has proper bounds
		/// when the Scene is running.
		/// </summary>
		/// <param name="customComparer">Custom comparer.</param>
		public Sprite(IEqualityComparer<TEnum> customComparer = null) : base(Graphics.Instance.PixelTexture)
		{
			_animations = new Dictionary<TEnum, SpriteAnimation>(customComparer);
		}

		public Sprite(IEqualityComparer<TEnum> customComparer, Subtexture subtexture) : base(subtexture)
		{
			_animations = new Dictionary<TEnum, SpriteAnimation>(customComparer);
		}

		/// <summary>
		/// Sprite needs a Subtexture at constructor time so that it knows how to size itself
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public Sprite(Subtexture subtexture) : this(null, subtexture)
		{
		}

		/// <summary>
		/// Sprite needs a Subtexture at constructor time so the first frame of the passed in animation will be used for this constructor
		/// </summary>
		/// <param name="animationKey">Animation key.</param>
		/// <param name="animation">Animation.</param>
		public Sprite(TEnum animationKey, SpriteAnimation animation) : this(null, animation.Frames[0])
		{
			AddAnimation(animationKey, animation);
		}


		#region Component overrides

		void IUpdatable.Update()
		{
			if (_currentAnimation == null || !IsPlaying)
				return;

			// handle delay
			if (!_delayComplete && _elapsedDelay < _currentAnimation.Delay)
			{
				_elapsedDelay += Time.DeltaTime;
				if (_elapsedDelay >= _currentAnimation.Delay)
					_delayComplete = true;

				return;
			}

			// count backwards if we are going in reverse
			if (_isReversed)
				_totalElapsedTime -= Time.DeltaTime;
			else
				_totalElapsedTime += Time.DeltaTime;


			_totalElapsedTime = Mathf.Clamp(_totalElapsedTime, 0f, _currentAnimation.TotalDuration);
			_completedIterations = Mathf.FloorToInt(_totalElapsedTime / _currentAnimation.IterationDuration);
			_isLoopingBackOnPingPong = false;


			// handle ping pong loops. if loop is false but pingPongLoop is true we allow a single forward-then-backward iteration
			if (_currentAnimation.PingPong)
			{
				if (_currentAnimation.Loop || _completedIterations < 2)
					_isLoopingBackOnPingPong = _completedIterations % 2 != 0;
			}


			var elapsedTime = 0f;
			if (_totalElapsedTime < _currentAnimation.IterationDuration)
			{
				elapsedTime = _totalElapsedTime;
			}
			else
			{
				elapsedTime = _totalElapsedTime % _currentAnimation.IterationDuration;

				// if we arent looping and elapsedTime is 0 we are done. Handle it appropriately
				if (!_currentAnimation.Loop && elapsedTime == 0)
				{
					// the animation is done so fire our event
					if (OnAnimationCompletedEvent != null)
						OnAnimationCompletedEvent(_currentAnimationKey);

					IsPlaying = false;

					switch (_currentAnimation.CompletionBehavior)
					{
						case AnimationCompletionBehavior.RemainOnFinalFrame:
							return;
						case AnimationCompletionBehavior.RevertToFirstFrame:
							SetSubtexture(_currentAnimation.Frames[0]);
							return;
						case AnimationCompletionBehavior.HideSprite:
							_subtexture = null;
							_currentAnimation = null;
							return;
					}
				}
			}


			// if we reversed the animation and we reached 0 total elapsed time handle un-reversing things and loop continuation
			if (_isReversed && _totalElapsedTime <= 0)
			{
				_isReversed = false;

				if (_currentAnimation.Loop)
				{
					_totalElapsedTime = 0f;
				}
				else
				{
					// the animation is done so fire our event
					if (OnAnimationCompletedEvent != null)
						OnAnimationCompletedEvent(_currentAnimationKey);

					IsPlaying = false;
					return;
				}
			}

			// time goes backwards when we are reversing a ping-pong loop
			if (_isLoopingBackOnPingPong)
				elapsedTime = _currentAnimation.IterationDuration - elapsedTime;


			// fetch our desired frame
			var desiredFrame = Mathf.FloorToInt(elapsedTime / _currentAnimation.SecondsPerFrame);
			if (desiredFrame != CurrentFrame)
			{
				CurrentFrame = desiredFrame;
				SetSubtexture(_currentAnimation.Frames[CurrentFrame]);
				HandleFrameChanged();

				// ping-pong needs special care. we don't want to double the frame time when wrapping so we man-handle the totalElapsedTime
				if (_currentAnimation.PingPong &&
				    (CurrentFrame == 0 || CurrentFrame == _currentAnimation.Frames.Count - 1))
				{
					if (_isReversed)
						_totalElapsedTime -= _currentAnimation.SecondsPerFrame;
					else
						_totalElapsedTime += _currentAnimation.SecondsPerFrame;
				}
			}
		}

		#endregion


		public Sprite<TEnum> AddAnimation(TEnum key, SpriteAnimation animation)
		{
			// if we have no subtexture use the first frame we find
			if (_subtexture == null && animation.Frames.Count > 0)
				SetSubtexture(animation.Frames[0]);
			_animations[key] = animation;

			return this;
		}

		public SpriteAnimation GetAnimation(TEnum key)
		{
			Insist.IsTrue(_animations.ContainsKey(key), "{0} is not present in animations", key);
			return _animations[key];
		}


		#region Playback

		/// <summary>
		/// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
		/// </summary>
		/// <param name="animationKey">Animation key.</param>
		/// <param name="startFrame">Start frame.</param>
		public SpriteAnimation Play(TEnum animationKey, int startFrame = 0)
		{
			Insist.IsTrue(_animations.ContainsKey(animationKey), "Attempted to play an animation that doesnt exist");

			var animation = _animations[animationKey];
			animation.PrepareForUse();

			_currentAnimationKey = animationKey;
			_currentAnimation = animation;
			IsPlaying = true;
			_isReversed = false;
			CurrentFrame = startFrame;
			SetSubtexture(_currentAnimation.Frames[CurrentFrame]);

			_totalElapsedTime = (float) startFrame * _currentAnimation.SecondsPerFrame;
			return animation;
		}

		public bool IsAnimationPlaying(TEnum animationKey)
		{
			return _currentAnimation != null && _currentAnimationKey.Equals(animationKey);
		}

		public void Pause()
		{
			IsPlaying = false;
		}

		public void UnPause()
		{
			IsPlaying = true;
		}

		public void ReverseAnimation()
		{
			_isReversed = !_isReversed;
		}

		public void Stop()
		{
			IsPlaying = false;
			_currentAnimation = null;
		}

		#endregion


		void HandleFrameChanged()
		{
			// TODO: add animation frame triggers
		}
	}
}