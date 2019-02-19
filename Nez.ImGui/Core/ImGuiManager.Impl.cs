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
		/// <summary>
		/// here we do some cleanup in preparation for a new Scene
		/// </summary>
		void onSceneChanged()
		{
			// when the Scene changes we need to rewire ourselves up as the IFinalRenderDelegate in the new Scene
			// if we were previously enabled and do some cleanup
			_drawCommands.Clear();
			_entityInspectors.Clear();

			if( _renderTargetId != IntPtr.Zero )
			{
				_renderer.unbindTexture( _renderTargetId );
				_renderTargetId = IntPtr.Zero;
			}
			_lastRenderTarget = null;

			if( enabled )
				onEnabled();
		}

		/// <summary>
		/// draws the game window and deals with overriding Nez.Input when appropriate
		/// </summary>
		void drawGameWindow()
		{
			if( _lastRenderTarget == null )
				return;

			var rtAspectRatio = (float)_lastRenderTarget.Width / (float)_lastRenderTarget.Height;
			var maxSize = new Num.Vector2( _lastRenderTarget.Width, _lastRenderTarget.Height );
			if( maxSize.X >= Screen.width || maxSize.Y >= Screen.height )
			{
				maxSize.X = Screen.width * 0.8f;
				maxSize.Y = maxSize.X / rtAspectRatio;
			}
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

			ImGui.SetNextWindowPos( new Num.Vector2( 345, 25 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSize( new Num.Vector2( Screen.width / 2, ( Screen.width / 2 ) / rtAspectRatio ), ImGuiCond.FirstUseEver );
			ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, new Num.Vector2( 0, 0 ) );
			ImGui.Begin( "Game Window" );

			// convert mouse input to the game windows coordinates
			overrideMouseInput();

			if( !ImGui.IsWindowFocused() )
				Input.setCurrentKeyboardState( new KeyboardState() );

			ImGui.End();

			ImGui.PopStyleVar();
		}

		/// <summary>
		/// converts the mouse position from global window position to the game window's coordinates and overrides Nez.Input with
		/// the new value. This keeps input working properly in the game window.
		/// </summary>
		void overrideMouseInput()
		{
			// ImGui.GetCursorScreenPos() is the position of top-left pixel in windows drawable area
			var offset = new Vector2( ImGui.GetCursorScreenPos().X, ImGui.GetCursorScreenPos().Y );

			// remove window position offset from our raw input. this gets us normalized back to the top-left origin.
			// We are essentilly removing any input delta that is not in the game window.
			var normalizedPos = Input.rawMousePosition.ToVector2() - offset;

			var scaleX = ImGui.GetContentRegionAvail().X / _lastRenderTarget.Width;
			var scaleY = ImGui.GetContentRegionAvail().Y / _lastRenderTarget.Height;
			var scale = new Vector2( scaleX, scaleY );

			// scale the rest of the input since it is in a scaled window (the offset portion is not scaled since
			// it is outside the scaled portion)
			normalizedPos /= scale;


			// trick the input system. Take our normalizedPos and undo the scale and offsets (do the
			// reverse of what Input.scaledPosition does) so that any consumers of mouse input can get
			// the correct coordinates.
			var unNormalizedPos = normalizedPos / Input.resolutionScale;
			unNormalizedPos += Input.resolutionOffset;

			var mouseState = Input.currentMouseState;
			var newMouseState = new MouseState( (int)unNormalizedPos.X, (int)unNormalizedPos.Y, mouseState.ScrollWheelValue,
				mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2 );
			Input.setCurrentMouseState( newMouseState );
		}


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
			// we have to do our layout in update so that if the game window is not focused or being displayed we can wipe
			// the Input, essentially letting ImGui consume it
			_renderer.beforeLayout( Time.deltaTime );
			layoutGui();
		}

		#endregion


        #region IFinalRenderDelegate

		void IFinalRenderDelegate.handleFinalRender( RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source, Rectangle finalRenderDestinationRect, SamplerState samplerState )
		{
			if( _showSeperateGameWindow )
			{
				if( _lastRenderTarget != source )
				{
					// unbind the old texture if we had one
					if( _lastRenderTarget != null )
						_renderer.unbindTexture( _renderTargetId );

					// bind the new texture
					_lastRenderTarget = source;
					_renderTargetId = _renderer.bindTexture( source );
				}

				// we cant draw the game window until we have the texture bound so we append it here
				ImGui.Begin( "Game Window" );
				ImGui.Image( _renderTargetId, ImGui.GetContentRegionAvail() );
				ImGui.End();

				Core.graphicsDevice.SamplerStates[0] = samplerState;
				Core.graphicsDevice.setRenderTarget( finalRenderTarget );
				Core.graphicsDevice.Clear( letterboxColor );
			}
			else
			{
				Core.graphicsDevice.setRenderTarget( finalRenderTarget );
				Core.graphicsDevice.Clear( letterboxColor );
				Graphics.instance.batcher.begin( BlendState.Opaque, samplerState, null, null );
				Graphics.instance.batcher.draw( source, finalRenderDestinationRect, Color.White );
				Graphics.instance.batcher.end();
			}

			_renderer.afterLayout();
		}

		void IFinalRenderDelegate.onAddedToScene( Scene scene )
		{ }

		void IFinalRenderDelegate.onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{ }

		void IFinalRenderDelegate.unload()
		{ }

        #endregion


		#region IDisposable Support

		bool _isDisposed = false; // To detect redundant calls

		void Dispose( bool disposing )
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