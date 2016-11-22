using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NoiseEffect : Effect
	{
		/// <summary>
		/// Intensity of the noise. Defaults to 1.
		/// </summary>
		public float noise
		{
			get { return _noise; }
			set
			{
				if( _noise != value )
				{
					_noise = value;
					_noiseParam.SetValue( _noise );
				}
			}
		}

		float _noise = 1f;
		EffectParameter _noiseParam;


		public NoiseEffect() : base( Core.graphicsDevice, EffectResource.noiseBytes )
		{
			_noiseParam = Parameters["noise"];
			_noiseParam.SetValue( _noise );
		}
	}
}

