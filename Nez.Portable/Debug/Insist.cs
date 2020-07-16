using System.Diagnostics;


namespace Nez
{
	public static class Insist
	{
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Fail()
		{
			System.Diagnostics.Debug.Assert(false);
			Debugger.Break();
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Fail(string message, params object[] args)
		{
			System.Diagnostics.Debug.Assert(false, string.Format(message, args));
			Debugger.Break();
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsTrue(bool condition)
		{
			if (!condition)
			{
				Fail();
			}
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsTrue(bool condition, string message, params object[] args)
		{
			if (!condition)
			{
				Fail(message, args);
			}
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsFalse(bool condition)
		{
			IsTrue(!condition);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsFalse(bool condition, string message, params object[] args)
		{
			IsTrue(!condition, message, args);
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsNull(object obj)
		{
			IsTrue(obj == null);
		}


		/// <summary>
		/// asserts that obj is null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsNull(object obj, string message, params object[] args)
		{
			IsTrue(obj == null, message, args);
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsNotNull(object obj)
		{
			IsTrue(obj != null);
		}


		/// <summary>
		/// asserts that obj is not null
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void IsNotNull(object obj, string message, params object[] args)
		{
			IsTrue(obj != null, message, args);
		}


		/// <summary>
		/// asserts that first is equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AreEqual(object first, object second, string message, params object[] args)
		{
			if (first != second)
				Fail(message, args);
		}


		/// <summary>
		/// asserts that first is not equal to second
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="message">Message.</param>
		/// <param name="args">Arguments.</param>
		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void AreNotEqual(object first, object second, string message, params object[] args)
		{
			if (first == second)
				Fail(message, args);
		}
	}
}