using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class HeatDistortionPostProcessor : PostProcessor
	{
		public float distortionFactor
		{
			get { return _distortionFactor; }
			set
			{
				if( _distortionFactor != value )
				{
					_distortionFactor = value;

					if( effect != null )
						_distortionFactorParam.SetValue( _distortionFactor );
				}
			}
		}

		public float riseFactor
		{
			get { return _riseFactor; }
			set
			{
				if( _riseFactor != value )
				{
					_riseFactor = value;

					if( effect != null )
						_riseFactorParam.SetValue( _riseFactor );
				}
			}
		}

		public Texture2D distortionTexture
		{
			set { effect.Parameters["_distortionTexture"].SetValue( value ); }
		}


		float _distortionFactor = 0.005f;
		float _riseFactor = 0.15f;
		EffectParameter _timeParam;
		EffectParameter _distortionFactorParam;
		EffectParameter _riseFactorParam;


		public HeatDistortionPostProcessor( int executionOrder ) : base( executionOrder )
		{}


		public override void onAddedToScene()
		{
			effect = scene.contentManager.loadEffect<Effect>( "heatDistortion", EffectResource.heatDistortionBytes );

			_timeParam = effect.Parameters["_time"];
			_distortionFactorParam = effect.Parameters["_distortionFactor"];
			_riseFactorParam = effect.Parameters["_riseFactor"];

			_distortionFactorParam.SetValue( _distortionFactor );
			_riseFactorParam.SetValue( _riseFactor );

			distortionTexture = scene.contentManager.Load<Texture2D>( "nez/textures/heatDistortionNoise" );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			_timeParam.SetValue( Time.time );
			base.process( source, destination );
		}
	}
}

