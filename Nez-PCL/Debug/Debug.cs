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

		[Conditional( "DEBUG" )]
		internal static void render()
		{
			if( _debugDrawItems.Count == 0 )
				return;

			if( Core.scene != null && Core.scene.camera != null )
				Graphics.instance.spriteBatch.Begin( transformMatrix: Core.scene.camera.transformMatrix );
			else
				Graphics.instance.spriteBatch.Begin();

			for( var i = _debugDrawItems.Count - 1; i >= 0; i-- )
			{
				var item = _debugDrawItems[i];
				if( item.draw( Graphics.instance ) )
					_debugDrawItems.RemoveAt( i );
			}

			Graphics.instance.spriteBatch.End();
		}


		[Conditional( "DEBUG" )]
		public static void drawLine( Vector2 start, Vector2 end, Color color, float duration = 0f )
		{
			_debugDrawItems.Add( new DebugDrawItem( start, end, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawHollowRect( Rectangle rectangle, Color color, float duration = 0f )
		{
			_debugDrawItems.Add( new DebugDrawItem( rectangle, color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawHollowBox( Vector2 center, int size, Color color, float duration = 0f )
		{
			var halfSize = size * 0.5f;
			_debugDrawItems.Add( new DebugDrawItem( new Rectangle( (int)( center.X - halfSize ), (int)( center.Y - halfSize ), size, size ), color, duration ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( BitmapFont font, string text, Vector2 position, Color color, float duration = 0f, float scale = 1f )
		{
			_debugDrawItems.Add( new DebugDrawItem( font, text, position, color, duration, scale ) );
		}


		[Conditional( "DEBUG" )]
		public static void drawText( SpriteFont font, string text, Vector2 position, Color color, float duration = 0f, float scale = 1f )
		{
			_debugDrawItems.Add( new DebugDrawItem( font, text, position, color, duration, scale ) );
		}

		#endregion

	}
}

