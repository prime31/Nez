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
			public Texture2D Texture;
			public Vector3[] Verts;
			public Vector2[] TexCoords;
			public Color Color;

			public void Initialize()
			{
				Verts = new Vector3[4];
				TexCoords = new Vector2[4];
			}
		}

		public float Width => _size.X;

		public float Height => _size.Y;

		public Vector2 Origin => _origin;

		public float Rotation;
		public Vector2 Position;

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


		HorizontalAlign _horizontalAlign;
		VerticalAlign _verticalAlign;
		BitmapFont _font;
		string _text;
		Vector2 _size;
		Color _color = Color.White;
		Vector2 _origin;
		Vector2 _scale = Vector2.One;
		CharDetails[] _charDetails;

		static readonly float[] _cornerOffsetX = {0.0f, 1.0f, 0.0f, 1.0f};
		static readonly float[] _cornerOffsetY = {0.0f, 0.0f, 1.0f, 1.0f};


		public TextRun(BitmapFont font)
		{
			_font = font;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;
		}


		#region Fluent setters

		public TextRun SetFont(BitmapFont font)
		{
			_font = font;
			UpdateSize();
			return this;
		}


		public TextRun SetText(string text)
		{
			_text = text;
			UpdateSize();
			UpdateCentering();
			return this;
		}


		public TextRun SetHorizontalAlign(HorizontalAlign hAlign)
		{
			_horizontalAlign = hAlign;
			UpdateCentering();
			return this;
		}


		public TextRun SetVerticalAlign(VerticalAlign vAlign)
		{
			_verticalAlign = vAlign;
			UpdateCentering();
			return this;
		}

		#endregion


		void UpdateSize()
		{
			_size = _font.MeasureString(_text) * _scale;
			UpdateCentering();
		}


		void UpdateCentering()
		{
			var newOrigin = Vector2.Zero;

			if (_horizontalAlign == HorizontalAlign.Left)
				newOrigin.X = 0;
			else if (_horizontalAlign == HorizontalAlign.Center)
				newOrigin.X = _size.X / 2;
			else
				newOrigin.X = _size.X;

			if (_verticalAlign == VerticalAlign.Top)
				newOrigin.Y = 0;
			else if (_verticalAlign == VerticalAlign.Center)
				newOrigin.Y = _size.Y / 2;
			else
				newOrigin.Y = _size.Y;

			_origin = new Vector2((int) (newOrigin.X * _scale.X), (int) (newOrigin.Y * _scale.Y));
		}


		/// <summary>
		/// compiles the text into raw verts/texture coordinates. This method must be called anytime text or any other properties are
		/// changed.
		/// </summary>
		public void Compile()
		{
			_charDetails = new CharDetails[_text.Length];
			Character currentCharacter = null;
			var effects = (byte) SpriteEffects.None;

			var _transformationMatrix = Matrix2D.Identity;
			var requiresTransformation = Rotation != 0f || _scale != Vector2.One;
			if (requiresTransformation)
			{
				Matrix2D temp;
				Matrix2D.CreateTranslation(-_origin.X, -_origin.Y, out _transformationMatrix);
				Matrix2D.CreateScale(_scale.X, _scale.Y, out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
				Matrix2D.CreateRotation(Rotation, out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
				Matrix2D.CreateTranslation(Position.X, Position.Y, out temp);
				Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
			}

			var offset = requiresTransformation ? Vector2.Zero : Position - _origin;

			for (var i = 0; i < _text.Length; ++i)
			{
				_charDetails[i].Initialize();
				_charDetails[i].Color = _color;

				var c = _text[i];
				if (c == '\n')
				{
					offset.X = requiresTransformation ? 0f : Position.X - _origin.X;
					offset.Y += _font.LineHeight;
					currentCharacter = null;
					continue;
				}

				if (currentCharacter != null)
					offset.X += _font.Spacing.X + currentCharacter.XAdvance;

				currentCharacter = _font[c];
				var p = offset;
				p.X += currentCharacter.Offset.X;
				p.Y += currentCharacter.Offset.Y;

				// transform our point if we need to
				if (requiresTransformation)
					Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

				var destination = new Vector4(p.X, p.Y, currentCharacter.Bounds.Width * _scale.X,
					currentCharacter.Bounds.Height * _scale.Y);
				_charDetails[i].Texture = _font.Textures[_font[currentCharacter.Char].TexturePage];

				//_charDetails[i].texture = currentCharacter.sprite.texture2D;


				// Batcher calculations
				var sourceRectangle = currentCharacter.Bounds;
				float sourceX, sourceY, sourceW, sourceH;
				var destW = destination.Z;
				var destH = destination.W;

				// calculate uvs
				var inverseTexW = 1.0f / (float) currentCharacter.Bounds.Width;
				var inverseTexH = 1.0f / (float) currentCharacter.Bounds.Height;

				sourceX = sourceRectangle.X * inverseTexW;
				sourceY = sourceRectangle.Y * inverseTexH;
				sourceW = Math.Max(sourceRectangle.Width, float.Epsilon) * inverseTexW;
				sourceH = Math.Max(sourceRectangle.Height, float.Epsilon) * inverseTexH;

				// Rotation Calculations
				float rotationMatrix1X;
				float rotationMatrix1Y;
				float rotationMatrix2X;
				float rotationMatrix2Y;
				if (!Mathf.WithinEpsilon(Rotation))
				{
					var sin = Mathf.Sin(Rotation);
					var cos = Mathf.Cos(Rotation);
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
				_charDetails[i].Verts[0].X = rotationMatrix2X + rotationMatrix1X + destination.X - 1;
				_charDetails[i].Verts[0].Y = rotationMatrix2Y + rotationMatrix1Y + destination.Y - 1;

				// top-right
				var cornerX = _cornerOffsetX[1] * destW;
				var cornerY = _cornerOffsetY[1] * destH;
				_charDetails[i].Verts[1].X = (
					(rotationMatrix2X * cornerY) +
					(rotationMatrix1X * cornerX) +
					destination.X
				);
				_charDetails[i].Verts[1].Y = (
					(rotationMatrix2Y * cornerY) +
					(rotationMatrix1Y * cornerX) +
					destination.Y
				);

				// bottom-left
				cornerX = _cornerOffsetX[2] * destW;
				cornerY = _cornerOffsetY[2] * destH;
				_charDetails[i].Verts[2].X = (
					(rotationMatrix2X * cornerY) +
					(rotationMatrix1X * cornerX) +
					destination.X
				);
				_charDetails[i].Verts[2].Y = (
					(rotationMatrix2Y * cornerY) +
					(rotationMatrix1Y * cornerX) +
					destination.Y
				);

				// bottom-right
				cornerX = _cornerOffsetX[3] * destW;
				cornerY = _cornerOffsetY[3] * destH;
				_charDetails[i].Verts[3].X = (
					(rotationMatrix2X * cornerY) +
					(rotationMatrix1X * cornerX) +
					destination.X
				);
				_charDetails[i].Verts[3].Y = (
					(rotationMatrix2Y * cornerY) +
					(rotationMatrix1Y * cornerX) +
					destination.Y
				);


				// texture coordintes
				_charDetails[i].TexCoords[0].X = (_cornerOffsetX[0 ^ effects] * sourceW) + sourceX;
				_charDetails[i].TexCoords[0].Y = (_cornerOffsetY[0 ^ effects] * sourceH) + sourceY;
				_charDetails[i].TexCoords[1].X = (_cornerOffsetX[1 ^ effects] * sourceW) + sourceX;
				_charDetails[i].TexCoords[1].Y = (_cornerOffsetY[1 ^ effects] * sourceH) + sourceY;
				_charDetails[i].TexCoords[2].X = (_cornerOffsetX[2 ^ effects] * sourceW) + sourceX;
				_charDetails[i].TexCoords[2].Y = (_cornerOffsetY[2 ^ effects] * sourceH) + sourceY;
				_charDetails[i].TexCoords[3].X = (_cornerOffsetX[3 ^ effects] * sourceW) + sourceX;
				_charDetails[i].TexCoords[3].Y = (_cornerOffsetY[3 ^ effects] * sourceH) + sourceY;
			}
		}


		public void Render(Batcher batcher)
		{
			for (var i = 0; i < _charDetails.Length; i++)
				batcher.DrawRaw(_charDetails[i].Texture, _charDetails[i].Verts, _charDetails[i].TexCoords,
					_charDetails[i].Color);
		}
	}
}