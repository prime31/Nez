using Microsoft.Xna.Framework;
using Nez.Sprites;


namespace Nez
{
	public class Text : Sprite
	{
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, _size.X, _size.Y );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
		public string text
		{
			get => _text;
			set => setText( value );
		}

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign horizontalOrigin
		{
			get => _horizontalAlign;
			set => setHorizontalAlign( value );
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign verticalOrigin
		{
			get => _verticalAlign;
			set => setVerticalAlign( value );
		}


		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		protected IFont _font;
		protected string _text;
		Vector2 _size;


		public Text() : this( Graphics.instance.bitmapFont, "", Vector2.Zero, Color.White )
		{}

		public Text( IFont font, string text, Vector2 localOffset, Color color )
		{
			_font = font;
			_text = text;
			_localOffset = localOffset;
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

