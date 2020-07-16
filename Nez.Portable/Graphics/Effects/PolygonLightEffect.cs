using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class PolygonLightEffect : Effect
	{
		public Matrix ViewProjectionMatrix { set { _viewProjectionMatrixParam.SetValue(value); } }
		public Vector2 LightSource { set { _lightSourceParam.SetValue(value); } }
		public Vector3 LightColor { set { _lightColorParam.SetValue(value); } }
		public float LightRadius { set { _lightRadius.SetValue(value); } }

		EffectParameter _viewProjectionMatrixParam;
		EffectParameter _lightSourceParam;
		EffectParameter _lightColorParam;
		EffectParameter _lightRadius;

		public PolygonLightEffect() : base(Core.GraphicsDevice, EffectResource.PolygonLightBytes)
		{
			_viewProjectionMatrixParam = Parameters["viewProjectionMatrix"];
			_lightSourceParam = Parameters["lightSource"];
			_lightColorParam = Parameters["lightColor"];
			_lightRadius = Parameters["lightRadius"];
		}
	}
}