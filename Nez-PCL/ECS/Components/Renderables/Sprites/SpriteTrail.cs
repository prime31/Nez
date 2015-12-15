using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using System.Collections.Generic;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	// TODO: if Sprite<T> class was separated into a Sprite class with rendering stuff and Sprite<T> subclass with animation this class
	// could then take in a Sprite instance and deal with spawning automatically.
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
				// fading block
				if( _elapsedTime > _fadeDelay && _elapsedTime < _fadeDuration + _fadeDelay )
				{
					var t = Mathf.map01( _elapsedTime + _fadeDelay, 0f, _fadeDelay + _fadeDuration );
					ColorExt.lerp( ref _initialColor, ref _targetColor, out _renderColor, t );
				}
				else if( _elapsedTime > _fadeDuration + _fadeDelay )
				{
					return true;
				}

				_elapsedTime += Time.deltaTime;

				return false;
			}


			public void render( Graphics graphics, Camera camera )
			{
				graphics.spriteBatch.Draw( _subtexture, _position, _subtexture.sourceRect, _renderColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth );
			}
		}


		public override float width
		{
			get { return 0; }
		}

		public override float height
		{
			get { return 0; }
		}
			
		public float minDistanceBetweenInstances = 30f;
		public float fadeDuration = 0.8f;
		public float fadeDelay = 0.1f;
		public Color initialColor = Color.White;
		public Color fadeToColor = Color.TransparentBlack;

		Stack<SpriteTrailInstance> _availableSpriteTrailInstances;
		List<SpriteTrailInstance> _liveSpriteTrailInstances;
		bool _movedFarEnough;
		Vector2 _lastPosition;


		public SpriteTrail( int maxInstances = 15 )
		{
			_liveSpriteTrailInstances = new List<SpriteTrailInstance>( 5 );
			_availableSpriteTrailInstances = new Stack<SpriteTrailInstance>( maxInstances );
			for( var i = 0; i < maxInstances; i++ )
				_availableSpriteTrailInstances.Push( new SpriteTrailInstance() );
		}


		public override void update()
		{
			var distanceMoved = Math.Abs( Vector2.Distance( renderPosition, _lastPosition ) );
			if( distanceMoved >= minDistanceBetweenInstances )
				_movedFarEnough = true;

			// update any live instances
			for( var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i-- )
			{
				if( _liveSpriteTrailInstances[i].update() )
				{
					_availableSpriteTrailInstances.Push( _liveSpriteTrailInstances[i] );
					_liveSpriteTrailInstances.RemoveAt( i );
				}
			}

			if( _liveSpriteTrailInstances.Count == 0 )
				enabled = false;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			for( var i = 0; i < _liveSpriteTrailInstances.Count; i++ )
				_liveSpriteTrailInstances[i].render( graphics, camera );
		}


		void spawnInstance( Subtexture subtexture, float rotation, Vector2 origin, Vector2 scale, SpriteEffects spriteEffects, float layerDepth )
		{
			if( !_movedFarEnough || _availableSpriteTrailInstances.Count == 0 )
				return;

			_lastPosition = renderPosition;
			_movedFarEnough = false;

			var instance = _availableSpriteTrailInstances.Pop();
			instance.spawn( renderPosition, subtexture, fadeDuration, fadeDelay, initialColor, fadeToColor );
			instance.setSpriteRenderOptions( rotation, origin, scale, spriteEffects, layerDepth );
			_liveSpriteTrailInstances.Add( instance );
		}

	}
}

