using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using System;


namespace Nez.UI
{
	/// <summary>
	/// A table that can be dragged and resized. The top padding is used as the window's title height.
	///
	/// The preferred size of a window is the preferred size of the title text and the children as laid out by the table. After adding
	/// children to the window, it can be convenient to call {@link #pack()} to size the window to the size of the children.
	/// </summary>
	public class Window : Table, IInputListener
	{
		static private int MOVE = 1 << 5;

		private WindowStyle style;
		bool _isMovable = true, _isResizable;
		int resizeBorderSize = 8;
		bool _dragging;
		bool _keepWithinStage = true;
		Label titleLabel;
		Table titleTable;


		public Window(string title, WindowStyle style)
		{
			Insist.IsNotNull(title, "title cannot be null");

			touchable = Touchable.Enabled;
			Clip = true;

			titleLabel = new Label(title, new LabelStyle(style.TitleFont, style.TitleFontColor, style.TitleFontScaleX, style.TitleFontScaleY));
			titleLabel.SetEllipsis(true);

			titleTable = new Table();
			titleTable.Add(titleLabel).SetExpandX().SetFillX().SetMinWidth(0);
			AddElement(titleTable);

			SetStyle(style);
			width = 150;
			height = 150;
		}


		public Window(string title, Skin skin, string styleName = null) : this(title, skin.Get<WindowStyle>(styleName))
		{ }


		#region IInputListener

		int edge;
		float startX, startY, lastX, lastY;

		void IInputListener.OnMouseEnter()
		{ }


		void IInputListener.OnMouseExit()
		{ }


		bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
		{
			float width = GetWidth(), height = GetHeight();
			edge = 0;
			if (_isResizable && mousePos.X >= 0 && mousePos.X < width && mousePos.Y >= 0 && mousePos.Y < height)
			{
				if (mousePos.X < resizeBorderSize)
					edge |= (int)AlignInternal.Left;
				if (mousePos.X > width - resizeBorderSize)
					edge |= (int)AlignInternal.Right;
				if (mousePos.Y < resizeBorderSize)
					edge |= (int)AlignInternal.Top;
				if (mousePos.Y > height - resizeBorderSize)
					edge |= (int)AlignInternal.Bottom;

				int tempResizeBorderSize = resizeBorderSize;
				if (edge != 0)
					tempResizeBorderSize += 25;
				if (mousePos.X < tempResizeBorderSize)
					edge |= (int)AlignInternal.Left;
				if (mousePos.X > width - tempResizeBorderSize)
					edge |= (int)AlignInternal.Right;
				if (mousePos.Y < tempResizeBorderSize)
					edge |= (int)AlignInternal.Top;
				if (mousePos.Y > height - tempResizeBorderSize)
					edge |= (int)AlignInternal.Bottom;
			}

			if (_isMovable && edge == 0 && mousePos.Y >= 0 && mousePos.Y <= GetPadTop() && mousePos.X >= 0 &&
				mousePos.X <= width)
				edge = MOVE;

			_dragging = edge != 0;

			startX = mousePos.X;
			startY = mousePos.Y;
			lastX = mousePos.X;
			lastY = mousePos.Y;

			return true;
		}

		bool IInputListener.OnRightMousePressed(Vector2 mousePos)
		{
			return false;
		}

		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
			if (!_dragging)
				return;

			float width = GetWidth(), height = GetHeight();
			float windowX = GetX(), windowY = GetY();

			var stage = GetStage();
			var parentWidth = stage.GetWidth();
			var parentHeight = stage.GetHeight();

			var clampPosition = _keepWithinStage && GetParent() == stage.GetRoot();

			if ((edge & MOVE) != 0)
			{
				float amountX = mousePos.X - startX, amountY = mousePos.Y - startY;

				if (clampPosition)
				{
					if (windowX + amountX < 0)
						amountX = -windowX;
					if (windowY + amountY < 0)
						amountY = -windowY;
					if (windowX + width + amountX > parentWidth)
						amountX = parentWidth - windowX - width;
					if (windowY + height + amountY > parentHeight)
						amountY = parentHeight - windowY - height;
				}

				windowX += amountX;
				windowY += amountY;
			}

			if ((edge & (int)AlignInternal.Left) != 0)
			{
				float amountX = mousePos.X - startX;
				if (width - amountX < MinWidth)
					amountX = -(MinWidth - width);
				if (clampPosition && windowX + amountX < 0)
					amountX = -windowX;
				width -= amountX;
				windowX += amountX;
			}

			if ((edge & (int)AlignInternal.Top) != 0)
			{
				float amountY = mousePos.Y - startY;
				if (height - amountY < MinHeight)
					amountY = -(MinHeight - height);
				if (clampPosition && windowY + amountY < 0)
					amountY = -windowY;
				height -= amountY;
				windowY += amountY;
			}

			if ((edge & (int)AlignInternal.Right) != 0)
			{
				float amountX = mousePos.X - lastX;
				if (width + amountX < MinWidth)
					amountX = MinWidth - width;
				if (clampPosition && windowX + width + amountX > parentWidth)
					amountX = parentWidth - windowX - width;
				width += amountX;
			}

			if ((edge & (int)AlignInternal.Bottom) != 0)
			{
				float amountY = mousePos.Y - lastY;
				if (height + amountY < MinHeight)
					amountY = MinHeight - height;
				if (clampPosition && windowY + height + amountY > parentHeight)
					amountY = parentHeight - windowY - height;
				height += amountY;
			}

			lastX = mousePos.X;
			lastY = mousePos.Y;
			SetBounds(Mathf.Round(windowX), Mathf.Round(windowY), Mathf.Round(width), Mathf.Round(height));
		}


		void IInputListener.OnLeftMouseUp(Vector2 mousePos)
		{
			_dragging = false;
		}
		void IInputListener.OnRightMouseUp(Vector2 mousePos)
		{

		}


		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			return false;
		}

		#endregion


		public Window SetStyle(WindowStyle style)
		{
			this.style = style;
			SetBackground(style.Background);

			var labelStyle = titleLabel.GetStyle();
			labelStyle.Font = style.TitleFont ?? labelStyle.Font;
			labelStyle.FontColor = style.TitleFontColor;
			labelStyle.FontScaleX = style.TitleFontScaleX;
			labelStyle.FontScaleY = style.TitleFontScaleY;
			titleLabel.SetStyle(labelStyle);

			InvalidateHierarchy();
			return this;
		}


		/// <summary>
		/// Returns the window's style. Modifying the returned style may not have an effect until {@link #setStyle(WindowStyle)} is called
		/// </summary>
		/// <returns>The style.</returns>
		public WindowStyle GetStyle()
		{
			return style;
		}


		public void KeepWithinStage()
		{
			if (!_keepWithinStage)
				return;

			var stage = GetStage();
			var parentWidth = stage.GetWidth();
			var parentHeight = stage.GetHeight();

			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (GetY(AlignInternal.Bottom) > parentHeight)
				y = parentHeight - height;
			if (GetX(AlignInternal.Right) > parentWidth)
				x = parentWidth - width;
		}


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			KeepWithinStage();

			if (style.StageBackground != null)
			{
				var stagePos = StageToLocalCoordinates(Vector2.Zero);
				var stageSize = StageToLocalCoordinates(new Vector2(_stage.GetWidth(), _stage.GetHeight()));
				DrawStageBackground(batcher, parentAlpha, GetX() + stagePos.X, GetY() + stagePos.Y,
					GetX() + stageSize.X, GetY() + stageSize.Y);
			}

			base.Draw(batcher, parentAlpha);
		}


		protected void DrawStageBackground(Batcher batcher, float parentAlpha, float x, float y, float width, float height)
		{
			style.StageBackground.Draw(batcher, x, y, width, height, ColorExt.Create(color, (int)(color.A * parentAlpha)));
		}


		protected override void DrawBackground(Batcher batcher, float parentAlpha, float x, float y)
		{
			base.DrawBackground(batcher, parentAlpha, x, y);

			// Manually draw the title table before clipping is done.
			titleTable.color.A = color.A;
			float padTop = GetPadTop(), padLeft = GetPadLeft();
			titleTable.SetSize(GetWidth() - padLeft - GetPadRight(), padTop);
			titleTable.SetPosition(padLeft, 0);
		}


		public override Element Hit(Vector2 point)
		{
			var hit = base.Hit(point);
			if (hit == null || hit == this)
				return hit;

			if (point.Y >= 0 && point.Y <= GetPadTop() && point.X >= 0 && point.X <= width)
			{
				// Hit the title bar, don't use the hit child if it is in the Window's table.
				Element current = hit;
				while (current.GetParent() != this)
					current = current.GetParent();

				if (GetCell(current) != null)
					return this;
			}

			return hit;
		}


		public bool IsMovable() => _isMovable;


		public Window SetMovable(bool isMovable)
		{
			_isMovable = isMovable;
			return this;
		}


		public Window SetKeepWithinStage(bool keepWithinStage)
		{
			_keepWithinStage = keepWithinStage;
			return this;
		}


		public bool IsResizable() => _isResizable;


		public Window SetResizable(bool isResizable)
		{
			_isResizable = isResizable;
			return this;
		}


		public Window SetResizeBorderSize(int resizeBorderSize)
		{
			this.resizeBorderSize = resizeBorderSize;
			return this;
		}


		public bool IsDragging() => _dragging;


		public float GetPrefWidth()
		{
			return Math.Max(base.PreferredWidth, titleLabel.PreferredWidth + GetPadLeft() + GetPadRight());
		}


		public Table GetTitleTable() => titleTable;


		public Label GetTitleLabel() => titleLabel;
	}


	public class WindowStyle
	{
		public BitmapFont TitleFont;

		/** Optional. */
		public float TitleFontScaleX = 1f;

		/** Optional. */
		public float TitleFontScaleY = 1f;

		/** Optional. */
		public IDrawable Background;

		/** Optional. */
		public Color TitleFontColor = Color.White;

		/** Optional. */
		public IDrawable StageBackground;




		public WindowStyle()
		{
			TitleFont = Graphics.Instance.BitmapFont;
		}


        public WindowStyle(BitmapFont titleFont, Color titleFontColor, IDrawable background) : this(titleFont, titleFontColor, background, 1f)
        { }


        public WindowStyle(BitmapFont titleFont, Color titleFontColor, IDrawable background, float titleFontScale) : this(titleFont, titleFontColor, background, titleFontScale, titleFontScale)
        { }


        public WindowStyle(BitmapFont titleFont, Color titleFontColor, IDrawable background, float titleFontScaleX, float titleFontScaleY)
        {
            TitleFont = titleFont ?? Graphics.Instance.BitmapFont;
            Background = background;
            TitleFontColor = titleFontColor;
            TitleFontScaleX = titleFontScaleX;
            TitleFontScaleY = titleFontScaleY;
        }


		public WindowStyle Clone()
		{
			return new WindowStyle
			{
				Background = Background,
				TitleFont = TitleFont,
				TitleFontColor = TitleFontColor,
				TitleFontScaleX = TitleFontScaleX,
				TitleFontScaleY = TitleFontScaleY,
				StageBackground = StageBackground
			};
		}
	}
}