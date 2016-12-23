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
			public Vector2 position;
			Subtexture _subtexture;
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


			public void spawn( Vector2 position, Subtexture subtexture, float fadeDuration, float fadeDelay, Color initialColor, Color targetColor )
			{
				this.position = position;
				_subtexture = subtexture;

				_initialColor = initialColor;
				_elapsedTime = 0f;

				_fadeDuration = fadeDuration;
				_fadeDelay = fadeDelay;
				_initialColor = initialColor;
				_targetColor = targetColor;
			}


			public void setSpriteRenderOptions( float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, float layerDepth )
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
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public bool update()
			{
				_elapsedTime += Time.deltaTime;
				// fading block
				if( _elapsedTime > _fadeDelay && _elapsedTime < _fadeDuration + _fadeDelay )
				{
					var t = Mathf.map01( _elapsedTime, 0f, _fadeDelay + _fadeDuration );
					ColorExt.lerp( ref _initialColor, ref _targetColor, out _renderColor, t );
				}
				else if( _elapsedTime >= _fadeDuration + _fadeDelay )
				{
					return true;
				}

				return false;
			}


			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public void render( Graphics graphics, Camera camera )
			{
				graphics.batcher.draw( _subtexture, position, _renderColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth );
			}
		}


		public override RectangleF bounds { get { return _bounds; } }

		public int maxSpriteInstances
		{
			get { return _maxSpriteInstances; }
			set { setMaxSpriteInstances( value ); }
		}

		/// <summary>
		/// how far does the Sprite have to move before a new instance is spawned
		/// </summary>
		public float minDistanceBetweenInstances = 30f;

		/// <summary>
		/// total duration of the fade from initialColor to fadeToColor
		/// </summary>
		public float fadeDuration = 0.8f;

		/// <summary>
		/// delay before starting the color fade
		/// </summary>
		public float fadeDelay = 0.1f;

		/// <summary>
		/// initial color of the trail instances
		/// </summary>
		public Color initialColor = Color.White;

		/// <summary>
		/// final color that will be lerped to over the course of fadeDuration
		/// </summary>
		public Color fadeToColor = Color.Transparent;

		int _maxSpriteInstances = 15;
		Stack<SpriteTrailInstance> _availableSpriteTrailInstances = new Stack<SpriteTrailInstance>();
		List<SpriteTrailInstance> _liveSpriteTrailInstances = new List<SpriteTrailInstance>( 5 );
		Vector2 _lastPosition;
		Sprite _sprite;

		/// <summary>
		/// flag when true it will always add a new instance regardless of the distance check
		/// </summary>
		bool _isFirstInstance;

		/// <summary>
		/// if awaitingDisable all instances are allowed to fade out before the component is disabled
		/// </summary>
		bool _awaitingDisable;


		public SpriteTrail()
		{}


		public SpriteTrail( Sprite sprite )
		{
			_sprite = sprite;
		}


		#region Fluent setters

		public SpriteTrail setMaxSpriteInstances( int maxSpriteInstances )
		{
			// if our new value is greater than our previous count instantiate the required SpriteTrailInstances
			if( _availableSpriteTrailInstances.Count < maxSpriteInstances )
			{
				var newInstances = _availableSpriteTrailInstances.Count - maxSpriteInstances;
				for( var i = 0; i < newInstances; i++ )
					_availableSpriteTrailInstances.Push( new SpriteTrailInstance() );
			}

			// if our new value is less than our previous count trim the List
			if( _availableSpriteTrailInstances.Count > maxSpriteInstances )
			{
				var excessInstances = maxSpriteInstances - _availableSpriteTrailInstances.Count;
				for( var i = 0; i < excessInstances; i++ )
					_availableSpriteTrailInstances.Pop();
			}

			_maxSpriteInstances = maxSpriteInstances;


			return this;
		}


		public SpriteTrail setMinDistanceBetweenInstances( float minDistanceBetweenInstances )
		{
			this.minDistanceBetweenInstances = minDistanceBetweenInstances;
			return this;
		}


		public SpriteTrail setFadeDuration( float fadeDuration )
		{
			this.fadeDuration = fadeDuration;
			return this;
		}


		public SpriteTrail setFadeDelay( float fadeDelay )
		{
			this.fadeDelay = fadeDelay;
			return this;
		}


		public SpriteTrail setInitialColor( Color initialColor )
		{
			this.initialColor = initialColor;
			return this;
		}


		public SpriteTrail setFadeToColor( Color fadeToColor )
		{
			this.fadeToColor = fadeToColor;
			return this;
		}

		#endregion


		/// <summary>
		/// enables the SpriteTrail
		/// </summary>
		/// <returns>The sprite trail.</returns>
		public SpriteTrail enableSpriteTrail()
		{
			_awaitingDisable = false;
			_isFirstInstance = true;
			enabled = true;
			return this;
		}


		/// <summary>
		/// disables the SpriteTrail optionally waiting for the current trail to fade out first
		/// </summary>
		/// <param name="completeCurrentTrail">If set to <c>true</c> complete current trail.</param>
		public void disableSpriteTrail( bool completeCurrentTrail = true )
		{
			if( completeCurrentTrail )
			{
				_awaitingDisable = true;
			}
			else
			{
				enabled = false;

				for( var i = 0; i < _liveSpriteTrailInstances.Count; i++ )
					_availableSpriteTrailInstances.Push( _liveSpriteTrailInstances[i] );
				_liveSpriteTrailInstances.Clear();
			}
		}


		public override void onAddedToEntity()
		{
			if( _sprite == null )
				_sprite = this.getComponent<Sprite>();

			Assert.isNotNull( _sprite, "There is no Sprite on this Entity for the SpriteTrail to use" );

			// move the trail behind the Sprite
			layerDepth = _sprite.layerDepth + 0.001f;

			// if setMaxSpriteInstances is called it will handle initializing the SpriteTrailInstances so make sure we dont do it twice
			if( _availableSpriteTrailInstances.Count == 0 )
			{
				for( var i = 0; i < _maxSpriteInstances; i++ )
					_availableSpriteTrailInstances.Push( new SpriteTrailInstance() );
			}
		}


		void IUpdatable.update()
		{
			if( _isFirstInstance )
			{
				_isFirstInstance = false;
				spawnInstance();
			}
			else
			{
				var distanceMoved = Math.Abs( Vector2.Distance( entity.transform.position + _localOffset, _lastPosition ) );
				if( distanceMoved >= minDistanceBetweenInstances )
					spawnInstance();
			}

			var min = new Vector2( float.MaxValue, float.MaxValue );
			var max = new Vector2( float.MinValue, float.MinValue );

			// update any live instances
			for( var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i-- )
			{
				if( _liveSpriteTrailInstances[i].update() )
				{
					_availableSpriteTrailInstances.Push( _liveSpriteTrailInstances[i] );
					_liveSpriteTrailInstances.RemoveAt( i );
				}
				else
				{
					// calculate our min/max for the bounds
					Vector2.Min( ref min, ref _liveSpriteTrailInstances[i].position, out min );
					Vector2.Max( ref max, ref _liveSpriteTrailInstances[i].position, out max );
				}
			}

			_bounds.location = min;
			_bounds.width = max.X - min.X;
			_bounds.height = max.Y - min.Y;
			_bounds.inflate( _sprite.width, _sprite.height );

			// nothing left to render. disable ourself
			if( _awaitingDisable && _liveSpriteTrailInstances.Count == 0 )
				enabled = false;
		}


		/// <summary>
		/// stores the last position for distance calculations and spawns a new trail instance if there is one available in the stack
		/// </summary>
		void spawnInstance()
		{
			_lastPosition = _sprite.entity.transform.position + _sprite.localOffset;

			if( _awaitingDisable || _availableSpriteTrailInstances.Count == 0 )
				return;

			var instance = _availableSpriteTrailInstances.Pop();
			instance.spawn( _lastPosition, _sprite.subtexture, fadeDuration, fadeDelay, initialColor, fadeToColor );
			instance.setSpriteRenderOptions( _sprite.entity.transform.rotation, _sprite.origin, _sprite.entity.transform.scale, _sprite.spriteEffects, layerDepth );
			_liveSpriteTrailInstances.Add( instance );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			for( var i = 0; i < _liveSpriteTrailInstances.Count; i++ )
				_liveSpriteTrailInstances[i].render( graphics, camera );
		}

	}
}

