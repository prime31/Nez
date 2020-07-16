using System;
using System.Text;
using System.Collections.Generic;


namespace Nez.Console
{
	/// <summary>
	/// add this attribute to any static method
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class CommandAttribute : Attribute
	{
		public string Name;
		public string Help;


		public CommandAttribute(string name, string help)
		{
			Name = name;
			Help = help;
		}
	}
}


#if DEBUG
namespace Nez.Console
{
	public partial class DebugConsole
	{
		[Command("clear", "Clears the terminal")]
		static void Clear()
		{
			Instance._drawCommands.Clear();
		}


		[Command("exit", "Exits the game")]
		static void Exit()
		{
			Core.Exit();
		}


		[Command("inspect",
			"Inspects the Entity with the passed in name, or pass in 'pp' or 'postprocessors' to inspect all PostProccessors in the Scene. Pass in no name to close the inspector.")]
		static void InspectEntity(string entityName = "")
		{
			// clean up no matter what
			if (Instance._runtimeInspector != null)
			{
				Instance._runtimeInspector.Dispose();
				Instance._runtimeInspector = null;
			}

			if (entityName == "pp" || entityName == "postprocessors")
			{
				Instance._runtimeInspector = new RuntimeInspector();
				Instance.IsOpen = false;
			}
			else if (entityName != "")
			{
				var entity = Core.Scene.FindEntity(entityName);
				if (entity == null)
				{
					Instance.Log("could not find entity named " + entityName);
					return;
				}

				Instance._runtimeInspector = new RuntimeInspector(entity);
				Instance.IsOpen = false;
			}
		}


		[Command("console-scale", "Sets the scale that the console is rendered. Defaults to 1 and has a max of 5.")]
		static void SetScale(float scale = 1f)
		{
			RenderScale = Mathf.Clamp(scale, 0.2f, 5f);
		}


		[Command("assets", "Logs all loaded assets. Pass 's' for scene assets or 'g' for global assets")]
		static void LogLoadedAssets(string whichAssets = "s")
		{
			if (whichAssets == "s")
				Instance.Log(Core.Scene.Content.LogLoadedAssets());
			else if (whichAssets == "g")
				Instance.Log(Core.Content.LogLoadedAssets());
			else
				Instance.Log("Invalid parameter");
		}


		[Command("vsync", "Enables or disables vertical sync")]
		static void Vsync(bool enabled = true)
		{
			Screen.SynchronizeWithVerticalRetrace = enabled;
			Instance.Log("Vertical Sync " + (enabled ? "Enabled" : "Disabled"));
		}


		[Command("fixed", "Enables or disables fixed time step")]
		static void FixedTimestep(bool enabled = true)
		{
			Core._instance.IsFixedTimeStep = enabled;
			Instance.Log("Fixed Time Step " + (enabled ? "Enabled" : "Disabled"));
		}


		[Command("framerate", "Sets the target framerate. Defaults to 60.")]
		static void Framerate(float target = 60f)
		{
			Core._instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / target);
		}


		static ITimer _drawCallTimer;

		[Command("log-drawcalls",
			"Enables/disables logging of draw calls in the standard console. Call once to enable and again to disable. delay is how often they should be logged and defaults to 1s.")]
		static void LogDrawCalls(float delay = 1f)
		{
			if (_drawCallTimer != null)
			{
				_drawCallTimer.Stop();
				_drawCallTimer = null;
				Debug.Log("Draw call logging stopped");
			}
			else
			{
				_drawCallTimer = Core.Schedule(delay, true, timer => { Debug.Log("Draw Calls: {0}", Core.drawCalls); });
			}
		}


		[Command("entity-count",
			"Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag")]
		static void EntityCount(int tagIndex = -1)
		{
			if (Core.Scene == null)
			{
				Instance.Log("Current Scene is null!");
				return;
			}

			if (tagIndex < 0)
				Instance.Log("Total entities: " + Core.Scene.Entities.Count.ToString());
			else
				Instance.Log("Total entities with tag [" + tagIndex + "] " +
				                          Core.Scene.FindEntitiesWithTag(tagIndex).Count.ToString());
		}


		[Command("renderable-count",
			"Logs amount of Renderables in the Scene. Pass a renderLayer to count only Renderables in that layer")]
		static void RenderableCount(int renderLayer = int.MinValue)
		{
			if (Core.Scene == null)
			{
				Instance.Log("Current Scene is null!");
				return;
			}

			if (renderLayer != int.MinValue)
				Instance.Log("Total renderables with tag [" + renderLayer + "] " +
				                          Core.Scene.RenderableComponents.ComponentsWithRenderLayer(renderLayer).Length
					                          .ToString());
			else
				Instance.Log("Total renderables: " + Core.Scene.RenderableComponents.Count.ToString());
		}


		[Command("renderable-log",
			"Logs the Renderables in the Scene. Pass a renderLayer to log only Renderables in that layer")]
		static void RenderableLog(int renderLayer = int.MinValue)
		{
			if (Core.Scene == null)
			{
				Instance.Log("Current Scene is null!");
				return;
			}

			var builder = new StringBuilder();
			for (var i = 0; i < Core.Scene.RenderableComponents.Count; i++)
			{
				var renderable = Core.Scene.RenderableComponents[i];
				if (renderLayer == int.MinValue || renderable.RenderLayer == renderLayer)
					builder.AppendFormat("{0}\n", renderable);
			}

			Instance.Log(builder.ToString());
		}


		[Command("entity-list", "Logs all entities")]
		static void LogEntities(string whichAssets = "s")
		{
			if (Core.Scene == null)
			{
				Instance.Log("Current Scene is null!");
				return;
			}

			var builder = new StringBuilder();
			for (var i = 0; i < Core.Scene.Entities.Count; i++)
				builder.AppendLine(Core.Scene.Entities[i].ToString());

			Instance.Log(builder.ToString());
		}


		[Command("timescale", "Sets the timescale. Defaults to 1")]
		static void Tilescale(float timeScale = 1)
		{
			Time.TimeScale = timeScale;
		}


		[Command("physics", "Logs the total Collider count in the spatial hash")]
		static void Physics(float secondsToDisplay = 5f)
		{
			// store off the current state so we can reset it when we are done
			var debugRenderState = Core.DebugRenderEnabled;
			Core.DebugRenderEnabled = true;

			var ticker = 0f;
			Core.Schedule(0f, true, null, timer =>
			{
				Nez.Physics.DebugDraw(0f);
				ticker += Time.DeltaTime;
				if (ticker >= secondsToDisplay)
				{
					timer.Stop();
					Core.DebugRenderEnabled = debugRenderState;
				}
			});

			Instance.Log("Physics system collider count: " +
			                          ((HashSet<Collider>) Nez.Physics.GetAllColliders()).Count);
		}


		[Command("debug-render", "enables/disables debug rendering")]
		static void DebugRender()
		{
			Core.DebugRenderEnabled = !Core.DebugRenderEnabled;
			Instance.Log(string.Format("Debug rendering {0}",
				Core.DebugRenderEnabled ? "enabled" : "disabled"));
		}


		[Command("help", "Shows usage help for a given command")]
		static void Help(string command)
		{
			if (Instance._sorted.Contains(command))
			{
				var c = Instance._commands[command];
				StringBuilder str = new StringBuilder();

				//Title
				str.Append(":: ");
				str.Append(command);

				//Usage
				if (!string.IsNullOrEmpty(c.Usage))
				{
					str.Append(" ");
					str.Append(c.Usage);
				}

				Instance.Log(str.ToString());

				//Help
				if (string.IsNullOrEmpty(c.Help))
					Instance.Log("No help info set");
				else
					Instance.Log(c.Help);
			}
			else
			{
				StringBuilder str = new StringBuilder();
				str.Append("Commands list: ");
				str.Append(string.Join(", ", Instance._sorted));
				Instance.Log(str.ToString());
				Instance.Log("Type 'help command' for more info on that command!");
			}
		}
	}
}
#endif