using System;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class BMFontText : RenderableComponent
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
		BitmapFont _font;
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


		public BMFontText( BitmapFont font, string text, Vector2 position, Color color, HorizontalAlign horizontalAlign = HorizontalAlign.Center, VerticalAlign verticalAlign = VerticalAlign.Top )
		{
			_font = font;
			_text = text;
			this.position = position;
			this.color = color;
			_horizontalAlign = horizontalAlign;
			_verticalAlign = verticalAlign;

			updateSize();
		}


		public BitmapFont font
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
			_size = font.getStringRectangle( text, Vector2.Zero ).Size.ToVector2();
			updateCentering();
		}


		void updateCentering()
		{
			var oldOrigin = _origin;

			if( _horizontalAlign == HorizontalAlign.Left )
				oldOrigin.X = 0;
			else if( _horizontalAlign == HorizontalAlign.Center )
				oldOrigin.X = _size.X / 2;
			else
				oldOrigin.X = _size.X;

			if( _verticalAlign == VerticalAlign.Top )
				oldOrigin.Y = 0;
			else if( _verticalAlign == VerticalAlign.Center )
				oldOrigin.Y = _size.Y / 2;
			else
				oldOrigin.Y = _size.Y;

			origin = new Vector2( (int)oldOrigin.X, (int)oldOrigin.Y );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.spriteBatch.drawString( font, text, renderPosition, color );
		}

	}
}

