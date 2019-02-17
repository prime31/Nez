using Nez;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nez
{
	public class ImGuiManager : GlobalManager, IFinalRenderDelegate, IDisposable
	{
		public Scene scene { get; set; }
		public List<Action> drawCommands = new List<Action>();
		public ImGuiRenderer renderer { get; private set; }

		RenderTarget2D _lastRenderTarget;
		IntPtr _renderTargetId;
		bool _isGameWindowFocused;

		public ImGuiManager( string pathToFontFile, float fontSizePixels )
		{
			renderer = new ImGuiRenderer( Core.instance );

			if( pathToFontFile != null )
				ImGui.GetIO().Fonts.AddFontFromFileTTF( pathToFontFile, fontSizePixels );

			renderer.rebuildFontAtlas();

			Core.emitter.addObserver( CoreEvents.SceneChanged, onSceneChanged );
		}

		public ImGuiManager() : this( null, -1 )
		{}

		void onSceneChanged()
		{
			// when the Scene changes we need to rewire ourselves up as the IFinalRenderDelegate in the new Scene
			// if we were previously enabled
			drawCommands.Clear();
			if( enabled )
				onEnabled();
		}

		/// <summary>
		/// this is where we issue any and all ImGui commands to be drawn
		/// </summary>
		void layoutGui()
		{
			for( var i = drawCommands.Count - 1; i >= 0; i-- )
				drawCommands[i]();

			ImGui.ShowDemoWindow();

			if( _lastRenderTarget == null )
				return;

			var maxSize = new System.Numerics.Vector2( _lastRenderTarget.Width, _lastRenderTarget.Height );
			var minSize = maxSize / 4;
			maxSize *= 4;
			unsafe
			{
				ImGui.SetNextWindowSizeConstraints( minSize, maxSize, data =>
				{
					var size = ( *data ).CurrentSize;
					var ratio = size.X / _lastRenderTarget.Width;
					( *data ).DesiredSize.Y = ratio * _lastRenderTarget.Height;
				} );
			}

			ImGui.SetNextWindowPos( new System.Numerics.Vector2( 0, 0 ), ImGuiCond.FirstUseEver );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2( 0, 0 ) );
			ImGui.Begin( "Game Window" );
			_isGameWindowFocused = ImGui.IsWindowFocused();

			//Nugget.InputDisplay.cursorScreenPos = new Vector2( ImGui.GetCursorScreenPos().X, ImGui.GetCursorScreenPos().Y );
			//Nugget.InputDisplay.scaleX = ImGui.GetContentRegionAvail().X / _lastRenderTarget.Width;
			//Nugget.InputDisplay.scaleY = ImGui.GetContentRegionAvail().Y / _lastRenderTarget.Height;

			//Debug.log( $"window pos: {ImGui.GetWindowPos()}" );
			//Debug.log( $"avail size: {ImGui.GetContentRegionAvail()}" );
			//Debug.log( $"rt {_lastRenderTarget.Width} x {_lastRenderTarget.Height}" );
			//Debug.log( $"scaleX: {ImGui.GetContentRegionAvail().X / _lastRenderTarget.Width}" );
			//Debug.log( $"scaleY: {ImGui.GetContentRegionAvail().Y / _lastRenderTarget.Height}" );
			//Debug.log( ImGui.GetWindowSize() - ImGui.GetContentRegionAvail() );
			//Debug.log( $"titleHeight: {titleHeight}" );
			//Debug.log( $"screenPos: {ImGui.GetCursorScreenPos()}" );


			ImGui.Image( _renderTargetId, ImGui.GetContentRegionAvail() );
			ImGui.End();

			ImGui.PopStyleVar();
		}

		#region Public API

		/// <summary>
		/// registers an Action that will be called and any ImGui drawing can be done in it
		/// </summary>
		/// <param name="drawCommand"></param>
		public void registerDrawCommand( Action drawCommand )
		{
			drawCommands.Add( drawCommand );
		}

		/// <summary>
		/// removes the Action from the draw commands
		/// </summary>
		/// <param name="drawCommand"></param>
		public void unregisterDrawCommand( Action drawCommand )
		{
			drawCommands.Remove( drawCommand );
		}

		/// <summary>
		/// adds the font to the atlas and regenerates it
		/// </summary>
		/// <param name="pathToFontFile"></param>
		/// <param name="fontSizePixels"></param>
		/// <returns></returns>
		public ImFontPtr addFontAndRegenerateAtlas( string pathToFontFile, float fontSizePixels )
		{
			var fontPtr = ImGui.GetIO().Fonts.AddFontFromFileTTF( pathToFontFile, fontSizePixels );
			renderer.rebuildFontAtlas();
			return fontPtr;
		}

		#endregion

		#region GlobalManager Lifecycle

		public override void onEnabled()
		{
			Core.scene.finalRenderDelegate = this;
		}

		public override void onDisabled()
		{
			Core.scene.finalRenderDelegate = null;
		}

		public override void update()
		{
			renderer.beforeLayout( Time.deltaTime );
			layoutGui();
		}

		#endregion

        #region IFinalRenderDelegate

		public void handleFinalRender( RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source, Rectangle finalRenderDestinationRect, SamplerState samplerState )
		{
			if( _lastRenderTarget != source )
			{
				// unbind the old texture if we had one
				if( _lastRenderTarget != null )
					renderer.unbindTexture( _renderTargetId );

				// bind the new texture
				_lastRenderTarget = source;
				_renderTargetId = renderer.bindTexture( source );
			}

			Core.graphicsDevice.setRenderTarget( finalRenderTarget );
			Core.graphicsDevice.Clear( letterboxColor );

			renderer.afterLayout();
		}

		public void onAddedToScene()
		{ }

		public void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{ }

		public void unload()
		{ }

        #endregion

		#region IDisposable Support

		bool _isDisposed = false; // To detect redundant calls

		protected virtual void Dispose( bool disposing )
		{
			if( !_isDisposed )
			{
				if( disposing )
				{
					Core.emitter.removeObserver( CoreEvents.SceneChanged, onSceneChanged );
				}

				_isDisposed = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose( true );
		}

		#endregion

		[Console.Command( "toggle-imgui", "Toggles the Dear ImGui renderer" )]
		static void toggleImGui()
		{
			// install the service if it isnt already there
			var service = Core.getGlobalManager<ImGuiManager>();
			if( service == null )
			{
				service = new ImGuiManager();
				Core.registerGlobalManager( service );
			}

			service.setEnabled( !service.enabled );
		}

	}
}