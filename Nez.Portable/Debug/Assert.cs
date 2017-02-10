using System.Diagnostics;


namespace Nez
{
	public static class Assert
	{
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void fail()
		{
			System.Diagnostics.Debug.Assert( false );
			Debugger.Break();
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void fail( string message, params object[] args )
		{
			System.Diagnostics.Debug.Assert( false, string.Format( message, args ) );
			Debugger.Break();
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isTrue( bool condition )
		{
			if( !condition )
			{
				fail();
			}
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isTrue( bool condition, string message, params object[] args )
		{
			if( !condition )
			{
				fail( message, args );
			}
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isFalse( bool condition )
		{
			isTrue( !condition );
		}


		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isFalse( bool condition, string message, params object[] args )
		{
			isTrue( !condition, message, args );
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isNull( object obj )
		{
			isTrue( obj == null );
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isNull( object obj, string message, params object[] args )
		{
			isTrue( obj == null, message, args );
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isNotNull( object obj )
		{
			isTrue( obj != null );
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void isNotNull( object obj, string message, params object[] args )
		{
			isTrue( obj != null, message, args );
		}


		/// <summary>
		/// asserts that first is equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void areEqual( object first, object second, string message, params object[] args )
		{
			if( first != second )
				fail( message, args );
		}


		/// <summary>
		/// asserts that first is not equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional( "DEBUG" )]
		[DebuggerHidden]
		public static void areNotEqual( object first, object second, string message, params object[] args )
		{
			if( first == second )
				fail( message, args );
		}

	}
}

