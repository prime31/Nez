using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Pipeline.Content
{
	public static class ContentManagerExtensions
	{
		public static Stream OpenStream(this ContentManager contentManager, string path)
		{
			return TitleContainer.OpenStream(Path.Combine(contentManager.RootDirectory, path));
		}


		public static GraphicsDevice GetGraphicsDevice(this ContentManager contentManager)
		{
			// http://konaju.com/?p=21
			var serviceProvider = contentManager.ServiceProvider;
			var graphicsDeviceService =
				(IGraphicsDeviceService) serviceProvider.GetService(typeof(IGraphicsDeviceService));
			return graphicsDeviceService.GraphicsDevice;
		}
	}
}