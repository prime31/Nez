using System;
using System.Text;
using System.Collections.Generic;



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
}


#if DEBUG
namespace Nez.Console
{
	public partial class DebugConsole
	{
		[Command( "clear", "Clears the terminal" )]
		static void clear()
		{
			instance._drawCommands.Clear();
		}


		[Command( "exit", "Exits the game" )]
		static void exit()
		{
			Core.exit();
		}


		[Command( "inspect", "Inspects the Entity with the passed in name. Pass in no name to stop inspecting the current Entity." )]
		static void inspectEntity( string entityName = "" )
		{
			if( entityName == "" )
			{
				instance._runtimeInspector = null;
			}
			else
			{
				var entity = Core.scene.findEntity( entityName );
				if( entity == null )
				{
					DebugConsole.instance.log( "could not find entity named " + entityName );
					return;
				}

				instance._runtimeInspector = new RuntimeInspector( entity );
			}
		}


		[Command( "console-scale", "Sets the scale that the console is rendered. Defaults to 1 and has a max of 5." )]
		static void setScale( float scale = 1f )
		{
			renderScale = Mathf.clamp( scale, 0.2f, 5f );
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
		static void vsync( bool enabled = true )
		{
			Screen.synchronizeWithVerticalRetrace = enabled;
			DebugConsole.instance.log( "Vertical Sync " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "fixed", "Enables or disables fixed time step" )]
		static void fixedTimestep( bool enabled = true )
		{
			Core._instance.IsFixedTimeStep = enabled;
			DebugConsole.instance.log( "Fixed Time Step " + ( enabled ? "Enabled" : "Disabled" ) );
		}


		[Command( "framerate", "Sets the target framerate. Defaults to 60." )]
		static void framerate( float target = 60f )
		{
			Core._instance.TargetElapsedTime = TimeSpan.FromSeconds( 1.0 / target );
		}


		static ITimer _drawCallTimer;
		[Command( "log-drawcalls", "Enables/disables logging of draw calls in the standard console. Call once to enable and again to disable. delay is how often they should be logged and defaults to 1s." )]
		static void logDrawCalls( float delay = 1f )
		{
			if( _drawCallTimer != null )
			{
				_drawCallTimer.stop();
				_drawCallTimer = null;
				Debug.log( "Draw call logging stopped" );
			}
			else
			{
#if DEBUG
				_drawCallTimer = Core.schedule( delay, true, timer =>
				{
					Debug.log( "Draw Calls: {0}", Core.drawCalls );
				} );
#endif
			}
		}


		[Command( "entity-count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag" )]
		static void entityCount( int tagIndex = -1 )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			if( tagIndex < 0 )
				DebugConsole.instance.log( "Total entities: " + Core.scene.entities.Count.ToString() );
			else
				DebugConsole.instance.log( "Total entities with tag [" + tagIndex + "] " + Core.scene.findEntitiesWithTag( tagIndex ).Count.ToString() );
		}


		[Command( "renderable-count", "Logs amount of Renderables in the Scene. Pass a renderLayer to count only Renderables in that layer" )]
		static void renderableCount( int renderLayer = int.MinValue )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			if( renderLayer != int.MinValue )
				DebugConsole.instance.log( "Total renderables with tag [" + renderLayer + "] " + Core.scene.renderableComponents.componentsWithRenderLayer( renderLayer ).Count.ToString() );
			else
				DebugConsole.instance.log( "Total renderables: " + Core.scene.renderableComponents.Count.ToString() );
		}


		[Command( "renderable-log", "Logs the Renderables in the Scene. Pass a renderLayer to log only Renderables in that layer" )]
		static void renderableLog( int renderLayer = int.MinValue )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			var builder = new StringBuilder();
			foreach( var renderable in Core.scene.renderableComponents )
			{
				if( renderLayer == int.MinValue || renderable.renderLayer == renderLayer )
				{
					builder.AppendFormat( "{0}\n", renderable );
				}
			}

			DebugConsole.instance.log( builder.ToString() );
		}


		[Command( "entity-list", "Logs all entities" )]
		static void logEntities( string whichAssets = "s" )
		{
			if( Core.scene == null )
			{
				DebugConsole.instance.log( "Current Scene is null!" );
				return;
			}

			var builder = new StringBuilder();
			foreach( var entity in Core.scene.entities )
				builder.AppendLine( entity.ToString() );

			DebugConsole.instance.log( builder.ToString() );
		}


		[Command( "timescale", "Sets the timescale. Defaults to 1" )]
		static void tilescale( float timeScale = 1 )
		{
			Time.timeScale = timeScale;
		}


		[Command( "physics", "Logs the total Collider count in the spatial hash" )]
		static void physics( float secondsToDisplay = 5f )
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

			DebugConsole.instance.log( "Physics system collider count: " + ((HashSet<Collider>)Physics.getAllColliders()).Count );
		}


		[Command( "debug-render", "enables/disables debug rendering" )]
		static void debugRender()
		{
			Core.debugRenderEnabled = !Core.debugRenderEnabled;
			DebugConsole.instance.log( string.Format( "Debug rendering {0}", Core.debugRenderEnabled ? "enabled" : "disabled" ) );
		}


		[Command( "help", "Shows usage help for a given command" )]
		static void help( string command )
		{
			if( instance._sorted.Contains( command ) )
			{
				var c = instance._commands[command];
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
				str.Append( string.Join( ", ", instance._sorted ) );
				DebugConsole.instance.log( str.ToString() );
				DebugConsole.instance.log( "Type 'help command' for more info on that command!" );
			}
		}

	}
}
#endif