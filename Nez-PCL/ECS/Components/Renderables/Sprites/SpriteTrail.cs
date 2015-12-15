using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using System.Collections.Generic;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Sprites
{
	public class SpriteTrail : RenderableComponent
	{
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


			public void render( Graphics graphics, Camera camera )
			{
				graphics.spriteBatch.Draw( _subtexture, _position, _subtexture.sourceRect, _renderColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth );
			}
		}


		public override float width
		{
			get { return _sprite.width; }
		}

		public override float height
		{
			get { return _sprite.height; }
		}
			
		public float minDistanceBetweenInstances = 30f;
		public float fadeDuration = 0.8f;
		public float fadeDelay = 0.1f;
		public Color initialColor = Color.White;
		public Color fadeToColor = Color.TransparentBlack;

		Stack<SpriteTrailInstance> _availableSpriteTrailInstances;
		List<SpriteTrailInstance> _liveSpriteTrailInstances;
		Vector2 _lastPosition;
		Sprite _sprite;
		bool _isFirstInstance;
		bool _awaitingDisable;


		public SpriteTrail( Sprite sprite, int maxInstances = 15 )
		{
			// we want to store the sprite and move ourself before the Sprite in render order
			_sprite = sprite;
			origin = _sprite.origin;
			layerDepth = _sprite.layerDepth + 0.001f;

			_liveSpriteTrailInstances = new List<SpriteTrailInstance>( 5 );
			_availableSpriteTrailInstances = new Stack<SpriteTrailInstance>( maxInstances );
			for( var i = 0; i < maxInstances; i++ )
				_availableSpriteTrailInstances.Push( new SpriteTrailInstance() );
		}


		public override void update()
		{
			if( _isFirstInstance )
			{
				_isFirstInstance = false;
				spawnInstance();
			}
			else
			{
				var distanceMoved = Math.Abs( Vector2.Distance( renderPosition, _lastPosition ) );
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


		public override void render( Graphics graphics, Camera camera )
		{
			for( var i = 0; i < _liveSpriteTrailInstances.Count; i++ )
				_liveSpriteTrailInstances[i].render( graphics, camera );
		}


		public void enableSpriteTrail()
		{
			_awaitingDisable = false;
			_isFirstInstance = true;
			enabled = true;
		}


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


		void spawnInstance()
		{
			_lastPosition = _sprite.renderPosition;

			if( _awaitingDisable || _availableSpriteTrailInstances.Count == 0 )
				return;

			var instance = _availableSpriteTrailInstances.Pop();
			instance.spawn( _lastPosition, _sprite.subtexture, fadeDuration, fadeDelay, initialColor, fadeToColor );
			instance.setSpriteRenderOptions( _sprite.rotation, _sprite.origin, _sprite.scale, _sprite.spriteEffects, layerDepth );
			_liveSpriteTrailInstances.Add( instance );
		}

	}
}

