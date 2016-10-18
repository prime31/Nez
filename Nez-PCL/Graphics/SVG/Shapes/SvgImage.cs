using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;


namespace Nez.Svg
{
	public class SvgImage : SvgElement
	{
		[XmlAttribute( "x" )]
		public float x;

		[XmlAttribute( "y" )]
		public float y;

		[XmlAttribute( "width" )]
		public float width;

		[XmlAttribute( "height" )]
		public float height;

		/// <summary>
		/// the rect encompassing this image. Note that the rect is with no transforms applied.
		/// </summary>
		/// <value>The rect.</value>
		public RectangleF rect { get { return new RectangleF( x, y, width, height ); } }

		[XmlAttribute( "href", Namespace = "http://www.w3.org/1999/xlink" )]
		public string href;


		/// <summary>
		/// attempts to get a texture for the image
		/// - first it will check the href for a png file name. If it finds one it will load it with the ContentManager passed in
		/// - next it will see if the href is a url and if so it will load it
		/// - next it checks for an embedded, base64 image. It will load that if it finds one
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="content">Content.</param>
		public Texture2D getTexture( NezContentManager content )
		{
			if( _texture != null )
				return _texture;

			// check for a url
			if( href.StartsWith( "http" ) )
			{
				using( var client = new System.Net.Http.HttpClient() )
				{
					var stream = client.GetStreamAsync( href ).Result;
					_texture = Texture2D.FromStream( Core.graphicsDevice, stream );
				}
			}
			// see if we have a path to a png files in the href
			else if( href.EndsWith( "png" ) )
			{
				_texture = content.Load<Texture2D>( href );
			}
			// attempt to parse the base64 string if it is embedded in the href
			else if( href.StartsWith( "data:" ) )
			{
				var startIndex = href.IndexOf( "base64,", StringComparison.OrdinalIgnoreCase ) + 7;
				var imageContents = href.Substring( startIndex );
				var bytes = Convert.FromBase64String( imageContents );

				using( var m = new MemoryStream() )
				{
					m.Write( bytes, 0, bytes.Length );
					m.Seek( 0, SeekOrigin.Begin );

					_texture = Texture2D.FromStream( Core.graphicsDevice, m );
				}

			}

			return _texture;
		}
		Texture2D _texture;

	}
}
