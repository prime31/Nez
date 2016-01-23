using System;
using System.Text;


namespace Nez.Console
{
	/// <summary>
	/// add this attribute to any static method
	/// </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
	public class CommandAttribute : Attribute
	{
		public string name;
		public string help;


		public CommandAttribute( string name, string help )
		{
			this.name = name;
			this.help = help;
		}
	}


	public partial class DebugConsole
	{
		[Command( "clear", "Clears the terminal" )]
		static void clear()
		{
			DebugConsole.instance._drawCommands.Clear();
		}


		[Command( "exit", "Exits the game" )]
		static void exit()
		{
			Core.exit();
		}


		[Command( "assets", "Logs all loaded assets. Pass 's' for scene assets or 'g' for global assets" )]
		static void logLoadedAssets( string whichAssets = "s" )
		{
			if( whichAssets == "s" )
				DebugConsole.instance.log( Core.scene.contentManager.logLoadedAssets() );
			else if( whichAssets == "g" )
				DebugConsole.instance.log( Core.contentManager.logLoadedAssets() );
			else
				DebugConsole.instance.log( "Invalid parameter" );
		}


		[Command( "vsync", "Enables or disables vertical sync" )]
		static private void vsync( bool enabled = true )
		{
			Screen.synchronizeWithVerticalRetrace = enabled;
			DebugConsole.instance.log( "Vertical Sync " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "fixed", "Enables or disables fixed time step" )]
		static private void fixedTimestep( bool enabled = true )
		{
			Core._instance.IsFixedTimeStep = enabled;
			DebugConsole.instance.log( "Fixed Time Step " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "framerate", "Sets the target framerate" )]
		static private void framerate( float target = 60f )
		{
			Core._instance.TargetElapsedTime = TimeSpan.FromSeconds( 1.0 / target );
		}


		[Command( "entity-count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag" )]
		static private void count( int tagIndex = -1 )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			if( tagIndex < 0 )
				DebugConsole.instance.log( "Total entities: " + Core.scene.entities.Count.ToString() );
			else
				DebugConsole.instance.log( "Total entities with tag [" + tagIndex + "] " + Core.scene.findEntitiesByTag( tagIndex ).Count.ToString() );
		}


		//[Command( "tracker", "Logs all tracked objects in the scene. Set mode to 'e' for just entities, or 'c' for just components" )]
		static private void tracker( string mode )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			switch( mode )
			{
				default:
					DebugConsole.instance.log( "-- Entities --" );
					//Engine.Scene.Tracker.LogEntities();
					DebugConsole.instance.log( "-- Components --" );
					//Engine.Scene.Tracker.LogComponents();
				break;

				case "e":
					//Engine.Scene.Tracker.LogEntities();
				break;

				case "c":
					//Engine.Scene.Tracker.LogComponents();
				break;
			}
		}


		//[Command( "pooler", "Logs the pooled Entity counts" )]
		static private void pooler()
		{
			//Engine.Pooler.Log();
			Debug.log( "Pooler" );
		}


		[Command( "physics", "Logs the total Collider count in the spatial hash" )]
		static private void physics( float secondsToDisplay = 5f )
		{
			// store off the current state so we can reset it when we are done
			var debugRenderState = Core.debugRenderEnabled;
			Core.debugRenderEnabled = true;

			var ticker = 0f;
			Core.schedule( 0f, true, null, timer =>
			{
				Physics.debugDraw( 0f );
				ticker += Time.deltaTime;
				if( ticker >= secondsToDisplay )
				{
					timer.stop();
					Core.debugRenderEnabled = debugRenderState;
				}
			});

			DebugConsole.instance.log( "Physics system collider count: " + Physics.getAllColliders().Count );
		}


		[Command( "debug-render", "enables/disables debug rendering" )]
		static private void debugRender()
		{
			Core.debugRenderEnabled = !Core.debugRenderEnabled;
			DebugConsole.instance.log( string.Format( "Debug rendering {0}", Core.debugRenderEnabled ? "enabled" : "disabled" ) );
		}


		[Command( "help", "Shows usage help for a given command" )]
		static private void help( string command )
		{
			if( DebugConsole.instance._sorted.Contains( command ) )
			{
				var c = DebugConsole.instance._commands[command];
				StringBuilder str = new StringBuilder();

				//Title
				str.Append( ":: " );
				str.Append( command );

				//Usage
				if( !string.IsNullOrEmpty( c.usage ) )
				{
					str.Append( " " );
					str.Append( c.usage );
				}
				DebugConsole.instance.log( str.ToString() );

				//Help
				if( string.IsNullOrEmpty( c.help ) )
					DebugConsole.instance.log( "No help info set" );
				else
					DebugConsole.instance.log( c.help );
			}
			else
			{
				StringBuilder str = new StringBuilder();
				str.Append( "Commands list: " );
				str.Append( string.Join( ", ", DebugConsole.instance._sorted ) );
				DebugConsole.instance.log( str.ToString() );
				DebugConsole.instance.log( "Type 'help command' for more info on that command!" );
			}
		}

	}
}

