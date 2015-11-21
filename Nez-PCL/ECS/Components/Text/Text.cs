using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Text : RenderableComponent
	{
		public enum HorizontalAlign
		{
			Left,
			Center,
			Right
		};


		public enum VerticalAlign
		{
			Top,
			Center,
			Bottom
		};

		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		SpriteFont _font;
		string _text;
		Vector2 _size;

		public override float width
		{
			get { return _size.X; }
		}

		public override float height
		{
			get { return _size.Y; }
		}


		public Text()
		{}


		public Text( SpriteFont font, string text, Vector2 position, Color color, HorizontalAlign horizontalAlign = HorizontalAlign.Center, VerticalAlign verticalAlign = VerticalAlign.Top )
		{
			_font = font;
			_text = text;
			this.position = position;
			this.color = color;
			_horizontalAlign = horizontalAlign;
			_verticalAlign = verticalAlign;

			updateSize();
		}


		public SpriteFont font
		{
			get { return _font; }
			set
			{
				_font = value;
				updateSize();
			}
		}

		public string text
		{
			get { return _text; }
			set
			{
				_text = value;
				updateSize();
				updateCentering();
			}
		}

		public HorizontalAlign horizontalOrigin
		{
			get { return _horizontalAlign; }
			set
			{
				_horizontalAlign = value;
				updateCentering();
			}
		}

		public VerticalAlign verticalOrigin
		{
			get { return _verticalAlign; }
			set
			{
				_verticalAlign = value;
				updateCentering();
			}
		}


		void updateSize()
		{
			_size = font.MeasureString( text );
			updateCentering();
		}


		void updateCentering()
		{
			if( _horizontalAlign == HorizontalAlign.Left )
				origin.X = 0;
			else if( _horizontalAlign == HorizontalAlign.Center )
				origin.X = _size.X / 2;
			else
				origin.X = _size.X;

			if( _verticalAlign == VerticalAlign.Top )
				origin.Y = 0;
			else if( _verticalAlign == VerticalAlign.Center )
				origin.Y = _size.Y / 2;
			else
				origin.Y = _size.Y;

			origin = new Vector2( (int)origin.X, (int)origin.Y );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.spriteBatch.DrawString( font, text, renderPosition, color, rotation, origin, scale, spriteEffects, layerDepth );
		}

	}
}

