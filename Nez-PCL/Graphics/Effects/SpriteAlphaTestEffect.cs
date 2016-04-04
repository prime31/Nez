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

		public int referenceAlpha
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
		int _referenceAlpha = 127;


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
			// convert to float so we are in the 0-1 range in the shader
			value.X = (float)_referenceAlpha / 255f;

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

