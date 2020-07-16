using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NineSliceSpriteRenderer : SpriteRenderer
	{
		public new float Width
		{
			get => _finalRenderRect.Width;
			set
			{
				_finalRenderRect.Width = (int)value;
				_destRectsDirty = true;
			}
		}

		public new float Height
		{
			get => _finalRenderRect.Height;
			set
			{
				_finalRenderRect.Height = (int)value;
				_destRectsDirty = true;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.Location = Entity.Transform.Position + _localOffset;
					_bounds.Width = Width;
					_bounds.Height = Height;
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public new NinePatchSprite Sprite;


		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;

		Rectangle[] _destRects = new Rectangle[9];
		bool _destRectsDirty = true;


		public NineSliceSpriteRenderer(NinePatchSprite sprite) : base(sprite)
		{
			Sprite = sprite;
		}

		public NineSliceSpriteRenderer(Sprite sprite, int top, int bottom, int left, int right) : this(
			new NinePatchSprite(sprite, left, right, top, bottom))
		{ }

		public NineSliceSpriteRenderer(Texture2D texture, int top, int bottom, int left, int right) : this(
			new NinePatchSprite(texture, left, right, top, bottom))
		{ }

		public override void Render(Batcher batcher, Camera camera)
		{
			if (_destRectsDirty)
			{
				Sprite.GenerateNinePatchRects(_finalRenderRect, _destRects, Sprite.Left, Sprite.Right, Sprite.Top, Sprite.Bottom);
				_destRectsDirty = false;
			}

			var pos = (Entity.Transform.Position + _localOffset).ToPoint();

			for (var i = 0; i < 9; i++)
			{
				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += pos.X;
				dest.Y += pos.Y;
				batcher.Draw(Sprite, dest, Sprite.NinePatchRects[i], Color);
			}
		}
	}
}