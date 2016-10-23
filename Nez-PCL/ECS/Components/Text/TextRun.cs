using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;

namespace Nez
{
	/// <summary>
	/// provides a cached run of text for super fast text drawing. Note that this is only appropriate for text that doesnt change often
	/// and doesnt move.
	/// </summary>
	public class TextRun
	{
		struct CharDetails
		{
			public Texture2D texture;
			public Vector3[] verts;
			public Vector2[] texCoords;
			public Color color;

			public void initialize()
			{
				verts = new Vector3[4];
				texCoords = new Vector2[4];
			}
		}

		public float width { get { return _size.X; } }
		public float height { get { return _size.Y; } }
		public Vector2 origin { get { return _origin; } }
		public float rotation;
		public Vector2 position;

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
		public string text
		{
			get { return _text; }
			set { setText( value ); }
		}

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign horizontalOrigin
		{
			get { return _horizontalAlign; }
			set { setHorizontalAlign( value ); }
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign verticalOrigin
		{
			get { return _verticalAlign; }
			set { setVerticalAlign( value ); }
		}


		HorizontalAlign _horizontalAlign;
		VerticalAlign _verticalAlign;
		BitmapFont _font;
		string _text;
		Vector2 _size;
		Color _color = Color.White;
		Vector2 _origin;
		Vector2 _scale = Vector2.One;
		CharDetails[] _charDetails;

		static readonly float[] _cornerOffsetX = { 0.0f, 1.0f, 0.0f, 1.0f };
		static readonly float[] _cornerOffsetY = { 0.0f, 0.0f, 1.0f, 1.0f };


		public TextRun( BitmapFont font )
		{
			_font = font;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;
		}


		#region Fluent setters

		public TextRun setFont( BitmapFont font )
		{
			_font = font;
			updateSize();
			return this;
		}


		public TextRun setText( string text )
		{
			_text = text;
			updateSize();
			updateCentering();
			return this;
		}


		public TextRun setHorizontalAlign( HorizontalAlign hAlign )
		{
			_horizontalAlign = hAlign;
			updateCentering();
			return this;
		}


		public TextRun setVerticalAlign( VerticalAlign vAlign )
		{
			_verticalAlign = vAlign;
			updateCentering();
			return this;
		}

		#endregion


		void updateSize()
		{
			_size = _font.measureString( _text ) * _scale;
			updateCentering();
		}


		void updateCentering()
		{
			var newOrigin = Vector2.Zero;

			if( _horizontalAlign == HorizontalAlign.Left )
				newOrigin.X = 0;
			else if( _horizontalAlign == HorizontalAlign.Center )
				newOrigin.X = _size.X / 2;
			else
				newOrigin.X = _size.X;

			if( _verticalAlign == VerticalAlign.Top )
				newOrigin.Y = 0;
			else if( _verticalAlign == VerticalAlign.Center )
				newOrigin.Y = _size.Y / 2;
			else
				newOrigin.Y = _size.Y;

			_origin = new Vector2( (int)( newOrigin.X * _scale.X ), (int)( newOrigin.Y * _scale.Y ) );
		}


		/// <summary>
		/// compiles the text into raw verts/texture coordinates. This method must be called anytime text or any other properties are
		/// changed.
		/// </summary>
		public void compile()
		{
			_charDetails = new CharDetails[_text.Length];
			BitmapFontRegion currentFontRegion = null;
			var effects = (byte)SpriteEffects.None;

			var _transformationMatrix = Matrix2D.identity;
			var requiresTransformation = rotation != 0f || _scale != Vector2.One;
			if( requiresTransformation )
			{
				Matrix2D temp;
				Matrix2D.createTranslation( -_origin.X, -_origin.Y, out _transformationMatrix );
				Matrix2D.createScale( _scale.X, _scale.Y, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createRotation( rotation, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
				Matrix2D.createTranslation( position.X, position.Y, out temp );
				Matrix2D.multiply( ref _transformationMatrix, ref temp, out _transformationMatrix );
			}

			var offset = requiresTransformation ? Vector2.Zero : position - _origin;

			for( var i = 0; i < _text.Length; ++i )
			{
				_charDetails[i].initialize();
				_charDetails[i].color = _color;

				var c = _text[i];
				if( c == '\n' )
				{
					offset.X = requiresTransformation ? 0f : position.X - _origin.X;
					offset.Y += _font.lineHeight;
					currentFontRegion = null;
					continue;
				}

				if( currentFontRegion != null )
					offset.X += _font.spacing + currentFontRegion.xAdvance;

				currentFontRegion = _font.fontRegionForChar( c, true );
				var p = offset;
				p.X += currentFontRegion.xOffset;
				p.Y += currentFontRegion.yOffset;

				// transform our point if we need to
				if( requiresTransformation )
					Vector2Ext.transform( ref p, ref _transformationMatrix, out p );

				var destination = new Vector4( p.X, p.Y, currentFontRegion.width * _scale.X, currentFontRegion.height * _scale.Y );
				_charDetails[i].texture = currentFontRegion.subtexture.texture2D;


				// Batcher calculations
				var sourceRectangle = currentFontRegion.subtexture.sourceRect;
				float sourceX, sourceY, sourceW, sourceH;
				var destW = destination.Z;
				var destH = destination.W;

				// calculate uvs
				var inverseTexW = 1.0f / (float)currentFontRegion.subtexture.texture2D.Width;
				var inverseTexH = 1.0f / (float)currentFontRegion.subtexture.texture2D.Height;

				sourceX = sourceRectangle.X * inverseTexW;
				sourceY = sourceRectangle.Y * inverseTexH;
				sourceW = Math.Max( sourceRectangle.Width, float.Epsilon ) * inverseTexW;
				sourceH = Math.Max( sourceRectangle.Height, float.Epsilon ) * inverseTexH;

				// Rotation Calculations
				float rotationMatrix1X;
				float rotationMatrix1Y;
				float rotationMatrix2X;
				float rotationMatrix2Y;
				if( !Mathf.withinEpsilon( rotation, 0.0f ) )
				{
					var sin = Mathf.sin( rotation );
					var cos = Mathf.cos( rotation );
					rotationMatrix1X = cos;
					rotationMatrix1Y = sin;
					rotationMatrix2X = -sin;
					rotationMatrix2Y = cos;
				}
				else
				{
					rotationMatrix1X = 1.0f;
					rotationMatrix1Y = 0.0f;
					rotationMatrix2X = 0.0f;
					rotationMatrix2Y = 1.0f;
				}

				// Calculate vertices, finally.
				// top-left
				_charDetails[i].verts[0].X = rotationMatrix2X + rotationMatrix1X + destination.X - 1;
				_charDetails[i].verts[0].Y = rotationMatrix2Y + rotationMatrix1Y + destination.Y - 1;

				// top-right
				var cornerX = _cornerOffsetX[1] * destW;
				var cornerY = _cornerOffsetY[1] * destH;
				_charDetails[i].verts[1].X = (
					( rotationMatrix2X * cornerY ) +
					( rotationMatrix1X * cornerX ) +
					destination.X
				);
				_charDetails[i].verts[1].Y = (
					( rotationMatrix2Y * cornerY ) +
					( rotationMatrix1Y * cornerX ) +
					destination.Y
				);

				// bottom-left
				cornerX = _cornerOffsetX[2] * destW;
				cornerY = _cornerOffsetY[2] * destH;
				_charDetails[i].verts[2].X = (
					( rotationMatrix2X * cornerY ) +
					( rotationMatrix1X * cornerX ) +
					destination.X
				);
				_charDetails[i].verts[2].Y = (
					( rotationMatrix2Y * cornerY ) +
					( rotationMatrix1Y * cornerX ) +
					destination.Y
				);

				// bottom-right
				cornerX = _cornerOffsetX[3] * destW;
				cornerY = _cornerOffsetY[3] * destH;
				_charDetails[i].verts[3].X = (
					( rotationMatrix2X * cornerY ) +
					( rotationMatrix1X * cornerX ) +
					destination.X
				);
				_charDetails[i].verts[3].Y = (
					( rotationMatrix2Y * cornerY ) +
					( rotationMatrix1Y * cornerX ) +
					destination.Y
				);


				// texture coordintes
				_charDetails[i].texCoords[0].X = ( _cornerOffsetX[0 ^ effects] * sourceW ) + sourceX;
				_charDetails[i].texCoords[0].Y = ( _cornerOffsetY[0 ^ effects] * sourceH ) + sourceY;
				_charDetails[i].texCoords[1].X = ( _cornerOffsetX[1 ^ effects] * sourceW ) + sourceX;
				_charDetails[i].texCoords[1].Y = ( _cornerOffsetY[1 ^ effects] * sourceH ) + sourceY;
				_charDetails[i].texCoords[2].X = ( _cornerOffsetX[2 ^ effects] * sourceW ) + sourceX;
				_charDetails[i].texCoords[2].Y = ( _cornerOffsetY[2 ^ effects] * sourceH ) + sourceY;
				_charDetails[i].texCoords[3].X = ( _cornerOffsetX[3 ^ effects] * sourceW ) + sourceX;
				_charDetails[i].texCoords[3].Y = ( _cornerOffsetY[3 ^ effects] * sourceH ) + sourceY;
			}
		}


		public void render( Graphics graphics )
		{
			for( var i = 0; i < _charDetails.Length; i++ )
				graphics.batcher.drawRaw( _charDetails[i].texture, _charDetails[i].verts, _charDetails[i].texCoords, _charDetails[i].color );
		}

	}
}

