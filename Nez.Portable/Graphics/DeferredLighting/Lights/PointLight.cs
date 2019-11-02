using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// PointLights radiate light in a circle. Note that PointLights are affected by Transform.scale. The Transform.scale.X value is multiplied
	/// by the lights radius when sent to the GPU. It is expected that scale will be linear.
	/// </summary>
	public class PointLight : DeferredLight
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					// the size of the light only uses the x scale value
					var size = Radius * Entity.Transform.Scale.X * 2;
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _radius * Entity.Transform.Scale,
						Vector2.One, 0, size, size);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// "height" above the scene in the z direction
		/// </summary>
		public float ZPosition = 150f;

		/// <summary>
		/// how far does this light reaches
		/// </summary>
		public float Radius => _radius;

		/// <summary>
		/// brightness of the light
		/// </summary>
		public float Intensity = 3f;


		protected float _radius;


		public PointLight()
		{
			SetRadius(400f);
		}


		public PointLight(Color color) : this()
		{
			Color = color;
		}


		public PointLight SetZPosition(float z)
		{
			ZPosition = z;
			return this;
		}


		/// <summary>
		/// how far does this light reach
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public PointLight SetRadius(float radius)
		{
			_radius = radius;
			_areBoundsDirty = true;
			return this;
		}


		/// <summary>
		/// brightness of the light
		/// </summary>
		/// <returns>The intensity.</returns>
		/// <param name="intensity">Intensity.</param>
		public PointLight SetIntensity(float intensity)
		{
			Intensity = intensity;
			return this;
		}


		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		public override void DebugRender(Batcher batcher)
		{
			batcher.DrawCircle(Entity.Transform.Position + _localOffset, Radius * Entity.Transform.Scale.X, Color.DarkOrchid, 2);
		}
	}
}