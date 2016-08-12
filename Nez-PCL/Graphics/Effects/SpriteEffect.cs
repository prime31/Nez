using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class SpriteEffect : Effect
	{
		EffectParameter _matrixTransformParam;


		public SpriteEffect() : base( Core.graphicsDevice, EffectResource.spriteEffectBytes )
		{
			_matrixTransformParam = Parameters["MatrixTransform"];
		}


		public void setMatrixTransform( ref Matrix matrixTransform )
		{
			_matrixTransformParam.SetValue( matrixTransform );
		}

	}
}

