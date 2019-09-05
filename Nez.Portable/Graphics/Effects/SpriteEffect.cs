using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class SpriteEffect : Effect
	{
		EffectParameter _matrixTransformParam;


		public SpriteEffect() : base(Core.GraphicsDevice, EffectResource.SpriteEffectBytes)
		{
			_matrixTransformParam = Parameters["MatrixTransform"];
		}


		public void SetMatrixTransform(ref Matrix matrixTransform)
		{
			_matrixTransformParam.SetValue(matrixTransform);
		}
	}
}