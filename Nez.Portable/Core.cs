using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Nez.Systems;
using Nez.Console;
using Nez.Tweens;
using Nez.Timers;
using Nez.BitmapFonts;
using Nez.Analysis;
using Nez.Textures;
using System.Diagnostics;


[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nez.ImGui")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nez.Persistence")]


namespace Nez
{
	public class Core : Game
	{
		/// <summary>
		/// core emitter. emits only Core level events.
		/// </summary>
		public static Emitter<CoreEvents> Emitter;

		/// <summary>
		/// enables/disables if we should quit the app when escape is pressed
		/// </summary>
		public static bool ExitOnEscapeKeypress = true;

		/// <summary>
		/// enables/disables pausing when focus is lost. No update or render methods will be called if true when not in focus.
		/// </summary>
		public static bool PauseOnFocusLost = true;

		/// <summary>
		/// enables/disables debug rendering
		/// </summary>
		public static bool DebugRenderEnabled = false;

		/// <summary>
		/// global access to the graphicsDevice
		/// </summary>
		public new static GraphicsDevice GraphicsDevice;

		/// <summary>
		/// global content manager for loading any assets that should stick around between scenes
		/// </summary>
		public new static NezContentManager Content;

		/// <summary>
		/// default SamplerState used by Materials. Note that this must be set at launch! Changing it after that time will result in only
		/// Materials created after it was set having the new SamplerState
		/// </summary>
		public static SamplerState DefaultSamplerState = new SamplerState
		{
			Filter = TextureFilter.Point
		};

		/// <summary>
		/// default wrapped SamplerState. Determined by the Filter of the defaultSamplerState.
		/// </summary>
		/// <value>The default state of the wraped sampler.</value>
		public static SamplerState DefaultWrappedSamplerState =>
			DefaultSamplerState.Filter == TextureFilter.Point
				? SamplerState.PointWrap
				: SamplerState.LinearWrap;

		/// <summary>
		/// default GameServiceContainer access
		/// </summary>
		/// <value>The services.</value>
		public new static GameServiceContainer Services => ((Game) _instance).Services;

		/// <summary>
		/// provides access to the single Core/Game instance
		/// </summary>
		public static Core Instance => _instance;

		/// <summary>
		/// facilitates easy access to the global Content instance for internal classes
		/// </summary>
		internal static Core _instance;

		/// <summary>
		/// internal flag used to determine if EntitySystems should be used or not
		/// </summary>
		internal static bool entitySystemsEnabled;

#if DEBUG
		internal static long drawCalls;
		TimeSpan _frameCounterElapsedTime = TimeSpan.Zero;
		int _frameCounter = 0;
		string _windowTitle;
#endif

		Scene _scene;
		Scene _nextScene;
		internal SceneTransition _sceneTransition;

		/// <summary>
		/// used to coalesce GraphicsDeviceReset events
		/// </summary>
		ITimer _graphicsDeviceChangeTimer;

		// globally accessible systems
		FastList<GlobalManager> _globalManagers = new FastList<GlobalManager>();
		CoroutineManager _coroutineManager = new CoroutineManager();
		TimerManager _timerManager = new TimerManager();


		/// <summary>
		/// The currently active Scene. Note that if set, the Scene will not actually change until the end of the Update
		/// </summary>
		public static Scene Scene
		{
			get => _instance._scene;
			set
			{
				Insist.IsNotNull(value, "Scene cannot be null!");

				// handle our initial Scene. If we have no Scene and one is assigned directly wire it up
				if (_instance._scene == null)
				{
					_instance._scene = value;
					_instance._scene.Begin();
					_instance.OnSceneChanged();
				}
				else
				{
					_instance._nextScene = value;
				}
			}
		}


		public Core(int width = 1280, int height = 720, bool isFullScreen = false, bool enableEntitySystems = true,
		            string windowTitle = "Nez", string contentDirectory = "Content")
		{
#if DEBUG
			_windowTitle = windowTitle;
#endif

			_instance = this;
			Emitter = new Emitter<CoreEvents>(new CoreEventsComparer());

			var graphicsManager = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = width,
				PreferredBackBufferHeight = height,
				IsFullScreen = isFullScreen,
				SynchronizeWithVerticalRetrace = true
			};
			graphicsManager.DeviceReset += OnGraphicsDeviceReset;
			graphicsManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

			Screen.Initialize(graphicsManager);
			Window.ClientSizeChanged += OnGraphicsDeviceReset;
			Window.OrientationChanged += OnOrientationChanged;

			base.Content.RootDirectory = contentDirectory;
			Content = new NezGlobalContentManager(Services, base.Content.RootDirectory);
			IsMouseVisible = true;
			IsFixedTimeStep = false;

			entitySystemsEnabled = enableEntitySystems;

			// setup systems
			RegisterGlobalManager(_coroutineManager);
			RegisterGlobalManager(new TweenManager());
			RegisterGlobalManager(_timerManager);
			RegisterGlobalManager(new RenderTarget());
		}

		void OnOrientationChanged(object sender, EventArgs e)
		{
			Emitter.Emit(CoreEvents.OrientationChanged);
		}

		/// <summary>
		/// this gets called whenever the screen size changes
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void OnGraphicsDeviceReset(object sender, EventArgs e)
		{
			// we coalese these to avoid spamming events
			if (_graphicsDeviceChangeTimer != null)
			{
				_graphicsDeviceChangeTimer.Reset();
			}
			else
			{
				_graphicsDeviceChangeTimer = Schedule(0.05f, false, this, t =>
				{
					(t.Context as Core)._graphicsDeviceChangeTimer = null;
					Emitter.Emit(CoreEvents.GraphicsDeviceReset);
				});
			}
		}


		#region Passthroughs to Game

		public new static void Exit()
		{
			((Game) _instance).Exit();
		}

		#endregion


		#region Game overides

		protected override void Initialize()
		{
			base.Initialize();

			// prep the default Graphics system
			GraphicsDevice = base.GraphicsDevice;
			var font = Content.Load<BitmapFont>("nez://Nez.Content.NezDefaultBMFont.xnb");
			Graphics.Instance = new Graphics(font);
		}

		protected override void Update(GameTime gameTime)
		{
			if (PauseOnFocusLost && !IsActive)
			{
				SuppressDraw();
				return;
			}

			StartDebugUpdate();

			// update all our systems and global managers
			Time.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
			Input.Update();

			if (ExitOnEscapeKeypress &&
			    (Input.IsKeyDown(Keys.Escape) || Input.GamePads[0].IsButtonReleased(Buttons.Back)))
			{
				base.Exit();
				return;
			}

			if (_scene != null)
			{
				for (var i = _globalManagers.Length - 1; i >= 0; i--)
				{
					if (_globalManagers.Buffer[i].Enabled)
						_globalManagers.Buffer[i].Update();
				}

				// read carefully:
				// - we do not update the Scene while a SceneTransition is happening
				// 		- unless it is SceneTransition that doesn't change Scenes (no reason not to update)
				//		- or it is a SceneTransition that has already switched to the new Scene (the new Scene needs to do its thing)
				if (_sceneTransition == null ||
				    (_sceneTransition != null &&
				     (!_sceneTransition._loadsNewScene || _sceneTransition._isNewSceneLoaded)))
				{
					_scene.Update();
				}

				if (_nextScene != null)
				{
					_scene.End();

					_scene = _nextScene;
					_nextScene = null;
					OnSceneChanged();

					_scene.Begin();
				}
			}

			EndDebugUpdate();

#if FNA
			// MonoGame only updates old-school XNA Components in Update which we dont care about. FNA's core FrameworkDispatcher needs
			// Update called though so we do so here.
			FrameworkDispatcher.Update();
#endif
		}

		protected override void Draw(GameTime gameTime)
		{
			if (PauseOnFocusLost && !IsActive)
				return;

			StartDebugDraw(gameTime.ElapsedGameTime);

			if (_sceneTransition != null)
				_sceneTransition.PreRender(Graphics.Instance.Batcher);

			// special handling of SceneTransition if we have one. We either render the SceneTransition or the Scene
			if (_sceneTransition != null)
			{
				if (_scene != null && _sceneTransition.WantsPreviousSceneRender &&
				    !_sceneTransition.HasPreviousSceneRender)
				{
					_scene.Render();
					_scene.PostRender(_sceneTransition.PreviousSceneRender);
					StartCoroutine(_sceneTransition.OnBeginTransition());
				}
				else if (_scene != null && _sceneTransition._isNewSceneLoaded)
				{
					_scene.Render();
					_scene.PostRender();
				}

				_sceneTransition.Render(Graphics.Instance.Batcher);
			}
			else if (_scene != null)
			{
				_scene.Render();

#if DEBUG
				if (DebugRenderEnabled)
					Debug.Render();
#endif

				// render as usual if we dont have an active SceneTransition
				_scene.PostRender();
			}

			EndDebugDraw();
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			base.OnExiting(sender, args);
			Emitter.Emit(CoreEvents.Exiting);
		}

		#endregion

		#region Debug Injection

		[Conditional("DEBUG")]
		void StartDebugUpdate()
		{
#if DEBUG
			TimeRuler.Instance.StartFrame();
			TimeRuler.Instance.BeginMark("update", Color.Green);
#endif
		}

		[Conditional("DEBUG")]
		void EndDebugUpdate()
		{
#if DEBUG
			TimeRuler.Instance.EndMark("update");
			DebugConsole.Instance.Update();
			drawCalls = 0;
#endif
		}

		[Conditional("DEBUG")]
		void StartDebugDraw(TimeSpan elapsedGameTime)
		{
#if DEBUG
			TimeRuler.Instance.BeginMark("draw", Color.Gold);

			// fps counter
			_frameCounter++;
			_frameCounterElapsedTime += elapsedGameTime;
			if (_frameCounterElapsedTime >= TimeSpan.FromSeconds(1))
			{
				var totalMemory = (GC.GetTotalMemory(false) / 1048576f).ToString("F");
				Window.Title = string.Format("{0} {1} fps - {2} MB", _windowTitle, _frameCounter, totalMemory);
				_frameCounter = 0;
				_frameCounterElapsedTime -= TimeSpan.FromSeconds(1);
			}
#endif
		}

		[Conditional("DEBUG")]
		void EndDebugDraw()
		{
#if DEBUG
			TimeRuler.Instance.EndMark("draw");
			DebugConsole.Instance.Render();

			// the TimeRuler only needs to render when the DebugConsole is not open
			if (!DebugConsole.Instance.IsOpen)
				TimeRuler.Instance.Render();

#if !FNA
			drawCalls = GraphicsDevice.Metrics.DrawCount;
#endif
#endif
		}

		#endregion

		/// <summary>
		/// Called after a Scene ends, before the next Scene begins
		/// </summary>
		void OnSceneChanged()
		{
			Emitter.Emit(CoreEvents.SceneChanged);
			Time.SceneChanged();
			GC.Collect();
		}

		/// <summary>
		/// temporarily runs SceneTransition allowing one Scene to transition to another smoothly with custom effects.
		/// </summary>
		/// <param name="sceneTransition">Scene transition.</param>
		public static T StartSceneTransition<T>(T sceneTransition) where T : SceneTransition
		{
			Insist.IsNull(_instance._sceneTransition,
				"You cannot start a new SceneTransition until the previous one has completed");
			_instance._sceneTransition = sceneTransition;
			return sceneTransition;
		}


		#region Global Managers

		/// <summary>
		/// adds a global manager object that will have its update method called each frame before Scene.update is called
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <param name="manager">Manager.</param>
		public static void RegisterGlobalManager(GlobalManager manager)
		{
			_instance._globalManagers.Add(manager);
			manager.Enabled = true;
		}

		/// <summary>
		/// removes the global manager object
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <param name="manager">Manager.</param>
		public static void UnregisterGlobalManager(GlobalManager manager)
		{
			_instance._globalManagers.Remove(manager);
			manager.Enabled = false;
		}

		/// <summary>
		/// gets the global manager of type T
		/// </summary>
		/// <returns>The global manager.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetGlobalManager<T>() where T : GlobalManager
		{
			for (var i = 0; i < _instance._globalManagers.Length; i++)
			{
				if (_instance._globalManagers.Buffer[i] is T)
					return _instance._globalManagers.Buffer[i] as T;
			}

			return null;
		}

		#endregion


		#region Systems access

		/// <summary>
		/// starts a coroutine. Coroutines can yeild ints/floats to delay for seconds or yeild to other calls to startCoroutine.
		/// Yielding null will make the coroutine get ticked the next frame.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="enumerator">Enumerator.</param>
		public static ICoroutine StartCoroutine(IEnumerator enumerator)
		{
			return _instance._coroutineManager.StartCoroutine(enumerator);
		}

		/// <summary>
		/// schedules a one-time or repeating timer that will call the passed in Action
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="repeats">If set to <c>true</c> repeats.</param>
		/// <param name="context">Context.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer Schedule(float timeInSeconds, bool repeats, object context, Action<ITimer> onTime)
		{
			return _instance._timerManager.Schedule(timeInSeconds, repeats, context, onTime);
		}

		/// <summary>
		/// schedules a one-time timer that will call the passed in Action after timeInSeconds
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="context">Context.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer Schedule(float timeInSeconds, object context, Action<ITimer> onTime)
		{
			return _instance._timerManager.Schedule(timeInSeconds, false, context, onTime);
		}

		/// <summary>
		/// schedules a one-time or repeating timer that will call the passed in Action
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="repeats">If set to <c>true</c> repeats.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer Schedule(float timeInSeconds, bool repeats, Action<ITimer> onTime)
		{
			return _instance._timerManager.Schedule(timeInSeconds, repeats, null, onTime);
		}

		/// <summary>
		/// schedules a one-time timer that will call the passed in Action after timeInSeconds
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="onTime">On time.</param>
		public static ITimer Schedule(float timeInSeconds, Action<ITimer> onTime)
		{
			return _instance._timerManager.Schedule(timeInSeconds, false, null, onTime);
		}

		#endregion
	}
}