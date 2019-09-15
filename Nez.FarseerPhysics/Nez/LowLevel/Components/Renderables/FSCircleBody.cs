using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSCircleBody : FSRenderableBody
	{
		public FSCircleBody(Sprite sprite) : base(sprite)
		{
		}


		public FSCircleBody(Texture2D texture) : this(new Sprite(texture))
		{
		}


		public override void Initialize()
		{
			base.Initialize();
			Body.AttachCircle(Sprite.SourceRect.Width / 2, 1);
		}


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			base.OnEntityTransformChanged(comp);
			if (_ignoreTransformChanges)
				return;

			// we only care about scale. base handles pos/rot
			if (comp == Transform.Component.Scale)
				Body.FixtureList[0].Shape.Radius =
					Sprite.SourceRect.Width * Transform.Scale.X * 0.5f * FSConvert.DisplayToSim;
		}
	}
}