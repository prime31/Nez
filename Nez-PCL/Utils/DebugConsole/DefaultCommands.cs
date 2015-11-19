using System;
using System.Text;


namespace Nez.Console
{
	/// <summary>
	/// add this attribute to any static method
	/// </summary>
	public class Command : Attribute
	{
		public string name;
		public string help;


		public Command( string name, string help )
		{
			this.name = name;
			this.help = help;
		}
	}


	public partial class DebugConsole
	{
		[Command( "clear", "Clears the terminal" )]
		static public void clear()
		{
			DebugConsole.instance._drawCommands.Clear();
		}


		[Command( "exit", "Exits the game" )]
		static private void exit()
		{
			Core.instance.Exit();
		}


		[Command( "vsync", "Enables or disables vertical sync" )]
		static private void vsync( bool enabled = true )
		{
			Core.instance._graphicsManager.SynchronizeWithVerticalRetrace = enabled;
			//Core.instance._graphicsManager.ApplyChanges(); // crashes mac

			DebugConsole.instance.log( "Vertical Sync " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "fixed", "Enables or disables fixed time step" )]
		static private void fixedTimestep( bool enabled = true )
		{
			Core.instance.IsFixedTimeStep = enabled;
			DebugConsole.instance.log( "Fixed Time Step " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "framerate", "Sets the target framerate" )]
		static private void framerate( float target )
		{
			Core.instance.TargetElapsedTime = TimeSpan.FromSeconds( 1.0 / target );
		}


		[Command( "count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag" )]
		static private void count( int tagIndex = -1 )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			if( tagIndex < 0 )
				DebugConsole.instance.log( Core.scene.entities.Count.ToString() );
			else
				DebugConsole.instance.log( Core.scene.findEntitiesByTag( tagIndex ).Count.ToString() );
		}


		[Command( "tracker", "Logs all tracked objects in the scene. Set mode to 'e' for just entities, or 'c' for just components" )]
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


		[Command( "pooler", "Logs the pooled Entity counts" )]
		static private void pooler()
		{
			//Engine.Pooler.Log();
			Debug.log( "Pooler" );
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

