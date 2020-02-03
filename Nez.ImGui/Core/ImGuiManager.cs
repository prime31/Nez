using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;


namespace Nez.ImGuiTools
{
	public partial class ImGuiManager : GlobalManager, IFinalRenderDelegate, IDisposable
	{
		public bool ShowDemoWindow = false;
		public bool ShowStyleEditor = false;
		public bool ShowSceneGraphWindow = true;
		public bool ShowCoreWindow = true;
		public bool ShowSeperateGameWindow = true;
		public bool ShowMenuBar = true;

		List<Type> _sceneSubclasses = new List<Type>();
		System.Reflection.MethodInfo[] _themes;

		CoreWindow _coreWindow = new CoreWindow();
		SceneGraphWindow _sceneGraphWindow = new SceneGraphWindow();
		SpriteAtlasEditorWindow _spriteAtlasEditorWindow;

		List<EntityInspector> _entityInspectors = new List<EntityInspector>();
		List<Action> _drawCommands = new List<Action>();
		ImGuiRenderer _renderer;

		Num.Vector2 _gameWindowFirstPosition;
		string _gameWindowTitle;
		ImGuiWindowFlags _gameWindowFlags = 0;

		RenderTarget2D _lastRenderTarget;
		IntPtr _renderTargetId = IntPtr.Zero;
		Num.Vector2? _gameViewForcedSize;
		WindowPosition? _gameViewForcedPos;
		float _mainMenuBarHeight;

		public ImGuiManager(ImGuiOptions options = null)
		{
			if (options == null)
				options = new ImGuiOptions();

			_gameWindowFirstPosition = options._gameWindowFirstPosition;
			_gameWindowTitle = options._gameWindowTitle;
			_gameWindowFlags = options._gameWindowFlags;

			LoadSettings();
			_renderer = new ImGuiRenderer(Core.Instance);

			_renderer.RebuildFontAtlas(options);
			Core.Emitter.AddObserver(CoreEvents.SceneChanged, OnSceneChanged);
			NezImGuiThemes.DarkTheme1();

			// find all Scenes
			_sceneSubclasses = ReflectionUtils.GetAllSubclasses(typeof(Scene), true);

			// tone down indent
			ImGui.GetStyle().IndentSpacing = 12;
			ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

			// find all themes
			_themes = typeof(NezImGuiThemes).GetMethods(System.Reflection.BindingFlags.Static |
			                                            System.Reflection.BindingFlags.Public);
		}

		/// <summary>
		/// this is where we issue any and all ImGui commands to be drawn
		/// </summary>
		void LayoutGui()
		{
			if (ShowMenuBar)
				DrawMainMenuBar();

			if (ShowSeperateGameWindow)
				DrawGameWindow();
			DrawEntityInspectors();

			for (var i = _drawCommands.Count - 1; i >= 0; i--)
				_drawCommands[i]();

			_sceneGraphWindow.Show(ref ShowSceneGraphWindow);
			_coreWindow.Show(ref ShowCoreWindow);

			if (_spriteAtlasEditorWindow != null)
			{
				if (!_spriteAtlasEditorWindow.Show())
					_spriteAtlasEditorWindow = null;
			}

			if (ShowDemoWindow)
				ImGui.ShowDemoWindow(ref ShowDemoWindow);

			if (ShowStyleEditor)
			{
				ImGui.Begin("Style Editor", ref ShowStyleEditor);
				ImGui.ShowStyleEditor();
				ImGui.End();
			}
		}

		/// <summary>
		/// draws the main menu bar
		/// </summary>
		void DrawMainMenuBar()
		{
			if (ImGui.BeginMainMenuBar())
			{
				_mainMenuBarHeight = ImGui.GetWindowHeight();
				if (ImGui.BeginMenu("File"))
				{
					if (ImGui.MenuItem("Open Sprite Atlas Editor"))
						_spriteAtlasEditorWindow = _spriteAtlasEditorWindow ?? new SpriteAtlasEditorWindow();

					if (ImGui.MenuItem("Quit ImGui"))
						SetEnabled(false);
					ImGui.EndMenu();
				}

				if (_sceneSubclasses.Count > 0 && ImGui.BeginMenu("Scenes"))
				{
					foreach (var sceneType in _sceneSubclasses)
					{
						if (ImGui.MenuItem(sceneType.Name))
						{
							var scene = (Scene) Activator.CreateInstance(sceneType);
							Core.StartSceneTransition(new FadeTransition(() => scene));
						}
					}

					ImGui.EndMenu();
				}

				if (_themes.Length > 0 && ImGui.BeginMenu("Themes"))
				{
					foreach (var theme in _themes)
					{
						if (ImGui.MenuItem(theme.Name))
							theme.Invoke(null, new object[] { });
					}

					ImGui.EndMenu();
				}

				if (ImGui.BeginMenu("Game View"))
				{
					var rtSize = Core.Scene.SceneRenderTargetSize;

					if (ImGui.BeginMenu("Resize"))
					{
						if (ImGui.MenuItem("0.25x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 4f, rtSize.Y / 4f);
						if (ImGui.MenuItem("0.5x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 2f, rtSize.Y / 2f);
						if (ImGui.MenuItem("0.75x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X / 1.33f, rtSize.Y / 1.33f);
						if (ImGui.MenuItem("1x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X, rtSize.Y);
						if (ImGui.MenuItem("1.5x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 1.5f, rtSize.Y * 1.5f);
						if (ImGui.MenuItem("2x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 2, rtSize.Y * 2);
						if (ImGui.MenuItem("3x"))
							_gameViewForcedSize = new Num.Vector2(rtSize.X * 3, rtSize.Y * 3);
						ImGui.EndMenu();
					}

					if (ImGui.BeginMenu("Reposition"))
					{
						foreach (var pos in Enum.GetNames(typeof(WindowPosition)))
						{
							if (ImGui.MenuItem(pos))
								_gameViewForcedPos = (WindowPosition) Enum.Parse(typeof(WindowPosition), pos);
						}

						ImGui.EndMenu();
					}


					ImGui.EndMenu();
				}

				if (ImGui.BeginMenu("Window"))
				{
					ImGui.MenuItem("ImGui Demo Window", null, ref ShowDemoWindow);
					ImGui.MenuItem("Style Editor", null, ref ShowStyleEditor);
					if (ImGui.MenuItem("Open imgui_demo.cpp on GitHub"))
					{
						System.Diagnostics.Process.Start("https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp");
					}

					ImGui.Separator();
					ImGui.MenuItem("Core Window", null, ref ShowCoreWindow);
					ImGui.MenuItem("Scene Graph Window", null, ref ShowSceneGraphWindow);
					ImGui.MenuItem("Separate Game Window", null, ref ShowSeperateGameWindow);
					ImGui.EndMenu();
				}

				ImGui.EndMainMenuBar();
			}
		}

		/// <summary>
		/// draws all the EntityInspectors
		/// </summary>
		void DrawEntityInspectors()
		{
			for (var i = _entityInspectors.Count - 1; i >= 0; i--)
				_entityInspectors[i].Draw();
		}


		#region Public API

		/// <summary>
		/// registers an Action that will be called and any ImGui drawing can be done in it
		/// </summary>
		/// <param name="drawCommand"></param>
		public void RegisterDrawCommand(Action drawCommand) => _drawCommands.Add(drawCommand);

		/// <summary>
		/// removes the Action from the draw commands
		/// </summary>
		/// <param name="drawCommand"></param>
		public void UnregisterDrawCommand(Action drawCommand) => _drawCommands.Remove(drawCommand);

		/// <summary>
		/// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image" />.
		/// That pointer is then used by ImGui to let us know what texture to draw
		/// </summary>
		/// <param name="textureId"></param>
		public void UnbindTexture(IntPtr textureId) => _renderer.UnbindTexture(textureId);

		/// <summary>
		/// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public IntPtr BindTexture(Texture2D texture) => _renderer.BindTexture(texture);

		/// <summary>
		/// creates an EntityInspector window
		/// </summary>
		/// <param name="entity"></param>
		public void StartInspectingEntity(Entity entity)
		{
			// if we are already inspecting the Entity focus the window
			foreach (var inspector in _entityInspectors)
			{
				if (inspector.Entity == entity)
				{
					inspector.SetWindowFocus();
					return;
				}
			}

			var entityInspector = new EntityInspector(entity);
			entityInspector.SetWindowFocus();
			_entityInspectors.Add(entityInspector);
		}

		/// <summary>
		/// removes the EntityInspector for this Entity
		/// </summary>
		/// <param name="entity"></param>
		public void StopInspectingEntity(Entity entity)
		{
			for (var i = 0; i < _entityInspectors.Count; i++)
			{
				var inspector = _entityInspectors[i];
				if (inspector.Entity == entity)
				{
					_entityInspectors.RemoveAt(i);
					return;
				}
			}
		}

		/// <summary>
		/// removes the EntityInspector
		/// </summary>
		/// <param name="entityInspector"></param>
		public void StopInspectingEntity(EntityInspector entityInspector)
		{
			_entityInspectors.RemoveAt(_entityInspectors.IndexOf(entityInspector));
		}

		#endregion
	}
}