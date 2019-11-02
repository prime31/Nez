using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Systems;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Scene
	{
		public enum SceneResolutionPolicy
		{
			/// <summary>
			/// Default. RenderTarget matches the sceen size
			/// </summary>
			None,

			/// <summary>
			/// The entire application is visible in the specified area without trying to preserve the original aspect ratio.
			/// Distortion can occur, and the application may appear stretched or compressed.
			/// </summary>
			ExactFit,

			/// <summary>
			/// The entire application fills the specified area, without distortion but possibly with some cropping,
			/// while maintaining the original aspect ratio of the application.
			/// </summary>
			NoBorder,

			/// <summary>
			/// Pixel perfect version of NoBorder. Scaling is limited to integer values.
			/// </summary>
			NoBorderPixelPerfect,

			/// <summary>
			/// The entire application is visible in the specified area without distortion while maintaining the original
			/// aspect ratio of the application. Borders can appear on two sides of the application.
			/// </summary>
			ShowAll,

			/// <summary>
			/// Pixel perfect version of ShowAll. Scaling is limited to integer values.
			/// </summary>
			ShowAllPixelPerfect,

			/// <summary>
			/// The application takes the height of the design resolution size and modifies the width of the internal
			/// canvas so that it fits the aspect ratio of the device.
			/// no distortion will occur however you must make sure your application works on different
			/// aspect ratios
			/// </summary>
			FixedHeight,

			/// <summary>
			/// Pixel perfect version of FixedHeight. Scaling is limited to integer values.
			/// </summary>
			FixedHeightPixelPerfect,

			/// <summary>
			/// The application takes the width of the design resolution size and modifies the height of the internal
			/// canvas so that it fits the aspect ratio of the device.
			/// no distortion will occur however you must make sure your application works on different
			/// aspect ratios
			/// </summary>
			FixedWidth,

			/// <summary>
			/// Pixel perfect version of FixedWidth. Scaling is limited to integer values.
			/// </summary>
			FixedWidthPixelPerfect,

			/// <summary>
			/// The application takes the width and height that best fits the design resolution with optional cropping inside of the "bleed area"
			/// and possible letter/pillar boxing. Works just like ShowAll except with horizontal/vertical bleed (padding). Gives you an area much
			/// like the old TitleSafeArea. Example: if design resolution is 1348x900 and bleed is 148x140 the safe area would be 1200x760 (design
			/// resolution - bleed).
			/// </summary>
			BestFit
		}


		/// <summary>
		/// default scene Camera
		/// </summary>
		public Camera Camera;

		/// <summary>
		/// clear color that is used in preRender to clear the screen
		/// </summary>
		public Color ClearColor = Color.CornflowerBlue;

		/// <summary>
		/// clear color for the final render of the RenderTarget to the framebuffer
		/// </summary>
		public Color LetterboxColor = Color.Black;

		/// <summary>
		/// SamplerState used for the final draw of the RenderTarget to the framebuffer
		/// </summary>
		public SamplerState SamplerState = Core.DefaultSamplerState;

		/// <summary>
		/// Scene-specific ContentManager. Use it to load up any resources that are needed only by this scene. If you have global/multi-scene
		/// resources you can use Core.contentManager to load them since Nez will not ever unload them.
		/// </summary>
		public readonly NezContentManager Content;

		/// <summary>
		/// global toggle for PostProcessors
		/// </summary>
		public bool EnablePostProcessing = true;

		/// <summary>
		/// The list of entities within this Scene
		/// </summary>
		public readonly EntityList Entities;

		/// <summary>
		/// Manages a list of all the RenderableComponents that are currently on scene Entitys
		/// </summary>
		public readonly RenderableComponentList RenderableComponents;

		/// <summary>
		/// Stoes and manages all entity processors
		/// </summary>
		public readonly EntityProcessorList EntityProcessors;

		/// <summary>
		/// gets the size of the sceneRenderTarget
		/// </summary>
		/// <value>The size of the scene render texture.</value>
		public Point SceneRenderTargetSize =>
			new Point(_sceneRenderTarget.Bounds.Width, _sceneRenderTarget.Bounds.Height);

		/// <summary>
		/// accesses the main scene RenderTarget. Some Renderers that use multiple RenderTargets may need to render into them first and then
		/// render the result into the sceneRenderTarget.
		/// </summary>
		/// <value>The scene render target.</value>
		public RenderTarget2D SceneRenderTarget => _sceneRenderTarget;

		/// <summary>
		/// if the ResolutionPolicy is pixel perfect this will be set to the scale calculated for it
		/// </summary>
		public int PixelPerfectScale = 1;

		/// <summary>
		/// the final render to the screen can be deferred to this delegate if set. This is really only useful for cases where the final render
		/// might need a full screen size effect even though a small back buffer is used.
		/// </summary>
		/// <value>The final render delegate.</value>
		public IFinalRenderDelegate FinalRenderDelegate
		{
			set
			{
				if (_finalRenderDelegate != null)
					_finalRenderDelegate.Unload();

				_finalRenderDelegate = value;

				if (_finalRenderDelegate != null)
					_finalRenderDelegate.OnAddedToScene(this);
			}
			get => _finalRenderDelegate;
		}

		IFinalRenderDelegate _finalRenderDelegate;


		#region SceneResolutionPolicy private fields

		/// <summary>
		/// default resolution size used for all scenes
		/// </summary>
		static Point _defaultDesignResolutionSize;

		/// <summary>
		/// default bleed size for <see cref="SceneResolutionPolicy.BestFit"/> resolution policy
		/// </summary>
		static Point _defaultDesignBleedSize;

		/// <summary>
		/// default resolution policy used for all scenes
		/// </summary>
		static SceneResolutionPolicy _defaultSceneResolutionPolicy = SceneResolutionPolicy.None;

		/// <summary>
		/// resolution policy used by the scene
		/// </summary>
		SceneResolutionPolicy _resolutionPolicy;

		/// <summary>
		/// design resolution size used by the scene
		/// </summary>
		Point _designResolutionSize;

		/// <summary>
		/// bleed size for <see cref="SceneResolutionPolicy.BestFit"/> resolution policy
		/// </summary>
		Point _designBleedSize;

		/// <summary>
		/// this gets setup based on the resolution policy and is used for the final blit of the RenderTarget
		/// </summary>
		Rectangle _finalRenderDestinationRect;

		#endregion


		RenderTarget2D _sceneRenderTarget;
		RenderTarget2D _destinationRenderTarget;
		Action<Texture2D> _screenshotRequestCallback;

		internal readonly FastList<SceneComponent> _sceneComponents = new FastList<SceneComponent>();
		internal FastList<Renderer> _renderers = new FastList<Renderer>();
		internal readonly FastList<Renderer> _afterPostProcessorRenderers = new FastList<Renderer>();
		internal readonly FastList<PostProcessor> _postProcessors = new FastList<PostProcessor>();
		bool _didSceneBegin;


		/// <summary>
		/// sets the default design size and resolution policy that new scenes will use. horizontal/verticalBleed are only relevant for BestFit.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="sceneResolutionPolicy">Scene resolution policy.</param>
		/// <param name="horizontalBleed">Horizontal bleed size. Used only if resolution policy is set to <see cref="SceneResolutionPolicy.BestFit"/>.</param>
		/// <param name="verticalBleed">Vertical bleed size. Used only if resolution policy is set to <see cref="SceneResolutionPolicy.BestFit"/>.</param>
		public static void SetDefaultDesignResolution(int width, int height,
		                                              SceneResolutionPolicy sceneResolutionPolicy,
		                                              int horizontalBleed = 0, int verticalBleed = 0)
		{
			_defaultDesignResolutionSize = new Point(width, height);
			_defaultSceneResolutionPolicy = sceneResolutionPolicy;
			if (_defaultSceneResolutionPolicy == SceneResolutionPolicy.BestFit)
				_defaultDesignBleedSize = new Point(horizontalBleed, verticalBleed);
		}


		#region Scene creation helpers

		/// <summary>
		/// helper that creates a scene with the DefaultRenderer attached and ready for use
		/// </summary>
		/// <returns>The with default renderer.</returns>
		public static Scene CreateWithDefaultRenderer(Color? clearColor = null)
		{
			var scene = new Scene();

			if (clearColor.HasValue)
				scene.ClearColor = clearColor.Value;
			scene.AddRenderer(new DefaultRenderer());
			return scene;
		}

		/// <summary>
		/// helper that creates a scene of type T with the DefaultRenderer attached and ready for use
		/// </summary>
		/// <returns>The with default renderer.</returns>
		[Obsolete("use new Scene() instead")]
		public static T CreateWithDefaultRenderer<T>(Color? clearColor = null) where T : Scene, new()
		{
			var scene = new T();

			if (clearColor.HasValue)
				scene.ClearColor = clearColor.Value;
			scene.AddRenderer(new DefaultRenderer());
			return scene;
		}

		/// <summary>
		/// helper that creates a scene with no Renderer
		/// </summary>
		/// <returns>The with default renderer.</returns>
		[Obsolete("use new Scene() instead")]
		public static Scene Create(Color? clearColor = null)
		{
			var scene = new Scene();

			if (clearColor.HasValue)
				scene.ClearColor = clearColor.Value;

			return scene;
		}

		/// <summary>
		/// helper that creates a scene of type T with no Renderer
		/// </summary>
		/// <returns>The with default renderer.</returns>
		[Obsolete("use new Scene() instead")]
		public static T Create<T>(Color? clearColor = null) where T : Scene, new()
		{
			var scene = new T();

			if (clearColor.HasValue)
				scene.ClearColor = clearColor.Value;

			return scene;
		}

		#endregion


		public Scene()
		{
			Entities = new EntityList(this);
			RenderableComponents = new RenderableComponentList();
			Content = new NezContentManager();

			var cameraEntity = CreateEntity("camera");
			Camera = cameraEntity.AddComponent(new Camera());

			if (Core.entitySystemsEnabled)
				EntityProcessors = new EntityProcessorList();

			// setup our resolution policy. we'll commit it in begin
			_resolutionPolicy = _defaultSceneResolutionPolicy;
			_designResolutionSize = _defaultDesignResolutionSize;
			_designBleedSize = _defaultDesignBleedSize;

			Initialize();
		}


		#region Scene lifecycle

		/// <summary>
		/// override this in Scene subclasses and do your loading here. This is called from the contructor after the scene sets itself up but
		/// before begin is ever called.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// override this in Scene subclasses. this will be called when Core sets this scene as the active scene.
		/// </summary>
		public virtual void OnStart()
		{
		}

		/// <summary>
		/// override this in Scene subclasses and do any unloading necessary here. this is called when Core removes this scene from the active slot.
		/// </summary>
		public virtual void Unload()
		{
		}

		internal void Begin()
		{
			if (_renderers.Length == 0)
			{
				AddRenderer(new DefaultRenderer());
				Debug.Warn(
					"Scene has begun with no renderer. A DefaultRenderer was added automatically so that something is visible.");
			}

			Physics.Reset();

			// prep our render textures
			UpdateResolutionScaler();
			Core.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);

			if (EntityProcessors != null)
				EntityProcessors.Begin();
			Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);

			_didSceneBegin = true;
			OnStart();
		}

		internal void End()
		{
			_didSceneBegin = false;

			// we kill Renderers and PostProcessors first since they rely on Entities
			for (var i = 0; i < _renderers.Length; i++)
				_renderers.Buffer[i].Unload();

			for (var i = 0; i < _postProcessors.Length; i++)
				_postProcessors.Buffer[i].Unload();

			// now we can remove the Entities and finally the SceneComponents
			Core.Emitter.RemoveObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
			Entities.RemoveAllEntities();

			for (var i = 0; i < _sceneComponents.Length; i++)
				_sceneComponents.Buffer[i].OnRemovedFromScene();
			_sceneComponents.Clear();

			Camera = null;
			Content.Dispose();
			_sceneRenderTarget.Dispose();
			Physics.Clear();

			if (_destinationRenderTarget != null)
				_destinationRenderTarget.Dispose();

			if (EntityProcessors != null)
				EntityProcessors.End();

			Unload();
		}

		public virtual void Update()
		{
			// we set the RenderTarget here so that the Viewport will match the RenderTarget properly
			Core.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);

			// update our lists in case they have any changes
			Entities.UpdateLists();

			// update our SceneComponents
			for (var i = _sceneComponents.Length - 1; i >= 0; i--)
			{
				if (_sceneComponents.Buffer[i].Enabled)
					_sceneComponents.Buffer[i].Update();
			}

			// update our EntityProcessors
			if (EntityProcessors != null)
				EntityProcessors.Update();

			// update our Entities
			Entities.Update();

			if (EntityProcessors != null)
				EntityProcessors.LateUpdate();

			// we update our renderables after entity.update in case any new Renderables were added
			RenderableComponents.UpdateLists();
		}

		internal void Render()
		{
			if (_renderers.Length == 0)
			{
				Debug.Error("There are no Renderers in the Scene!");
				return;
			}

			// Renderers should always have those that require a RenderTarget first. They clear themselves and set themselves as
			// the current RenderTarget when they render. If the first Renderer wants the sceneRenderTarget we set and clear it now.
			if (_renderers[0].WantsToRenderToSceneRenderTarget)
			{
				Core.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
				Core.GraphicsDevice.Clear(ClearColor);
			}


			var lastRendererHadRenderTarget = false;
			for (var i = 0; i < _renderers.Length; i++)
			{
				// MonoGame follows the XNA implementation so it will clear the entire buffer if we change the render target even if null.
				// Because of that, we track when we are done with our RenderTargets and clear the scene at that time.
				if (lastRendererHadRenderTarget && _renderers.Buffer[i].WantsToRenderToSceneRenderTarget)
				{
					Core.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
					Core.GraphicsDevice.Clear(ClearColor);

					// force a Camera matrix update to account for the new Viewport size
					if (_renderers.Buffer[i].Camera != null)
						_renderers.Buffer[i].Camera.ForceMatrixUpdate();
					Camera.ForceMatrixUpdate();
				}

				_renderers.Buffer[i].Render(this);
				lastRendererHadRenderTarget = _renderers.Buffer[i].RenderTexture != null;
			}
		}

		/// <summary>
		/// any PostProcessors present get to do their processing then we do the final render of the RenderTarget to the screen.
		/// In almost all cases finalRenderTarget will be null. The only time it will have a value is the first frame of a
		/// SceneTransition if the transition is requesting the render.
		/// </summary>
		/// <returns>The render.</returns>
		internal void PostRender(RenderTarget2D finalRenderTarget = null)
		{
			var enabledCounter = 0;
			if (EnablePostProcessing)
			{
				for (var i = 0; i < _postProcessors.Length; i++)
				{
					if (_postProcessors.Buffer[i].Enabled)
					{
						var isEven = Mathf.IsEven(enabledCounter);
						enabledCounter++;

						var source = isEven ? _sceneRenderTarget : _destinationRenderTarget;
						var destination = !isEven ? _sceneRenderTarget : _destinationRenderTarget;
						_postProcessors.Buffer[i].Process(source, destination);
					}
				}
			}

			// deal with our Renderers that want to render after PostProcessors if we have any
			for (var i = 0; i < _afterPostProcessorRenderers.Length; i++)
			{
				if (i == 0)
				{
					// we need to set the proper RenderTarget here. We want the last one that was the destination of our PostProcessors
					var currentRenderTarget = Mathf.IsEven(enabledCounter) ? _sceneRenderTarget : _destinationRenderTarget;
					Core.GraphicsDevice.SetRenderTarget(currentRenderTarget);
				}

				// force a Camera matrix update to account for the new Viewport size
				if (_afterPostProcessorRenderers.Buffer[i].Camera != null)
					_afterPostProcessorRenderers.Buffer[i].Camera.ForceMatrixUpdate();
				_afterPostProcessorRenderers.Buffer[i].Render(this);
			}

			// if we have a screenshot request deal with it before the final render to the backbuffer
			if (_screenshotRequestCallback != null)
			{
				var tex = new Texture2D(Core.GraphicsDevice, _sceneRenderTarget.Width, _sceneRenderTarget.Height);
				var data = new int[tex.Bounds.Width * tex.Bounds.Height];

				var currentRenderTarget = Mathf.IsEven(enabledCounter) ? _sceneRenderTarget : _destinationRenderTarget;
				currentRenderTarget.GetData(data);
				tex.SetData(data);
				_screenshotRequestCallback(tex);

				_screenshotRequestCallback = null;
			}

			// render our final result to the backbuffer or let our delegate do so
			if (_finalRenderDelegate != null)
			{
				var currentRenderTarget = Mathf.IsEven(enabledCounter) ? _sceneRenderTarget : _destinationRenderTarget;
				_finalRenderDelegate.HandleFinalRender(finalRenderTarget, LetterboxColor, currentRenderTarget, _finalRenderDestinationRect, SamplerState);
			}
			else
			{
				var currentRenderTarget = Mathf.IsEven(enabledCounter) ? _sceneRenderTarget : _destinationRenderTarget;
				Core.GraphicsDevice.SetRenderTarget(finalRenderTarget);
				Core.GraphicsDevice.Clear(LetterboxColor);

				Graphics.Instance.Batcher.Begin(BlendState.Opaque, SamplerState, null, null);
				Graphics.Instance.Batcher.Draw(currentRenderTarget, _finalRenderDestinationRect, Color.White);
				Graphics.Instance.Batcher.End();
			}
		}

		void OnGraphicsDeviceReset() => UpdateResolutionScaler();

		#endregion


		#region Resolution Policy

		/// <summary>
		/// sets the design size and resolution policy then updates the render textures
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="sceneResolutionPolicy">Scene resolution policy.</param>
		/// <param name="horizontalBleed">Horizontal bleed size. Used only if resolution policy is set to <see cref="SceneResolutionPolicy.BestFit"/>.</param>
		/// <param name="verticalBleed">Horizontal bleed size. Used only if resolution policy is set to <see cref="SceneResolutionPolicy.BestFit"/>.</param>
		public void SetDesignResolution(int width, int height, SceneResolutionPolicy sceneResolutionPolicy,
		                                int horizontalBleed = 0, int verticalBleed = 0)
		{
			_designResolutionSize = new Point(width, height);
			_resolutionPolicy = sceneResolutionPolicy;
			if (_resolutionPolicy == SceneResolutionPolicy.BestFit)
				_designBleedSize = new Point(horizontalBleed, verticalBleed);
			UpdateResolutionScaler();
		}

		void UpdateResolutionScaler()
		{
			var designSize = _designResolutionSize;
			var screenSize = new Point(Screen.Width, Screen.Height);
			var screenAspectRatio = (float) screenSize.X / (float) screenSize.Y;

			var renderTargetWidth = screenSize.X;
			var renderTargetHeight = screenSize.Y;

			var resolutionScaleX = (float) screenSize.X / (float) designSize.X;
			var resolutionScaleY = (float) screenSize.Y / (float) designSize.Y;

			var rectCalculated = false;

			// calculate the scale used by the PixelPerfect variants
			PixelPerfectScale = 1;
			if (_resolutionPolicy != SceneResolutionPolicy.None)
			{
				if ((float) designSize.X / (float) designSize.Y > screenAspectRatio)
					PixelPerfectScale = screenSize.X / designSize.X;
				else
					PixelPerfectScale = screenSize.Y / designSize.Y;

				if (PixelPerfectScale == 0)
					PixelPerfectScale = 1;
			}

			switch (_resolutionPolicy)
			{
				case SceneResolutionPolicy.None:
					_finalRenderDestinationRect.X = _finalRenderDestinationRect.Y = 0;
					_finalRenderDestinationRect.Width = screenSize.X;
					_finalRenderDestinationRect.Height = screenSize.Y;
					rectCalculated = true;
					break;
				case SceneResolutionPolicy.ExactFit:
					// exact design size render texture
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.NoBorder:
					// exact design size render texture
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;

					resolutionScaleX = resolutionScaleY = Math.Max(resolutionScaleX, resolutionScaleY);
					break;
				case SceneResolutionPolicy.NoBorderPixelPerfect:
					// exact design size render texture
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;

					// we are going to do some cropping so we need to use floats for the scale then round up
					PixelPerfectScale = 1;
					if ((float) designSize.X / (float) designSize.Y < screenAspectRatio)
					{
						var floatScale = (float) screenSize.X / (float) designSize.X;
						PixelPerfectScale = Mathf.CeilToInt(floatScale);
					}
					else
					{
						var floatScale = (float) screenSize.Y / (float) designSize.Y;
						PixelPerfectScale = Mathf.CeilToInt(floatScale);
					}

					if (PixelPerfectScale == 0)
						PixelPerfectScale = 1;

					_finalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
					_finalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
					_finalRenderDestinationRect.X = (screenSize.X - _finalRenderDestinationRect.Width) / 2;
					_finalRenderDestinationRect.Y = (screenSize.Y - _finalRenderDestinationRect.Height) / 2;
					rectCalculated = true;

					break;
				case SceneResolutionPolicy.ShowAll:
					resolutionScaleX = resolutionScaleY = Math.Min(resolutionScaleX, resolutionScaleY);

					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.ShowAllPixelPerfect:
					// exact design size render texture
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;

					_finalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
					_finalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
					_finalRenderDestinationRect.X = (screenSize.X - _finalRenderDestinationRect.Width) / 2;
					_finalRenderDestinationRect.Y = (screenSize.Y - _finalRenderDestinationRect.Height) / 2;
					rectCalculated = true;

					break;
				case SceneResolutionPolicy.FixedHeight:
					resolutionScaleX = resolutionScaleY;
					designSize.X = Mathf.CeilToInt(screenSize.X / resolutionScaleX);

					// exact design size render texture for height but not width
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.FixedHeightPixelPerfect:
					// start with exact design size render texture height. the width may change
					renderTargetHeight = designSize.Y;

					_finalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * resolutionScaleX);
					_finalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
					_finalRenderDestinationRect.X = (screenSize.X - _finalRenderDestinationRect.Width) / 2;
					_finalRenderDestinationRect.Y = (screenSize.Y - _finalRenderDestinationRect.Height) / 2;
					rectCalculated = true;

					renderTargetWidth = (int) (designSize.X * resolutionScaleX / PixelPerfectScale);
					break;
				case SceneResolutionPolicy.FixedWidth:
					resolutionScaleY = resolutionScaleX;
					designSize.Y = Mathf.CeilToInt(screenSize.Y / resolutionScaleY);

					// exact design size render texture for width but not height
					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.FixedWidthPixelPerfect:
					// start with exact design size render texture width. the height may change
					renderTargetWidth = designSize.X;

					_finalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
					_finalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * resolutionScaleY);
					_finalRenderDestinationRect.X = (screenSize.X - _finalRenderDestinationRect.Width) / 2;
					_finalRenderDestinationRect.Y = (screenSize.Y - _finalRenderDestinationRect.Height) / 2;
					rectCalculated = true;

					renderTargetHeight = (int) (designSize.Y * resolutionScaleY / PixelPerfectScale);

					break;
				case SceneResolutionPolicy.BestFit:
					var safeScaleX = (float) screenSize.X / (designSize.X - _designBleedSize.X);
					var safeScaleY = (float) screenSize.Y / (designSize.Y - _designBleedSize.Y);

					var resolutionScale = MathHelper.Max(resolutionScaleX, resolutionScaleY);
					var safeScale = MathHelper.Min(safeScaleX, safeScaleY);

					resolutionScaleX = resolutionScaleY = MathHelper.Min(resolutionScale, safeScale);

					renderTargetWidth = designSize.X;
					renderTargetHeight = designSize.Y;

					break;
			}

			// if we didnt already calculate a rect (None and all pixel perfect variants calculate it themselves) calculate it now
			if (!rectCalculated)
			{
				// calculate the display rect of the RenderTarget
				var renderWidth = designSize.X * resolutionScaleX;
				var renderHeight = designSize.Y * resolutionScaleY;

				_finalRenderDestinationRect = RectangleExt.FromFloats((screenSize.X - renderWidth) / 2,
					(screenSize.Y - renderHeight) / 2, renderWidth, renderHeight);
			}


			// set some values in the Input class to translate mouse position to our scaled resolution
			var scaleX = renderTargetWidth / (float) _finalRenderDestinationRect.Width;
			var scaleY = renderTargetHeight / (float) _finalRenderDestinationRect.Height;

			Input._resolutionScale = new Vector2(scaleX, scaleY);
			Input._resolutionOffset = _finalRenderDestinationRect.Location;

			// resize our RenderTargets
			if (_sceneRenderTarget != null)
				_sceneRenderTarget.Dispose();
			_sceneRenderTarget = RenderTarget.Create(renderTargetWidth, renderTargetHeight);

			// only create the destinationRenderTarget if it already exists, which would indicate we have PostProcessors
			if (_destinationRenderTarget != null)
			{
				_destinationRenderTarget.Dispose();
				_destinationRenderTarget = RenderTarget.Create(renderTargetWidth, renderTargetHeight);
			}

			// notify the Renderers, PostProcessors and FinalRenderDelegate of the change in render texture size
			for (var i = 0; i < _renderers.Length; i++)
				_renderers.Buffer[i].OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

			for (var i = 0; i < _afterPostProcessorRenderers.Length; i++)
				_afterPostProcessorRenderers.Buffer[i]
					.OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

			for (var i = 0; i < _postProcessors.Length; i++)
				_postProcessors.Buffer[i].OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

			if (_finalRenderDelegate != null)
				_finalRenderDelegate.OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

			Camera.OnSceneRenderTargetSizeChanged(renderTargetWidth, renderTargetHeight);
		}

		#endregion


		#region Utils

		/// <summary>
		/// after the next draw completes this will clone the backbuffer and call callback with the clone. Note that you must dispose of the
		/// Texture2D when done with it!
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void RequestScreenshot(Action<Texture2D> callback) => _screenshotRequestCallback = callback;

		#endregion


		#region SceneComponent Management

		/// <summary>
		/// Adds and returns a SceneComponent to the components list
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddSceneComponent<T>() where T : SceneComponent, new() => AddSceneComponent(new T());

		/// <summary>
		/// Adds and returns a SceneComponent to the components list
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddSceneComponent<T>(T component) where T : SceneComponent
		{
			component.Scene = this;
			component.OnEnabled();
			_sceneComponents.Add(component);
			_sceneComponents.Sort();
			return component;
		}

		/// <summary>
		/// Gets the first SceneComponent of type T and returns it. If no component is found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetSceneComponent<T>() where T : SceneComponent
		{
			for (var i = 0; i < _sceneComponents.Length; i++)
			{
				var component = _sceneComponents.Buffer[i];
				if (component is T)
					return component as T;
			}

			return null;
		}

		/// <summary>
		/// Gets the first SceneComponent of type T and returns it. If no SceneComponent is found the SceneComponent will be created.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetOrCreateSceneComponent<T>() where T : SceneComponent, new()
		{
			var comp = GetSceneComponent<T>();
			if (comp == null)
				comp = AddSceneComponent<T>();

			return comp;
		}

		/// <summary>
		/// removes the first SceneComponent of type T from the components list
		/// </summary>
		/// <returns><c>true</c>, if component was removed, <c>false</c> otherwise.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool RemoveSceneComponent<T>() where T : SceneComponent
		{
			var comp = GetSceneComponent<T>();
			if (comp != null)
			{
				RemoveSceneComponent(comp);
				return true;
			}

			return false;
		}

		/// <summary>
		/// removes a SceneComponent from the SceneComponents list
		/// </summary>
		public void RemoveSceneComponent(SceneComponent component)
		{
			Insist.IsTrue(_sceneComponents.Contains(component), "SceneComponent {0} is not in the SceneComponents list!", component);
			_sceneComponents.Remove(component);
			component.OnRemovedFromScene();
		}

		#endregion


		#region Renderer/PostProcessor Management

		/// <summary>
		/// adds a Renderer to the scene
		/// </summary>
		/// <returns>The renderer.</returns>
		/// <param name="renderer">Renderer.</param>
		public T AddRenderer<T>(T renderer) where T : Renderer
		{
			if (renderer.WantsToRenderAfterPostProcessors)
			{
				_afterPostProcessorRenderers.Add(renderer);
				_afterPostProcessorRenderers.Sort();
			}
			else
			{
				_renderers.Add(renderer);
				_renderers.Sort();
			}

			renderer.OnAddedToScene(this);

			// if we already began let the PostProcessor know what size our RenderTarget is
			if (_didSceneBegin)
				renderer.OnSceneBackBufferSizeChanged(_sceneRenderTarget.Width, _sceneRenderTarget.Height);

			return renderer;
		}

		/// <summary>
		/// gets the first Renderer of Type T
		/// </summary>
		/// <returns>The renderer.</returns>
		public T GetRenderer<T>() where T : Renderer
		{
			for (var i = 0; i < _renderers.Length; i++)
			{
				if (_renderers.Buffer[i] is T)
					return _renderers[i] as T;
			}

			for (var i = 0; i < _afterPostProcessorRenderers.Length; i++)
			{
				if (_afterPostProcessorRenderers.Buffer[i] is T)
					return _afterPostProcessorRenderers.Buffer[i] as T;
			}

			return null;
		}

		/// <summary>
		/// removes the Renderer from the scene
		/// </summary>
		/// <param name="renderer">Renderer.</param>
		public void RemoveRenderer(Renderer renderer)
		{
			Insist.IsTrue(_renderers.Contains(renderer) || _afterPostProcessorRenderers.Contains(renderer));

			if (renderer.WantsToRenderAfterPostProcessors)
				_afterPostProcessorRenderers.Remove(renderer);
			else
				_renderers.Remove(renderer);
			renderer.Unload();
		}

		/// <summary>
		/// adds a PostProcessor to the scene. Sets the scene field and calls PostProcessor.onAddedToScene so that PostProcessors can load
		/// resources using the scenes ContentManager.
		/// </summary>
		/// <param name="postProcessor">Post processor.</param>
		public T AddPostProcessor<T>(T postProcessor) where T : PostProcessor
		{
			_postProcessors.Add(postProcessor);
			_postProcessors.Sort();
			postProcessor.OnAddedToScene(this);

			// if we already began let the PostProcessor know what size our RenderTarget is
			if (_didSceneBegin)
				postProcessor.OnSceneBackBufferSizeChanged(_sceneRenderTarget.Width, _sceneRenderTarget.Height);

			// lazily create the 2nd RenderTarget for post processing only when a PostProcessor is added
			if (_destinationRenderTarget == null)
			{
				if (_sceneRenderTarget != null)
					_destinationRenderTarget = RenderTarget.Create(_sceneRenderTarget.Width, _sceneRenderTarget.Height);
				else
					_destinationRenderTarget = RenderTarget.Create();
			}

			return postProcessor;
		}

		/// <summary>
		/// gets the first PostProcessor of Type T
		/// </summary>
		/// <returns>The post processor.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetPostProcessor<T>() where T : PostProcessor
		{
			for (var i = 0; i < _postProcessors.Length; i++)
			{
				if (_postProcessors.Buffer[i] is T)
					return _postProcessors[i] as T;
			}

			return null;
		}

		/// <summary>
		/// removes a PostProcessor. Note that unload is not called when removing so if you no longer need the PostProcessor be sure to call
		/// unload to free resources.
		/// </summary>
		/// <param name="postProcessor">Step.</param>
		public void RemovePostProcessor(PostProcessor postProcessor)
		{
			Insist.IsTrue(_postProcessors.Contains(postProcessor));

			_postProcessors.Remove(postProcessor);
			postProcessor.Unload();
		}

		#endregion


		#region Entity Management

		/// <summary>
		/// add the Entity to this Scene, and return it
		/// </summary>
		/// <returns></returns>
		public Entity CreateEntity(string name)
		{
			var entity = new Entity(name);
			return AddEntity(entity);
		}

		/// <summary>
		/// add the Entity to this Scene at position, and return it
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="name">Name.</param>
		/// <param name="position">Position.</param>
		public Entity CreateEntity(string name, Vector2 position)
		{
			var entity = new Entity(name);
			entity.Transform.Position = position;
			return AddEntity(entity);
		}

		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public Entity AddEntity(Entity entity)
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity.Scene = this;

			for (var i = 0; i < entity.Transform.ChildCount; i++)
				AddEntity(entity.Transform.GetChild(i).Entity);

			return entity;
		}

		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public T AddEntity<T>(T entity) where T : Entity
		{
			Insist.IsFalse(Entities.Contains(entity), "You are attempting to add the same entity to a scene twice: {0}", entity);
			Entities.Add(entity);
			entity.Scene = this;
			return entity;
		}

		/// <summary>
		/// removes all entities from the scene
		/// </summary>
		public void DestroyAllEntities()
		{
			for (var i = 0; i < Entities.Count; i++)
				Entities[i].Destroy();
		}

		/// <summary>
		/// searches for and returns the first Entity with name
		/// </summary>
		/// <returns>The entity.</returns>
		/// <param name="name">Name.</param>
		public Entity FindEntity(string name) => Entities.FindEntity(name);

		/// <summary>
		/// returns all entities with the given tag
		/// </summary>
		/// <returns>The entities by tag.</returns>
		/// <param name="tag">Tag.</param>
		public List<Entity> FindEntitiesWithTag(int tag) => Entities.EntitiesWithTag(tag);

		/// <summary>
		/// returns all entities of Type T
		/// </summary>
		/// <returns>The of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<Entity> EntitiesOfType<T>() where T : Entity => Entities.EntitiesOfType<T>();

		/// <summary>
		/// returns the first enabled loaded component of Type T
		/// </summary>
		/// <returns>The component of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T FindComponentOfType<T>() where T : Component => Entities.FindComponentOfType<T>();

		/// <summary>
		/// returns a list of all enabled loaded components of Type T
		/// </summary>
		/// <returns>The components of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> FindComponentsOfType<T>() where T : Component => Entities.FindComponentsOfType<T>();

		#endregion


		#region Entity System Processors

		/// <summary>
		/// adds an EntitySystem processor to the scene
		/// </summary>
		/// <returns>The processor.</returns>
		/// <param name="processor">Processor.</param>
		public EntitySystem AddEntityProcessor(EntitySystem processor)
		{
			processor.Scene = this;
			EntityProcessors.Add(processor);
			return processor;
		}

		/// <summary>
		/// removes an EntitySystem processor from the scene
		/// </summary>
		/// <param name="processor">Processor.</param>
		public void RemoveEntityProcessor(EntitySystem processor) => EntityProcessors.Remove(processor);

		/// <summary>
		/// gets an EntitySystem processor
		/// </summary>
		/// <returns>The processor.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetEntityProcessor<T>() where T : EntitySystem => EntityProcessors.GetProcessor<T>();

		#endregion
	}
}