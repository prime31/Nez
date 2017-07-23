using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// MarkupText lets you set fonts, textures and conditionals and provide some XML text to render that uses them. You must first
	/// set the fonts, textures and conditionals before you can use them in your markup.
	/// 
	/// <![CDATA[
	/// <markuptext face="RegularFont" color="#ffffff" align="left">
	/// 	<font face="Large" color="#00ff00" scale="3.2,3.2">Lorem Ipsum</font>
	/// 	<p align="right">Some more text <text color='#ff9900'>with color</text> in the middle</p>
	/// 	<p align="center">Images are inlined too <img src="texture" scale="2,2" /></p>
	/// 	<if condition="isTrue"><img src="otherTexture" scale="0.2,0.2" /></if>
	/// 	<p>conditions can be negated as well <if condition="!isTrue">isTrue isnt true<else>isTrue is true</else></if></p>
	/// </markuptext>
	/// ]]>
	/// </summary>
	public class MarkupText : RenderableComponent
	{
		public override float width { get { return _textWidth; } }
		public override float height { get { return _textHeight; } }

		public string text { get { return _text; } }
		public float textWidth { get { return _textWidth; } }

		string _text;
		float _textWidth, _textHeight;

		List<ICompiledElement> _compiledMarkup;
		Dictionary<string,IFont> _fontDict = new Dictionary<string,IFont>();
		Dictionary<string,Texture2D> _textureDict = new Dictionary<string,Texture2D>();
		Dictionary<string,bool> _conditionalDict = new Dictionary<string,bool>();


		public MarkupText()
		{
			setFont( "default", Graphics.instance.bitmapFont );
		}


		public MarkupText( float textWidth ) : this()
		{
			_textWidth = textWidth;
		}


		/// <summary>
		/// sets the text used for the run. Text should be valid XML.
		/// </summary>
		/// <returns>The text.</returns>
		/// <param name="text">Text.</param>
		public MarkupText setText( string text )
		{
			_text = text;
			return this;
		}


		/// <summary>
		/// sets the width that the run will fill
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="textWidth">Width.</param>
		public MarkupText setTextWidth( float textWidth )
		{
			_textWidth = textWidth;
			return this;
		}


		/// <summary>
		/// sets a font that can be used in a text tag via the font attribute
		/// </summary>
		/// <returns>The font.</returns>
		/// <param name="name">Name.</param>
		/// <param name="font">Font.</param>
		public MarkupText setFont( string name, IFont font )
		{
			_fontDict[name] = font;
			return this;
		}


		/// <summary>
		/// sets a texture which can be used in an img tag via the source attribute
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="name">Name.</param>
		/// <param name="texture">Texture.</param>
		public MarkupText setTexture( string name, Texture2D texture )
		{
			_textureDict[name] = texture;
			return this;
		}


		/// <summary>
		/// sets a conditional which can be used in an if tag with a condition attribute. It is also valid to negate the condition by
		/// prepending a ! to the name in the if tag
		/// </summary>
		/// <returns>The conditional.</returns>
		/// <param name="name">Name.</param>
		/// <param name="conditional">Conditional.</param>
		public MarkupText setConditional( string name, bool conditional )
		{
			_conditionalDict[name] = conditional;
			return this;
		}


		/// <summary>
		/// renders the MarkupText
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		public override void render( Graphics graphics, Camera camera )
		{
			if( _compiledMarkup == null )
				return;
			
			for( var i = 0; i < _compiledMarkup.Count; i++ )
				_compiledMarkup[i].render( graphics, transform.position + _localOffset );
		}


		/// <summary>
		/// compiles the current text to prepare it for rendering
		/// </summary>
		public void compile()
		{
			if( _compiledMarkup == null )
				_compiledMarkup = new List<ICompiledElement>();
			else
				_compiledMarkup.Clear();

			var reader = XmlReader.Create( new StringReader( _text ) );
			var position = Vector2.Zero;
			var formatingStack = new Stack<FormatInstruction>();
			var conditionalsStack = new Stack<bool>();
			var alignStack = new Stack<HorizontalAlign>();
			var lineBuffer = new List<ICompiledElement>();
			var currentLineHeight = 0f;
			var currentTotalHeight = 0f;
			var currentLineWidth = 0f;

			while( reader.Read() )
			{
				if( reader.NodeType == XmlNodeType.Element )
				{
					switch( reader.Name )
					{
						case "font":
						case "markuptext":
						{
							IFont font;
							var s = reader.GetAttribute( "face" );
							if( !string.IsNullOrEmpty( s ) )
								font = _fontDict[s];
							else if( formatingStack.Count > 0 )
								font = formatingStack.Peek().font;
							else
								font = _fontDict["default"];

							var color = Color.White;
							s = reader.GetAttribute( "color" );
							if( !string.IsNullOrEmpty( s ) )
								color = parseColor( s );
							else if( formatingStack.Count > 0 )
								color = formatingStack.Peek().color;

							var scale = Vector2.One;
							s = reader.GetAttribute( "scale" );
							if( !string.IsNullOrEmpty( s ) )
								scale = parseVector2( s );

							if( reader.Name == "markuptext" )
							{
								var value = reader.GetAttribute( "align" );
								var align = HorizontalAlign.Left;
								Enum.TryParse( value, out align );
								alignStack.Push( align );
							}

							formatingStack.Push( new FormatInstruction( font, color, scale ) );
							break;
						}

						case "if":
						{
							var condition = reader.GetAttribute( "condition" );
							var negateCondition = condition[0] == '!';
							if( negateCondition )
								condition = condition.Substring( 1 );

							var value = _conditionalDict[condition];
							conditionalsStack.Push( negateCondition ? !value : value );
							break;
						}

						case "else":
						{
							var value = conditionalsStack.Pop();
							conditionalsStack.Push( !value );
							break;
						}

						case "p":
						case "br":
						{
							if( lineBuffer.Count > 0 )
							{
								position = wrapLine( position, lineBuffer, alignStack.Peek(), out currentLineHeight );
								currentTotalHeight += currentLineHeight;
							}
							else
							{
								var currentFormatting = formatingStack.Peek();
								position.Y += currentFormatting.font.lineSpacing * currentFormatting.scale.Y;
								currentTotalHeight += currentFormatting.font.lineSpacing * currentFormatting.scale.Y;
							}
							currentLineWidth = 0;

							if( reader.Name == "p" )
							{
								var value = reader.GetAttribute( "align" );
								if( string.IsNullOrEmpty( value ) )
									value = "Left";
								value = char.ToUpper( value[0] ) + value.Substring( 1 );

								HorizontalAlign align;
								if( Enum.TryParse( value, out align ) )
									alignStack.Push( align );
								else
									throw new InvalidOperationException( "Invalid alignemnt: " + value );
							}
							break;
						}

						case "img":
						{
							if( conditionalsStack.Count != 0 && !conditionalsStack.Peek() )
								continue;

							var imgSrc = reader.GetAttribute( "src" );
							var color = Color.White;
							var s = reader.GetAttribute( "color" );
							if( !string.IsNullOrEmpty( s ) )
								color = parseColor( s );

							var scale = Vector2.One;
							s = reader.GetAttribute( "scale" );
							if( !string.IsNullOrEmpty( s ) )
								scale = parseVector2( s );

							var image = _textureDict[imgSrc];
							var imageWidth = image.Width * scale.X;
							if( position.X + imageWidth > _textWidth )
							{
								position = wrapLine( position, lineBuffer, alignStack.Peek(), out currentLineHeight );
								currentTotalHeight += currentLineHeight;
							}

							lineBuffer.Add( new CompiledImageElement( image, color, position, scale ) );
							position.X += imageWidth;
							currentLineWidth += imageWidth;
							break;
						}

						case "nbsp":
						{
							var currentFormatting = formatingStack.Peek();
							var spaceX = currentFormatting.font.measureString( " " ).X * currentFormatting.scale.X;
							if( position.X + spaceX < _textWidth )
							{
								position.X += spaceX;
								currentLineWidth += spaceX;
							}
							break;
						}
					}
				}
				else if( reader.NodeType == XmlNodeType.Text )
				{
					if( conditionalsStack.Count != 0 && !conditionalsStack.Peek() )
						continue;

					var currentFormatting = formatingStack.Peek();
					var re = new Regex( @"\s+" );
					var words = re.Split( reader.Value );
					var spaceX = currentFormatting.font.measureString( " " ).X * currentFormatting.scale.X;

					var wordIndex = 0;
					var wordRunSize = 0f;
					var textRun = new StringBuilder();

					// figure out how many words we can fit on the line and compile them
					while( words.Length > wordIndex )
					{
						var isLastWord = wordIndex == words.Length - 1;
						var word = words[wordIndex];
						var wordSize = currentFormatting.font.measureString( word ) * currentFormatting.scale;

						// make sure the word fits
						var currentWordFits = position.X + wordRunSize + wordSize.X <= _textWidth;

						// if there is only one word and it doesnt fit we are gonna add it anyway else we will have nothing to show
						if( words.Length == 0 && !currentWordFits )
							currentWordFits = true;

						if( currentWordFits )
						{
							textRun.Append( word ).Append( " " );
							wordRunSize += wordSize.X + spaceX;
							wordIndex++;
						}

						// if this is the last word or the current word doesnt fit dump our text run
						if( ( isLastWord || !currentWordFits ) && textRun.Length > 0 )
						{
							// back off and add any words we accrued and remove the last space
							textRun.Length--;
							wordRunSize -= spaceX;

							lineBuffer.Add( new CompiledTextElement( textRun.ToString(), position, currentFormatting ) );
							textRun.Clear();

							position.X += wordRunSize;
							currentLineWidth += wordRunSize;
							wordRunSize = 0;
						}

						// if the current word doesnt fit we need a newline
						if( !currentWordFits )
						{
							position = wrapLine( position, lineBuffer, alignStack.Peek(), out currentLineHeight );
							currentTotalHeight += currentLineHeight;
							currentLineWidth = 0;
						}
					}
				}
				else if( reader.NodeType == XmlNodeType.EndElement )
				{
					switch( reader.Name )
					{
						case "font":
						case "markuptext":
						{
							formatingStack.Pop();
							break;
						}
						case "if":
						{
							conditionalsStack.Pop();
							break;
						}
						case "p":
						{
							if( lineBuffer.Count > 0 )
							{
								position = wrapLine( position, lineBuffer, alignStack.Peek(), out currentLineHeight );
								currentTotalHeight += currentLineHeight;
							}
							else
							{
								var currentFormatting = formatingStack.Peek();
								position.Y += currentFormatting.font.lineSpacing * currentFormatting.scale.Y;
								currentTotalHeight += currentFormatting.font.lineSpacing * currentFormatting.scale.Y;
							}
							currentLineWidth = 0;
							alignStack.Pop();
							break;
						}
					}
				}
			}

			if( lineBuffer.Count > 0 )
			{
				wrapLine( position, lineBuffer, alignStack.Peek(), out currentLineHeight );
				for( var i = 0; i < lineBuffer.Count; i++ )
				{
					var element = lineBuffer[i];
					element.position = new Vector2( element.position.X, position.Y + currentLineHeight / 2f );
				}
				currentTotalHeight += currentLineHeight;
				_compiledMarkup.AddRange( lineBuffer );
				lineBuffer.Clear();
			}

			_textHeight = currentTotalHeight;
		}


		Vector2 wrapLine( Vector2 position, List<ICompiledElement> lineBuffer, HorizontalAlign alignment, out float currentLineHeight )
		{
			currentLineHeight = 0;
			var lineWidth = 0f;

			// figure out the max height for anything in the line
			for( var i = 0; i < lineBuffer.Count; i++ )
			{
				currentLineHeight = Math.Max( currentLineHeight, lineBuffer[i].size.Y );
				lineWidth += lineBuffer[i].size.X;
			}

			// calculate the offset that we will shift each line
			var xOffset = 0f;
			switch( alignment )
			{
				case HorizontalAlign.Right:
					xOffset = _textWidth - lineWidth;
					break;
				case HorizontalAlign.Center:
					xOffset = ( _textWidth - lineWidth ) / 2f;
					break;
			}

			// run back through and reset the y position of all the items to match the currentLineHeight and apply the xOffset
			for( var i = 0; i < lineBuffer.Count; i++ )
				lineBuffer[i].position = new Vector2( lineBuffer[i].position.X + xOffset, position.Y + currentLineHeight / 2f );

			_compiledMarkup.AddRange( lineBuffer );
			lineBuffer.Clear();
			position.X = 0;
			position.Y += currentLineHeight;

			return position;
		}


		static Color parseColor( string hexString )
		{
			if( hexString.StartsWith( "#" ) )
				hexString = hexString.Substring( 1 );

			var hex = uint.Parse( hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture );
			var color = Color.White;
			if( hexString.Length == 8 )
			{
				color.A = (byte)( hex >> 24 );
				color.R = (byte)( hex >> 16 );
				color.G = (byte)( hex >> 8 );
				color.B = (byte)( hex );
			}
			else if( hexString.Length == 6 )
			{
				color.R = (byte)( hex >> 16 );
				color.G = (byte)( hex >> 8 );
				color.B = (byte)( hex );
			}
			else
			{
				throw new InvalidOperationException();
			}
			return color;
		}


		static Vector2 parseVector2( string vectorString )
		{
			var split = Regex.Split( vectorString, @"[\\s,]+" );
			return new Vector2( float.Parse( split[0], CultureInfo.InvariantCulture ), float.Parse( split[1], CultureInfo.InvariantCulture ) );
		}

	}


	#region Internal helpers

	struct FormatInstruction
	{
		public readonly Color color;
		public readonly IFont font;
		public readonly Vector2 scale;


		public FormatInstruction( IFont font, Color color, Vector2 scale )
		{
			this.font = font;
			this.color = color;
			this.scale = scale;
		}
	}


	interface ICompiledElement
	{
		Vector2 size { get; }
		Vector2 position { get; set; }
		void render( Graphics graphics, Vector2 offset );
	}


	struct CompiledTextElement : ICompiledElement
	{
		public Vector2 position { get; set; }
		public Vector2 size { get; set; }

		readonly FormatInstruction _formatInstruction;
		readonly string _text;


		public CompiledTextElement( string text, Vector2 position, FormatInstruction formatInstruction )
		{
			_text = text;
			this.position = position;
			_formatInstruction = formatInstruction;
			size = formatInstruction.font.measureString( text ) * formatInstruction.scale;
		}


		public void render( Graphics graphics, Vector2 offset )
		{
			var origin = new Vector2( 0, size.Y / ( 2 * _formatInstruction.scale.Y ) );
			graphics.batcher.drawString( _formatInstruction.font, _text, offset + position, _formatInstruction.color, 0, origin, _formatInstruction.scale, SpriteEffects.None, 0 );
		}

	}


	struct CompiledImageElement : ICompiledElement
	{
		public Vector2 position { get; set; }
		public Vector2 size { get; set; }

		readonly Color _color;
		readonly Texture2D _image;
		readonly Vector2 _scale;


		public CompiledImageElement( Texture2D image, Color color, Vector2 position, Vector2 scale )
		{
			_image = image;
			this.position = position;
			_color = color;
			_scale = scale;
			size = new Vector2( image.Width, image.Height ) * scale;
		}


		public void render( Graphics graphics, Vector2 offset )
		{
			var origin = new Vector2( 0, _image.Height / 2f );
			graphics.batcher.draw( _image, offset + position, null, _color, 0, origin, _scale, SpriteEffects.None, 0 );
		}

	}

	#endregion

}
