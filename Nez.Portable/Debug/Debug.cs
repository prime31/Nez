using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.BitmapFonts;


namespace Nez
{
	// TODO: add Conditionals for all log levels
	public static partial class Debug
	{
		enum LogType
		{
			Error,
			Warn,
			Log,
			Info,
			Trace
		}


		#region Logging

		[DebuggerHidden]
		static void log( LogType type, string format, params object[] args )
		{
			switch( type )
			{
				case LogType.Error:
					System.Diagnostics.Debug.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Warn:
					System.Diagnostics.Debug.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Log:
					System.Diagnostics.Debug.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Info:
					System.Diagnostics.Debug.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Trace:
					System.Diagnostics.Debug.WriteLine( type.ToString() + ": " + format, args );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		[DebuggerHidden]
		public static void error( string format, params object[] args )
		{
			log( LogType.Error, format, args );
		}


		[DebuggerHidden]
		public static void errorIf( bool condition, string format, params object[] args )
		{
			if( condition )
				log( LogType.Error, format, args );
		}


		[DebuggerHidden]
		public static void warn( string format, params object[] args )
		{
			log( LogType.Warn, format, args );
		}


		[DebuggerHidden]
		public static void warnIf( bool condition, string format, params object[] args )
		{
			if( condition )
				log( LogType.Warn, format, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void log( object obj )
		{
			log( LogType.Log, "{0}", obj );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void log( string format, params object[] args )
		{
			log( LogType.Log, format, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void logIf( bool condition, string format, params object[] args )
		{
			if( condition )
				log( LogType.Log, format, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void info( string format, params object[] args )
		{
			log( LogType.Info, format, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void trace( string format, params object[] args )
		{
			log( LogType.Trace, format, args );
		}

		#endregion


		#region Drawing

		public static bool drawTextFromBottom = false;

		static List<DebugDrawItem> _debugDrawItems = new List<DebugDrawItem>();
		static List<DebugDrawItem> _screenSpaceDebugDrawItems = new List<DebugDrawItem>();

		[Conditional( "DEBUG" )]
		internal static void render()
		{
			if( _debugDrawItems.Count > 0 )
			{
				if( Core.scene != null && Core.scene.camera != null )
					Graphics.instance.batcher.begin( Core.scene.camera.transformMatrix );
				else
					Graphics.instance.batcher.begin();

				for( var i = _debugDrawItems.Count - 1; i >= 0; i-- )
				{
					var item = _debugDrawItems[i];
					if( item.draw( Graphics.instance ) )
						_debugDrawItems.RemoveAt( i );
				}

				Graphics.instance.batcher.end();
			}

			if( _screenSpaceDebugDrawItems.Count > 0 )
			{
				var pos = drawTextFromBottom ? new Vector2( 0, Core.scene.sceneRenderTargetSize.Y ) : Vector2.Zero;
				Graphics.instance.batcher.begin();

				for( var i = _screenSpaceDebugDrawItems.Count - 1; i >= 0; i-- )
				{
					var item = _screenSpaceDebugDrawItems[i];
					var itemHeight = item.getHeight();

					if( drawTextFromBottom )
						item.position = pos - new Vector2( 0, itemHeight );
					else
						item.position = pos;

					if( item.draw( Graphics.instance ) )
						_screenSpaceDebugDrawItems.RemoveAt( i );

					if( drawTextFromBottom )
						pos.Y -= itemHeight;
					else
						pos.Y += itemHeight;
				}

				Graphics.instance.batcher.end();
			}
		}


		[Conditional( "DEBUG" )]
		public static void drawLine( Vector2 start, Vector2 end, Color color, float duration = 0f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( start, end, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawPixel( float x, float y, int size, Color color, float duration = 0f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( x, y, size, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawPixel( Vector2 position, int size, Color color, float duration = 0f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( position.X, position.Y, size, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawHollowRect( Rectangle rectangle, Color color, float duration = 0f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( rectangle, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawHollowBox( Vector2 center, int size, Color color, float duration = 0f )
		{
			if( !Core.debugRenderEnabled )
				return;
			var halfSize = size * 0.5f;
			_debugDrawItems.Add( new DebugDrawItem( new Rectangle( (int)( center.X - halfSize ), (int)( center.Y - halfSize ), size, size ), color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( BitmapFont font, string text, Vector2 position, Color color, float duration = 0f, float scale = 1f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( font, text, position, color, duration, scale ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( NezSpriteFont font, string text, Vector2 position, Color color, float duration = 0f, float scale = 1f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_debugDrawItems.Add( new DebugDrawItem( font, text, position, color, duration, scale ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( string text, float duration = 0 )
		{
			drawText( text, Colors.debugText, duration );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( string format, params object[] args )
		{
			var text = string.Format( format, args );
			drawText( text, Colors.debugText );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( string text, Color color, float duration = 1f, float scale = 1f )
		{
			if( !Core.debugRenderEnabled )
				return;
			_screenSpaceDebugDrawItems.Add( new DebugDrawItem( text, color, duration, scale ) );
		}

		#endregion


		[Conditional( "DEBUG" )]
		public static void breakIf( bool condition )
		{
			if( condition )
				System.Diagnostics.Debugger.Break();
		}


		[Conditional( "DEBUG" )]
		public static void break_()
		{
			System.Diagnostics.Debugger.Break();
		}


		/// <summary>
		/// times how long an Action takes to run and returns the TimeSpan
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="action">Action.</param>
		public static TimeSpan timeAction( Action action, uint numberOfIterations = 1 )
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for( var i = 0; i < numberOfIterations; i++ )
				action();
			stopwatch.Stop();

			if( numberOfIterations > 1 )
				return TimeSpan.FromTicks( stopwatch.Elapsed.Ticks / numberOfIterations );

			return stopwatch.Elapsed;
		}

	}
}

