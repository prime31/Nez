using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Text;
using Microsoft.Xna.Framework;
using Nez.Console;


namespace Nez.Analysis
{
#if DEBUG

	/// <summary>
	/// You can visually find bottle necks, and basic CPU usage by using this class.
	/// 
	/// TimeRuler provide the following features:
	///  * Up to 8 bars (Configurable)
	///  * Change colors for each markers
	///  * Marker logging.
	///  * It won't even generate BeginMark/EndMark method calls when you got rid of the DEBUG constant.
	///  * It supports up to 32 (Configurable) nested BeginMark method calls.
	///  * Multithreaded safe
	///  * Automatically changes display frames based on frame duration.
	///  
	/// How to use:
	/// call timerRuler.startFrame in top of the Game.Update method. Then, surround the code that you want measure by BbginMark and endMark.
	/// 
	/// timeRuler.beginMark( "Update", Color.Blue );
	/// // process that you want to measure.
	/// timerRuler.endMark( "Update" );
	/// 
	/// Also, you can specify bar index of marker (default value is 0)
	/// 
	/// timeRuler.bginMark( 1, "Update", Color.Blue );
	/// </remarks>
	public class TimeRuler
	{
		#region Constants

		/// <summary>
		/// Max bar count.
		/// </summary>
		const int maxBars = 8;

		/// <summary>
		/// Maximum sample number for each bar.
		/// </summary>
		const int maxSamples = 256;

		/// <summary>
		/// Maximum nest calls for each bar.
		/// </summary>
		const int maxNestCall = 32;

		/// <summary>
		/// Maximum display frames.
		/// </summary>
		const int maxSampleFrames = 4;

		/// <summary>
		/// Duration (in frame count) for take snap shot of log.
		/// </summary>
		const int logSnapDuration = 120;

		/// <summary>
		/// Height(in pixels) of bar.
		/// </summary>
		const int barHeight = 8;

		/// <summary>
		/// Padding(in pixels) of bar.
		/// </summary>
		const int barPadding = 2;

		/// <summary>
		/// Delay frame count for auto display frame adjustment.
		/// </summary>
		const int autoAdjustDelay = 30;

		#endregion


		#region Properties and Fields

		/// <summary>
		/// Gets/Set log display or no.
		/// </summary>
		public bool ShowLog = false;

		/// <summary>
		/// Gets/Sets target sample frames.
		/// </summary>
		public int TargetSampleFrames;

		/// <summary>
		/// Gets/Sets timer ruler width.
		/// </summary>
		public int Width;

		public bool Enabled = true;

		public static TimeRuler Instance;

		/// <summary>
		/// Marker structure.
		/// </summary>
		private struct Marker
		{
			public int MarkerId;
			public float BeginTime;
			public float EndTime;
			public Color Color;
		}


		/// <summary>
		/// Collection of markers.
		/// </summary>
		private class MarkerCollection
		{
			// Marker collection.
			public Marker[] Markers = new Marker[maxSamples];
			public int MarkCount;

			// Marker nest information.
			public int[] MarkerNests = new int[maxNestCall];
			public int NestCount;
		}


		/// <summary>
		/// Frame logging information.
		/// </summary>
		private class FrameLog
		{
			public MarkerCollection[] Bars;


			public FrameLog()
			{
				// Initialize markers.
				Bars = new MarkerCollection[maxBars];
				for (int i = 0; i < maxBars; ++i)
					Bars[i] = new MarkerCollection();
			}
		}


		/// <summary>
		/// Marker information
		/// </summary>
		private class MarkerInfo
		{
			// Name of marker.
			public string Name;

			// Marker log.
			public MarkerLog[] Logs = new MarkerLog[maxBars];


			public MarkerInfo(string name)
			{
				Name = name;
			}
		}


		/// <summary>
		/// Marker log information.
		/// </summary>
		private struct MarkerLog
		{
			public float SnapMin;
			public float SnapMax;
			public float SnapAvg;
			public float Min;
			public float Max;
			public float Avg;
			public int Samples;
			public Color Color;
			public bool Initialized;
		}

		// Logs for each frames.
		FrameLog[] logs;

		// Previous frame log.
		FrameLog prevLog;

		// Current log.
		FrameLog curLog;

		// Current frame count.
		int frameCount;

		// Stopwatch for measure the time.
		Stopwatch stopwatch = new Stopwatch();

		// Marker information array.
		List<MarkerInfo> markers = new List<MarkerInfo>();

		// Dictionary that maps from marker name to marker id.
		Dictionary<string, int> markerNameToIdMap = new Dictionary<string, int>();

		// Display frame adjust counter.
		int frameAdjust;

		// Current display frame count.
		int sampleFrames;

		// Marker log string.
		StringBuilder logString = new StringBuilder(512);

		// You want to call StartFrame at beginning of Game.Update method.
		// But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
		// In this case, we should ignore StartFrame call.
		// To do this, we just keep tracking of number of StartFrame calls until Draw gets called.
		int updateCount;


		// TimerRuler draw position.
		Vector2 _position;

		#endregion


		#region Initialization

		static TimeRuler()
		{
			Instance = new TimeRuler();
		}

		public TimeRuler()
		{
			// Initialize Parameters.
			logs = new FrameLog[2];
			for (int i = 0; i < logs.Length; ++i)
				logs[i] = new FrameLog();

			sampleFrames = TargetSampleFrames = 1;

			Width = (int) (Core.GraphicsDevice.Viewport.Width * 0.8f);

			Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
			OnGraphicsDeviceReset();
		}

		void OnGraphicsDeviceReset()
		{
			var layout = new Layout(Core.GraphicsDevice.Viewport);
			_position = layout.Place(new Vector2(Width, barHeight), 0, 0.01f, Alignment.BottomCenter);
		}

		[Command("timeruler", "Toggles the display of the TimerRuler on/off")]
		static void ToggleTimeRuler()
		{
			Instance.ShowLog = !Instance.ShowLog;
			DebugConsole.Instance.Log("TimeRuler enabled: " + (Instance.ShowLog ? "yes" : "no"));
			DebugConsole.Instance.IsOpen = false;
		}

		#endregion


		#region Measuring methods

		/// <summary>
		/// Start new frame.
		/// </summary>
		[Conditional("DEBUG")]
		public void StartFrame()
		{
			lock (this)
			{
				// We skip reset frame when this method gets called multiple times.
				var count = Interlocked.Increment(ref updateCount);
				if (Enabled && (1 < count && count < maxSampleFrames))
					return;

				// Update current frame log.
				prevLog = logs[frameCount++ & 0x1];
				curLog = logs[frameCount & 0x1];

				var endFrameTime = (float) stopwatch.Elapsed.TotalMilliseconds;

				// Update marker and create a log.
				for (var barIdx = 0; barIdx < prevLog.Bars.Length; ++barIdx)
				{
					var prevBar = prevLog.Bars[barIdx];
					var nextBar = curLog.Bars[barIdx];

					// Re-open marker that didn't get called EndMark in previous frame.
					for (var nest = 0; nest < prevBar.NestCount; ++nest)
					{
						var markerIdx = prevBar.MarkerNests[nest];

						prevBar.Markers[markerIdx].EndTime = endFrameTime;

						nextBar.MarkerNests[nest] = nest;
						nextBar.Markers[nest].MarkerId =
							prevBar.Markers[markerIdx].MarkerId;
						nextBar.Markers[nest].BeginTime = 0;
						nextBar.Markers[nest].EndTime = -1;
						nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
					}

					// Update marker log.
					for (var markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
					{
						var duration = prevBar.Markers[markerIdx].EndTime -
						               prevBar.Markers[markerIdx].BeginTime;

						int markerId = prevBar.Markers[markerIdx].MarkerId;
						MarkerInfo m = markers[markerId];

						m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

						if (!m.Logs[barIdx].Initialized)
						{
							// First frame process.
							m.Logs[barIdx].Min = duration;
							m.Logs[barIdx].Max = duration;
							m.Logs[barIdx].Avg = duration;

							m.Logs[barIdx].Initialized = true;
						}
						else
						{
							// Process after first frame.
							m.Logs[barIdx].Min = Math.Min(m.Logs[barIdx].Min, duration);
							m.Logs[barIdx].Max = Math.Min(m.Logs[barIdx].Max, duration);
							m.Logs[barIdx].Avg += duration;
							m.Logs[barIdx].Avg *= 0.5f;

							if (m.Logs[barIdx].Samples++ >= logSnapDuration)
							{
								m.Logs[barIdx].SnapMin = m.Logs[barIdx].Min;
								m.Logs[barIdx].SnapMax = m.Logs[barIdx].Max;
								m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
								m.Logs[barIdx].Samples = 0;
							}
						}
					}

					nextBar.MarkCount = prevBar.NestCount;
					nextBar.NestCount = prevBar.NestCount;
				}

				// Start measuring.
				stopwatch.Reset();
				stopwatch.Start();
			}
		}

		/// <summary>
		/// Start measure time.
		/// </summary>
		/// <param name="markerName">name of marker.</param>
		/// <param name="color">color/param>
		[Conditional("DEBUG")]
		public void BeginMark(string markerName, Color color)
		{
			BeginMark(markerName, color, 0);
		}

		/// <summary>
		/// Start measure time.
		/// </summary>
		/// <param name="barIndex">index of bar</param>
		/// <param name="markerName">name of marker.</param>
		/// <param name="color">color/param>
		[Conditional("DEBUG")]
		public void BeginMark(string markerName, Color color, int barIndex)
		{
			lock (this)
			{
				if (barIndex < 0 || barIndex >= maxBars)
					throw new ArgumentOutOfRangeException("barIndex");

				var bar = curLog.Bars[barIndex];

				if (bar.MarkCount >= maxSamples)
				{
					throw new OverflowException(
						"Exceeded sample count.\n" +
						"Either set larger number to TimeRuler.MaxSmpale or" +
						"lower sample count.");
				}

				if (bar.NestCount >= maxNestCall)
				{
					throw new OverflowException(
						"Exceeded nest count.\n" +
						"Either set larget number to TimeRuler.MaxNestCall or" +
						"lower nest calls.");
				}

				// Gets registered marker.
				int markerId;
				if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
				{
					// Register this if this marker is not registered.
					markerId = markers.Count;
					markerNameToIdMap.Add(markerName, markerId);
					markers.Add(new MarkerInfo(markerName));
				}

				// Start measuring.
				bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

				// Fill marker parameters.
				bar.Markers[bar.MarkCount].MarkerId = markerId;
				bar.Markers[bar.MarkCount].Color = color;
				bar.Markers[bar.MarkCount].BeginTime = (float) stopwatch.Elapsed.TotalMilliseconds;

				bar.Markers[bar.MarkCount].EndTime = -1;

				bar.MarkCount++;
			}
		}

		/// <summary>
		/// End measuring.
		/// </summary>
		/// <param name="markerName">Name of marker.</param>
		[Conditional("DEBUG")]
		public void EndMark(string markerName)
		{
			EndMark(markerName, 0);
		}

		/// <summary>
		/// End measuring.
		/// </summary>
		/// <param name="barIndex">Index of bar.</param>
		/// <param name="markerName">Name of marker.</param>
		[Conditional("DEBUG")]
		public void EndMark(string markerName, int barIndex)
		{
			lock (this)
			{
				if (barIndex < 0 || barIndex >= maxBars)
					throw new ArgumentOutOfRangeException("barIndex");

				var bar = curLog.Bars[barIndex];

				if (bar.NestCount <= 0)
					throw new InvalidOperationException("Call beginMark method before calling endMark method.");

				int markerId;
				if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
				{
					throw new InvalidOperationException(
						$"Marker '{markerName}' is not registered. Make sure you specifed same name as you used for BeginMark method");
				}

				var markerIdx = bar.MarkerNests[--bar.NestCount];
				if (bar.Markers[markerIdx].MarkerId != markerId)
				{
					throw new InvalidOperationException(
						"Incorrect call order of beginMark/endMark method." +
						"beginMark(A), beginMark(B), endMark(B), endMark(A)" +
						" But you can't called it like " +
						"beginMark(A), beginMark(B), endMark(A), endMark(B).");
				}

				bar.Markers[markerIdx].EndTime = (float) stopwatch.Elapsed.TotalMilliseconds;
			}
		}

		/// <summary>
		/// Get average time of given bar index and marker name.
		/// </summary>
		/// <param name="barIndex">Index of bar</param>
		/// <param name="markerName">name of marker</param>
		/// <returns>average spending time in ms.</returns>
		public float GetAverageTime(int barIndex, string markerName)
		{
			if (barIndex < 0 || barIndex >= maxBars)
				throw new ArgumentOutOfRangeException("barIndex");

			var result = 0f;
			int markerId;
			if (markerNameToIdMap.TryGetValue(markerName, out markerId))
				result = markers[markerId].Logs[barIndex].Avg;

			return result;
		}

		/// <summary>
		/// Reset marker log.
		/// </summary>
		[Conditional("DEBUG")]
		public void ResetLog()
		{
			lock (this)
			{
				foreach (var markerInfo in markers)
				{
					for (var i = 0; i < markerInfo.Logs.Length; ++i)
					{
						markerInfo.Logs[i].Initialized = false;
						markerInfo.Logs[i].SnapMin = 0;
						markerInfo.Logs[i].SnapMax = 0;
						markerInfo.Logs[i].SnapAvg = 0;

						markerInfo.Logs[i].Min = 0;
						markerInfo.Logs[i].Max = 0;
						markerInfo.Logs[i].Avg = 0;

						markerInfo.Logs[i].Samples = 0;
					}
				}
			}
		}

		#endregion


		#region Draw

		[Conditional("DEBUG")]
		public void Render()
		{
			Render(_position, Width);
		}

		[Conditional("DEBUG")]
		public void Render(Vector2 position, int width)
		{
			// Reset update count.
			Interlocked.Exchange(ref updateCount, 0);

			if (!ShowLog)
				return;

			// Gets Batcher, SpriteFont, and WhiteTexture from Batcher.
			var batcher = Graphics.Instance.Batcher;
			var font = Graphics.Instance.BitmapFont;

			// Adjust size and position based of number of bars we should draw.
			var height = 0;
			var maxTime = 0f;
			foreach (var bar in prevLog.Bars)
			{
				if (bar.MarkCount > 0)
				{
					height += barHeight + barPadding * 2;
					maxTime = Math.Max(maxTime, bar.Markers[bar.MarkCount - 1].EndTime);
				}
			}

			// Auto display frame adjustment.
			// For example, if the entire process of frame doesn't finish in less than 16.6ms
			// then it will adjust display frame duration as 33.3ms.
			const float frameSpan = 1.0f / 60.0f * 1000f;
			var sampleSpan = (float) sampleFrames * frameSpan;

			if (maxTime > sampleSpan)
				frameAdjust = Math.Max(0, frameAdjust) + 1;
			else
				frameAdjust = Math.Min(0, frameAdjust) - 1;

			if (Math.Abs(frameAdjust) > autoAdjustDelay)
			{
				sampleFrames = Math.Min(maxSampleFrames, sampleFrames);
				sampleFrames = Math.Max(TargetSampleFrames, (int) (maxTime / frameSpan) + 1);

				frameAdjust = 0;
			}

			// compute factor that converts from ms to pixel.
			var msToPs = (float) width / sampleSpan;

			// Draw start position.
			var startY = (int) position.Y - (height - barHeight);

			// Current y position.
			var y = startY;

			batcher.Begin();

			// Draw transparency background.
			var rc = new Rectangle((int) position.X, y, width, height);
			batcher.DrawRect(rc, new Color(0, 0, 0, 128));

			// Draw markers for each bars.
			rc.Height = barHeight;
			foreach (var bar in prevLog.Bars)
			{
				rc.Y = y + barPadding;
				if (bar.MarkCount > 0)
				{
					for (var j = 0; j < bar.MarkCount; ++j)
					{
						var bt = bar.Markers[j].BeginTime;
						var et = bar.Markers[j].EndTime;
						var sx = (int) (position.X + bt * msToPs);
						var ex = (int) (position.X + et * msToPs);
						rc.X = sx;
						rc.Width = Math.Max(ex - sx, 1);

						batcher.DrawRect(rc, bar.Markers[j].Color);
					}
				}

				y += barHeight + barPadding;
			}

			// Draw grid lines.
			// Each grid represents ms.
			rc = new Rectangle((int) position.X, (int) startY, 1, height);
			for (float t = 1.0f; t < sampleSpan; t += 1.0f)
			{
				rc.X = (int) (position.X + t * msToPs);
				batcher.DrawRect(rc, Color.Gray);
			}

			// Draw frame grid.
			for (var i = 0; i <= sampleFrames; ++i)
			{
				rc.X = (int) (position.X + frameSpan * (float) i * msToPs);
				batcher.DrawRect(rc, Color.White);
			}

			// Generate log string.
			y = startY - font.LineHeight;
			logString.Length = 0;
			foreach (var markerInfo in markers)
			{
				for (var i = 0; i < maxBars; ++i)
				{
					if (markerInfo.Logs[i].Initialized)
					{
						if (logString.Length > 0)
							logString.Append("\n");

						logString.Append(" Bar ");
						logString.Append(i);
						logString.Append("   [");
						logString.Append(markerInfo.Name);

						logString.Append("] Avg.:  ");
						logString.Append(markerInfo.Logs[i].SnapAvg.ToString("0.0000"));
						logString.Append(" ms");

						y -= font.LineHeight;
					}
				}
			}

			// Compute background size and draw it.
			var size = font.MeasureString(logString);
			rc = new Rectangle((int) position.X, (int) y, (int) size.X + 25, (int) size.Y + 5);
			batcher.DrawRect(rc, new Color(0, 0, 0, 128));

			// Draw log string.
			batcher.DrawString(font, logString, new Vector2(position.X + 22, y + 3), Color.White);


			// Draw log color boxes.
			y += (int) ((float) font.LineHeight * 0.3f);
			rc = new Rectangle((int) position.X + 4, y, 10, 10);
			var rc2 = new Rectangle((int) position.X + 5, y + 1, 8, 8);
			foreach (var markerInfo in markers)
			{
				for (var i = 0; i < maxBars; ++i)
				{
					if (markerInfo.Logs[i].Initialized)
					{
						rc.Y = y;
						rc2.Y = y + 1;
						batcher.DrawRect(rc, Color.White);
						batcher.DrawRect(rc2, markerInfo.Logs[i].Color);

						y += font.LineHeight;
					}
				}
			}

			batcher.End();
		}

		#endregion
	}

#endif
}