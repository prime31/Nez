using System;
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
		public float referenceAlpha
		{
			get { return _referenceAlpha; }
			set
			{
				if( _referenceAlpha != value )
				{
					_referenceAlpha = value;
					updateEffectParameter();
				}
			}
		}

		public AlphaTestCompareFunction compareFunction
		{
			get { return _compareFunction; }
			set
			{
				if( _compareFunction != value )
				{
					_compareFunction = value;
					updateEffectParameter();
				}
			}
		}

		float _referenceAlpha = 0.5f;
		AlphaTestCompareFunction _compareFunction = AlphaTestCompareFunction.Greater;

		EffectParameter _alphaTestParam;


		public SpriteAlphaTestEffect() : base( Core.graphicsDevice, EffectResource.spriteAlphaTestBytes )
		{
			_alphaTestParam = Parameters["_alphaTest"];
			updateEffectParameter();
		}


		void updateEffectParameter()
		{
			var value = new Vector3();

			// reference alpha is packed in the x param
			value.X = _referenceAlpha;

			switch( _compareFunction )
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

			_alphaTestParam.SetValue( value );
		}
	}
}

