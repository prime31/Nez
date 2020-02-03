namespace Nez
{
	/// <summary>
	/// Class holds all the settings used to tweak the bloom effect.
	/// </summary>
	public class BloomSettings
	{
		// Controls how bright a pixel needs to be before it will bloom.
		// Zero makes everything bloom equally, while higher values select
		// only brighter colors. Somewhere between 0.25 and 0.5 is good.
		public readonly float Threshold;

		// Controls how much blurring is applied to the bloom image.
		// The typical range is from 1 up to 10 or so.
		public readonly float BlurAmount;

		// Controls the amount of the bloom and base images that
		// will be mixed into the final scene. Range 0 to 1.
		public readonly float Intensity;
		public readonly float BaseIntensity;

		// Independently control the color saturation of the bloom and
		// base images. Zero is totally desaturated, 1.0 leaves saturation
		// unchanged, while higher values increase the saturation level.
		public readonly float Saturation;
		public readonly float BaseSaturation;


		/// <summary>
		/// Constructs a new bloom settings descriptor.
		/// </summary>
		public BloomSettings(float bloomThreshold, float blurAmount, float bloomIntensity, float baseIntensity,
		                     float bloomSaturation, float baseSaturation)
		{
			Threshold = bloomThreshold;
			BlurAmount = blurAmount;
			Intensity = bloomIntensity;
			BaseIntensity = baseIntensity;
			Saturation = bloomSaturation;
			BaseSaturation = baseSaturation;
		}

		/// <summary>
		/// Table of preset bloom settings. Note that BaseSat needs to be near 0 if the final render needs transparency!
		/// </summary>
		public static BloomSettings[] PresetSettings =
		{
			//                Thresh  Blur Bloom  Base  BloomSat BaseSat
			new BloomSettings(0.1f, 0.6f, 2f, 1f, 1, 0), // Default
			new BloomSettings(0, 3, 1, 1, 1, 1), // Soft
			new BloomSettings(0.5f, 8, 2, 1, 0, 1), // Desaturated
			new BloomSettings(0.25f, 8, 1.3f, 1, 1, 0), // Saturated
			new BloomSettings(0, 2, 1, 0.1f, 1, 1), // Blurry
			new BloomSettings(0.5f, 2, 1, 1, 1, 1), // Subtle
		};
	}
}