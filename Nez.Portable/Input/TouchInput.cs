using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;


namespace Nez
{
	/// <summary>
	/// to enable touch input you must first call enableTouchSupport()
	/// </summary>
	public class TouchInput
	{
		public bool IsConnected => _isConnected;
		public TouchCollection CurrentTouches => _currentTouches;

		public TouchCollection PreviousTouches => _previousTouches;
		public List<GestureSample> PreviousGestures => _previousGestures;
		public List<GestureSample> CurrentGestures => _currentGestures;

		TouchCollection _previousTouches;
		TouchCollection _currentTouches;
		List<GestureSample> _previousGestures = new List<GestureSample>();
		List<GestureSample> _currentGestures = new List<GestureSample>();

		bool _isConnected;


		void OnGraphicsDeviceReset()
		{
			TouchPanel.DisplayWidth = Core.GraphicsDevice.Viewport.Width;
			TouchPanel.DisplayHeight = Core.GraphicsDevice.Viewport.Height;
			TouchPanel.DisplayOrientation = Core.GraphicsDevice.PresentationParameters.DisplayOrientation;
		}


		internal void Update()
		{
			if (!_isConnected)
				return;

			_previousTouches = _currentTouches;
			_currentTouches = TouchPanel.GetState();

			_previousGestures.Clear();
			_previousGestures.AddRange(_currentGestures);
			_currentGestures.Clear();
			while (TouchPanel.IsGestureAvailable)
				_currentGestures.Add(TouchPanel.ReadGesture());
		}


		public void EnableTouchSupport()
		{
			_isConnected = TouchPanel.GetCapabilities().IsConnected;

			if (_isConnected)
			{
				Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
				Core.Emitter.AddObserver(CoreEvents.OrientationChanged, OnGraphicsDeviceReset);
				OnGraphicsDeviceReset();
			}
		}
	}
}