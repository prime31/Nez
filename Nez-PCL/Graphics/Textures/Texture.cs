using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Nez.Content;


namespace Nez.Textures
{
	public class Texture
	{
		/// <summary>
		/// the actual Texture2D
		/// </summary>
		public Texture2D texture2D;

		/// <summary>
		/// the full bounds of the Texture2D
		/// </summary>
		public Rectangle textureBounds;

		/// <summary>
		/// is this texture loaded?
		/// </summary>
		/// <value><c>true</c> if is loaded; otherwise, <c>false</c>.</value>
		public bool isLoaded { get { return texture2D != null; } }


		protected Texture() {}


		public Texture( Texture2D texture )
		{
			this.texture2D = texture;
			textureBounds = texture.Bounds;
		}


		public Texture( int width, int height, Color color )
		{
			texture2D = new Texture2D( Core.graphicsDevice, width, height );
			var data = new Color[width * height];

			for( var i = 0; i < data.Length; i++ )
				data[i] = color;
			texture2D.SetData<Color>( data );

			textureBounds = new Rectangle( 0, 0, width, height );
		}


		public virtual void load( string imagePath, ContentManager content )
		{
			using( var stream = content.openStream( imagePath ) )
			{
				texture2D = Texture2D.FromStream( content.getGraphicsDevice(), stream );
				textureBounds = new Rectangle( 0, 0, texture2D.Width, texture2D.Height );
			}
		}


		public virtual void unload()
		{
			texture2D.Dispose();
			texture2D = null;
		}


		public static implicit operator Texture2D( Texture tex )
		{
			return tex.texture2D;
		}

	}
}

