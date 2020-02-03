using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A listener that shows a tooltip Element when another Element is hovered over with the mouse.
	/// </summary>
	public class Tooltip : Element
	{
		protected Container _container;
		protected Element _targetElement;

		TooltipManager _manager;
		bool _instant, _always;
		bool _isMouseOver;


		public Tooltip(Element contents, Element targetElement)
		{
			_manager = TooltipManager.GetInstance();

			_container = new Container(contents);
			_container.SetOrigin(AlignInternal.Center);
			_targetElement = targetElement;
			_container.SetTouchable(Touchable.Disabled);
		}


		#region config

		public TooltipManager GetManager()
		{
			return _manager;
		}


		public Container GetContainer()
		{
			return _container;
		}


		public Tooltip SetElement(Element contents)
		{
			_container.SetElement(contents);
			return this;
		}


		public Element GetElement()
		{
			return _container.GetElement();
		}


		public T GetElement<T>() where T : Element
		{
			return _container.GetElement<T>();
		}


		public Tooltip SetTargetElement(Element targetElement)
		{
			_targetElement = targetElement;
			return this;
		}


		public Element GetTargetElement()
		{
			return _targetElement;
		}


		/// <summary>
		/// If true, this tooltip is shown without delay when hovered
		/// </summary>
		/// <param name="instant">Instant.</param>
		public Tooltip SetInstant(bool instant)
		{
			_instant = instant;
			return this;
		}


		public bool GetInstant()
		{
			return _instant;
		}


		/// <summary>
		/// If true, this tooltip is shown even when tooltips are not TooltipManager#enabled
		/// </summary>
		/// <param name="always">Always.</param>
		public Tooltip SetAlways(bool always)
		{
			_always = always;
			return this;
		}


		public bool GetAlways()
		{
			return _always;
		}

		#endregion


		public override Element Hit(Vector2 point)
		{
			// we do some rejiggering here by checking for hits on our target and using that
			var local = _targetElement.ScreenToLocalCoordinates(point);
			if (_targetElement.Hit(local) != null)
			{
				if (!_isMouseOver)
				{
					_isMouseOver = true;
					_manager.Enter(this);
				}

				SetContainerPosition(local.X, local.Y);
			}
			else if (_isMouseOver)
			{
				_isMouseOver = false;
				_manager.Hide(this);
			}

			return null;
		}


		void SetContainerPosition(float x, float y)
		{
			var stage = _targetElement.GetStage();
			if (stage == null)
				return;

			_container.Pack();
			float offsetX = _manager.OffsetX, offsetY = _manager.OffsetY, dist = _manager.EdgeDistance;
			var point = _targetElement.LocalToStageCoordinates(new Vector2(x + offsetX - _container.GetWidth() / 2,
				y - offsetY - _container.GetHeight()));
			if (point.Y < dist)
				point = _targetElement.LocalToStageCoordinates(new Vector2(x + offsetX, y + offsetY));
			if (point.X < dist)
				point.X = dist;
			if (point.X + _container.GetWidth() > stage.GetWidth() - dist)
				point.X = stage.GetWidth() - dist - _container.GetWidth();
			if (point.Y + _container.GetHeight() > stage.GetHeight() - dist)
				point.Y = stage.GetHeight() - dist - _container.GetHeight();
			_container.SetPosition(point.X, point.Y);

			point = _targetElement.LocalToStageCoordinates(new Vector2(_targetElement.GetWidth() / 2,
				_targetElement.GetHeight() / 2));
			point -= new Vector2(_container.GetX(), _container.GetY());
			_container.SetOrigin(point.X, point.Y);
		}
	}
}