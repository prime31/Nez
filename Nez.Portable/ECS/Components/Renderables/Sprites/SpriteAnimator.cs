using System;
using System.Collections.Generic;
using Nez.SpriteAtlases;
using Nez.Textures;


namespace Nez.Sprites
{
	public class SpriteAnimator : Component, IUpdatable
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

		public float Speed = 1;
		public bool IsPaused { get; private set; }
		public SpriteAtlases.SpriteAnimation CurrentAnimation => _currentAnimation;
		public string CurrentAnimationName => _currentAnimationName;

		SpriteRenderer _spriteRenderer;
		Dictionary<string, SpriteAtlases.SpriteAnimation> _animations = new Dictionary<string, SpriteAtlases.SpriteAnimation>();
		SpriteAtlases.SpriteAnimation _currentAnimation;
		string _currentAnimationName;
		float _time;
		LoopMode _loopMode;

		public override void OnAddedToEntity()
		{
			_spriteRenderer = Entity.GetComponent<SpriteRenderer>();
		}

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
				_spriteRenderer.Sprite = _currentAnimation.Sprites[0];
				return;
			}

			if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
			{
				_spriteRenderer.Sprite = _currentAnimation.Sprites.LastItem();
				return;
			}

			// figure out what iteration we are on
			var completedIterations = Mathf.FloorToInt(time / iterationDuration);
			var currentElapsed = time % iterationDuration;

			// if we are coming backwards on a PingPong we need to reverse elapsed
			if ((_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce) && completedIterations % 2 != 0)
				currentElapsed = iterationDuration - currentElapsed;

			var desiredFrame = Mathf.FloorToInt(currentElapsed / secondsPerFrame);
			_spriteRenderer.Sprite = _currentAnimation.Sprites[desiredFrame];
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
		public void AddAnimation(string name, SpriteAtlases.SpriteAnimation animation)
		{
			// make a best effort to have a Sprite set. If we have no SpriteRenderer this is being called before the Scene ticked
			if (_spriteRenderer == null)
				Entity.GetComponent<SpriteRenderer>()?.SetSprite(animation.Sprites[0]);
			_animations[name] = animation;
		}

		public void AddAnimation(string name, Sprite[] frames, float fps = 10)
		{
			AddAnimation(name, new SpriteAtlases.SpriteAnimation(frames, 10));
		}

		#region Playback

		/// <summary>
		/// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
		/// </summary>
		public void Play(string name, LoopMode? loopMode = null)
		{
			_currentAnimation = _animations[name];
			_currentAnimationName = name;

			_spriteRenderer?.SetSprite(_currentAnimation.Sprites[0]);
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