using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Light that works with the StencilLightRenderer. This uses a texture-less shader with a simple falloff calculation to draw a light.
	/// </summary>
	public class StencilLight : RenderableComponent
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					var scale = MathHelper.Max(Entity.Transform.Scale.X, Entity.Transform.Scale.Y);
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, new Vector2(_radius * scale, _radius * scale),
						Vector2.One, 0, _radius * scale * 2f, _radius * scale * 2f);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Radius of influence of the light
		/// </summary>
		[Range(0, 2000)]
		public float Radius
		{
			get => _radius;
			set => SetRadius(value);
		}

		/// <summary>
		/// Power of the light, from 0 (turned off) to 1 for maximum brightness
		/// </summary>
		[Range(0, 1)]
		public float Power;

		protected float _radius;
		StencilLightEffect _lightEffect;


		public StencilLight() : this(400)
		{ }

		public StencilLight(float radius) : this(radius, Color.White)
		{ }

		public StencilLight(float radius, Color color) : this(radius, color, 1.0f)
		{ }

		public StencilLight(float radius, Color color, float power)
		{
			Radius = radius;
			Power = power;
			Color = color;
		}

		#region Fluent setters

		public virtual StencilLight SetRadius(float radius)
		{
			if (radius != _radius)
			{
				_radius = radius;
				_areBoundsDirty = true;

				if (_lightEffect != null)
					_lightEffect.Radius = radius * MathHelper.Max(Entity.Transform.Scale.X, Entity.Transform.Scale.Y);
			}

			return this;
		}

		public StencilLight SetPower(float power)
		{
			Power = power;
			return this;
		}

		#endregion

		#region Component and RenderableComponent

		public override void OnAddedToEntity()
		{
			_lightEffect = Entity.Scene.Content.LoadNezEffect<StencilLightEffect>();
			_lightEffect.Radius = _radius * MathHelper.Max(Entity.Transform.Scale.X, Entity.Transform.Scale.Y);

			Material = Material.StencilRead(0);
			Material.Effect = _lightEffect;
			Material.BlendState = new BlendState
			{
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.One,
				AlphaSourceBlend = Blend.SourceAlpha,
				AlphaDestinationBlend = Blend.One
			};
		}

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			base.OnEntityTransformChanged(comp);

			// if scale changes update our radius
			if (comp == Transform.Component.Scale)
				_lightEffect.Radius = _radius * MathHelper.Max(Entity.Transform.Scale.X, Entity.Transform.Scale.Y);
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			if (Power > 0 && IsVisibleFromCamera(camera))
			{
				_lightEffect.ViewProjectionMatrix = camera.ViewProjectionMatrix;
				_lightEffect.LightPosition = camera.WorldToScreenPoint(Entity.Transform.Position);
				_lightEffect.Color = Color * Power;

				batcher.DrawRect(Bounds, Color.White);
			}
		}

		public override void DebugRender(Batcher batcher)
		{
			var scale = MathHelper.Max(Entity.Transform.Scale.X, Entity.Transform.Scale.Y);
			batcher.DrawHollowRect(Bounds, Debug.Colors.RenderableBounds);
			batcher.DrawCircle(Transform.Position, _radius * scale, Debug.Colors.RenderableBounds);
		}

		#endregion
	}
}