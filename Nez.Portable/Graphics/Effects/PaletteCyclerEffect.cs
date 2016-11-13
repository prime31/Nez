using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// retro palette swap/cycle effect. If cycleSpeed is 0 (the default) this works as a plain old palette swap. It should be used with
	/// a grayscale texture and a paletteTexture that has a 1 pixel height.
	/// </summary>
	public class PaletteCyclerEffect : Effect
	{
		/// <summary>
		/// palette lookup texture. Should be a 1D texture with a height of 1 pixel
		/// </summary>
		/// <value>The palette texture.</value>
		public Texture2D paletteTexture { set { _paletteTextureParam.SetValue( value ); } }

		/// <summary>
		/// gets or sets the cycle speed
		/// </summary>
		/// <value>The cycle speed.</value>
		public float cycleSpeed
		{
			get { return _cycleSpeed; }
			set
			{
				_cycleSpeedParam.SetValue( value );
				_cycleSpeed = value;
			}
		}


		float _cycleSpeed;
		EffectParameter _paletteTextureParam;
		EffectParameter _cycleSpeedParam;
		EffectParameter _timeParam;


		public PaletteCyclerEffect() : base( Core.graphicsDevice, EffectResource.paletteCyclerBytes )
		{
			_paletteTextureParam = Parameters["_paletteTexture"];
			_cycleSpeedParam = Parameters["_cycleSpeed"];
			_timeParam = Parameters["_time"];
		}


		/// <summary>
		/// updates the _time param of the shader if cycleSpeed != 0
		/// </summary>
		/// <returns>The time.</returns>
		public void updateTime()
		{
			if( _cycleSpeed != 0 )
				_timeParam.SetValue( Time.time );
		}

	}
}

