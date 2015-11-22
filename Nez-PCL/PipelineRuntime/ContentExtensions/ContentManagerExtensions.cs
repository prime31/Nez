using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Content
{
	public static class ContentManagerExtensions
	{
		public const string DirectorySeparatorChar = "/";


		public static Stream openStream( this ContentManager contentManager, string path )
		{
			return TitleContainer.OpenStream( contentManager.RootDirectory + DirectorySeparatorChar + path );
		}


		public static GraphicsDevice getGraphicsDevice( this ContentManager contentManager )
		{
			// http://konaju.com/?p=21
			var serviceProvider = contentManager.ServiceProvider;
			var graphicsDeviceService = (IGraphicsDeviceService)serviceProvider.GetService( typeof( IGraphicsDeviceService ) );
			return graphicsDeviceService.GraphicsDevice;
		}
	}
}
