using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
    public class ColorVignettePostProcessor : PostProcessor
    {
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    _colorParam.SetValue(_color.ToVector4());
                }
            }
        }

        [Range(0.001f, 10f, 0.001f)]
        public float Power
        {
            get => _power;
            set
            {
                if (_power != value)
                {
                    _power = value;

                    if (Effect != null)
                        _powerParam.SetValue(_power);
                }
            }
        }

        [Range(0.001f, 10f, 0.001f)]
        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;

                    if (Effect != null)
                        _radiusParam.SetValue(_radius);
                }
            }
        }

        float _power = 1f;
        float _radius = 1.25f;
        Color _color = Color.Black;
        EffectParameter _powerParam;
        EffectParameter _radiusParam;
        EffectParameter _colorParam;


        public ColorVignettePostProcessor(int executionOrder) : base(executionOrder)
        {
        }

        public override void OnAddedToScene(Scene scene)
        {
            base.OnAddedToScene(scene);

            Effect = scene.Content.LoadEffect<Effect>("color-vignette", EffectResource.ColorVignetteBytes);

            _powerParam = Effect.Parameters["_power"];
            _radiusParam = Effect.Parameters["_radius"];
            _colorParam = Effect.Parameters["_color"];
            _powerParam.SetValue(_power);
            _radiusParam.SetValue(_radius);
            _colorParam.SetValue(_color.ToVector4());
        }

        public override void Unload()
        {
            _scene.Content.UnloadEffect(Effect);
            base.Unload();
        }
    }
}