using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public static class Screen
	{
		static internal GraphicsDeviceManager _graphicsManager;

		internal static void Initialize(GraphicsDeviceManager graphicsManager) => _graphicsManager = graphicsManager;

		/// <summary>
		/// width of the GraphicsDevice back buffer
		/// </summary>
		/// <value>The width.</value>
		public static int Width
		{
			get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
			set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value;
		}

		/// <summary>
		/// height of the GraphicsDevice back buffer
		/// </summary>
		/// <value>The height.</value>
		public static int Height
		{
			get => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
			set => _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value;
		}

		/// <summary>
		/// gets the Screen's center.null Note that this is the center of the backbuffer! If you are rendering to a smaller RenderTarget
		/// you will need to scale this value appropriately.
		/// </summary>
		/// <value>The center.</value>
		public static Vector2 Center => new Vector2(Width / 2, Height / 2);

		public static int PreferredBackBufferWidth
		{
			get => _graphicsManager.PreferredBackBufferWidth;
			set => _graphicsManager.PreferredBackBufferWidth = value;
		}

		public static int PreferredBackBufferHeight
		{
			get => _graphicsManager.PreferredBackBufferHeight;
			set => _graphicsManager.PreferredBackBufferHeight = value;
		}

		public static int MonitorWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

		public static int MonitorHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

		public static SurfaceFormat BackBufferFormat =>
			_graphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat;

		public static SurfaceFormat PreferredBackBufferFormat
		{
			get => _graphicsManager.PreferredBackBufferFormat;
			set => _graphicsManager.PreferredBackBufferFormat = value;
		}

		public static bool SynchronizeWithVerticalRetrace
		{
			get => _graphicsManager.SynchronizeWithVerticalRetrace;
			set => _graphicsManager.SynchronizeWithVerticalRetrace = value;
		}

		// defaults to Depth24Stencil8
		public static DepthFormat PreferredDepthStencilFormat
		{
			get => _graphicsManager.PreferredDepthStencilFormat;
			set => _graphicsManager.PreferredDepthStencilFormat = value;
		}

		public static bool IsFullscreen
		{
			get => _graphicsManager.IsFullScreen;
			set => _graphicsManager.IsFullScreen = value;
		}

		public static DisplayOrientation SupportedOrientations
		{
			get => _graphicsManager.SupportedOrientations;
			set => _graphicsManager.SupportedOrientations = value;
		}

		public static void ApplyChanges() => _graphicsManager.ApplyChanges();

		/// <summary>
		/// sets the preferredBackBuffer then applies the changes
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static void SetSize(int width, int height)
		{
			PreferredBackBufferWidth = width;
			PreferredBackBufferHeight = height;
			ApplyChanges();
		}
	}
}