using System;
using System.Diagnostics;


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

		[DebuggerHidden]
		static void log( LogType type, string format, params object[] args )
		{
			switch( type )
			{
				case LogType.Error:
					Console.Error.WriteLine( format, args );
					break;
				case LogType.Warn:
					Console.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Log:
					Console.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Info:
					Console.WriteLine( type.ToString() + ": " + format, args );
					break;
				case LogType.Trace:
					Console.WriteLine( type.ToString() + ": " + format, args );
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

	}
}

