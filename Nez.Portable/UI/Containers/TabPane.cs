using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Nez.UI
{
	public class TabPane : Table
	{
		public Tab CurrentTab;
		public List<Tab> Tabs;
		public List<TabButton> TabButtons;
		private TabWindowStyle _style;
		private Table _buttonsTable;
		private Table _tabTable;

		public TabPane(TabWindowStyle style)
		{
			_style = style;
			Init();
		}

		private void Init()
		{
			SetSize(100, 100);

			SetBackground(_style.Background);

			Top().Left();

			Tabs = new List<Tab>();
			TabButtons = new List<TabButton>();

			_buttonsTable = new Table();
			_buttonsTable.SetFillParent(true);
			_buttonsTable.Top().Left();
			_tabTable = new Table();
			_tabTable.Top().Left();

			Row();
			Add(_buttonsTable).Fill().SetExpandX();
			Row();
			Add(_tabTable).Fill().SetExpandY();
		}

		public void AddTab(Tab tab)
		{
			Tabs.Add(tab);

			var tabBtn = new TabButton(tab, _style.TabButtonStyle);
			tabBtn.OnClick += () => { SetActiveTab(tabBtn.GetTab()); };
			TabButtons.Add(tabBtn);
			_buttonsTable.Add(tabBtn);

			if (Tabs.Count == 1)
			{
				CurrentTab = Tabs[0];
				_tabTable.Add(tab).Left().Top().Fill().Expand();

				tabBtn.ToggleOn();
			}

			if (Tabs.Count == 1)
				SetActiveTab(0);
		}

		public void SetActiveTab(int index)
		{
			var tab = Tabs[index];
			if (tab != CurrentTab)
			{
				_tabTable.Clear();
				_tabTable.Add(tab).Left().Top().Fill().Expand();

				TabButtons[index].ToggleOn();

				var i = Tabs.IndexOf(CurrentTab);
				TabButtons[i].ToggleOff();

				CurrentTab = tab;
			}
		}

		protected void SetActiveTab(Tab tab)
		{
			var i = Tabs.IndexOf(tab);
			SetActiveTab(i);
		}
	}

	public class TabWindowStyle
	{
		public IDrawable Background;
		public TabButtonStyle TabButtonStyle;
	}

	public class Tab : Table
	{
		private TabStyle _style;
		public string TabName;

		public Tab(string name, TabStyle style)
		{
			TabName = name;
			_style = style;
			SetTouchable(Touchable.Enabled);
			Setup();
		}

		private void Setup()
		{
			SetBackground(_style.Background);
			SetFillParent(true);
			Top().Left();
		}
	}

	public class TabStyle
	{
		public IDrawable Background;
	}

	public class TabButton : Table, IInputListener
	{
		public enum TabButtonState
		{
			Inactive,
			Active,
			Locked
		}

		private TabButtonState state = TabButtonState.Inactive;

		private Label text;
		private TabButtonStyle style;
		private string tabName;
		private Tab tab;

		public Action OnClick;

		public TabButton(Tab tab, TabButtonStyle style)
		{
			this.style = style;
			tabName = tab.TabName;
			this.tab = tab;
			Init();
		}

		private void Init()
		{
			SetTouchable(Touchable.Enabled);
			text = new Label(tabName, style.LabelStyle);
			Add(text).SetFillX().Pad(8);
			SetBackground(style.Inactive);
			PadTop(style.PaddingTop);
		}

		public string GetTabeName()
		{
			return tabName;
		}

		public Tab GetTab()
		{
			return tab;
		}

		public bool IsSwitchedOn()
		{
			return state == TabButtonState.Active;
		}

		public void Toggle()
		{
			if (state != TabButtonState.Locked)
			{
				if (state == TabButtonState.Active)
				{
					state = TabButtonState.Inactive;
					SetBackground(style.Inactive);
				}
				else
				{
					state = TabButtonState.Active;
					SetBackground(style.Active);
				}
			}
		}

		public void ToggleOff()
		{
			if (state != TabButtonState.Locked)
			{
				state = TabButtonState.Inactive;

				SetBackground(style.Inactive);
			}
		}

		public void ToggleOn()
		{
			if (state != TabButtonState.Locked)
			{
				state = TabButtonState.Active;

				SetBackground(style.Active);
			}
		}

		public void ToggleLock()
		{
			if (state != TabButtonState.Inactive)
			{
				if (state == TabButtonState.Active)
				{
					state = TabButtonState.Locked;
					SetBackground(style.Locked);
				}
				else
				{
					state = TabButtonState.Active;
					SetBackground(style.Active);
				}
			}
		}

		public void Unlock()
		{
			if (state == TabButtonState.Locked)
			{
				state = TabButtonState.Active;
				SetBackground(style.Active);
			}
		}

		void IInputListener.OnMouseEnter()
		{
			if (state == TabButtonState.Inactive)
			{
				SetBackground(style.Hover);
			}
		}

		void IInputListener.OnMouseExit()
		{
			if (state == TabButtonState.Inactive)
			{
				SetBackground(style.Inactive);
			}
		}

		bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
		{
			return true;
		}

		bool IInputListener.OnRightMousePressed(Vector2 mousePos)
		{
			return false;
		}

		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
		}

		void IInputListener.OnLeftMouseUp(Vector2 mousePos)
		{
			OnClick?.Invoke();
		}

		void IInputListener.OnRightMouseUp(Vector2 mousePos)
		{
		}

		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			return true;
		}
	}

	public class TabButtonStyle
	{
		public IDrawable Active;
		public IDrawable Inactive;
		public IDrawable Locked;
		public IDrawable Hover;
		public float PaddingTop = 0.0F;
		public LabelStyle LabelStyle;
	}
}