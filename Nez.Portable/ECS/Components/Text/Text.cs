using Microsoft.Xna.Framework;
using Nez.Sprites;


namespace Nez
{
	public class TextField : Sprite
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, _origin, Entity.Transform.Scale,
						Entity.Transform.Rotation, _size.X, _size.Y);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
		public string Text
		{
			get => _text;
			set => SetText(value);
		}

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign HorizontalOrigin
		{
			get => _horizontalAlign;
			set => SetHorizontalAlign(value);
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign VerticalOrigin
		{
			get => _verticalAlign;
			set => SetVerticalAlign(value);
		}


		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		protected IFont _font;
		protected string _text;
		Vector2 _size;


		public TextField() : this(Graphics.Instance.BitmapFont, "", Vector2.Zero, Color.White)
		{
		}

		public TextField(IFont font, string text, Vector2 localOffset, Color color)
		{
			_font = font;
			_text = text;
			_localOffset = localOffset;
			this.Color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			UpdateSize();
		}


		#region Fluent setters

		public TextField SetFont(IFont font)
		{
			_font = font;
			UpdateSize();

			return this;
		}

		public TextField SetText(string text)
		{
			_text = text;
			UpdateSize();
			UpdateCentering();

			return this;
		}

		public TextField SetHorizontalAlign(HorizontalAlign hAlign)
		{
			_horizontalAlign = hAlign;
			UpdateCentering();

			return this;
		}

		public TextField SetVerticalAlign(VerticalAlign vAlign)
		{
			_verticalAlign = vAlign;
			UpdateCentering();

			return this;
		}

		#endregion


		void UpdateSize()
		{
			_size = _font.MeasureString(_text);
			UpdateCentering();
		}

		void UpdateCentering()
		{
			var oldOrigin = _origin;

			if (_horizontalAlign == HorizontalAlign.Left)
				oldOrigin.X = 0;
			else if (_horizontalAlign == HorizontalAlign.Center)
				oldOrigin.X = _size.X / 2;
			else
				oldOrigin.X = _size.X;

			if (_verticalAlign == VerticalAlign.Top)
				oldOrigin.Y = 0;
			else if (_verticalAlign == VerticalAlign.Center)
				oldOrigin.Y = _size.Y / 2;
			else
				oldOrigin.Y = _size.Y;

			Origin = new Vector2((int) oldOrigin.X, (int) oldOrigin.Y);
		}

		public override void Render(Graphics graphics, Camera camera)
		{
			graphics.Batcher.DrawString(_font, _text, Entity.Transform.Position + _localOffset, Color,
				Entity.Transform.Rotation, Origin, Entity.Transform.Scale, SpriteEffects, LayerDepth);
		}
	}
}