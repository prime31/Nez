using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;


namespace Nez.Audio
{
	/// <summary>
	/// plays a collection of SoundEffects randomly and optionally with random pitch
	/// </summary>
	public class AudioSource
	{
		List<SoundEffect> _soundEffects = new List<SoundEffect>();
		bool _useRandomPitch;
		float _pitchMin, _pitchMax;
		bool _useRandomPan;
		float _panMin, _panMax;


		/// <summary>
		/// if a pitch range is set every time play is called a random pitch will be used
		/// </summary>
		/// <returns>The pitch range.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public AudioSource setPitchRange( float min, float max )
		{
			_pitchMin = Mathf.clamp( min, -1, 1 );
			_pitchMax = Mathf.clamp( max, -1, 1 );
			_useRandomPitch = _pitchMin != 0 || _pitchMax != 0;

			return this;
		}


		/// <summary>
		/// if a pan range is set every time play is called a random pan will be used
		/// </summary>
		/// <returns>The pan range.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public AudioSource setPanRange( float min, float max )
		{
			_panMin = Mathf.clamp( min, -1, 1 );
			_panMax = Mathf.clamp( max, -1, 1 );
			_useRandomPan = _panMin != 0 || _panMax != 0;

			return this;
		}


		/// <summary>
		/// adds a SoundEffect to the AudioSource
		/// </summary>
		/// <returns>The sound effect.</returns>
		/// <param name="effect">Effect.</param>
		public AudioSource addSoundEffect( SoundEffect effect )
		{
			_soundEffects.Add( effect );
			return this;
		}


		public bool play()
		{
			if( _useRandomPitch || _useRandomPan )
				return _soundEffects.randomItem().Play( 1, Random.range( _pitchMin, _pitchMax ), Random.range( _panMin, _panMax ) );
			else
				return _soundEffects.randomItem().Play();
		}


		public void play( float volume, float pitch, float pan = 0 )
		{
			_soundEffects.randomItem().Play( volume, pitch, pan );
		}


		public SoundEffectInstance createInstance()
		{
			return _soundEffects.randomItem().CreateInstance();
		}

	}
}

