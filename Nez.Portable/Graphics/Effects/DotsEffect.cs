using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class DotsEffect : Effect
	{
		public float Scale
		{
			get => _scale;
			set
			{
				if (_scale != value)
				{
					_scale = value;
					_scaleParam.SetValue(_scale);
				}
			}
		}

		public float Angle
		{
			get => _angle;
			set
			{
				if (_angle != value)
				{
					_angle = value;
					_angleParam.SetValue(_angle);
				}
			}
		}

		float _scale = 0.5f;
		float _angle = 0.5f;

		EffectParameter _scaleParam;
		EffectParameter _angleParam;


		public DotsEffect() : base(Core.GraphicsDevice, EffectResource.DotsBytes)
		{
			_scaleParam = Parameters["scale"];
			_angleParam = Parameters["angle"];

			_scaleParam.SetValue(_scale);
			_angleParam.SetValue(_angle);
		}
	}
}