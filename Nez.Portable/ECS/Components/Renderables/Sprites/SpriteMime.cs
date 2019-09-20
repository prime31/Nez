namespace Nez.Sprites
{
	/// <summary>
	/// this component will draw the same frame of spriteToMime every frame. The only difference in rendering is that SpriteMime uses its own
	/// localOffset and color. This allows you to use it for the purpose of shadows (by offsetting via localPosition) or silhouettes (with a
	/// Material that has a stencil read).
	/// </summary>
	public class SpriteMime : RenderableComponent
	{
		public override float Width => _spriteToMime.Width;
		public override float Height => _spriteToMime.Height;
		public override RectangleF Bounds => _spriteToMime.Bounds;

		SpriteRenderer _spriteToMime;


		public SpriteMime()
		{
			Enabled = false;
		}

		public SpriteMime(SpriteRenderer spriteToMime)
		{
			_spriteToMime = spriteToMime;
		}

		public override void OnAddedToEntity()
		{
			if (_spriteToMime == null)
				_spriteToMime = this.GetComponent<SpriteRenderer>();
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			batcher.Draw(_spriteToMime.Sprite, Entity.Transform.Position + _localOffset, Color,
				Entity.Transform.Rotation, _spriteToMime.Origin, Entity.Transform.Scale, _spriteToMime.SpriteEffects,
				_layerDepth);
		}
	}
}