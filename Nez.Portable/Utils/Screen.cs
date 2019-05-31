using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public static class Screen
	{
		static internal GraphicsDeviceManager _graphicsManager;

		internal static void initialize( GraphicsDeviceManager graphicsManager ) => _graphicsManager = graphicsManager;

		/// <summary>
		/// width of the GraphicsDevice back buffer
		/// </summary>
		/// <value>The width.</value>
		public static int width
		{
			get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
			set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value;
		}

		/// <summary>
		/// height of the GraphicsDevice back buffer
		/// </summary>
		/// <value>The height.</value>
		public static int height
		{
			get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
			set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value;
		}

		/// <summary>
		/// gets the Screen's center.null Note that this is the center of the backbuffer! If you are rendering to a smaller RenderTarget
		/// you will need to scale this value appropriately.
		/// </summary>
		/// <value>The center.</value>
		public static Vector2 center => new Vector2( width / 2, height / 2 );

		public static int preferredBackBufferWidth
		{
			get => _graphicsManager.PreferredBackBufferWidth;
			set => _graphicsManager.PreferredBackBufferWidth = value;
		}

		public static int preferredBackBufferHeight
		{
			get => _graphicsManager.PreferredBackBufferHeight;
			set => _graphicsManager.PreferredBackBufferHeight = value;
		}

		public static int monitorWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

		public static int monitorHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

		public static SurfaceFormat backBufferFormat => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat;

		public static SurfaceFormat preferredBackBufferFormat
		{
			get => _graphicsManager.PreferredBackBufferFormat;
			set => _graphicsManager.PreferredBackBufferFormat = value;
		}

		public static bool synchronizeWithVerticalRetrace
		{
			get => _graphicsManager.SynchronizeWithVerticalRetrace;
			set => _graphicsManager.SynchronizeWithVerticalRetrace = value;
		}

		// defaults to Depth24Stencil8
		public static DepthFormat preferredDepthStencilFormat
		{
			get => _graphicsManager.PreferredDepthStencilFormat;
			set => _graphicsManager.PreferredDepthStencilFormat = value;
		}

		public static bool isFullscreen
		{
			get => _graphicsManager.IsFullScreen;
			set => _graphicsManager.IsFullScreen = value;
		}

		public static DisplayOrientation supportedOrientations
		{
			get => _graphicsManager.SupportedOrientations;
			set => _graphicsManager.SupportedOrientations = value;
		}

		public static void applyChanges() => _graphicsManager.ApplyChanges();

		/// <summary>
		/// sets the preferredBackBuffer then applies the changes
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static void setSize( int width, int height )
		{
			preferredBackBufferWidth = width;
			preferredBackBufferHeight = height;
			applyChanges();
		}

	}
}

