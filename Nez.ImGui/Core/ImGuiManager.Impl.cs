using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;
using Nez.Persistence.Binary;


namespace Nez.ImGuiTools
{
	public partial class ImGuiManager : GlobalManager, IFinalRenderDelegate, IDisposable
	{
		const string kShowStyleEditor = "ImGui_ShowStyleEditor";
		const string kShowSceneGraphWindow = "ImGui_ShowSceneGraphWindow";
		const string kShowCoreWindow = "ImGui_ShowCoreWindow";
		const string kShowSeperateGameWindow = "ImGui_ShowSeperateGameWindow";

		[Flags]
		enum WindowPosition
		{
			TopLeft,
			Top,
			TopRight,
			Left,
			Center,
			Right,
			BottomLeft,
			Bottom,
			BottomRight
		}

		void LoadSettings()
		{
			var fileDataStore = Core.Services.GetService<FileDataStore>() ?? new FileDataStore(Nez.Storage.GetStorageRoot());
			KeyValueDataStore.Default.Load(fileDataStore);

			ShowStyleEditor = KeyValueDataStore.Default.GetBool(kShowStyleEditor, ShowStyleEditor);
			ShowSceneGraphWindow = KeyValueDataStore.Default.GetBool(kShowSceneGraphWindow, ShowSceneGraphWindow);
			ShowCoreWindow = KeyValueDataStore.Default.GetBool(kShowCoreWindow, ShowCoreWindow);
			ShowSeperateGameWindow = KeyValueDataStore.Default.GetBool(kShowSeperateGameWindow, ShowSeperateGameWindow);

			Core.Emitter.AddObserver(CoreEvents.Exiting, PersistSettings);
		}

		void PersistSettings()
		{
			KeyValueDataStore.Default.Set(kShowStyleEditor, ShowStyleEditor);
			KeyValueDataStore.Default.Set(kShowSceneGraphWindow, ShowSceneGraphWindow);
			KeyValueDataStore.Default.Set(kShowCoreWindow, ShowCoreWindow);
			KeyValueDataStore.Default.Set(kShowSeperateGameWindow, ShowSeperateGameWindow);

			KeyValueDataStore.Default.Flush(Core.Services.GetOrAddService<FileDataStore>());
		}

		/// <summary>
		/// here we do some cleanup in preparation for a new Scene
		/// </summary>
		void OnSceneChanged()
		{
			// when the Scene changes we need to rewire ourselves up as the IFinalRenderDelegate in the new Scene
			// if we were previously enabled and do some cleanup
			Unload();
			_sceneGraphWindow.OnSceneChanged();

			if (Enabled)
				OnEnabled();
		}

		void Unload()
		{
			_drawCommands.Clear();
			_entityInspectors.Clear();

			if (_renderTargetId != IntPtr.Zero)
			{
				_renderer.UnbindTexture(_renderTargetId);
				_renderTargetId = IntPtr.Zero;
			}

			_lastRenderTarget = null;
		}

		/// <summary>
		/// draws the game window and deals with overriding Nez.Input when appropriate
		/// </summary>
		void DrawGameWindow()
		{
			if (_lastRenderTarget == null)
				return;

			var rtAspectRatio = (float) _lastRenderTarget.Width / (float) _lastRenderTarget.Height;
			var maxSize = new Num.Vector2(_lastRenderTarget.Width, _lastRenderTarget.Height);
			if (maxSize.X >= Screen.Width || maxSize.Y >= Screen.Height)
			{
				maxSize.X = Screen.Width * 0.8f;
				maxSize.Y = maxSize.X / rtAspectRatio;
			}

			var minSize = maxSize / 4;
			maxSize *= 4;
			unsafe
			{
				ImGui.SetNextWindowSizeConstraints(minSize, maxSize, data =>
				{
					var size = (*data).CurrentSize;
					var ratio = size.X / _lastRenderTarget.Width;
					(*data).DesiredSize.Y = ratio * _lastRenderTarget.Height;
				});
			}

			ImGui.SetNextWindowPos(_gameWindowFirstPosition, ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSize(new Num.Vector2(Screen.Width / 2, (Screen.Width / 2) / rtAspectRatio), ImGuiCond.FirstUseEver);

			HandleForcedGameViewParams();

			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
			ImGui.Begin(_gameWindowTitle, _gameWindowFlags);

			// convert mouse input to the game windows coordinates
			OverrideMouseInput();

			if (!ImGui.IsWindowFocused())
			{
				bool focusedWindow = false;

				// if the window's being hovered and we click on it with any mouse button, optionally focus the window.
				if (ImGui.IsWindowHovered())
				{
					if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)
					|| (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && FocusGameWindowOnRightClick)
					|| (ImGui.IsMouseClicked(ImGuiMouseButton.Middle) && FocusGameWindowOnMiddleClick))
					{
						ImGui.SetWindowFocus();
						focusedWindow = true;
					}
				}

				// if we failed to focus the window in the previous step, intercept mouse and keyboard input.
				if (!focusedWindow)
				{
					var mouseState = new MouseState(
						Input.CurrentMouseState.X,
						Input.CurrentMouseState.Y,
						DisableMouseWheelWhenGameWindowUnfocused ? 0 : Input.MouseWheel,
						ButtonState.Released,
						ButtonState.Released,
						ButtonState.Released,
						ButtonState.Released,
						ButtonState.Released
					);
					Input.SetCurrentMouseState(mouseState);

					if (DisableKeyboardInputWhenGameWindowUnfocused)
					{
						Input.SetCurrentKeyboardState(new KeyboardState());
					}
				}
			}

			ImGui.End();

			ImGui.PopStyleVar();
		}

		/// <summary>
		/// handles any SetNextWindow* options chosen from a menu
		/// </summary>
		void HandleForcedGameViewParams()
		{
			if (_gameViewForcedSize.HasValue)
			{
				ImGui.SetNextWindowSize(_gameViewForcedSize.Value);
				_gameViewForcedSize = null;
			}

			if (_gameViewForcedPos.HasValue)
			{
				ImGui.Begin(_gameWindowTitle, _gameWindowFlags);
				var windowSize = ImGui.GetWindowSize();
				ImGui.End();

				var pos = new Num.Vector2();
				switch (_gameViewForcedPos.Value)
				{
					case WindowPosition.TopLeft:
						pos.Y = _mainMenuBarHeight;
						pos.X = 0;
						break;
					case WindowPosition.Top:
						pos.Y = _mainMenuBarHeight;
						pos.X = (Screen.Width / 2f) - (windowSize.X / 2f);
						break;
					case WindowPosition.TopRight:
						pos.Y = _mainMenuBarHeight;
						pos.X = Screen.Width - windowSize.X;
						break;
					case WindowPosition.Left:
						pos.Y = (Screen.Height / 2f) - (windowSize.Y / 2f);
						pos.X = 0;
						break;
					case WindowPosition.Center:
						pos.Y = (Screen.Height / 2f) - (windowSize.Y / 2f);
						pos.X = (Screen.Width / 2f) - (windowSize.X / 2f);
						break;
					case WindowPosition.Right:
						pos.Y = (Screen.Height / 2f) - (windowSize.Y / 2f);
						pos.X = Screen.Width - windowSize.X;
						break;
					case WindowPosition.BottomLeft:
						pos.Y = Screen.Height - windowSize.Y;
						pos.X = 0;
						break;
					case WindowPosition.Bottom:
						pos.Y = Screen.Height - windowSize.Y;
						pos.X = (Screen.Width / 2f) - (windowSize.X / 2f);
						break;
					case WindowPosition.BottomRight:
						pos.Y = Screen.Height - windowSize.Y;
						pos.X = Screen.Width - windowSize.X;
						break;
				}

				ImGui.SetNextWindowPos(pos);
				_gameViewForcedPos = null;
			}
		}

		/// <summary>
		/// converts the mouse position from global window position to the game window's coordinates and overrides Nez.Input with
		/// the new value. This keeps input working properly in the game window.
		/// </summary>
		void OverrideMouseInput()
		{
			// ImGui.GetCursorScreenPos() is the position of top-left pixel in windows drawable area
			var offset = new Vector2(ImGui.GetCursorScreenPos().X, ImGui.GetCursorScreenPos().Y);

			// remove window position offset from our raw input. this gets us normalized back to the top-left origin.
			// We are essentilly removing any input delta that is not in the game window.
			var normalizedPos = Input.RawMousePosition.ToVector2() - offset;

			var scaleX = ImGui.GetContentRegionAvail().X / _lastRenderTarget.Width;
			var scaleY = ImGui.GetContentRegionAvail().Y / _lastRenderTarget.Height;
			var scale = new Vector2(scaleX, scaleY);

			// scale the rest of the input since it is in a scaled window (the offset portion is not scaled since
			// it is outside the scaled portion)
			normalizedPos /= scale;


			// trick the input system. Take our normalizedPos and undo the scale and offsets (do the
			// reverse of what Input.scaledPosition does) so that any consumers of mouse input can get
			// the correct coordinates.
			var unNormalizedPos = normalizedPos / Input.ResolutionScale;
			unNormalizedPos += Input.ResolutionOffset;

			var mouseState = Input.CurrentMouseState;
			var newMouseState = new MouseState((int) unNormalizedPos.X, (int) unNormalizedPos.Y,
				mouseState.ScrollWheelValue,
				mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1,
				mouseState.XButton2);
			Input.SetCurrentMouseState(newMouseState);
		}


		#region GlobalManager Lifecycle

		public override void OnEnabled()
		{
			if (Core.Scene != null)
			{
				Core.Scene.FinalRenderDelegate = this;

				// why call beforeLayout here? If added from the DebugConsole we missed the GlobalManger.update call and ImGui needs NextFrame
				// called or it fails. Calling NextFrame twice in a frame causes no harm, just missed input.
				_renderer.BeforeLayout(Time.DeltaTime);
			}
		}

		public override void OnDisabled()
		{
			Unload();
			if (Core.Scene != null)
				Core.Scene.FinalRenderDelegate = null;
		}

		public override void Update()
		{
			// we have to do our layout in update so that if the game window is not focused or being displayed we can wipe
			// the Input, essentially letting ImGui consume it
			_renderer.BeforeLayout(Time.DeltaTime);
			LayoutGui();
		}

		#endregion


		#region IFinalRenderDelegate

		void IFinalRenderDelegate.HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor,
		                                            RenderTarget2D source, Rectangle finalRenderDestinationRect,
		                                            SamplerState samplerState)
		{
			if (ShowSeperateGameWindow)
			{
				if (_lastRenderTarget != source)
				{
					// unbind the old texture if we had one
					if (_lastRenderTarget != null)
						_renderer.UnbindTexture(_renderTargetId);

					// bind the new texture
					_lastRenderTarget = source;
					_renderTargetId = _renderer.BindTexture(source);
				}

				// we cant draw the game window until we have the texture bound so we append it here
				ImGui.Begin(_gameWindowTitle, _gameWindowFlags);
				ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
				ImGui.ImageButton("SeparateGameWindowImageButton", _renderTargetId, ImGui.GetContentRegionAvail());
				ImGui.PopStyleVar();
				ImGui.End();

				Core.GraphicsDevice.SamplerStates[0] = samplerState;
				Core.GraphicsDevice.SetRenderTarget(finalRenderTarget);
				Core.GraphicsDevice.Clear(letterboxColor);
			}
			else
			{
				Core.GraphicsDevice.SetRenderTarget(finalRenderTarget);
				Core.GraphicsDevice.Clear(letterboxColor);
				Graphics.Instance.Batcher.Begin(BlendState.Opaque, samplerState, null, null);
				Graphics.Instance.Batcher.Draw(source, finalRenderDestinationRect, Color.White);
				Graphics.Instance.Batcher.End();
			}

			_renderer.AfterLayout();
		}

		void IFinalRenderDelegate.OnAddedToScene(Scene scene)
		{
		}

		void IFinalRenderDelegate.OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
		}

		void IFinalRenderDelegate.Unload()
		{
		}

		#endregion


		#region IDisposable Support

		bool _isDisposed = false; // To detect redundant calls

		void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					Core.Emitter.RemoveObserver(CoreEvents.SceneChanged, OnSceneChanged);
				}

				_isDisposed = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		#endregion

		[Console.Command("toggle-imgui", "Toggles the Dear ImGui renderer")]
		public static void ToggleImGui()
		{
			// install the service if it isnt already there
			var service = Core.GetGlobalManager<ImGuiManager>();
			if (service == null)
			{
				service = new ImGuiManager();
				Core.RegisterGlobalManager(service);
			}
			else
			{
				service.SetEnabled(!service.Enabled);
			}
		}
	}
}
