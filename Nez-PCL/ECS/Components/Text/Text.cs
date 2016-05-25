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


		public override float width
		{
			get { return _size.X; }
		}

		public override float height
		{
			get { return _size.Y; }
		}

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
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

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign horizontalOrigin
		{
			get { return _horizontalAlign; }
			set
			{
				_horizontalAlign = value;
				updateCentering();
			}
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign verticalOrigin
		{
			get { return _verticalAlign; }
			set
			{
				_verticalAlign = value;
				updateCentering();
			}
		}


		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		protected IFont _font;
		protected string _text;
		Vector2 _size;


		public Text( IFont font, string text, Vector2 position, Color color )
		{
			_font = font;
			_text = text;
			_localOffset = position;
			this.color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			updateSize();
		}


		#region Fluent setters

		public Text setFont( IFont font )
		{
			_font = font;
			updateSize();

			return this;
		}


		public Text setText( string text )
		{
			_text = text;
			updateSize();
			updateCentering();

			return this;
		}


		public Text setHorizontalAlign( HorizontalAlign hAlign )
		{
			_horizontalAlign = hAlign;
			updateCentering();

			return this;
		}


		public Text setVerticalAlign( VerticalAlign vAlign )
		{
			_verticalAlign = vAlign;
			updateCentering();

			return this;
		}

		#endregion


		void updateSize()
		{
			_size = _font.measureString( _text );
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
			graphics.batcher.drawString( _font, _text, entity.transform.position + _localOffset, color, entity.transform.rotation, origin, entity.transform.scale, spriteEffects, layerDepth );
		}

	}
}

