using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;


namespace Nez.Sprites
{
	/// <summary>
	/// renders and fades a series of copies of the Sprite on the same Entity. minDistanceBetweenInstances determines how often a trail
	/// sprite is added.
	/// </summary>
	public class SpriteTrail : RenderableComponent, IUpdatable
	{
		/// <summary>
		/// helper class that houses the data required for the individual trail instances
		/// </summary>
		class SpriteTrailInstance
		{
			public Vector2 Position;
			Sprite _sprite;
			float _fadeDuration;
			float _fadeDelay;
			float _elapsedTime;
			Color _initialColor;
			Color _targetColor;
			Color _renderColor;

			float _rotation;
			Vector2 _origin;
			Vector2 _scale;
			SpriteEffects _spriteEffects;
			float _layerDepth;


			public void Spawn(Vector2 position, Sprite sprite, float fadeDuration, float fadeDelay,
							  Color initialColor, Color targetColor)
			{
				Position = position;
				_sprite = sprite;

				_initialColor = initialColor;
				_elapsedTime = 0f;

				_fadeDuration = fadeDuration;
				_fadeDelay = fadeDelay;
				_initialColor = initialColor;
				_targetColor = targetColor;
			}


			public void SetSpriteRenderOptions(float rotation, Vector2 origin, Vector2 scale,
											   SpriteEffects spriteEffects, float layerDepth)
			{
				_rotation = rotation;
				_origin = origin;
				_scale = scale;
				_spriteEffects = spriteEffects;
				_layerDepth = layerDepth;
			}


			/// <summary>
			/// returns true when the fade out is complete
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Update()
			{
				_elapsedTime += Time.DeltaTime;

				// fading block
				if (_elapsedTime > _fadeDelay && _elapsedTime < _fadeDuration + _fadeDelay)
				{
					var t = Mathf.Map01(_elapsedTime, 0f, _fadeDelay + _fadeDuration);
					ColorExt.Lerp(ref _initialColor, ref _targetColor, out _renderColor, t);
				}
				else if (_elapsedTime >= _fadeDuration + _fadeDelay)
				{
					return true;
				}

				return false;
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Render(Batcher batcher, Camera camera)
			{
				batcher.Draw(_sprite, Position, _renderColor, _rotation, _origin, _scale, _spriteEffects,
					_layerDepth);
			}
		}


		public override RectangleF Bounds => _bounds;

		public int MaxSpriteInstances
		{
			get => _maxSpriteInstances;
			set => SetMaxSpriteInstances(value);
		}

		/// <summary>
		/// how far does the Sprite have to move before a new instance is spawned
		/// </summary>
		public float MinDistanceBetweenInstances = 30f;

		/// <summary>
		/// total duration of the fade from initialColor to fadeToColor
		/// </summary>
		public float FadeDuration = 0.8f;

		/// <summary>
		/// delay before starting the color fade
		/// </summary>
		public float FadeDelay = 0.1f;

		/// <summary>
		/// initial color of the trail instances
		/// </summary>
		public Color InitialColor = Color.White;

		/// <summary>
		/// final color that will be lerped to over the course of fadeDuration
		/// </summary>
		public Color FadeToColor = Color.Transparent;

		int _maxSpriteInstances = 15;
		Stack<SpriteTrailInstance> _availableSpriteTrailInstances = new Stack<SpriteTrailInstance>();
		List<SpriteTrailInstance> _liveSpriteTrailInstances = new List<SpriteTrailInstance>(5);
		Vector2 _lastPosition;
		SpriteRenderer _sprite;

		/// <summary>
		/// flag when true it will always add a new instance regardless of the distance check
		/// </summary>
		bool _isFirstInstance;

		/// <summary>
		/// if awaitingDisable all instances are allowed to fade out before the component is disabled
		/// </summary>
		bool _awaitingDisable;


		public SpriteTrail()
		{
		}

		public SpriteTrail(SpriteRenderer sprite)
		{
			_sprite = sprite;
		}


		#region Fluent setters

		public SpriteTrail SetMaxSpriteInstances(int maxSpriteInstances)
		{
			// if our new value is greater than our previous count instantiate the required SpriteTrailInstances
			if (_availableSpriteTrailInstances.Count < maxSpriteInstances)
			{
				var newInstances = _availableSpriteTrailInstances.Count - maxSpriteInstances;
				for (var i = 0; i < newInstances; i++)
					_availableSpriteTrailInstances.Push(new SpriteTrailInstance());
			}

			// if our new value is less than our previous count trim the List
			if (_availableSpriteTrailInstances.Count > maxSpriteInstances)
			{
				var excessInstances = maxSpriteInstances - _availableSpriteTrailInstances.Count;
				for (var i = 0; i < excessInstances; i++)
					_availableSpriteTrailInstances.Pop();
			}

			_maxSpriteInstances = maxSpriteInstances;


			return this;
		}


		public SpriteTrail SetMinDistanceBetweenInstances(float minDistanceBetweenInstances)
		{
			MinDistanceBetweenInstances = minDistanceBetweenInstances;
			return this;
		}


		public SpriteTrail SetFadeDuration(float fadeDuration)
		{
			FadeDuration = fadeDuration;
			return this;
		}


		public SpriteTrail SetFadeDelay(float fadeDelay)
		{
			FadeDelay = fadeDelay;
			return this;
		}


		public SpriteTrail SetInitialColor(Color initialColor)
		{
			InitialColor = initialColor;
			return this;
		}


		public SpriteTrail SetFadeToColor(Color fadeToColor)
		{
			FadeToColor = fadeToColor;
			return this;
		}

		#endregion


		/// <summary>
		/// enables the SpriteTrail
		/// </summary>
		/// <returns>The sprite trail.</returns>
		public SpriteTrail EnableSpriteTrail()
		{
			_awaitingDisable = false;
			_isFirstInstance = true;
			Enabled = true;
			return this;
		}

		/// <summary>
		/// disables the SpriteTrail optionally waiting for the current trail to fade out first
		/// </summary>
		/// <param name="completeCurrentTrail">If set to <c>true</c> complete current trail.</param>
		public void DisableSpriteTrail(bool completeCurrentTrail = true)
		{
			if (completeCurrentTrail)
			{
				_awaitingDisable = true;
			}
			else
			{
				Enabled = false;

				for (var i = 0; i < _liveSpriteTrailInstances.Count; i++)
					_availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
				_liveSpriteTrailInstances.Clear();
			}
		}

		public override void OnAddedToEntity()
		{
			if (_sprite == null)
				_sprite = this.GetComponent<SpriteRenderer>();

			if (_sprite == null)
			{
				Enabled = false;
				return;
			}

			// move the trail behind the Sprite
			LayerDepth = _sprite.LayerDepth + 0.001f;

			// if setMaxSpriteInstances is called it will handle initializing the SpriteTrailInstances so make sure we dont do it twice
			if (_availableSpriteTrailInstances.Count == 0)
			{
				for (var i = 0; i < _maxSpriteInstances; i++)
					_availableSpriteTrailInstances.Push(new SpriteTrailInstance());
			}
		}

		void IUpdatable.Update()
		{
			if (_isFirstInstance)
			{
				_isFirstInstance = false;
				SpawnInstance();
			}
			else
			{
				var distanceMoved = Math.Abs(Vector2.Distance(Entity.Transform.Position + _localOffset, _lastPosition));
				if (distanceMoved >= MinDistanceBetweenInstances)
					SpawnInstance();
			}

			var min = new Vector2(float.MaxValue, float.MaxValue);
			var max = new Vector2(float.MinValue, float.MinValue);

			// update any live instances
			for (var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i--)
			{
				if (_liveSpriteTrailInstances[i].Update())
				{
					_availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
					_liveSpriteTrailInstances.RemoveAt(i);
				}
				else
				{
					// calculate our min/max for the bounds
					Vector2.Min(ref min, ref _liveSpriteTrailInstances[i].Position, out min);
					Vector2.Max(ref max, ref _liveSpriteTrailInstances[i].Position, out max);
				}
			}

			_bounds.Location = min;
			_bounds.Width = max.X - min.X;
			_bounds.Height = max.Y - min.Y;
			_bounds.Inflate(_sprite.Width, _sprite.Height);

			// nothing left to render. disable ourself
			if (_awaitingDisable && _liveSpriteTrailInstances.Count == 0)
				Enabled = false;
		}

		/// <summary>
		/// stores the last position for distance calculations and spawns a new trail instance if there is one available in the stack
		/// </summary>
		void SpawnInstance()
		{
			_lastPosition = _sprite.Entity.Transform.Position + _sprite.LocalOffset;

			if (_awaitingDisable || _availableSpriteTrailInstances.Count == 0)
				return;

			var instance = _availableSpriteTrailInstances.Pop();
			instance.Spawn(_lastPosition, _sprite.Sprite, FadeDuration, FadeDelay, InitialColor, FadeToColor);
			instance.SetSpriteRenderOptions(_sprite.Entity.Transform.Rotation, _sprite.Origin,
				_sprite.Entity.Transform.Scale, _sprite.SpriteEffects, LayerDepth);
			_liveSpriteTrailInstances.Add(instance);
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			for (var i = 0; i < _liveSpriteTrailInstances.Count; i++)
				_liveSpriteTrailInstances[i].Render(batcher, camera);
		}
	}
}