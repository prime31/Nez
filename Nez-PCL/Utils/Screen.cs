using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public static class Screen
	{
		static internal GraphicsDeviceManager _graphicsManager;


		internal static void initialize( GraphicsDeviceManager graphicsManager )
		{
			_graphicsManager = graphicsManager;
		}


		public static int backBufferWidth
		{
			get { return _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth; }
			set { _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value; }
		}


		public static int backBufferHeight
		{
			get { return _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight; }
			set { _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value; }
		}


		public static int preferredBackBufferWidth
		{
			get { return _graphicsManager.PreferredBackBufferWidth; }
			set { _graphicsManager.PreferredBackBufferWidth = value; }
		}


		public static int preferredBackBufferHeight
		{
			get { return _graphicsManager.PreferredBackBufferHeight; }
			set { _graphicsManager.PreferredBackBufferHeight = value; }
		}


		public static int monitorWidth
		{
			get { return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; }
		}


		public static int monitorHeight
		{
			get { return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height; }
		}


		public static SurfaceFormat backBufferFormat
		{
			get { return _graphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat; }
		}


		public static SurfaceFormat preferredBackBufferFormat
		{
			get { return _graphicsManager.PreferredBackBufferFormat; }
			set { _graphicsManager.PreferredBackBufferFormat = value; }
		}


		public static bool synchronizeWithVerticalRetrace
		{
			get { return _graphicsManager.SynchronizeWithVerticalRetrace; }
			set { _graphicsManager.SynchronizeWithVerticalRetrace = value; }
		}


		// defaults to Depth24
		public static DepthFormat preferredDepthStencilFormat
		{
			get { return _graphicsManager.PreferredDepthStencilFormat; }
			set { _graphicsManager.PreferredDepthStencilFormat = value;	}
		}


		public static bool isFullscreen
		{
			get { return _graphicsManager.IsFullScreen; }
			set { _graphicsManager.IsFullScreen = value; }
		}


		public static void applyChanges()
		{
			//_graphicsManager.ApplyChanges();
			Debug.warn( "noop. applyChanges doesnt work properly on OS X. It causes a crash with no stack trace due to a MonoGame bug." );
		}


		/// <summary>
		/// sets the preferredBackBuffer then applies the changes
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static void updatePreferredBackBufferSize( int width, int height )
		{
			preferredBackBufferWidth = width;
			preferredBackBufferHeight = height;
			applyChanges();
		}

	}
}

