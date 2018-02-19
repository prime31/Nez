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
	/// Realtime CPU measuring tool
	/// </summary>
	/// <remarks>
	/// You can visually find bottle neck, and know how much you can put more CPU jobs
	/// by using this tool.
	/// Because of this is real time profile, you can find glitches in the game too.
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
	/// call timerRuler.StartFrame in top of the Game.Update method.
	/// 
	/// Then, surround the code that you want measure by BeginMark and EndMark.
	/// 
	/// timeRuler.BeginMark( "Update", Color.Blue );
	/// // process that you want to measure.
	/// timerRuler.EndMark( "Update" );
	/// 
	/// Also, you can specify bar index of marker (default value is 0)
	/// 
	/// timeRuler.BeginMark( 1, "Update", Color.Blue );
	/// 
	/// All profiling methods has CondionalAttribute with "DEBUG".
	/// If you not specified "DEBUG" constant, it doesn't even generate
	/// method calls for BeginMark/EndMark.
	/// So, don't forget remove "DEBUG" constant when you release your game.
	/// 
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
		public bool showLog = false;

		/// <summary>
		/// Gets/Sets target sample frames.
		/// </summary>
		public int targetSampleFrames;

		/// <summary>
		/// Gets/Sets timer ruler width.
		/// </summary>
		public int width;

		public bool enabled = true;

		public static TimeRuler instance;
		
		/// <summary>
		/// Marker structure.
		/// </summary>
		private struct Marker
		{
			public int markerId;
			public float beginTime;
			public float endTime;
			public Color color;
		}


		/// <summary>
		/// Collection of markers.
		/// </summary>
		private class MarkerCollection
		{
			// Marker collection.
			public Marker[] markers = new Marker[maxSamples];
			public int markCount;

			// Marker nest information.
			public int[] markerNests = new int[maxNestCall];
			public int nestCount;
		}


		/// <summary>
		/// Frame logging information.
		/// </summary>
		private class FrameLog
		{
			public MarkerCollection[] bars;


			public FrameLog()
			{
				// Initialize markers.
				bars = new MarkerCollection[maxBars];
				for( int i = 0; i < maxBars; ++i )
					bars[i] = new MarkerCollection();
			}
		}


		/// <summary>
		/// Marker information
		/// </summary>
		private class MarkerInfo
		{
			// Name of marker.
			public string name;

			// Marker log.
			public MarkerLog[] logs = new MarkerLog[maxBars];


			public MarkerInfo( string name )
			{
				this.name = name;
			}
		}


		/// <summary>
		/// Marker log information.
		/// </summary>
		private struct MarkerLog
		{
			public float snapMin;
			public float snapMax;
			public float snapAvg;
			public float min;
			public float max;
			public float avg;
			public int samples;
			public Color color;
			public bool initialized;
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
		StringBuilder logString = new StringBuilder( 512 );

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
			instance = new TimeRuler();
		}


		public TimeRuler()
		{
			// Initialize Parameters.
			logs = new FrameLog[2];
			for( int i = 0; i < logs.Length; ++i )
				logs[i] = new FrameLog();

			sampleFrames = targetSampleFrames = 1;

			width = (int)( Core.graphicsDevice.Viewport.Width * 0.8f );

			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
			onGraphicsDeviceReset();
		}


		void onGraphicsDeviceReset()
		{
			var layout = new Layout( Core.graphicsDevice.Viewport );
			_position = layout.place( new Vector2( width, barHeight ), 0, 0.01f, Alignment.BottomCenter );
		}


		[Command( "timeruler", "Toggles the display of the TimerRuler on/off" )]
		static void toggleTimeRuler()
		{
			instance.showLog = !instance.showLog;
			DebugConsole.instance.log( "TimeRuler enabled: " + ( instance.showLog ? "yes" : "no" ) );
			DebugConsole.instance.isOpen = false;
		}
	
		#endregion


		#region Measuring methods

		/// <summary>
		/// Start new frame.
		/// </summary>
		[Conditional( "DEBUG" )]
		public void startFrame()
		{
			lock( this )
			{
				// We skip reset frame when this method gets called multiple times.
				var count = Interlocked.Increment( ref updateCount );
				if( enabled && ( 1 < count && count < maxSampleFrames ) )
					return;

				// Update current frame log.
				prevLog = logs[frameCount++ & 0x1];
				curLog = logs[frameCount & 0x1];

				var endFrameTime = (float)stopwatch.Elapsed.TotalMilliseconds;

				// Update marker and create a log.
				for( var barIdx = 0; barIdx < prevLog.bars.Length; ++barIdx )
				{
					var prevBar = prevLog.bars[barIdx];
					var nextBar = curLog.bars[barIdx];

					// Re-open marker that didn't get called EndMark in previous frame.
					for( var nest = 0; nest < prevBar.nestCount; ++nest )
					{
						var markerIdx = prevBar.markerNests[nest];

						prevBar.markers[markerIdx].endTime = endFrameTime;

						nextBar.markerNests[nest] = nest;
						nextBar.markers[nest].markerId =
                            prevBar.markers[markerIdx].markerId;
						nextBar.markers[nest].beginTime = 0;
						nextBar.markers[nest].endTime = -1;
						nextBar.markers[nest].color = prevBar.markers[markerIdx].color;
					}

					// Update marker log.
					for( var markerIdx = 0; markerIdx < prevBar.markCount; ++markerIdx )
					{
						var duration = prevBar.markers[markerIdx].endTime -
						                                   prevBar.markers[markerIdx].beginTime;

						int markerId = prevBar.markers[markerIdx].markerId;
						MarkerInfo m = markers[markerId];

						m.logs[barIdx].color = prevBar.markers[markerIdx].color;

						if( !m.logs[barIdx].initialized )
						{
							// First frame process.
							m.logs[barIdx].min = duration;
							m.logs[barIdx].max = duration;
							m.logs[barIdx].avg = duration;

							m.logs[barIdx].initialized = true;
						}
						else
						{
							// Process after first frame.
							m.logs[barIdx].min = Math.Min( m.logs[barIdx].min, duration );
							m.logs[barIdx].max = Math.Min( m.logs[barIdx].max, duration );
							m.logs[barIdx].avg += duration;
							m.logs[barIdx].avg *= 0.5f;

							if( m.logs[barIdx].samples++ >= logSnapDuration )
							{
								m.logs[barIdx].snapMin = m.logs[barIdx].min;
								m.logs[barIdx].snapMax = m.logs[barIdx].max;
								m.logs[barIdx].snapAvg = m.logs[barIdx].avg;
								m.logs[barIdx].samples = 0;
							}
						}
					}

					nextBar.markCount = prevBar.nestCount;
					nextBar.nestCount = prevBar.nestCount;
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
		[Conditional( "DEBUG" )]
		public void beginMark( string markerName, Color color )
		{
			beginMark( 0, markerName, color );
		}


		/// <summary>
		/// Start measure time.
		/// </summary>
		/// <param name="barIndex">index of bar</param>
		/// <param name="markerName">name of marker.</param>
		/// <param name="color">color/param>
		[Conditional( "DEBUG" )]
		public void beginMark( int barIndex, string markerName, Color color )
		{
			lock( this )
			{
				if( barIndex < 0 || barIndex >= maxBars )
					throw new ArgumentOutOfRangeException( "barIndex" );

				var bar = curLog.bars[barIndex];

				if( bar.markCount >= maxSamples )
				{
					throw new OverflowException(
						"Exceeded sample count.\n" +
						"Either set larger number to TimeRuler.MaxSmpale or" +
						"lower sample count." );
				}

				if( bar.nestCount >= maxNestCall )
				{
					throw new OverflowException(
						"Exceeded nest count.\n" +
						"Either set larget number to TimeRuler.MaxNestCall or" +
						"lower nest calls." );
				}

				// Gets registered marker.
				int markerId;
				if( !markerNameToIdMap.TryGetValue( markerName, out markerId ) )
				{
					// Register this if this marker is not registered.
					markerId = markers.Count;
					markerNameToIdMap.Add( markerName, markerId );
					markers.Add( new MarkerInfo( markerName ) );
				}

				// Start measuring.
				bar.markerNests[bar.nestCount++] = bar.markCount;

				// Fill marker parameters.
				bar.markers[bar.markCount].markerId = markerId;
				bar.markers[bar.markCount].color = color;
				bar.markers[bar.markCount].beginTime = (float)stopwatch.Elapsed.TotalMilliseconds;

				bar.markers[bar.markCount].endTime = -1;

				bar.markCount++;
			}
		}


		/// <summary>
		/// End measuring.
		/// </summary>
		/// <param name="markerName">Name of marker.</param>
		[Conditional( "DEBUG" )]
		public void endMark( string markerName )
		{
			endMark( 0, markerName );
		}


		/// <summary>
		/// End measuring.
		/// </summary>
		/// <param name="barIndex">Index of bar.</param>
		/// <param name="markerName">Name of marker.</param>
		[Conditional( "DEBUG" )]
		public void endMark( int barIndex, string markerName )
		{
			lock( this )
			{
				if( barIndex < 0 || barIndex >= maxBars )
					throw new ArgumentOutOfRangeException( "barIndex" );

				var bar = curLog.bars[barIndex];

				if( bar.nestCount <= 0 )
				{
					throw new InvalidOperationException( "Call BeingMark method before calling EndMark method." );
				}

				int markerId;
				if( !markerNameToIdMap.TryGetValue( markerName, out markerId ) )
				{
					throw new InvalidOperationException(
						String.Format( "Maker '{0}' is not registered." +
						"Make sure you specifed same name as you used for BeginMark method",
							markerName ) );
				}

				var markerIdx = bar.markerNests[--bar.nestCount];
				if( bar.markers[markerIdx].markerId != markerId )
				{
					throw new InvalidOperationException(
						"Incorrect call order of BeginMark/EndMark method." +
						"You call it like BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)" +
						" But you can't call it like " +
						"BeginMark(A), BeginMark(B), EndMark(A), EndMark(B)." );
				}

				bar.markers[markerIdx].endTime = (float)stopwatch.Elapsed.TotalMilliseconds;
			}
		}


		/// <summary>
		/// Get average time of given bar index and marker name.
		/// </summary>
		/// <param name="barIndex">Index of bar</param>
		/// <param name="markerName">name of marker</param>
		/// <returns>average spending time in ms.</returns>
		public float getAverageTime( int barIndex, string markerName )
		{
			if( barIndex < 0 || barIndex >= maxBars )
				throw new ArgumentOutOfRangeException( "barIndex" );

			var result = 0f;
			int markerId;
			if( markerNameToIdMap.TryGetValue( markerName, out markerId ) )
				result = markers[markerId].logs[barIndex].avg;

			return result;
		}


		/// <summary>
		/// Reset marker log.
		/// </summary>
		[Conditional( "DEBUG" )]
		public void resetLog()
		{
			lock( this )
			{
				foreach( var markerInfo in markers )
				{
					for( var i = 0; i < markerInfo.logs.Length; ++i )
					{
						markerInfo.logs[i].initialized = false;
						markerInfo.logs[i].snapMin = 0;
						markerInfo.logs[i].snapMax = 0;
						markerInfo.logs[i].snapAvg = 0;

						markerInfo.logs[i].min = 0;
						markerInfo.logs[i].max = 0;
						markerInfo.logs[i].avg = 0;

						markerInfo.logs[i].samples = 0;
					}
				}
			}
		}

		#endregion


		#region Draw

		[Conditional( "DEBUG" )]
		public void render()
		{
			render( _position, width );
		}


		[Conditional( "DEBUG" )]
		public void render( Vector2 position, int width )
		{
			// Reset update count.
			Interlocked.Exchange( ref updateCount, 0 );

			if( !showLog )
				return;

			// Gets Batcher, SpriteFont, and WhiteTexture from Graphics.
			var batcher = Graphics.instance.batcher;
			var font = Graphics.instance.bitmapFont;

			// Adjust size and position based of number of bars we should draw.
			var height = 0;
			var maxTime = 0f;
			foreach( var bar in prevLog.bars )
			{
				if( bar.markCount > 0 )
				{
					height += barHeight + barPadding * 2;
					maxTime = Math.Max( maxTime, bar.markers[bar.markCount - 1].endTime );
				}
			}

			// Auto display frame adjustment.
			// For example, if the entire process of frame doesn't finish in less than 16.6ms
			// then it will adjust display frame duration as 33.3ms.
			const float frameSpan = 1.0f / 60.0f * 1000f;
			var sampleSpan = (float)sampleFrames * frameSpan;

			if( maxTime > sampleSpan )
				frameAdjust = Math.Max( 0, frameAdjust ) + 1;
			else
				frameAdjust = Math.Min( 0, frameAdjust ) - 1;

			if( Math.Abs( frameAdjust ) > autoAdjustDelay )
			{
				sampleFrames = Math.Min( maxSampleFrames, sampleFrames );
				sampleFrames = Math.Max( targetSampleFrames, (int)( maxTime / frameSpan ) + 1 );

				frameAdjust = 0;
			}

			// compute factor that converts from ms to pixel.
			var msToPs = (float)width / sampleSpan;

			// Draw start position.
			var startY = (int)position.Y - ( height - barHeight );

			// Current y position.
			var y = startY;

			batcher.begin();

			// Draw transparency background.
			var rc = new Rectangle( (int)position.X, y, width, height );
			batcher.drawRect( rc, new Color( 0, 0, 0, 128 ) );

			// Draw markers for each bars.
			rc.Height = barHeight;
			foreach( var bar in prevLog.bars )
			{
				rc.Y = y + barPadding;
				if( bar.markCount > 0 )
				{
					for( var j = 0; j < bar.markCount; ++j )
					{
						var bt = bar.markers[j].beginTime;
						var et = bar.markers[j].endTime;
						var sx = (int)( position.X + bt * msToPs );
						var ex = (int)( position.X + et * msToPs );
						rc.X = sx;
						rc.Width = Math.Max( ex - sx, 1 );

						batcher.drawRect( rc, bar.markers[j].color );
					}
				}

				y += barHeight + barPadding;
			}

			// Draw grid lines.
			// Each grid represents ms.
			rc = new Rectangle( (int)position.X, (int)startY, 1, height );
			for( float t = 1.0f; t < sampleSpan; t += 1.0f )
			{
				rc.X = (int)( position.X + t * msToPs );
				batcher.drawRect( rc, Color.Gray );
			}

			// Draw frame grid.
			for( var i = 0; i <= sampleFrames; ++i )
			{
				rc.X = (int)( position.X + frameSpan * (float)i * msToPs );
				batcher.drawRect( rc, Color.White );
			}
				
			// Generate log string.
			y = startY - font.lineHeight;
			logString.Length = 0;
			foreach( var markerInfo in markers )
			{
				for( var i = 0; i < maxBars; ++i )
				{
					if( markerInfo.logs[i].initialized )
					{
						if( logString.Length > 0 )
							logString.Append( "\n" );

						logString.Append( " Bar " );
						logString.Append( i );
						logString.Append( "   [" );
						logString.Append( markerInfo.name );

						logString.Append( "] Avg.:  " );
						logString.Append( markerInfo.logs[i].snapAvg.ToString( "0.0000" ) );
						logString.Append( " ms" );

						y -= font.lineHeight;
					}
				}
			}

			// Compute background size and draw it.
			var size = font.measureString( logString );
			rc = new Rectangle( (int)position.X, (int)y, (int)size.X + 25, (int)size.Y + 5 );
			batcher.drawRect( rc, new Color( 0, 0, 0, 128 ) );

			// Draw log string.
			batcher.drawString( font, logString, new Vector2( position.X + 22, y + 3 ), Color.White );


			// Draw log color boxes.
			y += (int)( (float)font.lineHeight * 0.3f );
			rc = new Rectangle( (int)position.X + 4, y, 10, 10 );
			var rc2 = new Rectangle( (int)position.X + 5, y + 1, 8, 8 );
			foreach( var markerInfo in markers )
			{
				for( var i = 0; i < maxBars; ++i )
				{
					if( markerInfo.logs[i].initialized )
					{
						rc.Y = y;
						rc2.Y = y + 1;
						batcher.drawRect( rc, Color.White );
						batcher.drawRect( rc2, markerInfo.logs[i].color );

						y += font.lineHeight;
					}
				}
			}

			batcher.end();
		}

		#endregion

	}

	#endif
}
