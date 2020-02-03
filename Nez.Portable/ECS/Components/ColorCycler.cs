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
		public Colorchannels ColorChannel = Colorchannels.All;
		public WaveFunctions WaveFunction = WaveFunctions.Sin;

		/// <summary>
		/// This value is added to the final result. 0 - 1 range.
		/// </summary>
		public float Offset = 0.0f;

		/// <summary>
		/// this value is multiplied by the calculated value
		/// </summary>
		public float Amplitude = 1.0f;

		/// <summary>
		/// start point in wave function. 0 - 1 range.
		/// </summary>
		public float Phase = 0.0f;

		/// <summary>
		/// cycles per second
		/// </summary>
		public float Frequency = 0.5f;

		// should the alpha be changed as well as colors
		public bool AffectsIntensity = true;

		// cache original values
		RenderableComponent _spriteRenderer;
		Color originalColor;
		float originalIntensity;


		public override void OnAddedToEntity()
		{
			_spriteRenderer = Entity.GetComponent<RenderableComponent>();
			originalColor = _spriteRenderer.Color;
			originalIntensity = originalColor.A;
		}


		void IUpdatable.Update()
		{
			var color = _spriteRenderer.Color;

			switch (ColorChannel)
			{
				case Colorchannels.All:
					color = originalColor * EvaluateWaveFunction();
					break;
				case Colorchannels.Red:
					color = new Color((int) (originalColor.R * EvaluateWaveFunction()), color.G, color.B, color.A);
					break;
				case Colorchannels.Green:
					color = new Color(color.R, (int) (originalColor.G * EvaluateWaveFunction()), color.B, color.A);
					break;
				case Colorchannels.Blue:
					color = new Color(color.R, color.G, (int) (originalColor.B * EvaluateWaveFunction()), color.A);
					break;
			}

			if (AffectsIntensity)
				color.A = (byte) (originalIntensity * EvaluateWaveFunction());
			else
				color.A = originalColor.A;

			_spriteRenderer.Color = color;
		}


		float EvaluateWaveFunction()
		{
			var t = (Time.TotalTime + Phase) * Frequency;
			t = t - Mathf.Floor(t); // normalized value (0..1)
			var y = 1f;

			switch (WaveFunction)
			{
				case WaveFunctions.Sin:
					y = Mathf.Sin(1f * t * MathHelper.Pi);
					break;
				case WaveFunctions.Triangle:
					if (t < 0.5f)
						y = 4.0f * t - 1.0f;
					else
						y = -4.0f * t + 3.0f;
					break;
				case WaveFunctions.Square:
					if (t < 0.5f)
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
					y = 1f - (Random.NextFloat() * 2f);
					break;
			}

			return (y * Amplitude) + Offset;
		}
	}
}