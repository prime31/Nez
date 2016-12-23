using Microsoft.Xna.Framework;


namespace Nez
{
	public enum Colorchannels
	{
		None,
		All,
		Red,
		Green,
		Blue
	}


	public enum WaveFunctions
	{
		Sin,
		Triangle,
		Square,
		SawTooth,
		IntertedSawTooth,
		Random
	}


	/// <summary>
	/// takes a RenderableComponent and cycles the color using different wave forms. A specific color channel can be affected or all of them.
	/// Useful for making flickering lights and adding atmosphere.
	/// </summary>
	public class ColorCycler : Component, IUpdatable
	{
		public Colorchannels colorChannel = Colorchannels.All;
		public WaveFunctions waveFunction = WaveFunctions.Sin;

		/// <summary>
		/// This value is added to the final result. 0 - 1 range.
		/// </summary>
		public float offset = 0.0f;

		/// <summary>
		/// this value is multiplied by the calculated value
		/// </summary>
		public float amplitude = 1.0f;

		/// <summary>
		/// start point in wave function. 0 - 1 range.
		/// </summary>
		public float phase = 0.0f;

		/// <summary>
		/// cycles per second
		/// </summary>
		public float frequency = 0.5f;

		// should the alpha be changed as well as colors
		public bool affectsIntensity = true;

		// cache original values
		RenderableComponent _spriteRenderer;
		Color originalColor;
		float originalIntensity;


		public override void onAddedToEntity()
		{
			_spriteRenderer = entity.getComponent<RenderableComponent>();
			originalColor = _spriteRenderer.color;
			originalIntensity = originalColor.A;
		}



		void IUpdatable.update()
		{
			var color = _spriteRenderer.color;

			switch( colorChannel )
			{
				case Colorchannels.All:
					color = originalColor * evaluateWaveFunction();
					break;
				case Colorchannels.Red:
					color = new Color( originalColor.R * evaluateWaveFunction(), color.G, color.B, color.A );
					break;
				case Colorchannels.Green:
					color = new Color( color.R, originalColor.G * evaluateWaveFunction(), color.B, color.A );
					break;
				case Colorchannels.Blue:
					color = new Color( color.R, color.G, originalColor.B * evaluateWaveFunction(), color.A );
					break;
			}

			if( affectsIntensity )
				color.A = (byte)(originalIntensity * evaluateWaveFunction());
			else
				color.A = originalColor.A;

			_spriteRenderer.color = color;
		}


		float evaluateWaveFunction()
		{
			var t = ( Time.time + phase ) * frequency;
			t = t - Mathf.floor( t ); // normalized value (0..1)
			var y = 1f;

			switch( waveFunction )
			{
				case WaveFunctions.Sin:
					y = Mathf.sin( 1f * t * MathHelper.Pi );
					break;
				case WaveFunctions.Triangle:
					if( t < 0.5f )
						y = 4.0f * t - 1.0f;
					else
						y = -4.0f * t + 3.0f; 
					break;
				case WaveFunctions.Square:
					if( t < 0.5f )
						y = 1.0f;
					else
						y = -1.0f; 
					break;
				case WaveFunctions.SawTooth:
					y = t;
					break;
				case WaveFunctions.IntertedSawTooth:
					y = 1.0f - t;
					break;
				case WaveFunctions.Random:
					y = 1f - ( Random.nextFloat() * 2f );
					break;
			}

			return ( y * amplitude ) + offset;
		}
	
	}
}