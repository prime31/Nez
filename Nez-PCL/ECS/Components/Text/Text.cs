using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;


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
		protected SpriteFont _spriteFont;
		protected BitmapFont _bitmapFont;
		protected string _text;
		Vector2 _size;

		public override float width
		{
			get { return _size.X; }
		}

		public override float height
		{
			get { return _size.Y; }
		}


		public Text( BitmapFont font, string text, Vector2 position, Color color )
		{
			_bitmapFont = font;
			_text = text;
			_localPosition = position;
			this.color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			updateSize();
		}


		public Text( SpriteFont font, string text, Vector2 position, Color color )
		{
			_spriteFont = font;
			_text = text;
			_localPosition = position;
			this.color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			updateSize();
		}


		public SpriteFont spriteFont
		{
			get { return _spriteFont; }
			set
			{
				_spriteFont = value;
				_bitmapFont = null;
				updateSize();
			}
		}

		public BitmapFont bitmapFont
		{
			get { return _bitmapFont; }
			set
			{
				_bitmapFont = value;
				_spriteFont = null;
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
			if( _bitmapFont != null )
				_size = _bitmapFont.measureString( text );
			else
				_size = _spriteFont.MeasureString( text );
			
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
			if( _bitmapFont != null )
				graphics.spriteBatch.DrawString( _bitmapFont, text, renderPosition, color, rotation, origin, scale, spriteEffects, layerDepth );
			else
				graphics.spriteBatch.DrawString( _spriteFont, text, renderPosition, color, rotation, origin, scale, spriteEffects, layerDepth );
		}

	}
}

