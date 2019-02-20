using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	public partial class ImGuiManager : GlobalManager, IFinalRenderDelegate, IDisposable
	{
		public bool showDemoWindow = false;
		public bool showStyleEditor = false;
		public bool showSceneGraphWindow = true;
		public bool showCoreWindow = true;
		public bool showSeperateGameWindow = true;

		List<Type> _sceneSubclasses = new List<Type>();
		System.Reflection.MethodInfo[] _themes;

		List<EntityInspector> _entityInspectors = new List<EntityInspector>();
		List<Action> _drawCommands = new List<Action>();
		ImGuiRenderer _renderer;

		RenderTarget2D _lastRenderTarget;
		IntPtr _renderTargetId = IntPtr.Zero;
		Num.Vector2? _gameViewForcedSize;
		WindowPosition? _gameViewForcedPos;
		float _mainMenuBarHeight;

		public ImGuiManager( ImGuiOptions options = null )
		{
			_renderer = new ImGuiRenderer( Core.instance );

			if( options != null )
			{
				foreach( var font in options._fonts )
					ImGui.GetIO().Fonts.AddFontFromFileTTF( font.Item1, font.Item2 );
			}

			_renderer.rebuildFontAtlas( options?._includeDefaultFont ?? true );
			Core.emitter.addObserver( CoreEvents.SceneChanged, onSceneChanged );
			NezImGuiThemes.photoshopDark();

			// find all Scenes
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( type.BaseType == typeof( Scene ) )
					{
						if( type.GetConstructor( Type.EmptyTypes ) != null )
							_sceneSubclasses.Add( type );
					}
				}
			}

			// find all themes
			_themes = typeof( NezImGuiThemes ).GetMethods( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public );
		}

		/// <summary>
		/// this is where we issue any and all ImGui commands to be drawn
		/// </summary>
		void layoutGui()
		{
			drawMainMenuBar();

			if( showSeperateGameWindow )
				drawGameWindow();
			drawEntityInspectors();

			for( var i = _drawCommands.Count - 1; i >= 0; i-- )
				_drawCommands[i]();

			SceneGraphWindow.show( ref showSceneGraphWindow );
			CoreWindow.show( ref showCoreWindow );

			if( showDemoWindow )
				ImGui.ShowDemoWindow( ref showDemoWindow );

			if( showStyleEditor )
			{
				ImGui.Begin( "Style Editor", ref showStyleEditor );
				ImGui.ShowStyleEditor();
				ImGui.End();
			}


			// this is just test/junk code
			ImGui.SetNextWindowPos( new Num.Vector2( 530, 475 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSize( new Num.Vector2( 340, 200 ), ImGuiCond.FirstUseEver );
			if( ImGui.Begin( "Debug##junk" ) )
			{
				ImGui.Text( $"Mouse position: {ImGui.GetMousePos()}" );
				ImGui.ShowStyleSelector( "Style" );

				ImGui.Checkbox( "Demo Window", ref showDemoWindow );
				ImGui.Checkbox( "Style Editor", ref showStyleEditor );
				ImGui.Checkbox( "Scene Graph", ref showSceneGraphWindow );
				ImGui.Checkbox( "Core Window", ref showCoreWindow );

				ImGui.Separator();

				float framerate = ImGui.GetIO().Framerate;
				ImGui.Text( $"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)" );
				ImGui.End();
			}
		}

		/// <summary>
		/// draws the main menu bar
		/// </summary>
		void drawMainMenuBar()
		{
			if( ImGui.BeginMainMenuBar() )
			{
				_mainMenuBarHeight = ImGui.GetWindowHeight();
				if( ImGui.BeginMenu( "File" ) )
				{
					if( ImGui.MenuItem( "Quit ImGui" ) )
						setEnabled( false );
					ImGui.EndMenu();
				}

				if( _sceneSubclasses.Count > 0 && ImGui.BeginMenu( "Scenes" ) )
				{
					foreach( var sceneType in _sceneSubclasses )
					{
						if( ImGui.MenuItem( sceneType.Name ) )
						{
							var scene = (Scene)sceneType.GetConstructor( Type.EmptyTypes ).Invoke( new object[] { } );
							Core.startSceneTransition( new CrossFadeTransition( () => scene ) );
						}
					}
					ImGui.EndMenu();
				}

				if( _themes.Length > 0 && ImGui.BeginMenu( "Themes" ) )
				{
					foreach( var theme in _themes )
					{
						if( ImGui.MenuItem( theme.Name ) )
							theme.Invoke( null, new object[] {} );
					}
					ImGui.EndMenu();
				}

				if( ImGui.BeginMenu( "Game View" ) )
				{
					var rtSize = Core.scene.sceneRenderTargetSize;

					if( ImGui.BeginMenu( "Resize" ) )
					{
						if( ImGui.MenuItem( "0.25x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X / 4f, rtSize.Y / 4f );
						if( ImGui.MenuItem( "0.5x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X / 2f, rtSize.Y / 2f );
						if( ImGui.MenuItem( "0.75x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X / 1.33f, rtSize.Y / 1.33f );
						if( ImGui.MenuItem( "1x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X, rtSize.Y );
						if( ImGui.MenuItem( "1.5x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X * 1.5f, rtSize.Y * 1.5f );
						if( ImGui.MenuItem( "2x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X * 2, rtSize.Y * 2 );
						if( ImGui.MenuItem( "3x" ) )
							_gameViewForcedSize = new Num.Vector2( rtSize.X * 3, rtSize.Y * 3 );
						ImGui.EndMenu();
					}

					if( ImGui.BeginMenu( "Reposition" ) )
					{
						foreach( var pos in Enum.GetNames( typeof( WindowPosition ) ) )
						{
							if( ImGui.MenuItem( pos ) )
								_gameViewForcedPos = (WindowPosition)Enum.Parse( typeof( WindowPosition ), pos );
						}
						ImGui.EndMenu();
					}


					ImGui.EndMenu();
				}

				if( ImGui.BeginMenu( "Window" ) )
				{
					ImGui.MenuItem( "Demo Window", null, ref showDemoWindow );
					ImGui.MenuItem( "Style Editor", null, ref showStyleEditor );
					ImGui.Separator();
					ImGui.MenuItem( "Core Window", null, ref showCoreWindow );
					ImGui.MenuItem( "Scene Graph Window", null, ref showSceneGraphWindow );
					ImGui.MenuItem( "Separate Game Window", null, ref showSeperateGameWindow );
					ImGui.EndMenu();
				}

				ImGui.EndMainMenuBar();
			}
		}

		/// <summary>
		/// draws all the EntityInspectors
		/// </summary>
		void drawEntityInspectors()
		{
			for( var i = _entityInspectors.Count - 1; i >= 0; i-- )
				_entityInspectors[i].draw();
		}

		#region Public API

		/// <summary>
		/// registers an Action that will be called and any ImGui drawing can be done in it
		/// </summary>
		/// <param name="drawCommand"></param>
		public void registerDrawCommand( Action drawCommand ) => _drawCommands.Add( drawCommand );

		/// <summary>
		/// removes the Action from the draw commands
		/// </summary>
		/// <param name="drawCommand"></param>
		public void unregisterDrawCommand( Action drawCommand ) => _drawCommands.Remove( drawCommand );

		/// <summary>
		/// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image" />.
		/// That pointer is then used by ImGui to let us know what texture to draw
		/// </summary>
		/// <param name="textureId"></param>
		public void unbindTexture( IntPtr textureId ) => _renderer.unbindTexture( textureId );

		/// <summary>
		/// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public IntPtr bindTexture( Texture2D texture ) => _renderer.bindTexture( texture );

		/// <summary>
		/// creates an EntityInspector window
		/// </summary>
		/// <param name="entity"></param>
		public void startInspectingEntity( Entity entity )
		{
			// if we are already inspecting the Entity focus the window
			foreach( var inspector in _entityInspectors )
			{
				if( inspector.entity == entity )
				{
					inspector.setWindowFocus();
					return;
				}
			}

			var entityInspector = new EntityInspector( entity );
			entityInspector.setWindowFocus();
			_entityInspectors.Add( entityInspector );
		}

		/// <summary>
		/// removes the EntityInspector for this Entity
		/// </summary>
		/// <param name="entity"></param>
		public void stopInspectingEntity( Entity entity )
		{
			for( var i = 0; i < _entityInspectors.Count; i++ )
			{
				var inspector = _entityInspectors[i];
				if( inspector.entity == entity )
				{
					_entityInspectors.RemoveAt( i );
					return;
				}
			}
		}

		/// <summary>
		/// removes the EntityInspector
		/// </summary>
		/// <param name="entityInspector"></param>
		public void stopInspectingEntity( EntityInspector entityInspector )
		{
			_entityInspectors.RemoveAt( _entityInspectors.IndexOf( entityInspector ) );
		}

		#endregion

	}
}
