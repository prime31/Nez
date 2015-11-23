using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class ViewportAdapter
	{
		public virtual int virtualWidth
		{
			get { return _graphicsDevice.Viewport.Width; }
		}

		public virtual int virtualHeight
		{
			get { return _graphicsDevice.Viewport.Height; }
		}

		public virtual int viewportWidth
		{
			get { return _graphicsDevice.Viewport.Width; }
		}

		public virtual int viewportHeight
		{
			get { return _graphicsDevice.Viewport.Height; }
		}

		public Matrix scaleMatrix;

		protected GraphicsDevice _graphicsDevice;


		public ViewportAdapter( GraphicsDevice graphicsDevice )
		{
			_graphicsDevice = graphicsDevice;
			scaleMatrix = Matrix.Identity;
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		protected virtual void onGraphicsDeviceReset()
		{}


		public void unload()
		{
			Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		public virtual Vector2 pointToVirtualViewport( Vector2 point )
		{
			return point;
		}


		public virtual Vector2 screenToVirtualViewport( Vector2 point )
		{
			return point;
		}

	}
}

