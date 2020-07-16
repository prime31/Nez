using Nez.Sprites;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// renders a sprite with an outline in a very inefficient (but simple) way. The sprite is rendered multiple times offset/colored then it
	/// is rendered normally on top of that.
	/// </summary>
	public class SpriteOutlineRenderer : RenderableComponent
	{
		public override float Width => _sprite.Width + OutlineWidth * 2;
		public override float Height => _sprite.Height + OutlineWidth * 2;

		public override RectangleF Bounds => _sprite.Bounds;

		/// <summary>
		/// the width of the outline
		/// </summary>
		public int OutlineWidth = 3;

		/// <summary>
		/// the color the sprite will be tinted when it is rendered
		/// </summary>
		public Color OutlineColor = Color.Black;

		SpriteRenderer _sprite;


		/// <summary>
		/// the Sprite passed in will be disabled. The SpriteOutlineRenderer will handle manually calling its render method.
		/// </summary>
		/// <param name="sprite">Sprite.</param>
		public SpriteOutlineRenderer(SpriteRenderer sprite)
		{
			_sprite = sprite;

			// RenderableComponent doesnt have an origin so we copy over the Sprite.origin to our localOffset
			_localOffset = sprite.Origin;
			_sprite.Enabled = false;
		}

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			base.OnEntityTransformChanged(comp);

			// our sprite is disabled so we need to forward the call over to it so it can update its bounds for rendering
			_sprite.OnEntityTransformChanged(comp);
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			_sprite.DrawOutline(batcher, camera, OutlineColor, OutlineWidth);
			_sprite.Render(batcher, camera);
		}
	}
}