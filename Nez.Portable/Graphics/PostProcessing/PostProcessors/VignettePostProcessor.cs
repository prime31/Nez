using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class VignettePostProcessor : PostProcessor
	{
		[Range( 0.001f, 10f, 0.001f )]
		public float power
		{
			get { return _power; }
			set
			{
				if( _power != value )
				{
					_power = value;

					if( effect != null )
						_powerParam.SetValue( _power );
				}
			}
		}

		[Range( 0.001f, 10f, 0.001f )]
		public float radius
		{
			get { return _radius; }
			set
			{
				if( _radius != value )
				{
					_radius = value;

					if( effect != null )
						_radiusParam.SetValue( _radius );
				}
			}
		}

		float _power = 1f;
		float _radius = 1.25f;
		EffectParameter _powerParam;
		EffectParameter _radiusParam;


		public VignettePostProcessor( int executionOrder ) : base( executionOrder )
		{}

		public override void onAddedToScene( Scene scene )
		{
			base.onAddedToScene( scene );

			effect = scene.content.loadEffect<Effect>( "vignette", EffectResource.vignetteBytes );

			_powerParam = effect.Parameters["_power"];
			_radiusParam = effect.Parameters["_radius"];
			_powerParam.SetValue( _power );
			_radiusParam.SetValue( _radius );
		}

		public override void unload()
		{
			_scene.content.unloadEffect( effect );
			base.unload();
		}

	}
}

