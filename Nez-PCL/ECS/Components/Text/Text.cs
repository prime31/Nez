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
		protected NezSpriteFont _spriteFont;
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
			_localOffset = position;
			this.color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			updateSize();
		}


		public Text( NezSpriteFont font, string text, Vector2 position, Color color )
		{
			_spriteFont = font;
			_text = text;
			_localOffset = position;
			this.color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			updateSize();
		}


		public NezSpriteFont spriteFont
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
				_size = _spriteFont.measureString( text );
			
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
				graphics.batcher.drawString( _bitmapFont, text, entity.transform.position + _localOffset, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, layerDepth );
			else
				graphics.batcher.drawString( _spriteFont, text, entity.transform.position + _localOffset, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, layerDepth );
		}

	}
}

