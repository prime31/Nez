using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class ReflectionEffect : Effect
	{
		/// <summary>
		/// 0 - 1 range. Intensity of the reflection where 0 is none and 1 is full reflected
		/// </summary>
		/// <value>The reflection intensity.</value>
		public float reflectionIntensity
		{
			get { return _reflectionIntensity; }
			set
			{
				if( _reflectionIntensity != value )
				{
					_reflectionIntensity = value;
					_reflectionIntensityParam.SetValue( _reflectionIntensity );
				}
			}
		}

		/// <summary>
		/// magnitude of the normal map contribution to the UV offset of the sampled RenderTarget. Default is 0.05. Very small numbers work best.
		/// </summary>
		/// <value>The normal magnitude.</value>
		public float normalMagnitude
		{
			get { return _normalMagnitude; }
			set
			{
				if( _normalMagnitude != value )
				{
					_normalMagnitude = value;
					_normalMagnitudeParam.SetValue( _normalMagnitude );
				}
			}
		}

		/// <summary>
		/// optinal normal map used to displace/refract the UV of the sampled RenderTarget.
		/// </summary>
		/// <value>The normal map.</value>
		public Texture2D normalMap { set { _normalMapParam.SetValue( value ); } }

		public RenderTarget2D renderTexture { set { _renderTextureParam.SetValue( value ); } }

		public Matrix matrixTransform { set { _matrixTransformParam.SetValue( value ); } }


		float _reflectionIntensity = 0.4f;
		float _normalMagnitude = 0.05f;

		EffectParameter _reflectionIntensityParam;
		EffectParameter _renderTextureParam;
		EffectParameter _normalMapParam;
		EffectParameter _matrixTransformParam;
		EffectParameter _normalMagnitudeParam;


		public ReflectionEffect() : base( Core.graphicsDevice, EffectResource.reflectionBytes )
		{
			_reflectionIntensityParam = Parameters["_reflectionIntensity"];
			_reflectionIntensityParam.SetValue( _reflectionIntensity );
			_renderTextureParam = Parameters["_renderTexture"];
			_normalMapParam = Parameters["_normalMap"];
			_matrixTransformParam = Parameters["_matrixTransform"];
			_normalMagnitudeParam = Parameters["_normalMagnitude"];
			_normalMagnitudeParam.SetValue( _normalMagnitude );
		}

	}
}

