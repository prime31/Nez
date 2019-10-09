using System;
using System.Collections.Generic;
using Nez.Textures;


namespace Nez.Sprites
{
	/// <summary>
	/// SpriteAnimator handles the display and animation of a sprite
	/// </summary>
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

		public enum State
		{
			None,
			Running,
			Paused,
			Completed
		}

		/// <summary>
		/// fired when an animation completes, includes the animation name;
		/// </summary>
		public event Action<string> OnAnimationCompletedEvent;

		/// <summary>
		/// animation playback speed
		/// </summary>
		public float Speed = 1;

		/// <summary>
		/// the current state of the animation
		/// </summary>
		public State AnimationState { get; private set; } = State.None;

		/// <summary>
		/// the current animation
		/// </summary>
		public SpriteAnimation CurrentAnimation { get; private set; }

		/// <summary>
		/// the name of the current animation
		/// </summary>
		public string CurrentAnimationName { get; private set; }

		/// <summary>
		/// index of the current frame in sprite array of the current animation
		/// </summary>
		public int CurrentFrame { get; private set; }

        /// <summary>
        /// checks to see if the CurrentAnimation is running
        /// </summary>
        public bool IsRunning => AnimationState == State.Running;

		readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();

		float _elapsedTime;
		LoopMode _loopMode;


		public SpriteAnimator()
		{ }

		public SpriteAnimator(Sprite sprite) => SetSprite(sprite);

		void IUpdatable.Update()
		{
			if (AnimationState != State.Running || CurrentAnimation == null)
				return;

			var animation = CurrentAnimation;
			var secondsPerFrame = 1 / (animation.FrameRate * Speed);
			var iterationDuration = secondsPerFrame * animation.Sprites.Length;

			_elapsedTime += Time.DeltaTime;
			var time = Math.Abs(_elapsedTime);

			// Once and PingPongOnce reset back to Time = 0 once they complete
			if (_loopMode == LoopMode.Once && time > iterationDuration ||
			    _loopMode == LoopMode.PingPongOnce && time > iterationDuration * 2)
			{
				AnimationState = State.Completed;
				_elapsedTime = 0;
				CurrentFrame = 0;
				Sprite = animation.Sprites[0];
				OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
				return;
			}

			if (_loopMode == LoopMode.ClampForever && time > iterationDuration)
			{
				AnimationState = State.Completed;
				CurrentFrame = animation.Sprites.Length - 1;
				Sprite = animation.Sprites[CurrentFrame];
				OnAnimationCompletedEvent?.Invoke(CurrentAnimationName);
				return;
			}

			// figure out what iteration we are on
			var completedIterations = Mathf.FloorToInt(time / iterationDuration);
			var currentElapsed = time % iterationDuration;

			// if we are coming backwards on a PingPong we need to reverse elapsed
			if ((_loopMode == LoopMode.PingPong || _loopMode == LoopMode.PingPongOnce) && completedIterations % 2 != 0)
				currentElapsed = iterationDuration - currentElapsed;

			CurrentFrame = Mathf.FloorToInt(currentElapsed / secondsPerFrame);
			Sprite = animation.Sprites[CurrentFrame];
		}

		/// <summary>
		/// adds all the animations from the SpriteAtlas
		/// </summary>
		public SpriteAnimator AddAnimationsFromAtlas(SpriteAtlas atlas)
		{
			for (var i = 0; i < atlas.AnimationNames.Length; i++)
				_animations.Add(atlas.AnimationNames[i], atlas.SpriteAnimations[i]);
			return this;
		}

		/// <summary>
		/// Adds a SpriteAnimation
		/// </summary>
		public SpriteAnimator AddAnimation(string name, SpriteAnimation animation)
		{
			// if we have no sprite use the first frame we find
			if (Sprite == null && animation.Sprites.Length > 0)
				SetSprite(animation.Sprites[0]);
			_animations[name] = animation;
			return this;
		}

		public SpriteAnimator AddAnimation(string name, Sprite[] sprites, float fps = 10) => AddAnimation(name, fps, sprites);

		public SpriteAnimator AddAnimation(string name, float fps, params Sprite[] sprites)
		{
			AddAnimation(name, new SpriteAnimation(sprites, fps));
			return this;
		}

		#region Playback

		/// <summary>
		/// plays the animation with the given name. If no loopMode is specified it is defaults to Loop
		/// </summary>
		public void Play(string name, LoopMode? loopMode = null)
		{
			CurrentAnimation = _animations[name];
			CurrentAnimationName = name;
			CurrentFrame = 0;
			AnimationState = State.Running;

			Sprite = CurrentAnimation.Sprites[0];
			_elapsedTime = 0;
			_loopMode = loopMode ?? LoopMode.Loop;
		}

		/// <summary>
		/// checks to see if the animation is playing (i.e. the animation is active. it may still be in the paused state)
		/// </summary>
		public bool IsAnimationActive(string name) => CurrentAnimation != null && CurrentAnimationName.Equals(name);

		/// <summary>
		/// pauses the animator
		/// </summary>
		public void Pause() => AnimationState = State.Paused;

		/// <summary>
		/// unpauses the animator
		/// </summary>
		public void UnPause() => AnimationState = State.Running;

		/// <summary>
		/// stops the current animation and nulls it out
		/// </summary>
		public void Stop()
		{
			CurrentAnimation = null;
			CurrentAnimationName = null;
			CurrentFrame = 0;
			AnimationState = State.None;
		}

		#endregion
	}
}