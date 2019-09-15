using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class HeatDistortionPostProcessor : PostProcessor
	{
		public float DistortionFactor
		{
			get => _distortionFactor;
			set
			{
				if (_distortionFactor != value)
				{
					_distortionFactor = value;

					if (Effect != null)
						_distortionFactorParam.SetValue(_distortionFactor);
				}
			}
		}

		public float RiseFactor
		{
			get => _riseFactor;
			set
			{
				if (_riseFactor != value)
				{
					_riseFactor = value;

					if (Effect != null)
						_riseFactorParam.SetValue(_riseFactor);
				}
			}
		}

		public Texture2D DistortionTexture
		{
			set => Effect.Parameters["_distortionTexture"].SetValue(value);
		}

		float _distortionFactor = 0.005f;
		float _riseFactor = 0.15f;
		EffectParameter _timeParam;
		EffectParameter _distortionFactorParam;
		EffectParameter _riseFactorParam;


		public HeatDistortionPostProcessor(int executionOrder) : base(executionOrder)
		{
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);
			Effect = _scene.Content.LoadEffect<Effect>("heatDistortion", EffectResource.HeatDistortionBytes);

			_timeParam = Effect.Parameters["_time"];
			_distortionFactorParam = Effect.Parameters["_distortionFactor"];
			_riseFactorParam = Effect.Parameters["_riseFactor"];

			_distortionFactorParam.SetValue(_distortionFactor);
			_riseFactorParam.SetValue(_riseFactor);

			DistortionTexture = scene.Content.Load<Texture2D>("nez/textures/heatDistortionNoise");
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			base.Unload();
		}

		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			_timeParam.SetValue(Time.TotalTime);
			base.Process(source, destination);
		}
	}
}