using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NoiseEffect : Effect
	{
		/// <summary>
		/// Intensity of the noise. Defaults to 1.
		/// </summary>
		[Range(0, 10)]
		public float Noise
		{
			get => _noise;
			set
			{
				if (_noise != value)
				{
					_noise = value;
					_noiseParam.SetValue(_noise);
				}
			}
		}

		float _noise = 1f;
		EffectParameter _noiseParam;


		public NoiseEffect() : base(Core.GraphicsDevice, EffectResource.NoiseBytes)
		{
			_noiseParam = Parameters["noise"];
			_noiseParam.SetValue(_noise);
		}
	}
}