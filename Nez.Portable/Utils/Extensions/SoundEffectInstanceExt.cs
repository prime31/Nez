#if FNA
using System.Reflection;
using Microsoft.Xna.Framework.Audio;


namespace Nez
{
	public static class SoundEffectInstanceExt
	{
		static MethodInfo _applyLowPassFilterMethod;
		static MethodInfo _applyHighPassFilterMethod;
		static MethodInfo _applyBandPassFilterMethod;
		static readonly object[] _gainContainer = { 1.0f };
		static readonly object[] _bandPassContainer = { 1.0f, 1.0f };


		/// <summary>
		/// applies a low pass filter to the SoundEffectInstance
		/// </summary>
		/// <returns>The low pass filter.</returns>
		/// <param name="self">Self.</param>
		/// <param name="hfGain">Hf gain.</param>
		public static void applyLowPassFilter( this SoundEffectInstance self, float hfGain )
		{
			if( _applyLowPassFilterMethod == null )
				_applyLowPassFilterMethod =
 self.GetType().GetMethod( "INTERNAL_applyLowPassFilter", BindingFlags.NonPublic | BindingFlags.Instance );

			_gainContainer[0] = hfGain;
			_applyLowPassFilterMethod.Invoke( self, _gainContainer );
		}


		/// <summary>
		/// applies a high pass filter to the SoundEffectInstance
		/// </summary>
		/// <returns>The high pass filter.</returns>
		/// <param name="self">Self.</param>
		/// <param name="lfGain">Lf gain.</param>
		public static void applyHighPassFilter( this SoundEffectInstance self, float lfGain )
		{
			if( _applyHighPassFilterMethod == null )
				_applyHighPassFilterMethod =
 self.GetType().GetMethod( "INTERNAL_applyHighPassFilter", BindingFlags.NonPublic | BindingFlags.Instance );

			_gainContainer[0] = lfGain;
			_applyHighPassFilterMethod.Invoke( self, _gainContainer );
		}


		/// <summary>
		/// applies a bandpass filter to the SoundEffectInstance
		/// </summary>
		/// <returns>The band pass filter.</returns>
		/// <param name="self">Self.</param>
		/// <param name="hfGain">Hf gain.</param>
		/// <param name="lfGain">Lf gain.</param>
		public static void applyBandPassFilter( this SoundEffectInstance self, float hfGain, float lfGain )
		{
			if( _applyBandPassFilterMethod == null )
				_applyBandPassFilterMethod =
 self.GetType().GetMethod( "INTERNAL_applyBandPassFilter", BindingFlags.NonPublic | BindingFlags.Instance );

			_bandPassContainer[0] = hfGain;
			_bandPassContainer[1] = lfGain;
			_applyBandPassFilterMethod.Invoke( self, _bandPassContainer );
		}

	}
}
#endif