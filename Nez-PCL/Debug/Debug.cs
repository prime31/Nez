using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	// TODO: add Conditionals for all log levels
	public static class Debug
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
				var pos = Vector2.Zero;
				Graphics.instance.batcher.begin();

				for( var i = _screenSpaceDebugDrawItems.Count - 1; i >= 0; i-- )
				{
					var item = _screenSpaceDebugDrawItems[i];
					item.position = pos;
					var itemHeight = item.bitmapFont.lineHeight * item.scale;
					if( item.draw( Graphics.instance ) )
						_screenSpaceDebugDrawItems.RemoveAt( i );

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
		public static void drawText( string text )
		{
			drawText( text, Color.White );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( string format, params object[] args )
		{
			var text = string.Format( format, args );
			drawText( text, Color.White );
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

	}
}

