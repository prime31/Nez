using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// AreaLights work like DirLights except they only affect a specific area specified by the width/height. Note that Transform.scale
	/// will affect the size of an AreaLight.
	/// </summary>
	public class AreaLight : DeferredLight
	{
		public override float Width => _areaWidth;
		public override float Height => _areaHeight;

		/// <summary>
		/// direction of the light
		/// </summary>
		public Vector3 Direction = new Vector3(500, 500, 50);

		/// <summary>
		/// brightness of the light
		/// </summary>
		public float Intensity = 12f;


		float _areaWidth, _areaHeight;

		public AreaLight() : this(200, 200)
		{
		}

		public AreaLight(float width, float height)
		{
			SetWidth(width).SetHeight(height);
		}

		public AreaLight SetWidth(float width)
		{
			_areaWidth = width;
			_areBoundsDirty = true;
			return this;
		}

		public AreaLight SetHeight(float height)
		{
			_areaHeight = height;
			_areBoundsDirty = true;
			return this;
		}

		public AreaLight SetDirection(Vector3 direction)
		{
			Direction = direction;
			return this;
		}

		public AreaLight SetIntensity(float intensity)
		{
			Intensity = intensity;
			return this;
		}
	}
}