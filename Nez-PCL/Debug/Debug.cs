using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


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


		#region Asserts

		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertIsTrue( bool condition, string message, params object[] args )
		{
			if( !condition )
				System.Diagnostics.Debug.Assert( false, string.Format( message, args ) );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertIsFalse( bool condition, string message, params object[] args )
		{
			assertIsTrue( !condition, message, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertIsNull( object obj, string message, params object[] args )
		{
			assertIsTrue( obj == null, message, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertIsNotNull( object obj, string message, params object[] args )
		{
			assertIsTrue( obj != null, message, args );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertAreEqual( object first, object second, string message, params object[] args )
		{
			if( first != second )
				System.Diagnostics.Debug.Assert( false, string.Format( message, args ) );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void assertAreNotEqual( object first, object second, string message, params object[] args )
		{
			if( first == second )
				System.Diagnostics.Debug.Assert( false, string.Format( message, args ) );
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

		#endregion

	}
}

