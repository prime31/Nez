using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// this effect requires that you render it twice. The first time horizontally (prepareForHorizontalBlur) and then
	/// vertically (prepareForVerticalBlur).
	/// </summary>
	public class GaussianBlurEffect : Effect
	{
		/// <summary>
		/// amount to blur. A range of 0.5 - 6 works well. Defaults to 2.
		/// </summary>
		/// <value>The blur amount.</value>
		[Range(0f, 6f, 0.2f)]
		public float BlurAmount
		{
			get => _blurAmount;
			set
			{
				if (_blurAmount != value)
				{
					// avoid 0 which will get is NaNs
					if (value == 0)
						value = 0.001f;

					_blurAmount = value;
					CalculateSampleWeights();
				}
			}
		}

		/// <summary>
		/// horizontal delta for the blur. Typically 1 / backbuffer width
		/// </summary>
		/// <value>The horizontal blur delta.</value>
		[Range(0.0001f, 0.005f, true)]
		public float HorizontalBlurDelta
		{
			get => _horizontalBlurDelta;
			set
			{
				if (value != _horizontalBlurDelta)
				{
					_horizontalBlurDelta = value;
					SetBlurEffectParameters(_horizontalBlurDelta, 0, _horizontalSampleOffsets);
				}
			}
		}

		/// <summary>
		/// vertical delta for the blur. Typically 1 / backbuffer height
		/// </summary>
		/// <value>The vertical blur delta.</value>
		[Range(0.0001f, 0.005f, true)]
		public float VerticalBlurDelta
		{
			get => _verticalBlurDelta;
			set
			{
				if (value != _verticalBlurDelta)
				{
					_verticalBlurDelta = value;
					SetBlurEffectParameters(0, _verticalBlurDelta, _verticalSampleOffsets);
				}
			}
		}

		float _blurAmount = 2f;
		float _horizontalBlurDelta = 0.01f;
		float _verticalBlurDelta = 0.01f;

		int _sampleCount;
		float[] _sampleWeights;
		Vector2[] _verticalSampleOffsets;
		Vector2[] _horizontalSampleOffsets;

		EffectParameter _blurWeightsParam;
		EffectParameter _blurOffsetsParam;


		public GaussianBlurEffect() : base(Core.GraphicsDevice, EffectResource.GaussianBlurBytes)
		{
			_blurWeightsParam = Parameters["_sampleWeights"];
			_blurOffsetsParam = Parameters["_sampleOffsets"];

			// Look up how many samples our gaussian blur effect supports.
			_sampleCount = _blurWeightsParam.Elements.Count;

			// Create temporary arrays for computing our filter settings.
			_sampleWeights = new float[_sampleCount];
			_verticalSampleOffsets = new Vector2[_sampleCount];
			_horizontalSampleOffsets = new Vector2[_sampleCount];

			// The first sample always has a zero offset.
			_verticalSampleOffsets[0] = Vector2.Zero;
			_horizontalSampleOffsets[0] = Vector2.Zero;

			// we can calculate the sample weights just once since they are always the same for horizontal or vertical blur
			CalculateSampleWeights();

			SetBlurEffectParameters(_horizontalBlurDelta, 0, _horizontalSampleOffsets);
			PrepareForHorizontalBlur();
		}

		/// <summary>
		/// prepares the Effect for performing a horizontal blur
		/// </summary>
		public void PrepareForHorizontalBlur()
		{
			_blurOffsetsParam.SetValue(_horizontalSampleOffsets);
		}

		/// <summary>
		/// prepares the Effect for performing a vertical blur
		/// </summary>
		public void PrepareForVerticalBlur()
		{
			_blurOffsetsParam.SetValue(_verticalSampleOffsets);
		}

		/// <summary>
		/// computes sample weightings and texture coordinate offsets for one pass of a separable gaussian blur filter.
		/// </summary>
		void SetBlurEffectParameters(float dx, float dy, Vector2[] offsets)
		{
			// Add pairs of additional sample taps, positioned along a line in both directions from the center.
			for (var i = 0; i < _sampleCount / 2; i++)
			{
				// To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
				// hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
				// will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
				// than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
				var sampleOffset = i * 2 + 1.5f;

				var delta = new Vector2(dx, dy) * sampleOffset;

				// Store texture coordinate offsets for the positive and negative taps.
				offsets[i * 2 + 1] = delta;
				offsets[i * 2 + 2] = -delta;
			}
		}

		/// <summary>
		/// calculates the sample weights and passes them along to the shader
		/// </summary>
		void CalculateSampleWeights()
		{
			// The first sample always has a zero offset.
			_sampleWeights[0] = ComputeGaussian(0);

			// Maintain a sum of all the weighting values.
			var totalWeights = _sampleWeights[0];

			// Add pairs of additional sample taps, positioned along a line in both directions from the center.
			for (var i = 0; i < _sampleCount / 2; i++)
			{
				// Store weights for the positive and negative taps.
				var weight = ComputeGaussian(i + 1);

				_sampleWeights[i * 2 + 1] = weight;
				_sampleWeights[i * 2 + 2] = weight;

				totalWeights += weight * 2;
			}

			// Normalize the list of sample weightings, so they will always sum to one.
			for (var i = 0; i < _sampleWeights.Length; i++)
				_sampleWeights[i] /= totalWeights;

			// Tell the effect about our new filter settings.
			_blurWeightsParam.SetValue(_sampleWeights);
		}

		/// <summary>
		/// Evaluates a single point on the gaussian falloff curve.
		/// Used for setting up the blur filter weightings.
		/// </summary>
		float ComputeGaussian(float n)
		{
			return (float) ((1.0 / Math.Sqrt(2 * Math.PI * _blurAmount)) *
			                Math.Exp(-(n * n) / (2 * _blurAmount * _blurAmount)));
		}
	}
}