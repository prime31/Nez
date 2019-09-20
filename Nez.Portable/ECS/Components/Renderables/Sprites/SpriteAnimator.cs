using System;
using System.Collections.Generic;
using Nez.Sprites;
using Nez.Textures;


namespace Nez.Sprites
{
	public class SpriteAnimator : SpriteRenderer, IUpdatable
	{
		public enum LoopMode
		{
			/// <summary>
			/// Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
			/// </summary>
			Loop,

			/// <summary>
			/// Play the sequence once [A][B][C] then pause and set time to 0 [A]
			/// </summary>
			Once,

			/// <summary>
			/// Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing
			/// </summary>
			ClampForever,

			/// <summary>
			/// Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
			/// </summary>
			PingPong,

			/// <summary>
			/// Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
			/// </summary>
			PingPongOnce
		}

		/// <summary>
		/// animation playback speed
		/// </summary>
		public float Speed = 1;

		/// <summary>
		/// is the current animatiomn paused
		/// </summary>
		public bool IsPaused { get; private set; }

		/// <summary>
		/// the current animation
		/// </summary>
		public SpriteAnimation CurrentAnimation => _currentAnimation;

		/// <summary>
		/// the name of the current animation
		/// </summary>
		public string CurrentAnimationName => _currentAnimationName;

		readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
		SpriteAnimation _currentAnimation;
		string _currentAnimationName;
		float _time;
		LoopMode _loopMode;


		public SpriteAnimator()
		{ }

		public SpriteAnimator(Sprite sprite) => SetSprite(sprite);

		void IUpdatable.Update()
		{
			if (IsPaused || _currentAnimation == null)
				return;

			var secondsPerFrame = 1 / (_currentAnimation.FrameRate * Speed);
			var iterationDuration = secondsPerFrame * _currentAnimation.Sprites.Length;

			_time += Time.DeltaTime;
			var time = Math.Abs(_time);

			// Once and PingPongOnce reset back to Time = 0 once they complete
			if (_loopMode == LoopMode.Once && time > iterationDuration ||
			    _loopMode == LoopMode.PingPongOnce && time > iterationDuration * 2)
			{
				IsPaused = true;
				_time = 0;
				Sprite = _currentAnimation.Sprites[0];
				return;
			}

			if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
			{
				Sprite = _currentAnimation.Sprites.LastItem();
				return;
			}

			// figure out what iteration we are on
			var completedIterations = Mathf.FloorToInt(time / iterationDuration);
			var currentElapsed = time % iterationDuration;

			// if we are coming backwards on a PingPong we need to reverse elapsed
			if ((_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce) && completedIterations % 2 != 0)
				currentElapsed = iterationDuration - currentElapsed;

			var desiredFrame = Mathf.FloorToInt(currentElapsed / secondsPerFrame);
			Sprite = _currentAnimation.Sprites[desiredFrame];
		}

		/// <summary>
		/// adds all the animations from the SpriteAtlas
		/// </summary>
		public void AddAnimations(SpriteAtlas atlas)
		{
			for (var i = 0; i < atlas.AnimationNames.Length; i++)
				_animations.Add(atlas.AnimationNames[i], atlas.SpriteAnimations[i]);
		}

		/// <summary>
		/// Adds a SpriteAnimation
		/// </summary>
		public void AddAnimation(string name, SpriteAnimation animation)
		{
			// if we have no sprite use the first frame we find
			if (Sprite == null && animation.Sprites.Length > 0)
				SetSprite(animation.Sprites[0]);
			_animations[name] = animation;
		}

		public void AddAnimation(string name, Sprite[] frames, float fps = 10)
		{
			AddAnimation(name, new SpriteAnimation(frames, fps));
		}

		public void AddAnimation(string name, float fps, params Sprite[] sprites)
		{
			AddAnimation(name, new SpriteAnimation(sprites, fps));
		}

		#region Playback

		/// <summary>
		/// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
		/// </summary>
		public void Play(string name, LoopMode? loopMode = null)
		{
			_currentAnimation = _animations[name];
			_currentAnimationName = name;

			Sprite = _currentAnimation.Sprites[0];
			IsPaused = false;

			if (loopMode.HasValue)
				_loopMode = loopMode.Value;
		}

		/// <summary>
		/// checks to see if the animation is playing
		/// </summary>
		public bool IsAnimationPlaying(string name)
		{
			return _currentAnimation != null && _currentAnimationName.Equals(name);
		}

		/// <summary>
		/// pauses the animator
		/// </summary>
		public void Pause() => IsPaused = true;

		/// <summary>
		/// unpauses the animator
		/// </summary>
		public void UnPause() => IsPaused = false;

		/// <summary>
		/// stops the current animation and nulls it out
		/// </summary>
		public void Stop()
		{
			IsPaused = true;
			_currentAnimation = null;
			_currentAnimationName = null;
		}

		#endregion
	}
}