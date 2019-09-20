using Microsoft.Xna.Framework.Graphics;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// effect used to render sprites that take part in deferred lighting. A normal map is required. The normal map can optionally use the alpha
	/// channel for self illumination by setitng useNormalAlphaChannelForSelfIllumination to true. Note that you need to turn off premultiplied
	/// alpha in the Pipeline tool when using the alpha for self illumination!
	/// </summary>
	public class DeferredSpriteEffect : Effect
	{
		/// <summary>
		/// alpha cutoff for the alpha test. defaults to 0.3
		/// </summary>
		/// <value>The alpha cutoff.</value>
		public float AlphaCutoff => _alphaCutoff;

		/// <summary>
		/// if true, the normal map alpha channel will be used for self illumination. Note that you need to turn off premultiplied
		/// alpha in the Pipeline tool when using the alpha for self illumination!
		/// </summary>
		/// <value><c>true</c> if use normal alpha channel for self illumination; otherwise, <c>false</c>.</value>
		public bool UseNormalAlphaChannelForSelfIllumination => _useNormalAlphaChannelForSelfIllumination;

		/// <summary>
		/// controls the power of the self illumination where 0 is no contribution and 1 is fully self illuminated
		/// </summary>
		/// <value>The self illumination power parameter.</value>
		public float SelfIlluminationPower => _selfIlluminationPower;


		float _alphaCutoff;
		bool _useNormalAlphaChannelForSelfIllumination;
		float _selfIlluminationPower;

		EffectParameter _normalMapParam;
		EffectParameter _alphaCutoffParam;
		EffectParameter _alphaAsSelfIlluminationParam;
		EffectParameter _selfIlluminationPowerParam;


		public DeferredSpriteEffect() : base(Core.GraphicsDevice, EffectResource.DeferredSpriteBytes)
		{
			_normalMapParam = Parameters["_normalMap"];
			_alphaCutoffParam = Parameters["_alphaCutoff"];
			_alphaAsSelfIlluminationParam = Parameters["_alphaAsSelfIllumination"];
			_selfIlluminationPowerParam = Parameters["_selfIlluminationPower"];

			SetAlphaCutoff(0.3f);
			SetSelfIlluminationPower(1);
		}


		#region Configuration

		public DeferredSpriteEffect SetNormalMap(Texture2D normalMap)
		{
			_normalMapParam.SetValue(normalMap);
			return this;
		}


		/// <summary>
		/// alpha cutoff for the alpha test. defaults to 0.3
		/// </summary>
		/// <returns>The alpha cutoff.</returns>
		/// <param name="alphaCutoff">Alpha cutoff.</param>
		public DeferredSpriteEffect SetAlphaCutoff(float alphaCutoff)
		{
			if (_alphaCutoff != alphaCutoff)
			{
				_alphaCutoff = alphaCutoff;
				_alphaCutoffParam.SetValue(alphaCutoff);
			}

			return this;
		}


		/// <summary>
		/// if true, the normal map alpha channel will be used for self illumination. Note that you need to turn off premultiplied
		/// alpha in the Pipeline tool when using the alpha for self illumination!
		/// </summary>
		/// <returns>The use normal alpha channel for self illumination.</returns>
		/// <param name="useNormalAlphaChannelForSelfIllumination">If set to <c>true</c> use normal alpha channel for self illumination.</param>
		public DeferredSpriteEffect SetUseNormalAlphaChannelForSelfIllumination(
			bool useNormalAlphaChannelForSelfIllumination)
		{
			if (_useNormalAlphaChannelForSelfIllumination != useNormalAlphaChannelForSelfIllumination)
			{
				_useNormalAlphaChannelForSelfIllumination = useNormalAlphaChannelForSelfIllumination;
				_alphaAsSelfIlluminationParam.SetValue(useNormalAlphaChannelForSelfIllumination ? 1f : 0f);
			}

			return this;
		}


		/// <summary>
		/// controls the power of the self illumination where 0 is no contribution and 1 is fully self illuminated
		/// </summary>
		/// <returns>The self illumination power.</returns>
		/// <param name="selfIlluminationPower">Self illumination power.</param>
		public DeferredSpriteEffect SetSelfIlluminationPower(float selfIlluminationPower)
		{
			if (_selfIlluminationPower != selfIlluminationPower)
			{
				_selfIlluminationPower = selfIlluminationPower;
				_selfIlluminationPowerParam.SetValue(selfIlluminationPower);
			}

			return this;
		}

		#endregion
	}
}