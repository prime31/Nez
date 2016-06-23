using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class SpriteEffect : Effect
	{
		public Matrix matrixTransform { set { _matrixTransformParam.SetValue( value ); } }

		EffectParameter _matrixTransformParam;


		public SpriteEffect() : base( Core.graphicsDevice, EffectResource.spriteEffectBytes )
		{
			_matrixTransformParam = Parameters["MatrixTransform"];
		}

	}
}

