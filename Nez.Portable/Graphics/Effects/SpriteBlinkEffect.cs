using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class SpriteBlinkEffect : Effect
	{
		/// <summary>
		/// color to blink the sprite. When the blinkColor has an alpha of 1 only the blink color will be shown. An alpha of 0 will result in
		/// just the sprite being displayed. Any value in between 0 and 1 will interpolate between the two colors.
		/// </summary>
		/// <value>The color of the blink.</value>
		public Color BlinkColor
		{
			get => new Color(_blinkColor);
			set
			{
				var blinkVec = value.ToVector4();
				if (_blinkColor != blinkVec)
				{
					_blinkColor = blinkVec;
					_blinkColorParam.SetValue(_blinkColor);
				}
			}
		}

		Vector4 _blinkColor = new Vector4(1, 1, 1, 0);
		EffectParameter _blinkColorParam;


		public SpriteBlinkEffect() : base(Core.GraphicsDevice, EffectResource.SpriteBlinkEffectBytes)
		{
			_blinkColorParam = Parameters["_blinkColor"];
			_blinkColorParam.SetValue(_blinkColor);
		}
	}
}