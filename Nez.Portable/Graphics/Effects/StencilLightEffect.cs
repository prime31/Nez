using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class StencilLightEffect : Effect
	{
		public Vector2 LightPosition { set { _lightPositionParam.SetValue(value); } }
		public Color Color { set { _lightColorParam.SetValue(value.ToVector3()); } }
		public float Radius { set { _lightRadius.SetValue(value); } }
		public Matrix ViewProjectionMatrix { set { _viewProjectionMatrixParam.SetValue(value); } }

		EffectParameter _lightPositionParam;
		EffectParameter _lightColorParam;
		EffectParameter _lightRadius;
		EffectParameter _viewProjectionMatrixParam;

		public StencilLightEffect() : base(Core.GraphicsDevice, EffectResource.StencilLightBytes)
		{
			_lightPositionParam = Parameters["_lightSource"];
			_lightColorParam = Parameters["_lightColor"];
			_lightRadius = Parameters["_lightRadius"];
			_viewProjectionMatrixParam = Parameters["_viewProjectionMatrix"];
		}
	}
}