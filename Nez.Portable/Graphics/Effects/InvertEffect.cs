using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class InvertEffect : Effect
	{
		public InvertEffect() : base(Core.GraphicsDevice, EffectResource.InvertBytes)
		{
		}
	}
}