using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;


namespace Nez
{
	/// <summary>
	/// to enable touch input you must first call enableTouchSupport()
	/// </summary>
	public class TouchInput
	{
		public bool isConnected => _isConnected;
		public TouchCollection currentTouches => _currentTouches;

		public TouchCollection previousTouches => _previousTouches;
		public List<GestureSample> previousGestures => _previousGestures;
		public List<GestureSample> currentGestures => _currentGestures;

		TouchCollection _previousTouches;
		TouchCollection _currentTouches;
		List<GestureSample> _previousGestures = new List<GestureSample>();
		List<GestureSample> _currentGestures = new List<GestureSample>();

		bool _isConnected;


		void onGraphicsDeviceReset()
		{
			TouchPanel.DisplayWidth = Core.graphicsDevice.Viewport.Width;
			TouchPanel.DisplayHeight = Core.graphicsDevice.Viewport.Height;
			TouchPanel.DisplayOrientation = Core.graphicsDevice.PresentationParameters.DisplayOrientation;
		}


		internal void update()
		{
			if( !_isConnected )
				return;

			_previousTouches = _currentTouches;
			_currentTouches = TouchPanel.GetState();

			_previousGestures.Clear();
			_previousGestures.AddRange( _currentGestures );
			_currentGestures.Clear();
			while( TouchPanel.IsGestureAvailable )
				_currentGestures.Add( TouchPanel.ReadGesture() );
		}


		public void enableTouchSupport()
		{
            _isConnected = TouchPanel.GetCapabilities().IsConnected;

			if( _isConnected )
			{
				Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
				Core.emitter.addObserver( CoreEvents.OrientationChanged, onGraphicsDeviceReset );
				onGraphicsDeviceReset();
			}
		}

	}
}

