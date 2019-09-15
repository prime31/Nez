using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class SpriteAlphaTestEffect : Effect
	{
		public enum AlphaTestCompareFunction
		{
			Greater,
			LessThan,
			Always,
			Never
		}

		/// <summary>
		/// alpha value used for the comparison. Should be in the 0 - 1 range. Defaults to 0.5f.
		/// </summary>
		/// <value>The reference alpha.</value>
		[Range(0, 1)]
		public float ReferenceAlpha
		{
			get => _referenceAlpha;
			set
			{
				if (_referenceAlpha != value)
				{
					_referenceAlpha = value;
					UpdateEffectParameter();
				}
			}
		}

		public AlphaTestCompareFunction CompareFunction
		{
			get => _compareFunction;
			set
			{
				if (_compareFunction != value)
				{
					_compareFunction = value;
					UpdateEffectParameter();
				}
			}
		}

		float _referenceAlpha = 0.5f;
		AlphaTestCompareFunction _compareFunction = AlphaTestCompareFunction.Greater;

		EffectParameter _alphaTestParam;


		public SpriteAlphaTestEffect() : base(Core.GraphicsDevice, EffectResource.SpriteAlphaTestBytes)
		{
			_alphaTestParam = Parameters["_alphaTest"];
			UpdateEffectParameter();
		}


		void UpdateEffectParameter()
		{
			var value = new Vector3();

			// reference alpha is packed in the x param
			value.X = _referenceAlpha;

			switch (_compareFunction)
			{
				case AlphaTestCompareFunction.Greater:
					value.Y = -1;
					value.Z = 1;
					break;
				case AlphaTestCompareFunction.LessThan:
					value.Y = 1;
					value.Z = -1;
					break;
				case AlphaTestCompareFunction.Always:
					value.Y = 1;
					value.Z = 1;
					break;
				case AlphaTestCompareFunction.Never:
					value.Y = -1;
					value.Z = -1;
					break;
			}

			_alphaTestParam.SetValue(value);
		}
	}
}