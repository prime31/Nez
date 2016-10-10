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
			Vector2 _position;
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
				_position = position;
				this._subtexture = subtexture;

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
				graphics.batcher.draw( _subtexture, _position, _subtexture.sourceRect, _renderColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth );
			}
		}


		public override float width { get { return _sprite.width; } }

		public override float height { get { return _sprite.height; } }

		public override RectangleF bounds { get { return _sprite.bounds; } }

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

		Stack<SpriteTrailInstance> _availableSpriteTrailInstances;
		List<SpriteTrailInstance> _liveSpriteTrailInstances;
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


		public SpriteTrail( Sprite sprite, int maxInstances = 15 )
		{
			// we want to store the sprite and move ourself before the Sprite in render order
			_sprite = sprite;
			layerDepth = _sprite.layerDepth + 0.001f;

			_liveSpriteTrailInstances = new List<SpriteTrailInstance>( 5 );
			_availableSpriteTrailInstances = new Stack<SpriteTrailInstance>( maxInstances );
			for( var i = 0; i < maxInstances; i++ )
				_availableSpriteTrailInstances.Push( new SpriteTrailInstance() );
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

			// update any live instances
			for( var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i-- )
			{
				if( _liveSpriteTrailInstances[i].update() )
				{
					_availableSpriteTrailInstances.Push( _liveSpriteTrailInstances[i] );
					_liveSpriteTrailInstances.RemoveAt( i );
				}
			}

			// nothing left to render. disable ourself
			if( _awaitingDisable && _liveSpriteTrailInstances.Count == 0 )
				enabled = false;
		}


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

