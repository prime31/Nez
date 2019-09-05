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
		static void Log(LogType type, string format, params object[] args)
		{
			switch (type)
			{
				case LogType.Error:
					System.Diagnostics.Debug.WriteLine(type.ToString() + ": " + format, args);
					break;
				case LogType.Warn:
					System.Diagnostics.Debug.WriteLine(type.ToString() + ": " + format, args);
					break;
				case LogType.Log:
					System.Diagnostics.Debug.WriteLine(type.ToString() + ": " + format, args);
					break;
				case LogType.Info:
					System.Diagnostics.Debug.WriteLine(type.ToString() + ": " + format, args);
					break;
				case LogType.Trace:
					System.Diagnostics.Debug.WriteLine(type.ToString() + ": " + format, args);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		[DebuggerHidden]
		public static void Error(string format, params object[] args)
		{
			Log(LogType.Error, format, args);
		}


		[DebuggerHidden]
		public static void ErrorIf(bool condition, string format, params object[] args)
		{
			if (condition)
				Log(LogType.Error, format, args);
		}


		[DebuggerHidden]
		public static void Warn(string format, params object[] args)
		{
			Log(LogType.Warn, format, args);
		}


		[DebuggerHidden]
		public static void WarnIf(bool condition, string format, params object[] args)
		{
			if (condition)
				Log(LogType.Warn, format, args);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Log(object obj)
		{
			Log(LogType.Log, "{0}", obj);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Log(string format, params object[] args)
		{
			Log(LogType.Log, format, args);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void LogIf(bool condition, string format, params object[] args)
		{
			if (condition)
				Log(LogType.Log, format, args);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Info(string format, params object[] args)
		{
			Log(LogType.Info, format, args);
		}


		[Conditional("DEBUG")]
		[DebuggerHidden]
		public static void Trace(string format, params object[] args)
		{
			Log(LogType.Trace, format, args);
		}

		#endregion


		#region Drawing

		public static bool DrawTextFromBottom = false;

		static List<DebugDrawItem> _debugDrawItems = new List<DebugDrawItem>();
		static List<DebugDrawItem> _screenSpaceDebugDrawItems = new List<DebugDrawItem>();

		[Conditional("DEBUG")]
		internal static void Render()
		{
			if (_debugDrawItems.Count > 0)
			{
				if (Core.Scene != null && Core.Scene.Camera != null)
					Graphics.Instance.Batcher.Begin(Core.Scene.Camera.TransformMatrix);
				else
					Graphics.Instance.Batcher.Begin();

				for (var i = _debugDrawItems.Count - 1; i >= 0; i--)
				{
					var item = _debugDrawItems[i];
					if (item.Draw(Graphics.Instance))
						_debugDrawItems.RemoveAt(i);
				}

				Graphics.Instance.Batcher.End();
			}

			if (_screenSpaceDebugDrawItems.Count > 0)
			{
				var pos = DrawTextFromBottom ? new Vector2(0, Core.Scene.SceneRenderTargetSize.Y) : Vector2.Zero;
				Graphics.Instance.Batcher.Begin();

				for (var i = _screenSpaceDebugDrawItems.Count - 1; i >= 0; i--)
				{
					var item = _screenSpaceDebugDrawItems[i];
					var itemHeight = item.GetHeight();

					if (DrawTextFromBottom)
						item.Position = pos - new Vector2(0, itemHeight);
					else
						item.Position = pos;

					if (item.Draw(Graphics.Instance))
						_screenSpaceDebugDrawItems.RemoveAt(i);

					if (DrawTextFromBottom)
						pos.Y -= itemHeight;
					else
						pos.Y += itemHeight;
				}

				Graphics.Instance.Batcher.End();
			}
		}


		[Conditional("DEBUG")]
		public static void DrawLine(Vector2 start, Vector2 end, Color color, float duration = 0f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(start, end, color, duration));
		}


		[Conditional("DEBUG")]
		public static void DrawPixel(float x, float y, int size, Color color, float duration = 0f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(x, y, size, color, duration));
		}


		[Conditional("DEBUG")]
		public static void DrawPixel(Vector2 position, int size, Color color, float duration = 0f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(position.X, position.Y, size, color, duration));
		}


		[Conditional("DEBUG")]
		public static void DrawHollowRect(Rectangle rectangle, Color color, float duration = 0f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(rectangle, color, duration));
		}


		[Conditional("DEBUG")]
		public static void DrawHollowBox(Vector2 center, int size, Color color, float duration = 0f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			var halfSize = size * 0.5f;
			_debugDrawItems.Add(new DebugDrawItem(
				new Rectangle((int) (center.X - halfSize), (int) (center.Y - halfSize), size, size), color, duration));
		}


		[Conditional("DEBUG")]
		public static void DrawText(BitmapFont font, string text, Vector2 position, Color color, float duration = 0f,
		                            float scale = 1f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(font, text, position, color, duration, scale));
		}


		[Conditional("DEBUG")]
		public static void DrawText(NezSpriteFont font, string text, Vector2 position, Color color, float duration = 0f,
		                            float scale = 1f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_debugDrawItems.Add(new DebugDrawItem(font, text, position, color, duration, scale));
		}


		[Conditional("DEBUG")]
		public static void DrawText(string text, float duration = 0)
		{
			DrawText(text, Colors.DebugText, duration);
		}


		[Conditional("DEBUG")]
		public static void DrawText(string format, params object[] args)
		{
			var text = string.Format(format, args);
			DrawText(text, Colors.DebugText);
		}


		[Conditional("DEBUG")]
		public static void DrawText(string text, Color color, float duration = 1f, float scale = 1f)
		{
			if (!Core.DebugRenderEnabled)
				return;

			_screenSpaceDebugDrawItems.Add(new DebugDrawItem(text, color, duration, scale));
		}

		#endregion


		[Conditional("DEBUG")]
		public static void BreakIf(bool condition)
		{
			if (condition)
				System.Diagnostics.Debugger.Break();
		}


		[Conditional("DEBUG")]
		public static void Break_()
		{
			System.Diagnostics.Debugger.Break();
		}


		/// <summary>
		/// times how long an Action takes to run and returns the TimeSpan
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="action">Action.</param>
		public static TimeSpan TimeAction(Action action, uint numberOfIterations = 1)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			for (var i = 0; i < numberOfIterations; i++)
				action();
			stopwatch.Stop();

			if (numberOfIterations > 1)
				return TimeSpan.FromTicks(stopwatch.Elapsed.Ticks / numberOfIterations);

			return stopwatch.Elapsed;
		}
	}
}