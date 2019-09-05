using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NineSliceSprite : Sprite
	{
		public new float Width
		{
			get => _finalRenderRect.Width;
			set
			{
				_finalRenderRect.Width = (int) value;
				_destRectsDirty = true;
			}
		}

		public new float Height
		{
			get => _finalRenderRect.Height;
			set
			{
				_finalRenderRect.Height = (int) value;
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

		public new NinePatchSubtexture Subtexture;


		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;

		Rectangle[] _destRects = new Rectangle[9];
		bool _destRectsDirty = true;


		public NineSliceSprite(NinePatchSubtexture subtexture) : base(subtexture)
		{
			this.Subtexture = subtexture;
		}

		public NineSliceSprite(Subtexture subtexture, int top, int bottom, int left, int right) : this(
			new NinePatchSubtexture(subtexture, left, right, top, bottom))
		{
		}

		public NineSliceSprite(Texture2D texture, int top, int bottom, int left, int right) : this(
			new NinePatchSubtexture(texture, left, right, top, bottom))
		{
		}

		public override void Render(Graphics graphics, Camera camera)
		{
			if (_destRectsDirty)
			{
				Subtexture.GenerateNinePatchRects(_finalRenderRect, _destRects, Subtexture.Left, Subtexture.Right,
					Subtexture.Top, Subtexture.Bottom);
				_destRectsDirty = false;
			}

			var pos = (Entity.Transform.Position + _localOffset).ToPoint();

			for (var i = 0; i < 9; i++)
			{
				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += pos.X;
				dest.Y += pos.Y;
				graphics.Batcher.Draw(Subtexture, dest, Subtexture.NinePatchRects[i], Color);
			}
		}
	}
}