using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class RuntimeInspector : IDisposable
	{
		const float kLeftCellWidth = 100;

		UICanvas ui;
		ScreenSpaceCamera _camera;
		List<InspectorList> _inspectors = new List<InspectorList>();

		// ui fields
		Skin _skin;
		ScrollPane _scrollPane;
		Table _table;
		Entity _entity;


		/// <summary>
		/// creates a PostProcessor inspector
		/// </summary>
		public RuntimeInspector()
		{
			Initialize();
		}


		/// <summary>
		/// creates an Entity inspector
		/// </summary>
		/// <param name="entity">Entity.</param>
		public RuntimeInspector(Entity entity)
		{
			_entity = entity;
			Initialize();
			CacheTransformInspector();
		}


		void Initialize()
		{
			PrepCanvas();
			_camera = new ScreenSpaceCamera();
			Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
			Core.Emitter.AddObserver(CoreEvents.SceneChanged, OnSceneChanged);
		}


		void OnGraphicsDeviceReset()
		{
			_scrollPane.SetHeight(Screen.Height);
		}


		void OnSceneChanged()
		{
			Console.DebugConsole.Instance._runtimeInspector = null;
			Dispose();
		}


		public void Update()
		{
			// if we have an Entity this is an Entity inspector else it is a PostProcessor inspector
			if (_entity != null)
			{
				// update transform, which has a null Component
				GetOrCreateInspectorList((Component) null).Update();

				for (var i = 0; i < _entity.Components.Count; i++)
					GetOrCreateInspectorList(_entity.Components[i]).Update();
			}
			else
			{
				for (var i = 0; i < Core.Scene._postProcessors.Length; i++)
					GetOrCreateInspectorList(Core.Scene._postProcessors.Buffer[i]).Update();
			}
		}


		public void Render()
		{
			// manually start a fresh batch and call the UICanvas Component lifecycle methods since it isnt attached to the Scene
			Graphics.Instance.Batcher.Begin();
			(ui as IUpdatable).Update();
			ui.Render(Graphics.Instance.Batcher, _camera);
			Graphics.Instance.Batcher.End();
		}


		/// <summary>
		/// attempts to find a cached version of the InspectorList and if it cant find one it will create a new one
		/// </summary>
		/// <returns>The or create inspector list.</returns>
		/// <param name="comp">Comp.</param>
		InspectorList GetOrCreateInspectorList(object comp)
		{
			var inspector = _inspectors.FirstOrDefault(i => i.Target == comp);
			if (inspector == null)
			{
				inspector = new InspectorList(comp);
				inspector.Initialize(_table, _skin, kLeftCellWidth);
				_inspectors.Add(inspector);
			}

			return inspector;
		}


		void CacheTransformInspector()
		{
			// add Transform separately
			var transformInspector = new InspectorList(_entity.Transform);
			transformInspector.Initialize(_table, _skin, kLeftCellWidth);
			_inspectors.Add(transformInspector);
		}


		void PrepCanvas()
		{
			_skin = Skin.CreateDefaultSkin();

			// modify some of the default styles to better suit our needs
			var tfs = _skin.Get<TextFieldStyle>();
			tfs.Background.LeftWidth = tfs.Background.RightWidth = 4;
			tfs.Background.BottomHeight = 0;
			tfs.Background.TopHeight = 3;

			var checkbox = _skin.Get<CheckBoxStyle>();
			checkbox.CheckboxOn.MinWidth = checkbox.CheckboxOn.MinHeight = 15;
			checkbox.CheckboxOff.MinWidth = checkbox.CheckboxOff.MinHeight = 15;
			checkbox.CheckboxOver.MinWidth = checkbox.CheckboxOver.MinHeight = 15;

			// since we arent using this as a Component on an Entity we'll fake it here
			ui = new UICanvas();
			ui.OnAddedToEntity();
			ui.Stage.IsFullScreen = true;

			_table = new Table();
			_table.Top().Left();
			_table.Defaults().SetPadTop(4).SetPadLeft(4).SetPadRight(0).SetAlign(Align.Left);
			_table.SetBackground(new PrimitiveDrawable(new Color(40, 40, 40)));

			// wrap up the table in a ScrollPane
			_scrollPane = ui.Stage.AddElement(new ScrollPane(_table, _skin));

			// force a validate which will layout the ScrollPane and populate the proper scrollBarWidth
			_scrollPane.Validate();
			_scrollPane.SetSize(295 + _scrollPane.GetScrollBarWidth(), Screen.Height);
		}


		#region IDisposable Support

		bool _disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				Core.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
				Core.Emitter.RemoveObserver(CoreEvents.SceneChanged, OnSceneChanged);
				_entity = null;
				_disposedValue = true;
			}
		}


		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
#endif