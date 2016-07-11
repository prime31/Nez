using System.Collections.Generic;
#if !FNA
using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace Nez
{
	/// <summary>
	/// to enable touch input you must first call enableTouchSupport()
	/// </summary>
	public class TouchInput
	{
		#if !FNA
		public bool isConnected
		{
			get { return _isConnected; }
		}

		public TouchCollection currentTouches
		{
			get { return _currentTouches; }
		}

		public TouchCollection previousTouches
		{
			get { return _previousTouches; }
		}

		public List<GestureSample> previousGestures
		{
			get { return _previousGestures; }
		}

		public List<GestureSample> currentGestures
		{
			get { return _currentGestures; }
		}

		TouchCollection _previousTouches;
		TouchCollection _currentTouches;
		List<GestureSample> _previousGestures = new List<GestureSample>();
		List<GestureSample> _currentGestures = new List<GestureSample>();
		#endif

		#pragma warning disable 0649
		bool _isConnected;
		#pragma warning restore 0649


		void onGraphicsDeviceReset()
		{
			#if !FNA
			TouchPanel.DisplayWidth = Core.graphicsDevice.Viewport.Width;
			TouchPanel.DisplayHeight = Core.graphicsDevice.Viewport.Height;
			TouchPanel.DisplayOrientation = Core.graphicsDevice.PresentationParameters.DisplayOrientation;
			#endif
		}


		internal void update()
		{
			if( !_isConnected )
				return;
			
			#if !FNA
			_previousTouches = _currentTouches;
			_currentTouches = TouchPanel.GetState();

			_previousGestures = _currentGestures;
			_currentGestures.Clear();
			while( TouchPanel.IsGestureAvailable )
				_currentGestures.Add( TouchPanel.ReadGesture() );
			#endif
		}


		public void enableTouchSupport()
		{
			#if !FNA
            _isConnected = TouchPanel.GetCapabilities().IsConnected;
			#endif

			if( _isConnected )
			{
				Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
				Core.emitter.addObserver( CoreEvents.OrientationChanged, onGraphicsDeviceReset );
				onGraphicsDeviceReset();
			}
		}

	}
}

